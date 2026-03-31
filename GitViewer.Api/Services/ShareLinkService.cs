using FluentResults;
using GitViewer.Api.Services.Interfaces;
using GitViewer.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GitViewer.Api.Services
{
    public class ShareLinkService : IShareLinkService
    {
        private readonly GitViewerServiceContext _context;
        private readonly ILoggingService _loggingService;

        public ShareLinkService(GitViewerServiceContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public Task<Guid> CreateShareLinkRepoIdAsync(Guid repoId, string name)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<Guid>> CreateShareLinkUserIdAsync(Guid userId, string name, int? expiryTimeInDays)
        {
            DateTime? expiry = expiryTimeInDays.HasValue
                ? DateTime.UtcNow.AddDays(expiryTimeInDays.Value)
                : null;

            var ShareLink = new ShareLink
            {
                Id = Guid.NewGuid(),
                Name = name,
                UserId = userId,
                Created = DateTime.UtcNow,
                ExpiryTime = expiry

            };

            _context.ShareLinks.Add(ShareLink);
            await _context.SaveChangesAsync();

            await _loggingService.LogShareLinkCreatedAsync(ShareLink, userId);

            return Result.Ok(ShareLink.Id).Value;
        }

        public async Task<Result> DeleteShareLinkAsync(Guid userId, Guid shareLinkId)
        {
            var shareLink = await _context.ShareLinks.FindAsync(shareLinkId);
            if (shareLink is null)
            {
                return Result.Fail("Sharelink not found");
            }

            if (shareLink.UserId != userId)
            {
                return Result.Fail("Unauthorized");
            }

            _context.ShareLinks.Remove(shareLink);
            await _context.SaveChangesAsync();

            await _loggingService.LogShareLinkDeletedAsync(shareLink, userId);

            return Result.Ok();
        }

        public async Task<Result<IEnumerable<ShareLink>>> GetShareLinksForUserAsync(Guid userId)
        {
            var shareLinks = await _context.ShareLinks
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return Result.Ok(shareLinks.AsEnumerable());
        }

        public async Task<Result<ShareLink>> GetShareLinkAsync(Guid shareLinkId)
        {
            var shareLink = await _context.ShareLinks.FindAsync(shareLinkId);
            if (shareLink is null)
            {
                return Result.Fail("Sharelink not found");
            }
            return Result.Ok(shareLink);
        }
    }
}