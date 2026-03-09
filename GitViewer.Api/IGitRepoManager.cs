namespace GitViewer.Api
{
    public interface IGitRepoManager
    {
        string GetRepoPath(Guid ownerUserId, Guid repoId);
        string GetKeyPath(Guid ownerUserId, Guid repoId);
        (string privateKey, string publicKey) GenerateDeployKey(Guid ownerUserId, Guid repoId);
        Task<bool> CloneRepoAsync(Guid ownerUserId, Guid repoId, string repoUrl, bool useSsh = false);
        Task<bool> DeleteRepoFilesAsync(Guid ownerUserId, Guid repoId);
        Task<bool> DeleteAllUserRepoFilesAsync(Guid ownerUserId);
    }
}
