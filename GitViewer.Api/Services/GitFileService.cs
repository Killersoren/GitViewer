using GitViewer.Api.Services.Interfaces;
using GitViewer.DataAccess.Models;
using System.IO.Compression;

namespace GitViewer.Api.Services
{
    public class GitFileService : IGitFileService
    {
        private readonly GitViewerServiceContext _context;
        private readonly IGitRepoManager _gitManager;

        public GitFileService(
            GitViewerServiceContext context,
            IGitRepoManager gitManager)
        {
            _context = context;
            _gitManager = gitManager;
        }

        public async Task<object> GetTreeAsync(Guid repoId, string? path)
        {
            var repo = await _context.Repository.FindAsync(repoId)
                ?? throw new KeyNotFoundException();

            var repoPath = _gitManager.GetRepoPath(repo.UserId, repo.Id);
            var fullPath = Path.Combine(repoPath, path ?? "");

            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException();
            }

            return new
            {
                directories = Directory.GetDirectories(fullPath).Select(Path.GetFileName),
                files = Directory.GetFiles(fullPath).Select(Path.GetFileName)
            };
        }

        public async Task<string> GetFileContentAsync(Guid repoId, string? path)
        {
            var repo = await _context.Repository.FindAsync(repoId)
                ?? throw new KeyNotFoundException();

            var repoPath = _gitManager.GetRepoPath(repo.UserId, repo.Id);
            var fullPath = Path.Combine(repoPath, path);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException();
            }

            return await File.ReadAllTextAsync(fullPath);
        }

        public async Task<(Stream stream, string fileName)> DownloadAsync(Guid repoId)
        {
            var repo = await _context.Repository.FindAsync(repoId)
                ?? throw new KeyNotFoundException();

            var repoPath = _gitManager.GetRepoPath(repo.UserId, repo.Id);

            var zipPath = Path.Combine(Path.GetTempPath(), $"{repoId}.zip");
            ZipFile.CreateFromDirectory(repoPath, zipPath);

            var stream = File.OpenRead(zipPath);
            return (stream, $"{repoId}.zip");
        }

        public Task<(Stream stream, string fileName)> DownloadAsync(Guid repoId, Guid requesterId)
        {
            throw new NotImplementedException();
        }
    }
}
