using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace _241RunnersAPI.Middleware
{
    /// <summary>
    /// Request validation middleware for security and data integrity
    /// Validates request size, content type, and basic security checks
    /// </summary>
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestValidationMiddleware> _logger;
        private readonly RequestValidationOptions _options;

        public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger, RequestValidationOptions options)
        {
            _next = next;
            _logger = logger;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Validate request size
                if (!await ValidateRequestSize(context))
                {
                    return;
                }

                // Validate content type for POST/PUT requests
                if (!ValidateContentType(context))
                {
                    return;
                }

                // Validate request headers
                if (!ValidateHeaders(context))
                {
                    return;
                }

                // Validate request body for potential security issues
                if (context.Request.ContentLength > 0)
                {
                    if (!await ValidateRequestBody(context))
                    {
                        return;
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request validation failed for {Path}", context.Request.Path);
                await WriteErrorResponse(context, "Request validation failed", 400);
            }
        }

        private async Task<bool> ValidateRequestSize(HttpContext context)
        {
            var contentLength = context.Request.ContentLength ?? 0;
            var maxSize = GetMaxRequestSize(context.Request.Path);

            if (contentLength > maxSize)
            {
                _logger.LogWarning("Request too large: {ContentLength} bytes for {Path}", contentLength, context.Request.Path);
                await WriteErrorResponse(context, $"Request too large. Maximum size: {maxSize} bytes", 413);
                return false;
            }

            return true;
        }

        private long GetMaxRequestSize(string path)
        {
            return path.ToLower() switch
            {
                var p when p.Contains("/image-upload") => 10 * 1024 * 1024, // 10MB for image uploads
                var p when p.Contains("/runners") && context.Request.Method == "POST" => 5 * 1024 * 1024, // 5MB for case reports
                var p when p.Contains("/auth") => 1 * 1024 * 1024, // 1MB for auth requests
                _ => 2 * 1024 * 1024 // 2MB default
            };
        }

        private bool ValidateContentType(HttpContext context)
        {
            if (context.Request.Method == "GET" || context.Request.Method == "DELETE")
            {
                return true;
            }

            var contentType = context.Request.ContentType?.ToLower();
            
            // Allow JSON and form data
            if (contentType == null || 
                contentType.Contains("application/json") || 
                contentType.Contains("multipart/form-data") ||
                contentType.Contains("application/x-www-form-urlencoded"))
            {
                return true;
            }

            _logger.LogWarning("Invalid content type: {ContentType} for {Path}", contentType, context.Request.Path);
            WriteErrorResponse(context, "Invalid content type. Expected JSON or form data.", 415);
            return false;
        }

        private bool ValidateHeaders(HttpContext context)
        {
            var request = context.Request;
            
            // Check for suspicious headers
            var suspiciousHeaders = new[]
            {
                "X-Forwarded-For",
                "X-Real-IP",
                "X-Originating-IP",
                "X-Remote-IP",
                "X-Remote-Addr"
            };

            foreach (var header in suspiciousHeaders)
            {
                if (request.Headers.ContainsKey(header))
                {
                    _logger.LogWarning("Suspicious header detected: {Header} = {Value}", header, request.Headers[header]);
                }
            }

            // Validate User-Agent
            var userAgent = request.Headers.UserAgent.ToString();
            if (string.IsNullOrEmpty(userAgent) || userAgent.Length > 500)
            {
                _logger.LogWarning("Invalid User-Agent: {UserAgent}", userAgent);
                WriteErrorResponse(context, "Invalid User-Agent header", 400);
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateRequestBody(HttpContext context)
        {
            try
            {
                // Read the request body
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body, Encoding.UTF8).ReadToEndAsync();
                context.Request.Body.Position = 0;

                // Check for potential security issues
                if (ContainsSuspiciousContent(body))
                {
                    _logger.LogWarning("Suspicious content detected in request body for {Path}", context.Request.Path);
                    await WriteErrorResponse(context, "Request contains suspicious content", 400);
                    return false;
                }

                // Validate JSON structure if content type is JSON
                if (context.Request.ContentType?.Contains("application/json") == true)
                {
                    if (!IsValidJson(body))
                    {
                        _logger.LogWarning("Invalid JSON in request body for {Path}", context.Request.Path);
                        await WriteErrorResponse(context, "Invalid JSON format", 400);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating request body for {Path}", context.Request.Path);
                await WriteErrorResponse(context, "Error processing request body", 400);
                return false;
            }
        }

        private bool ContainsSuspiciousContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            var suspiciousPatterns = new[]
            {
                "<script",
                "javascript:",
                "vbscript:",
                "onload=",
                "onerror=",
                "eval(",
                "document.cookie",
                "document.write",
                "window.location",
                "alert(",
                "confirm(",
                "prompt(",
                "exec(",
                "system(",
                "cmd.exe",
                "powershell",
                "bash",
                "sh",
                "SELECT * FROM",
                "INSERT INTO",
                "UPDATE SET",
                "DELETE FROM",
                "DROP TABLE",
                "UNION SELECT",
                "OR 1=1",
                "AND 1=1"
            };

            var lowerContent = content.ToLower();
            return suspiciousPatterns.Any(pattern => lowerContent.Contains(pattern));
        }

        private bool IsValidJson(string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private async Task WriteErrorResponse(HttpContext context, string message, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                error = message,
                statusCode = statusCode,
                timestamp = DateTimeOffset.UtcNow
            };

            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }

    public class RequestValidationOptions
    {
        public bool Enabled { get; set; } = true;
        public long MaxRequestSize { get; set; } = 10 * 1024 * 1024; // 10MB default
        public bool ValidateContentType { get; set; } = true;
        public bool ValidateHeaders { get; set; } = true;
        public bool ValidateRequestBody { get; set; } = true;
    }
}
