using FluentResults;
using GitViewer.DataAccess.Models;

namespace GitViewer.Api.Services.Interfaces
{
    public interface IShareLinkService
    {
        Task<Result<Guid>> CreateShareLinkUserIdAsync(Guid userId, string name, int? expiryTimeInDays);
        Task<Guid> CreateShareLinkRepoIdAsync(Guid repoId, string name);
        Task<Result> DeleteShareLinkAsync(Guid shareLinkId, Guid userId);
        Task<Result<IEnumerable<ShareLink>>> GetShareLinksForUserAsync(Guid userId);
    }
}
