using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace _241RunnersAPI.Middleware
{
    /// <summary>
    /// CSRF protection middleware for preventing Cross-Site Request Forgery attacks
    /// Implements double-submit cookie pattern for stateless CSRF protection
    /// </summary>
    public class CsrfProtectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CsrfProtectionMiddleware> _logger;
        private readonly IMemoryCache _cache;
        private readonly CsrfProtectionOptions _options;

        public CsrfProtectionMiddleware(RequestDelegate next, ILogger<CsrfProtectionMiddleware> logger, IMemoryCache cache, CsrfProtectionOptions options)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip CSRF protection for safe methods and certain endpoints
            if (ShouldSkipCsrfProtection(context))
            {
                await _next(context);
                return;
            }

            try
            {
                // Handle CSRF token generation for GET requests
                if (context.Request.Method == "GET")
                {
                    await HandleGetRequest(context);
                }
                // Handle CSRF token validation for state-changing requests
                else if (IsStateChangingRequest(context.Request.Method))
                {
                    if (!await ValidateCsrfToken(context))
                    {
                        await WriteCsrfErrorResponse(context);
                        return;
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CSRF protection failed for {Path}", context.Request.Path);
                await WriteCsrfErrorResponse(context);
            }
        }

        private bool ShouldSkipCsrfProtection(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method;

            // Skip for safe methods
            if (method == "GET" || method == "HEAD" || method == "OPTIONS")
            {
                return true;
            }

            // Skip for certain endpoints that don't need CSRF protection
            var skipEndpoints = new[]
            {
                "/api/v1/auth/login",
                "/api/v1/auth/register",
                "/api/v1/auth/oauth",
                "/api/v1/health",
                "/api/v1/database/health",
                "/swagger",
                "/hubs"
            };

            return skipEndpoints.Any(endpoint => path.StartsWith(endpoint));
        }

        private bool IsStateChangingRequest(string method)
        {
            return method == "POST" || method == "PUT" || method == "PATCH" || method == "DELETE";
        }

        private async Task HandleGetRequest(HttpContext context)
        {
            // Generate CSRF token for authenticated users
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var token = await GenerateCsrfToken(context);
                context.Response.Headers.Add("X-CSRF-Token", token);
                
                // Set CSRF cookie for double-submit pattern
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                };
                
                context.Response.Cookies.Append("X-CSRF-Token", token, cookieOptions);
            }
        }

        private async Task<bool> ValidateCsrfToken(HttpContext context)
        {
            // Get token from header
            var headerToken = context.Request.Headers["X-CSRF-Token"].FirstOrDefault();
            
            // Get token from cookie
            var cookieToken = context.Request.Cookies["X-CSRF-Token"];
            
            // Both tokens must be present and match
            if (string.IsNullOrEmpty(headerToken) || string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogWarning("Missing CSRF token for {Path} from {IP}", 
                    context.Request.Path, context.Connection.RemoteIpAddress);
                return false;
            }

            if (headerToken != cookieToken)
            {
                _logger.LogWarning("CSRF token mismatch for {Path} from {IP}", 
                    context.Request.Path, context.Connection.RemoteIpAddress);
                return false;
            }

            // Validate token format and expiration
            if (!await ValidateTokenFormat(headerToken, context))
            {
                _logger.LogWarning("Invalid CSRF token format for {Path} from {IP}", 
                    context.Request.Path, context.Connection.RemoteIpAddress);
                return false;
            }

            return true;
        }

        private async Task<string> GenerateCsrfToken(HttpContext context)
        {
            var userId = context.User?.FindFirst("sub")?.Value ?? "anonymous";
            var sessionId = context.Session?.Id ?? Guid.NewGuid().ToString();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            // Create token payload
            var payload = $"{userId}:{sessionId}:{timestamp}";
            
            // Generate HMAC signature
            var signature = GenerateHmacSignature(payload);
            
            // Combine payload and signature
            var token = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))}.{signature}";
            
            // Store token in cache for validation
            var cacheKey = $"csrf_token:{userId}:{sessionId}";
            await _cache.SetAsync(cacheKey, token, TimeSpan.FromHours(1));
            
            return token;
        }

        private async Task<bool> ValidateTokenFormat(string token, HttpContext context)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 2)
                {
                    return false;
                }

                var payload = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
                var signature = parts[1];
                
                var payloadParts = payload.Split(':');
                if (payloadParts.Length != 3)
                {
                    return false;
                }

                var userId = payloadParts[0];
                var sessionId = payloadParts[1];
                var timestamp = long.Parse(payloadParts[2]);
                
                // Check token expiration (1 hour)
                var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                if (DateTimeOffset.UtcNow - tokenTime > TimeSpan.FromHours(1))
                {
                    return false;
                }

                // Verify HMAC signature
                var expectedSignature = GenerateHmacSignature(payload);
                if (signature != expectedSignature)
                {
                    return false;
                }

                // Check if token exists in cache
                var cacheKey = $"csrf_token:{userId}:{sessionId}";
                var cachedToken = await _cache.GetAsync<string>(cacheKey);
                if (cachedToken != token)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating CSRF token format");
                return false;
            }
        }

        private string GenerateHmacSignature(string payload)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SecretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hash);
        }

        private async Task WriteCsrfErrorResponse(HttpContext context)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                error = "CSRF token validation failed",
                message = "Invalid or missing CSRF token. Please refresh the page and try again.",
                code = "CSRF_TOKEN_INVALID",
                timestamp = DateTimeOffset.UtcNow
            };

            var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }

    public class CsrfProtectionOptions
    {
        public bool Enabled { get; set; } = true;
        public string SecretKey { get; set; } = "your-csrf-secret-key-that-should-be-at-least-32-characters-long";
        public int TokenExpirationHours { get; set; } = 1;
        public bool RequireHttps { get; set; } = true;
        public string[] ExcludedPaths { get; set; } = Array.Empty<string>();
    }
}
