using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace _241RunnersAPI.Middleware
{
    /// <summary>
    /// Performance monitoring middleware
    /// Tracks request performance, response times, and system metrics
    /// </summary>
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly PerformanceMonitoringOptions _options;

        public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger, TelemetryClient telemetryClient, PerformanceMonitoringOptions options)
        {
            _next = next;
            _logger = logger;
            _telemetryClient = telemetryClient;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();
            
            // Add request ID to context
            context.Items["RequestId"] = requestId;
            context.Response.Headers.Add("X-Request-ID", requestId);

            try
            {
                // Track request start
                TrackRequestStart(context, requestId);

                await _next(context);

                // Track successful request
                stopwatch.Stop();
                TrackRequestSuccess(context, requestId, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                // Track failed request
                stopwatch.Stop();
                TrackRequestFailure(context, requestId, stopwatch.ElapsedMilliseconds, ex);
                throw;
            }
            finally
            {
                // Track performance metrics
                TrackPerformanceMetrics(context, stopwatch.ElapsedMilliseconds);
            }
        }

        private void TrackRequestStart(HttpContext context, string requestId)
        {
            var request = context.Request;
            var telemetry = new RequestTelemetry
            {
                Id = requestId,
                Name = $"{request.Method} {request.Path}",
                Url = request.GetDisplayUrl(),
                Timestamp = DateTimeOffset.UtcNow,
                Properties = {
                    ["User-Agent"] = request.Headers.UserAgent.ToString(),
                    ["Content-Type"] = request.ContentType ?? "",
                    ["Content-Length"] = request.ContentLength?.ToString() ?? "0",
                    ["Remote-IP"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                }
            };

            _telemetryClient.TrackRequest(telemetry);
            _logger.LogInformation("Request started: {Method} {Path} {RequestId}", request.Method, request.Path, requestId);
        }

        private void TrackRequestSuccess(HttpContext context, string requestId, long durationMs)
        {
            var response = context.Response;
            var telemetry = new RequestTelemetry
            {
                Id = requestId,
                Name = $"{context.Request.Method} {context.Request.Path}",
                Url = context.Request.GetDisplayUrl(),
                Timestamp = DateTimeOffset.UtcNow,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                ResponseCode = response.StatusCode.ToString(),
                Success = response.StatusCode < 400
            };

            _telemetryClient.TrackRequest(telemetry);

            // Track custom metrics
            _telemetryClient.TrackMetric("Request.Duration", durationMs, new Dictionary<string, string>
            {
                ["Endpoint"] = context.Request.Path,
                ["Method"] = context.Request.Method,
                ["Status"] = response.StatusCode.ToString()
            });

            _logger.LogInformation("Request completed: {Method} {Path} {StatusCode} {Duration}ms {RequestId}", 
                context.Request.Method, context.Request.Path, response.StatusCode, durationMs, requestId);
        }

        private void TrackRequestFailure(HttpContext context, string requestId, long durationMs, Exception exception)
        {
            var response = context.Response;
            var telemetry = new RequestTelemetry
            {
                Id = requestId,
                Name = $"{context.Request.Method} {context.Request.Path}",
                Url = context.Request.GetDisplayUrl(),
                Timestamp = DateTimeOffset.UtcNow,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                ResponseCode = response.StatusCode.ToString(),
                Success = false
            };

            _telemetryClient.TrackRequest(telemetry);
            _telemetryClient.TrackException(exception);

            // Track error metrics
            _telemetryClient.TrackMetric("Request.Error", 1, new Dictionary<string, string>
            {
                ["Endpoint"] = context.Request.Path,
                ["Method"] = context.Request.Method,
                ["Exception"] = exception.GetType().Name
            });

            _logger.LogError(exception, "Request failed: {Method} {Path} {Duration}ms {RequestId}", 
                context.Request.Method, context.Request.Path, durationMs, requestId);
        }

        private void TrackPerformanceMetrics(HttpContext context, long durationMs)
        {
            var endpoint = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;

            // Track endpoint-specific metrics
            _telemetryClient.TrackMetric($"Endpoint.{method}.{endpoint}.Duration", durationMs);
            _telemetryClient.TrackMetric($"Endpoint.{method}.{endpoint}.Count", 1);

            // Track slow requests
            if (durationMs > _options.SlowRequestThresholdMs)
            {
                _telemetryClient.TrackMetric("Request.Slow", durationMs, new Dictionary<string, string>
                {
                    ["Endpoint"] = endpoint,
                    ["Method"] = method,
                    ["Threshold"] = _options.SlowRequestThresholdMs.ToString()
                });

                _logger.LogWarning("Slow request detected: {Method} {Path} {Duration}ms (threshold: {Threshold}ms)", 
                    method, endpoint, durationMs, _options.SlowRequestThresholdMs);
            }

            // Track high-traffic endpoints
            if (IsHighTrafficEndpoint(endpoint))
            {
                _telemetryClient.TrackMetric("Request.HighTraffic", durationMs, new Dictionary<string, string>
                {
                    ["Endpoint"] = endpoint,
                    ["Method"] = method
                });
            }

            // Track memory usage
            var memoryUsage = GC.GetTotalMemory(false);
            _telemetryClient.TrackMetric("System.Memory", memoryUsage);

            // Track GC collections
            var gen0Collections = GC.CollectionCount(0);
            var gen1Collections = GC.CollectionCount(1);
            var gen2Collections = GC.CollectionCount(2);

            _telemetryClient.TrackMetric("System.GC.Gen0", gen0Collections);
            _telemetryClient.TrackMetric("System.GC.Gen1", gen1Collections);
            _telemetryClient.TrackMetric("System.GC.Gen2", gen2Collections);
        }

        private bool IsHighTrafficEndpoint(string endpoint)
        {
            var highTrafficEndpoints = new[]
            {
                "/api/v1/cases",
                "/api/v1/runners",
                "/api/v1/auth/login",
                "/api/v1/auth/register"
            };

            return highTrafficEndpoints.Any(ht => endpoint.Contains(ht));
        }
    }

    public class PerformanceMonitoringOptions
    {
        public bool Enabled { get; set; } = true;
        public int SlowRequestThresholdMs { get; set; } = 1000; // 1 second
        public bool TrackMemoryUsage { get; set; } = true;
        public bool TrackGCCollections { get; set; } = true;
        public bool TrackSlowRequests { get; set; } = true;
        public bool TrackHighTrafficEndpoints { get; set; } = true;
    }
}
