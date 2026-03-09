using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace GitViewer.Api
{
    public class GitRepoManager : IGitRepoManager
    {
        private readonly string reposRoot;
        private readonly string keysRoot;

        public GitRepoManager(IOptions<GitRepoSettings> settings)
        {
            reposRoot = settings.Value.ReposRoot;
            keysRoot = settings.Value.KeysRoot;

            Directory.CreateDirectory(reposRoot);
            Directory.CreateDirectory(keysRoot);
        }

        public string GetRepoPath(Guid ownerUserId, Guid repoId) =>
            Path.Combine(reposRoot, ownerUserId.ToString(), repoId.ToString());

        public string GetKeyPath(Guid ownerUserId, Guid repoId) =>
            Path.Combine(keysRoot, ownerUserId.ToString(), repoId.ToString());

        public (string privateKey, string publicKey) GenerateDeployKey(Guid ownerUserId, Guid repoId)
        {
            var keyPath = GetKeyPath(ownerUserId, repoId);
            Directory.CreateDirectory(keyPath);

            var privateKeyFile = Path.Combine(keyPath, "id_ed25519");
            var publicKeyFile = privateKeyFile + ".pub";

            if (File.Exists(privateKeyFile) && File.Exists(publicKeyFile))
            {
                Console.WriteLine($"[INFO] Deploy key already exists for repo {repoId}");
                return (File.ReadAllText(privateKeyFile), File.ReadAllText(publicKeyFile));
            }

            Console.WriteLine($"[INFO] Generating new deploy key for repo {repoId} at {keyPath}");

            var psi = new ProcessStartInfo("ssh-keygen", $"-t ed25519 -C \"gitviewer-{repoId}\" -f \"{privateKeyFile}\" -N \"\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var proc = Process.Start(psi)!;
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                throw new InvalidOperationException($"Failed to generate SSH key for repo {repoId}");
            }

            return (File.ReadAllText(privateKeyFile), File.ReadAllText(publicKeyFile));
        }

        public async Task<bool> CloneRepoAsync(Guid ownerUserId, Guid repoId, string repoUrl, bool useSsh = false)
        {
            var repoPath = GetRepoPath(ownerUserId, repoId);
            var gitFolder = Path.Combine(repoPath, ".git");

            try
            {
                // Try to update if repo exists
                if (Directory.Exists(gitFolder))
                {
                    Console.WriteLine($"[INFO] Repository already exists at {repoPath}. Attempting to update...");

                    if (await UpdateRepoAsync(repoPath, useSsh, ownerUserId, repoId))
                    {
                        Console.WriteLine($"[INFO] Repository updated successfully.");
                        return true;
                    }

                    Console.WriteLine($"[WARN] Repo update failed. Re-cloning from scratch...");
                    SafeDeleteDirectory(repoPath);
                }

                // Make a fresh clone if repo doesn't exist or update failed
                Console.WriteLine($"[INFO] Cloning repository from {repoUrl} to {repoPath}...");
                SafeDeleteDirectory(repoPath);

                var cloneArgs = $"clone {repoUrl} \"{repoPath}\"";
                if (!await RunGitCommandAsync(cloneArgs, useSsh, ownerUserId, repoId))
                {
                    Console.WriteLine($"[ERROR] Failed to clone repository.");
                    return false;
                }

                Console.WriteLine($"[INFO] Repository cloned successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception during clone: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> UpdateRepoAsync(string repoPath, bool useSsh, Guid ownerUserId, Guid repoId)
        {
            // Fetch all remotes, reset to origin/main, and clean up old files
            var commands = new[]
            {
        $"-C \"{repoPath}\" fetch --all",
        $"-C \"{repoPath}\" reset --hard origin/main",
        $"-C \"{repoPath}\" clean -fd"
    };

            foreach (var args in commands)
            {
                if (!await RunGitCommandAsync(args, useSsh, ownerUserId, repoId))
                    return false;
            }

            return true;
        }

        private async Task<bool> RunGitCommandAsync(string args, bool useSsh, Guid ownerUserId, Guid repoId)
        {
            ProcessStartInfo psi;

            if (useSsh)
            {
                var keyPath = Path.Combine(GetKeyPath(ownerUserId, repoId), "id_ed25519");
                if (!File.Exists(keyPath))
                {
                    Console.WriteLine($"[ERROR] SSH key not found at: {keyPath}");
                    return false;
                }

                string normalizedKeyPath = Path.GetFullPath(keyPath).Replace("\\", "/");

                // Normalize SSH key path for Git Bash or WSL environments
                if (OperatingSystem.IsWindows() && normalizedKeyPath.StartsWith("C:/"))
                    normalizedKeyPath = "/c/" + normalizedKeyPath.Substring(3);

                psi = new ProcessStartInfo("git", args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                psi.Environment["GIT_SSH_COMMAND"] = $"ssh -i \"{normalizedKeyPath}\" -o StrictHostKeyChecking=no";
            }
            else
            {
                psi = new ProcessStartInfo("git", args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
            }

            using var proc = Process.Start(psi)!;
            string output = await proc.StandardOutput.ReadToEndAsync();
            string error = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
            {
                Console.WriteLine($"[GIT ERROR] {error}");
                return false;
            }

            Console.WriteLine($"[GIT OUTPUT] {output}");
            return true;
        }

        public Task<bool> DeleteRepoFilesAsync(Guid ownerUserId, Guid repoId)
        {
            var repoPath = GetRepoPath(ownerUserId, repoId);
            var keyPath = GetKeyPath(ownerUserId, repoId);

            return Task.Run(() =>
            {
                var success = true;

                if (!SafeDeleteDirectory(repoPath))
                {
                    Console.WriteLine($"[WARN] Failed to fully delete repo folder: {repoPath}");
                    success = false;
                }

                if (!SafeDeleteDirectory(keyPath))
                {
                    Console.WriteLine($"[WARN] Failed to fully delete key folder: {keyPath}");
                    success = false;
                }

                return success;
            });
        }

        public Task<bool> DeleteAllUserRepoFilesAsync(Guid ownerUserId)
        {
            var userRepoRoot = Path.Combine(reposRoot, ownerUserId.ToString());
            var userKeyRoot = Path.Combine(keysRoot, ownerUserId.ToString());

            return Task.Run(() =>
            {
                var success = true;

                if (!SafeDeleteDirectory(userRepoRoot))
                {
                    Console.WriteLine($"[WARN] Failed to delete user repo root: {userRepoRoot}");
                    success = false;
                }

                if (!SafeDeleteDirectory(userKeyRoot))
                {
                    Console.WriteLine($"[WARN] Failed to delete user key root: {userKeyRoot}");
                    success = false;
                }

                return success;
            });
        }

        private bool SafeDeleteDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return true;  // nothing to delete -- treat as success
            }

            try
            {
                var dirInfo = new DirectoryInfo(path);
                foreach (var info in dirInfo.GetFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    info.Attributes = FileAttributes.Normal;
                }

                Directory.Delete(path, recursive: true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Safe delete failed for '{path}': {ex.Message}");
                try
                {
                    var psi = OperatingSystem.IsWindows()
                        ? new ProcessStartInfo("cmd.exe", $"/c rmdir /s /q \"{path}\"")
                        : new ProcessStartInfo("rm", $"-rf \"{path}\"");

                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi)?.WaitForExit();
                    return !Directory.Exists(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] Fallback delete failed for '{path}': {e.Message}");
                    return false;
                }
            }
        }
    }
}
