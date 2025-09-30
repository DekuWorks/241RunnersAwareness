using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace _241RunnersAPI.Middleware
{
    /// <summary>
    /// HTTPS enforcement middleware for secure communication
    /// Redirects HTTP requests to HTTPS and enforces secure headers
    /// </summary>
    public class HttpsEnforcementMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpsEnforcementMiddleware> _logger;
        private readonly HttpsEnforcementOptions _options;

        public HttpsEnforcementMiddleware(RequestDelegate next, ILogger<HttpsEnforcementMiddleware> logger, HttpsEnforcementOptions options)
        {
            _next = next;
            _logger = logger;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip HTTPS enforcement for certain conditions
            if (ShouldSkipHttpsEnforcement(context))
            {
                await _next(context);
                return;
            }

            try
            {
                // Check if request is already HTTPS
                if (context.Request.IsHttps)
                {
                    await HandleHttpsRequest(context);
                }
                else
                {
                    await HandleHttpRequest(context);
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTPS enforcement failed for {Path}", context.Request.Path);
                await WriteHttpsErrorResponse(context);
            }
        }

        private bool ShouldSkipHttpsEnforcement(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method;

            // Skip for health checks and certain endpoints
            var skipEndpoints = new[]
            {
                "/health",
                "/api/v1/health",
                "/api/v1/database/health",
                "/swagger",
                "/hubs"
            };

            if (skipEndpoints.Any(endpoint => path.StartsWith(endpoint)))
            {
                return true;
            }

            // Skip for localhost in development
            if (_options.AllowLocalhostInDevelopment && 
                context.Request.Host.Host == "localhost" && 
                context.Request.Host.Port == 5000)
            {
                return true;
            }

            return false;
        }

        private async Task HandleHttpsRequest(HttpContext context)
        {
            // Add security headers for HTTPS requests
            AddSecurityHeaders(context);

            // Check for secure cookie requirements
            if (_options.RequireSecureCookies)
            {
                EnsureSecureCookies(context);
            }

            // Check for HSTS requirements
            if (_options.RequireHsts)
            {
                AddHstsHeader(context);
            }
        }

        private async Task HandleHttpRequest(HttpContext context)
        {
            if (_options.RedirectToHttps)
            {
                // Redirect to HTTPS
                var httpsUrl = GetHttpsUrl(context);
                _logger.LogInformation("Redirecting HTTP request to HTTPS: {Url}", httpsUrl);
                
                context.Response.Redirect(httpsUrl, permanent: _options.PermanentRedirect);
                return;
            }
            else
            {
                // Return error for HTTP requests
                await WriteHttpsErrorResponse(context);
            }
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var response = context.Response;

            // Strict Transport Security
            if (_options.RequireHsts)
            {
                var hstsValue = $"max-age={_options.HstsMaxAge}";
                if (_options.HstsIncludeSubDomains)
                {
                    hstsValue += "; includeSubDomains";
                }
                if (_options.HstsPreload)
                {
                    hstsValue += "; preload";
                }
                response.Headers.Add("Strict-Transport-Security", hstsValue);
            }

            // Content Security Policy
            if (_options.RequireCsp)
            {
                var cspValue = "default-src 'self'; " +
                              "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                              "style-src 'self' 'unsafe-inline'; " +
                              "img-src 'self' data: https:; " +
                              "connect-src 'self' https: wss:; " +
                              "frame-ancestors 'none'; " +
                              "base-uri 'self'; " +
                              "form-action 'self'";
                response.Headers.Add("Content-Security-Policy", cspValue);
            }

            // X-Frame-Options
            if (_options.RequireFrameOptions)
            {
                response.Headers.Add("X-Frame-Options", "DENY");
            }

            // X-Content-Type-Options
            if (_options.RequireContentTypeOptions)
            {
                response.Headers.Add("X-Content-Type-Options", "nosniff");
            }

            // Referrer Policy
            if (_options.RequireReferrerPolicy)
            {
                response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            }

            // Permissions Policy
            if (_options.RequirePermissionsPolicy)
            {
                var permissionsValue = "geolocation=(), " +
                                    "microphone=(), " +
                                    "camera=(), " +
                                    "payment=(), " +
                                    "usb=(), " +
                                    "magnetometer=(), " +
                                    "gyroscope=(), " +
                                    "speaker=(), " +
                                    "vibrate=(), " +
                                    "fullscreen=(self), " +
                                    "sync-xhr=()";
                response.Headers.Add("Permissions-Policy", permissionsValue);
            }
        }

        private void EnsureSecureCookies(HttpContext context)
        {
            // This would be handled by cookie options in the application
            // The middleware just ensures the requirement is met
            _logger.LogDebug("Ensuring secure cookies for HTTPS request");
        }

        private void AddHstsHeader(HttpContext context)
        {
            var hstsValue = $"max-age={_options.HstsMaxAge}";
            if (_options.HstsIncludeSubDomains)
            {
                hstsValue += "; includeSubDomains";
            }
            if (_options.HstsPreload)
            {
                hstsValue += "; preload";
            }
            context.Response.Headers.Add("Strict-Transport-Security", hstsValue);
        }

        private string GetHttpsUrl(HttpContext context)
        {
            var request = context.Request;
            var host = request.Host.Host;
            var port = _options.HttpsPort;
            
            // Use custom HTTPS port if specified
            if (port.HasValue && port != 443)
            {
                return $"https://{host}:{port}{request.PathBase}{request.Path}{request.QueryString}";
            }
            
            // Use standard HTTPS port
            return $"https://{host}{request.PathBase}{request.Path}{request.QueryString}";
        }

        private async Task WriteHttpsErrorResponse(HttpContext context)
        {
            context.Response.StatusCode = 426; // Upgrade Required
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                error = "HTTPS required",
                message = "This application requires HTTPS. Please use HTTPS to access this resource.",
                code = "HTTPS_REQUIRED",
                timestamp = DateTimeOffset.UtcNow
            };

            var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }

    public class HttpsEnforcementOptions
    {
        public bool Enabled { get; set; } = true;
        public bool RedirectToHttps { get; set; } = true;
        public bool PermanentRedirect { get; set; } = true;
        public bool RequireSecureCookies { get; set; } = true;
        public bool RequireHsts { get; set; } = true;
        public bool RequireCsp { get; set; } = true;
        public bool RequireFrameOptions { get; set; } = true;
        public bool RequireContentTypeOptions { get; set; } = true;
        public bool RequireReferrerPolicy { get; set; } = true;
        public bool RequirePermissionsPolicy { get; set; } = true;
        public bool AllowLocalhostInDevelopment { get; set; } = true;
        public int? HttpsPort { get; set; }
        public int HstsMaxAge { get; set; } = 31536000; // 1 year
        public bool HstsIncludeSubDomains { get; set; } = true;
        public bool HstsPreload { get; set; } = false;
    }
}
