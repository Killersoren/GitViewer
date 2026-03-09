using GitViewer.DataAccess.Models;

namespace GitViewer.Api.Services
{
    public interface ILoggingService
    {
        Task LogRepositoryCreatedAsync(Repository repo, Guid userId);
        Task LogRepositoryUpdatedAsync(Repository repo, Guid userId);
        Task LogRepositoryDeletedAsync(Repository repo, Guid userId);
        Task LogRepositoryViewedAsync(Repository repo, Guid? viewerId, string clientIp);
        Task LogPublicReposViewedAsync(User user, Guid? viewerId, string clientIp);
        Task LogAccountCreatedAsync(User user);
    }
}
