using GitViewer.DataAccess.Models;

namespace GitViewer.Api.Services.Interfaces
{
    public interface ILoggingService
    {
        Task LogRepositoryCreatedAsync(Repository repo, Guid userId);
        Task LogRepositoryUpdatedAsync(Repository repo, Guid userId);
        Task LogRepositoryDeletedAsync(Repository repo, Guid userId);
        Task LogRepositoryViewedAsync(Repository repo, Guid? viewerId, string clientIp);
        Task LogPublicReposViewedAsync(User user, Guid? viewerId, string clientIp);
        Task LogAccountCreatedAsync(User user);
        Task LogShareLinkViewedAsync(ShareLink shareLink, Guid? requesterId, string clientIp);
        Task LogRepositoryViewedAsyncWithShareLink(Repository repo, ShareLink shareLink, Guid? viewerId, string clientIp);
        Task LogShareLinkDeletedAsync(ShareLink shareLink, Guid userId);
        Task LogShareLinkCreatedAsync(ShareLink shareLink, Guid userId);
    }
}
