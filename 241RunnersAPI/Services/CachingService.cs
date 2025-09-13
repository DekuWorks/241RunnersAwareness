using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for caching frequently accessed data
    /// </summary>
    public class CachingService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingService> _logger;
        private readonly TimeSpan _defaultCacheExpiration = TimeSpan.FromMinutes(15);

        public CachingService(IMemoryCache cache, ILogger<CachingService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Get a cached value or execute the function and cache the result
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T cachedValue))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            var item = await getItem();
            
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultCacheExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(key, item, cacheOptions);
            _logger.LogDebug("Cached item for key: {Key} with expiration: {Expiration}", key, cacheOptions.AbsoluteExpirationRelativeToNow);
            
            return item;
        }

        /// <summary>
        /// Get a cached value or execute the function and cache the result (synchronous)
        /// </summary>
        public T GetOrSet<T>(string key, Func<T> getItem, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T cachedValue))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            var item = getItem();
            
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultCacheExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(key, item, cacheOptions);
            _logger.LogDebug("Cached item for key: {Key} with expiration: {Expiration}", key, cacheOptions.AbsoluteExpirationRelativeToNow);
            
            return item;
        }

        /// <summary>
        /// Remove a cached item
        /// </summary>
        public void Remove(string key)
        {
            _cache.Remove(key);
            _logger.LogDebug("Removed cache item for key: {Key}", key);
        }

        /// <summary>
        /// Clear all cache
        /// </summary>
        public void Clear()
        {
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
                _logger.LogInformation("Cleared all cache");
            }
        }

        /// <summary>
        /// Get cache statistics
        /// </summary>
        public object GetCacheStats()
        {
            if (_cache is MemoryCache memoryCache)
            {
                return new
                {
                    size = memoryCache.Count,
                    timestamp = DateTime.UtcNow
                };
            }
            
            return new { size = 0, timestamp = DateTime.UtcNow };
        }

        /// <summary>
        /// Cache user data
        /// </summary>
        public async Task<T> GetOrSetUserDataAsync<T>(int userId, string dataType, Func<Task<T>> getItem, TimeSpan? expiration = null)
        {
            var key = $"user_{userId}_{dataType}";
            return await GetOrSetAsync(key, getItem, expiration);
        }

        /// <summary>
        /// Cache admin data
        /// </summary>
        public async Task<T> GetOrSetAdminDataAsync<T>(string dataType, Func<Task<T>> getItem, TimeSpan? expiration = null)
        {
            var key = $"admin_{dataType}";
            return await GetOrSetAsync(key, getItem, expiration);
        }

        /// <summary>
        /// Cache system data
        /// </summary>
        public async Task<T> GetOrSetSystemDataAsync<T>(string dataType, Func<Task<T>> getItem, TimeSpan? expiration = null)
        {
            var key = $"system_{dataType}";
            return await GetOrSetAsync(key, getItem, expiration);
        }
    }
}
