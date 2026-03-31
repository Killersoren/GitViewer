using FluentResults;
using GitViewer.Api.Dto;
using GitViewer.DataAccess.Models;

namespace GitViewer.Api.Services.Interfaces
{
    public interface IRepositoryService
    {
        Task<Result<Repository>> GetRepoAsync(Guid repoId, Guid? requesterId, string clientIp);
        Task<Result<IAsyncEnumerable<Repository>>> GetPublicReposAsync(Guid userId, Guid? requesterId, string clientIp);
        Task<Result<IEnumerable<Repository>>> GetUserReposAsync(Guid userId);
        Task<Result<Repository>> AddRepoAsync(RepositoryDto dto, Guid userId);
        Task<Result> UpdateRepoAsync(Guid repoId, Guid userId, RepositoryDto dto);
        Task<Result> DeleteRepoAsync(Guid repoId, Guid userId);
        Task<Result> TogglePublicAsync(Guid repoId, Guid userId, bool isPublic);
        Task<Result> CloneRepoAsync(Guid repoId, Guid userId, bool useSsh = true);
        Task<Result<IAsyncEnumerable<Repository>>> GetUserReposFromShareLinkAsync(Guid shareLinkId, Guid? requesterId, string clientIp);
        Task<Result<Repository>> GetRepoAsyncWithShareLink(Guid repoId, Guid value, Guid? requesterId, string clientIp);
    }
}
