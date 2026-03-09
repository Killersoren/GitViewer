using Microsoft.Extensions.Caching.Memory;

namespace GitViewer.Api.Services
{
    public class LogRateLimiter : ILogRateLimiter
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _rateLimitDuration = TimeSpan.FromMinutes(5);

        public LogRateLimiter(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool CanSendLog(Guid? userId, Guid entityId, string eventType, string clientIdentifier)
        {
            var cacheKey = $"{eventType}:{entityId}:{userId?.ToString() ?? clientIdentifier}";


            if (_cache.TryGetValue(cacheKey, out _))
            {
                return false;
            }

            _cache.Set(cacheKey, true, _rateLimitDuration);
            return true;
        }
    }
}
