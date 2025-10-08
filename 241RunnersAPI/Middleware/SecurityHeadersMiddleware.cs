using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace _241RunnersAPI.Middleware
{
    /// <summary>
    /// Security headers middleware for enhanced security
    /// Adds security headers to all responses
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;
        private readonly SecurityHeadersOptions _options;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger, SecurityHeadersOptions options)
        {
            _next = next;
            _logger = logger;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            AddSecurityHeaders(context);

            await _next(context);
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var response = context.Response;

            // Content Security Policy
            if (_options.EnableContentSecurityPolicy)
            {
                response.Headers["Content-Security-Policy"] = 
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdn.skypack.dev; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                    "font-src 'self' https://fonts.gstatic.com; " +
                    "img-src 'self' data: https:; " +
                    "connect-src 'self' https://241runners-api-v2.azurewebsites.net wss:; " +
                    "frame-ancestors 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'";
            }

            // X-Content-Type-Options
            if (_options.EnableXContentTypeOptions)
            {
                response.Headers.Remove("X-Content-Type-Options");
                response.Headers["X-Content-Type-Options"] = "nosniff";
            }

            // X-Frame-Options
            if (_options.EnableXFrameOptions)
            {
                response.Headers.Remove("X-Frame-Options");
                response.Headers["X-Frame-Options"] = "DENY";
            }

            // X-XSS-Protection
            if (_options.EnableXXSSProtection)
            {
                response.Headers.Remove("X-XSS-Protection");
                response.Headers["X-XSS-Protection"] = "1; mode=block";
            }

            // Referrer Policy
            if (_options.EnableReferrerPolicy)
            {
                response.Headers.Remove("Referrer-Policy");
                response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            }

            // Permissions Policy
            if (_options.EnablePermissionsPolicy)
            {
                response.Headers["Permissions-Policy"] = 
                    "geolocation=(), " +
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
            }

            // Strict Transport Security (HTTPS only)
            if (_options.EnableHSTS && context.Request.IsHttps)
            {
                response.Headers["Strict-Transport-Security"] = 
                    $"max-age={_options.HSTSMaxAge}; includeSubDomains; preload";
            }

            // Cross-Origin Embedder Policy
            if (_options.EnableCOEP)
            {
                response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
            }

            // Cross-Origin Opener Policy
            if (_options.EnableCOOP)
            {
                response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
            }

            // Cross-Origin Resource Policy
            if (_options.EnableCORP)
            {
                response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
            }

            // Remove server header
            if (_options.RemoveServerHeader)
            {
                response.Headers.Remove("Server");
            }

            // Add custom security headers
            if (_options.CustomHeaders != null)
            {
                foreach (var header in _options.CustomHeaders)
                {
                    response.Headers[header.Key] = header.Value;
                }
            }
        }
    }

    public class SecurityHeadersOptions
    {
        public bool Enabled { get; set; } = true;
        public bool EnableContentSecurityPolicy { get; set; } = true;
        public bool EnableXContentTypeOptions { get; set; } = true;
        public bool EnableXFrameOptions { get; set; } = true;
        public bool EnableXXSSProtection { get; set; } = true;
        public bool EnableReferrerPolicy { get; set; } = true;
        public bool EnablePermissionsPolicy { get; set; } = true;
        public bool EnableHSTS { get; set; } = true;
        public bool EnableCOEP { get; set; } = false; // Can break some functionality
        public bool EnableCOOP { get; set; } = true;
        public bool EnableCORP { get; set; } = true;
        public bool RemoveServerHeader { get; set; } = true;
        public int HSTSMaxAge { get; set; } = 31536000; // 1 year
        public Dictionary<string, string>? CustomHeaders { get; set; }
    }
}
