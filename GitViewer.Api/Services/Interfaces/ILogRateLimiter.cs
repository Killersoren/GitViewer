namespace GitViewer.Api.Services.Interfaces
{
    public interface ILogRateLimiter
    {
        bool CanSendLog(Guid? userId, Guid entityId, string eventType, string clientIdentifier, Guid? shareLinkId);
    }
}
