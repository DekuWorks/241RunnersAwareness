using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace _241RunnersAPI.Middleware
{
    /// <summary>
    /// Custom rate limiting middleware for API endpoints
    /// Provides granular rate limiting based on endpoint and user
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IMemoryCache _cache;
        private readonly RateLimitOptions _options;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IMemoryCache cache, RateLimitOptions options)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Request.Path.Value?.ToLower() ?? "";
            var clientId = GetClientIdentifier(context);
            var rateLimitKey = $"{clientId}:{endpoint}";

            // Get rate limit configuration for this endpoint
            var limitConfig = GetRateLimitConfig(endpoint);
            
            if (limitConfig != null)
            {
                var isAllowed = await CheckRateLimit(rateLimitKey, limitConfig);
                
                if (!isAllowed)
                {
                    _logger.LogWarning("Rate limit exceeded for {ClientId} on {Endpoint}", clientId, endpoint);
                    
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.Headers.Add("Retry-After", limitConfig.WindowSeconds.ToString());
                    context.Response.Headers.Add("X-RateLimit-Limit", limitConfig.RequestsPerWindow.ToString());
                    context.Response.Headers.Add("X-RateLimit-Remaining", "0");
                    context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddSeconds(limitConfig.WindowSeconds).ToUnixTimeSeconds().ToString());
                    
                    await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                    return;
                }
                
                // Add rate limit headers
                var remaining = await GetRemainingRequests(rateLimitKey, limitConfig);
                context.Response.Headers.Add("X-RateLimit-Limit", limitConfig.RequestsPerWindow.ToString());
                context.Response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());
                context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddSeconds(limitConfig.WindowSeconds).ToUnixTimeSeconds().ToString());
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get user ID from JWT token first
            var userId = context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }

            // Fallback to IP address
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"ip:{ipAddress}";
        }

        private RateLimitConfig? GetRateLimitConfig(string endpoint)
        {
            // Define rate limits for different endpoints
            return endpoint switch
            {
                var e when e.Contains("/auth/login") => new RateLimitConfig { RequestsPerWindow = 5, WindowSeconds = 300 }, // 5 requests per 5 minutes
                var e when e.Contains("/auth/register") => new RateLimitConfig { RequestsPerWindow = 3, WindowSeconds = 300 }, // 3 requests per 5 minutes
                var e when e.Contains("/auth/oauth") => new RateLimitConfig { RequestsPerWindow = 10, WindowSeconds = 300 }, // 10 requests per 5 minutes
                var e when e.Contains("/runners") && context.Request.Method == "POST" => new RateLimitConfig { RequestsPerWindow = 10, WindowSeconds = 300 }, // 10 reports per 5 minutes
                var e when e.Contains("/runners") && context.Request.Method == "GET" => new RateLimitConfig { RequestsPerWindow = 100, WindowSeconds = 60 }, // 100 requests per minute
                var e when e.Contains("/cases") => new RateLimitConfig { RequestsPerWindow = 50, WindowSeconds = 60 }, // 50 requests per minute
                var e when e.Contains("/admin") => new RateLimitConfig { RequestsPerWindow = 200, WindowSeconds = 60 }, // 200 requests per minute for admin
                _ => new RateLimitConfig { RequestsPerWindow = 60, WindowSeconds = 60 } // Default: 60 requests per minute
            };
        }

        private async Task<bool> CheckRateLimit(string key, RateLimitConfig config)
        {
            var cacheKey = $"rate_limit:{key}";
            var now = DateTimeOffset.UtcNow;
            var windowStart = now.AddSeconds(-config.WindowSeconds);

            // Get existing requests
            var requests = _cache.Get<List<DateTimeOffset>>(cacheKey) ?? new List<DateTimeOffset>();
            
            // Remove old requests outside the window
            requests = requests.Where(r => r > windowStart).ToList();
            
            // Check if we're within the limit
            if (requests.Count >= config.RequestsPerWindow)
            {
                return false;
            }

            // Add current request
            requests.Add(now);
            
            // Cache the updated list
            _cache.Set(cacheKey, requests, TimeSpan.FromSeconds(config.WindowSeconds + 10));
            
            return true;
        }

        private async Task<int> GetRemainingRequests(string key, RateLimitConfig config)
        {
            var cacheKey = $"rate_limit:{key}";
            var requests = _cache.Get<List<DateTimeOffset>>(cacheKey) ?? new List<DateTimeOffset>();
            var now = DateTimeOffset.UtcNow;
            var windowStart = now.AddSeconds(-config.WindowSeconds);
            
            // Count requests within the window
            var recentRequests = requests.Count(r => r > windowStart);
            
            return Math.Max(0, config.RequestsPerWindow - recentRequests);
        }
    }

    public class RateLimitOptions
    {
        public bool Enabled { get; set; } = true;
        public int DefaultRequestsPerWindow { get; set; } = 60;
        public int DefaultWindowSeconds { get; set; } = 60;
    }

    public class RateLimitConfig
    {
        public int RequestsPerWindow { get; set; }
        public int WindowSeconds { get; set; }
    }
}
