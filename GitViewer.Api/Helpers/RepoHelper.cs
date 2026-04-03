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

        public static string GetClientIpAddress(this HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp))
            {
                return cfIp.ToString();
            }

            var ip = context.Connection.RemoteIpAddress;

            if (ip?.IsIPv4MappedToIPv6 == true)
            {
                ip = ip.MapToIPv4();
            }

            return ip?.ToString() ?? "unknown";
        }
    }
}
