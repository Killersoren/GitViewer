using GitViewer.Api.Dto;
using GitViewer.Api.RabbitMQ;

namespace GitViewer.Api.Helpers
{
    public static class RepoHelper
    {
        public static LogMessage<LogDto> CreateLogMessage(this LogDto logDto)
        {
            var logMessage = new LogMessage<LogDto>
            {
                EventType = logDto.EventType,
                EntityType = logDto.EntityType,
                EntityId = logDto.EntityId,
                EntityName = logDto.EntityName,
                UserId = logDto.UserId ?? Guid.Empty,
                IsAnonymous = logDto.IsAnonymous,
                Timestamp = logDto.Timestamp,
                Details = logDto.Details,
                ShareLink = logDto.ShareLink
            };

            return logMessage;
        }

    }
}
