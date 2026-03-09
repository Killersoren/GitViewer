namespace GitViewer.Api.Services
{
    public interface ILogRateLimiter
    {
        bool CanSendLog(Guid? userId, Guid entityId, string eventType, string clientIdentifier);
    }
}
