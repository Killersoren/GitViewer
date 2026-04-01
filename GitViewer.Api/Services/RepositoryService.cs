using FluentResults;
using GitViewer.Api.Dto;
using GitViewer.Api.Helpers;
using GitViewer.Api.Services.Interfaces;
using GitViewer.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GitViewer.Api.Services
{
    public class RepositoryService : IRepositoryService
    {
        private readonly GitViewerServiceContext _context;
        private readonly IGitRepoManager _gitManager;
        private readonly ILoggingService _loggingService;

        public RepositoryService(
            GitViewerServiceContext context,
            IGitRepoManager gitManager,
            ILoggingService loggingService)
        {
            _context = context;
            _gitManager = gitManager;
            _loggingService = loggingService;
        }

        public async Task<Result<Repository>> GetRepoAsync(Guid repoId, Guid? requesterId, string clientIp)
        {
            var repo = await _context.Repository.FindAsync(repoId);
            if (repo is null)
            {
                return Result.Fail("Repo not found");
            }

            if (!repo.IsPublic && !requesterId.HasValue)
            {
                return Result.Fail("Unauthorized");
            }

            if (!repo.IsPublic && repo.UserId != requesterId.Value)
            {
                return Result.Fail("Forbidden");
            }

            bool isOwner = repo.UserId == requesterId;

            if (!isOwner)
            {
                await _loggingService.LogRepositoryViewedAsync(repo, requesterId, clientIp);
            }

            return Result.Ok(repo);
        }

        public async Task<Result<Repository>> GetRepoAsyncWithShareLink(Guid shareLink, Guid repoId, Guid? requesterId, string clientIp)
        {
            var repo = await _context.Repository.FindAsync(repoId);
            if (repo is null)
            {
                return Result.Fail("Repo not found");
            }

            if (!repo.IsPublic && !requesterId.HasValue)
            {
                return Result.Fail("Unauthorized");
            }

            if (!repo.IsPublic && repo.UserId != requesterId.Value)
            {
                return Result.Fail("Forbidden");
            }

            var shareLinkEntity = await _context.ShareLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == shareLink);
            if (shareLinkEntity is null || shareLinkEntity.UserId != repo.UserId)
            {
                return Result.Fail("Invalid share link");
            }
            bool isOwner = repo.UserId == requesterId;
            if (!isOwner)
            {
                await _loggingService.LogRepositoryViewedAsyncWithShareLink(repo, shareLinkEntity, requesterId, clientIp);
            }
            return Result.Ok(repo);
        }

        public async Task<Result<IAsyncEnumerable<Repository>>> GetPublicReposAsync(Guid userId, Guid? requesterId, string clientIp)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return Result.Fail("User not found");
            }

            if (!user.IsPublic && !requesterId.HasValue)
            {
                return Result.Fail("Unauthorized");
            }

            if (!user.IsPublic && user.Id != requesterId.Value)
            {
                return Result.Fail("Forbidden");
            }

            var repos = _context.Repository
                .Where(r => r.UserId == userId && r.IsPublic)
                .AsNoTracking()
                .AsAsyncEnumerable();

            bool isOwner = requesterId.HasValue && user.Id == requesterId.Value;
            if (!isOwner)
            {
                await _loggingService.LogPublicReposViewedAsync(user, requesterId, clientIp);
            }

            return Result.Ok(repos);
        }

        public async Task<Result<Repository>> AddRepoAsync(RepositoryDto dto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Source))
            {
                return Result.Fail("Source is required");
            }

            // Validate URL
            if (!UrlValidator.ValidateUrlWithUriCreate(dto.Source, out Uri? uri, out bool isSsh))
            {
                return Result.Fail("Invalid URL format");
            }

            if (!isSsh && (uri is null || !uri.IsAbsoluteUri))
            {
                return Result.Fail("URL must be absolute or valid SSH format");
            }

            var repo = new Repository
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Source = dto.Source,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                Name = string.IsNullOrWhiteSpace(dto.Name)
                    ? (UrlValidator.ExtractRepoName(dto.Source) ?? dto.Source)
                    : dto.Name,
                IsPublic = false
            };

            _context.Repository.Add(repo);
            await _context.SaveChangesAsync();


            await _loggingService.LogRepositoryCreatedAsync(repo, userId);

            return Result.Ok(repo);
        }

        public async Task<Result> UpdateRepoAsync(Guid repoId, Guid userId, RepositoryDto dto)
        {
            var repo = await _context.Repository.FindAsync(repoId);
            if (repo is null)
            {
                return Result.Fail("Repository not found");
            }

            if (repo.UserId != userId)
            {
                return Result.Fail("Forbidden");
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                repo.Name = dto.Name;
            }

            if (!string.IsNullOrWhiteSpace(dto.Source))
            {
                if (!UrlValidator.ValidateUrlWithUriCreate(dto.Source, out Uri? uri, out bool isSsh))
                {
                    return Result.Fail("Invalid URL format");
                }

                if (!isSsh && (uri is null || !uri.IsAbsoluteUri))
                {
                    return Result.Fail("URL must be absolute or valid SSH format");
                }

                repo.Source = dto.Source;
            }

            repo.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _loggingService.LogRepositoryUpdatedAsync(repo, userId);

            return Result.Ok();
        }

        public async Task<Result> DeleteRepoAsync(Guid repoId, Guid userId)
        {
            var repo = await _context.Repository.FindAsync(repoId);
            if (repo is null)
            {
                return Result.Fail("Repository not found");
            }

            if (repo.UserId != userId)
            {
                return Result.Fail("Forbidden");
            }

            // Delete files 
            try
            {
                var deleteSuccess = await _gitManager.DeleteRepoFilesAsync(repo.UserId, repo.Id);
                if (!deleteSuccess)
                {
                    Console.WriteLine($"[WARN] File deletion partially failed for repo {repo.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception while deleting repo files: {ex.Message}");
            }

            _context.Repository.Remove(repo);
            await _context.SaveChangesAsync();


            await _loggingService.LogRepositoryDeletedAsync(repo, userId);

            return Result.Ok();
        }

        public async Task<Result> TogglePublicAsync(Guid repoId, Guid userId, bool isPublic)
        {
            var repo = await _context.Repository.FindAsync(repoId);
            if (repo is null)
            {
                return Result.Fail("Repository not found");
            }

            if (repo.UserId != userId)
            {
                return Result.Fail("Forbidden");
            }

            repo.IsPublic = isPublic;
            await _context.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> CloneRepoAsync(Guid repoId, Guid userId, bool useSsh = true)
        {
            var repo = await _context.Repository.FindAsync(repoId);
            if (repo is null)
            {
                return Result.Fail("Repository not found");
            }

            if (repo.UserId != userId)
            {
                return Result.Fail("Forbidden");
            }

            var success = await _gitManager.CloneRepoAsync(repo.UserId, repo.Id, repo.Source, useSsh);

            if (!success)
            {
                repo.Status = "CloneFailed";
                _context.Update(repo);
                await _context.SaveChangesAsync();
                return Result.Fail("Clone failed or already exists");
            }

            repo.Status = "Success";
            repo.Updated = DateTime.UtcNow;
            _context.Update(repo);
            await _context.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result<IEnumerable<Repository>>> GetUserReposAsync(Guid userId)
        {
            var repos = await _context.Repository
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return Result.Ok(repos.AsEnumerable());
        }

        public async Task<Result<IAsyncEnumerable<Repository>>> GetUserReposFromShareLinkAsync(Guid shareLinkId, Guid? requesterId, string clientIp)
        {
            var shareLink = await _context.ShareLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == shareLinkId);

            var userId = shareLink?.UserId;

            if (shareLinkId == Guid.Empty || shareLink is null)
            {
                return Result.Fail("Share link not found");
            }

            var repos = _context.Repository
                .Where(r => r.UserId == userId && r.IsPublic)
                .AsNoTracking()
                .AsAsyncEnumerable();

            bool isOwner = requesterId.HasValue && shareLink.UserId == requesterId.Value;
            if (!isOwner)
            {
                await _loggingService.LogShareLinkViewedAsync(shareLink, requesterId, clientIp);
            }

            return Result.Ok(repos);
        }
    }
}
