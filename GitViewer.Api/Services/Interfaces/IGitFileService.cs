namespace GitViewer.Api.Services.Interfaces
{
    public interface IGitFileService
    {
        Task<object> GetTreeAsync(Guid repoId, string? path);
        Task<string> GetFileContentAsync(Guid repoId, string? path);
        Task<(Stream stream, string fileName)> DownloadAsync(Guid repoId);
        Task<(Stream stream, string fileName)> DownloadAsync(Guid repoId, Guid requesterId);
    }
}
