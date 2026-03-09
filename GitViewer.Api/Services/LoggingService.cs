using GitViewer.Api.Dto;
using GitViewer.Api.Helpers;
using GitViewer.Api.RabbitMQ;
using GitViewer.DataAccess.Models;

namespace GitViewer.Api.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly IMessageProducer _messageProducer;
        private readonly ILogRateLimiter _rateLimiter;

        public LoggingService(IMessageProducer messageProducer, ILogRateLimiter rateLimiter)
        {
            _messageProducer = messageProducer;
            _rateLimiter = rateLimiter;
        }

        public async Task LogRepositoryCreatedAsync(Repository repo, Guid userId)
        {
            try
            {
                var logDto = new LogDto
                {
                    EventType = "RepositoryCreated",
                    EntityType = "Repository",
                    EntityName = repo.Name,
                    EntityId = repo.Id,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                var logMessage = logDto.CreateLogMessage();
                await _messageProducer.SendMessage(logMessage);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to log repository created: {ex.Message}");
            }
        }

        public async Task LogRepositoryUpdatedAsync(Repository repo, Guid userId)
        {
            try
            {
                var logDto = new LogDto
                {
                    EventType = "RepositoryUpdated",
                    EntityType = "Repository",
                    EntityName = repo.Name,
                    EntityId = repo.Id,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                var logMessage = logDto.CreateLogMessage();
                await _messageProducer.SendMessage(logMessage);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to log repository updated: {ex.Message}");
            }
        }

        public async Task LogRepositoryDeletedAsync(Repository repo, Guid userId)
        {
            try
            {
                var logDto = new LogDto
                {
                    EventType = "RepositoryDeleted",
                    EntityType = "Repository",
                    EntityName = repo.Name,
                    EntityId = repo.Id,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                var logMessage = logDto.CreateLogMessage();
                await _messageProducer.SendMessage(logMessage);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to log repository deleted: {ex.Message}");
            }
        }

        public async Task LogRepositoryViewedAsync(Repository repo, Guid? viewerId, string clientIp)
        {
            if (!_rateLimiter.CanSendLog(viewerId, repo.Id, "RepositoryViewed", clientIp))
                return;

            try
            {
                var logDto = new LogDto
                {
                    EventType = "RepositoryViewed",
                    EntityType = "Repository",
                    EntityName = repo.Name,
                    EntityId = repo.Id,
                    UserId = null, // Always null for anonymity
                    Timestamp = DateTime.UtcNow,
                    Details = $"RepositoryOwner:{repo.UserId}"
                };

                var logMessage = logDto.CreateLogMessage();
                await _messageProducer.SendMessage(logMessage);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to log repository viewed: {ex.Message}");
            }
        }

        public async Task LogPublicReposViewedAsync(User user, Guid? viewerId, string clientIp)
        {
            if (!_rateLimiter.CanSendLog(viewerId, user.Id, "PublicReposViewed", clientIp))
                return;

            try
            {
                var logDto = new LogDto
                {
                    EventType = "PublicReposViewed",
                    EntityType = "User",
                    EntityName = user.UserName,
                    EntityId = user.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"PublicReposOwner:{user.Id}"
                };

                var logMessage = logDto.CreateLogMessage();
                await _messageProducer.SendMessage(logMessage);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to log public repos viewed: {ex.Message}");
            }
        }

        public async Task LogAccountCreatedAsync(User user)
        {
            try
            {
                var logDto = new LogDto
                {
                    EventType = "AccountCreated",
                    EntityType = "Account",
                    EntityName = user.UserName,
                    EntityId = user.Id,
                    UserId = user.Id,
                    Timestamp = DateTime.UtcNow
                };

                var logMessage = logDto.CreateLogMessage();
                await _messageProducer.SendMessage(logMessage);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to log account created: {ex.Message}");
            }
        }
    }
}
