using Microsoft.Extensions.Caching.Memory;

namespace _241RunnersAwarenessAPI.Services
{
    public class RateLimitingService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _window = TimeSpan.FromMinutes(1);
        private readonly int _maxRequests = 60; // 60 requests per minute

        public RateLimitingService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool IsAllowed(string clientId)
        {
            var key = $"rate_limit_{clientId}";
            var current = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _window;
                return 0;
            });

            if (current >= _maxRequests)
            {
                return false;
            }

            _cache.Set(key, current + 1, _window);
            return true;
        }

        public int GetRemainingRequests(string clientId)
        {
            var key = $"rate_limit_{clientId}";
            var current = _cache.Get<int?>(key) ?? 0;
            return Math.Max(0, _maxRequests - current);
        }
    }
}
