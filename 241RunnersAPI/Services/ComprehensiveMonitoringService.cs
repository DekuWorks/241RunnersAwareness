using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;
using System.Text.Json;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Comprehensive monitoring service for application health, performance, and errors
    /// Provides real-time monitoring, alerting, and analytics
    /// </summary>
    public class ComprehensiveMonitoringService
    {
        private readonly ILogger<ComprehensiveMonitoringService> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MonitoringOptions _options;

        public ComprehensiveMonitoringService(
            ILogger<ComprehensiveMonitoringService> logger,
            TelemetryClient telemetryClient,
            IHttpContextAccessor httpContextAccessor,
            MonitoringOptions options)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _httpContextAccessor = httpContextAccessor;
            _options = options;
        }

        /// <summary>
        /// Track application performance metrics
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="duration">Operation duration</param>
        /// <param name="success">Whether operation was successful</param>
        /// <param name="additionalMetrics">Additional metrics</param>
        public async Task TrackPerformanceAsync(string operationName, TimeSpan duration, bool success, Dictionary<string, object>? additionalMetrics = null)
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                var userId = context?.User?.FindFirst("sub")?.Value ?? "anonymous";
                var ipAddress = context?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

                // Track custom metrics
                _telemetryClient.TrackMetric($"Performance.{operationName}.Duration", duration.TotalMilliseconds, new Dictionary<string, string>
                {
                    ["Operation"] = operationName,
                    ["Success"] = success.ToString(),
                    ["UserId"] = userId,
                    ["IpAddress"] = ipAddress
                });

                // Track success/failure rates
                _telemetryClient.TrackMetric($"Performance.{operationName}.{(success ? "Success" : "Failure")}", 1);

                // Track additional metrics
                if (additionalMetrics != null)
                {
                    foreach (var metric in additionalMetrics)
                    {
                        _telemetryClient.TrackMetric($"Performance.{operationName}.{metric.Key}", Convert.ToDouble(metric.Value));
                    }
                }

                // Log performance event
                _logger.LogInformation("Performance tracked: {Operation} took {Duration}ms, Success: {Success}", 
                    operationName, duration.TotalMilliseconds, success);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track performance for operation {Operation}", operationName);
            }
        }

        /// <summary>
        /// Track application errors and exceptions
        /// </summary>
        /// <param name="exception">Exception that occurred</param>
        /// <param name="context">Additional context information</param>
        /// <param name="severity">Error severity level</param>
        public async Task TrackErrorAsync(Exception exception, Dictionary<string, object>? context = null, ErrorSeverity severity = ErrorSeverity.Error)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var userId = httpContext?.User?.FindFirst("sub")?.Value ?? "anonymous";
                var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? "unknown";

                // Create error telemetry
                var errorTelemetry = new ExceptionTelemetry(exception)
                {
                    SeverityLevel = GetSeverityLevel(severity),
                    Properties = {
                        ["UserId"] = userId,
                        ["IpAddress"] = ipAddress,
                        ["UserAgent"] = userAgent,
                        ["Severity"] = severity.ToString(),
                        ["Timestamp"] = DateTimeOffset.UtcNow.ToString("O")
                    }
                };

                // Add context information
                if (context != null)
                {
                    foreach (var item in context)
                    {
                        errorTelemetry.Properties[item.Key] = item.Value?.ToString() ?? "null";
                    }
                }

                // Track the exception
                _telemetryClient.TrackException(errorTelemetry);

                // Track error metrics
                _telemetryClient.TrackMetric($"Errors.{severity}", 1);
                _telemetryClient.TrackMetric("Errors.Total", 1);

                // Log the error
                var logLevel = GetLogLevel(severity);
                _logger.Log(logLevel, exception, "Error tracked: {Message} for user {UserId}", 
                    exception.Message, userId);

                // Send alert for critical errors
                if (severity == ErrorSeverity.Critical)
                {
                    await SendCriticalErrorAlertAsync(exception, context);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track error: {Message}", exception.Message);
            }
        }

        /// <summary>
        /// Track user activity and behavior
        /// </summary>
        /// <param name="activityType">Type of user activity</param>
        /// <param name="userId">User ID</param>
        /// <param name="details">Activity details</param>
        public async Task TrackUserActivityAsync(string activityType, string userId, Dictionary<string, object>? details = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? "unknown";

                // Create custom event
                var eventTelemetry = new CustomEventTelemetry("UserActivity")
                {
                    Properties = {
                        ["ActivityType"] = activityType,
                        ["UserId"] = userId,
                        ["IpAddress"] = ipAddress,
                        ["UserAgent"] = userAgent,
                        ["Timestamp"] = DateTimeOffset.UtcNow.ToString("O")
                    }
                };

                // Add activity details
                if (details != null)
                {
                    foreach (var item in details)
                    {
                        eventTelemetry.Properties[item.Key] = item.Value?.ToString() ?? "null";
                    }
                }

                // Track the event
                _telemetryClient.TrackEvent(eventTelemetry);

                // Track activity metrics
                _telemetryClient.TrackMetric($"UserActivity.{activityType}", 1);
                _telemetryClient.TrackMetric("UserActivity.Total", 1);

                // Log user activity
                _logger.LogInformation("User activity tracked: {ActivityType} for user {UserId}", 
                    activityType, userId);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track user activity: {ActivityType}", activityType);
            }
        }

        /// <summary>
        /// Track system health metrics
        /// </summary>
        /// <param name="healthMetrics">System health metrics</param>
        public async Task TrackSystemHealthAsync(Dictionary<string, object> healthMetrics)
        {
            try
            {
                foreach (var metric in healthMetrics)
                {
                    var value = Convert.ToDouble(metric.Value);
                    _telemetryClient.TrackMetric($"SystemHealth.{metric.Key}", value);
                }

                // Track overall system health
                var overallHealth = CalculateOverallHealth(healthMetrics);
                _telemetryClient.TrackMetric("SystemHealth.Overall", overallHealth);

                // Log system health
                _logger.LogInformation("System health tracked: Overall {OverallHealth}%", overallHealth);

                // Send alert for poor health
                if (overallHealth < _options.HealthAlertThreshold)
                {
                    await SendHealthAlertAsync(overallHealth, healthMetrics);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track system health metrics");
            }
        }

        /// <summary>
        /// Track API usage and performance
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="method">HTTP method</param>
        /// <param name="duration">Request duration</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="responseSize">Response size in bytes</param>
        public async Task TrackApiUsageAsync(string endpoint, string method, TimeSpan duration, int statusCode, long responseSize)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var userId = httpContext?.User?.FindFirst("sub")?.Value ?? "anonymous";
                var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

                // Track API metrics
                _telemetryClient.TrackMetric($"API.{endpoint}.Duration", duration.TotalMilliseconds, new Dictionary<string, string>
                {
                    ["Endpoint"] = endpoint,
                    ["Method"] = method,
                    ["StatusCode"] = statusCode.ToString(),
                    ["UserId"] = userId,
                    ["IpAddress"] = ipAddress
                });

                _telemetryClient.TrackMetric($"API.{endpoint}.ResponseSize", responseSize);
                _telemetryClient.TrackMetric($"API.{endpoint}.StatusCode.{statusCode}", 1);

                // Track API usage patterns
                _telemetryClient.TrackMetric("API.TotalRequests", 1);
                _telemetryClient.TrackMetric($"API.{method}.Requests", 1);

                // Log API usage
                _logger.LogInformation("API usage tracked: {Method} {Endpoint} - {StatusCode} in {Duration}ms", 
                    method, endpoint, statusCode, duration.TotalMilliseconds);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track API usage for {Endpoint}", endpoint);
            }
        }

        /// <summary>
        /// Get comprehensive monitoring statistics
        /// </summary>
        /// <returns>Monitoring statistics</returns>
        public async Task<MonitoringStatistics> GetMonitoringStatisticsAsync()
        {
            try
            {
                // This would typically query Application Insights or a database
                // For now, return basic statistics
                return new MonitoringStatistics
                {
                    TotalRequests = 0, // Would be calculated from telemetry
                    TotalErrors = 0,
                    AverageResponseTime = 0,
                    SystemHealth = 100,
                    ActiveUsers = 0,
                    LastUpdated = DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get monitoring statistics");
                return new MonitoringStatistics
                {
                    LastUpdated = DateTimeOffset.UtcNow
                };
            }
        }

        private SeverityLevel GetSeverityLevel(ErrorSeverity severity)
        {
            return severity switch
            {
                ErrorSeverity.Low => SeverityLevel.Information,
                ErrorSeverity.Medium => SeverityLevel.Warning,
                ErrorSeverity.High => SeverityLevel.Error,
                ErrorSeverity.Critical => SeverityLevel.Critical,
                _ => SeverityLevel.Error
            };
        }

        private LogLevel GetLogLevel(ErrorSeverity severity)
        {
            return severity switch
            {
                ErrorSeverity.Low => LogLevel.Information,
                ErrorSeverity.Medium => LogLevel.Warning,
                ErrorSeverity.High => LogLevel.Error,
                ErrorSeverity.Critical => LogLevel.Critical,
                _ => LogLevel.Error
            };
        }

        private double CalculateOverallHealth(Dictionary<string, object> healthMetrics)
        {
            // Simple health calculation - would be more sophisticated in practice
            var totalMetrics = healthMetrics.Count;
            var healthyMetrics = healthMetrics.Count(m => Convert.ToDouble(m.Value) > 0.8);
            return totalMetrics > 0 ? (double)healthyMetrics / totalMetrics * 100 : 100;
        }

        private async Task SendCriticalErrorAlertAsync(Exception exception, Dictionary<string, object>? context)
        {
            try
            {
                _logger.LogCritical("CRITICAL ERROR ALERT: {Message} - {StackTrace}", 
                    exception.Message, exception.StackTrace);

                // This would integrate with alerting systems (email, SMS, Slack, etc.)
                _telemetryClient.TrackMetric("Alerts.CriticalError", 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send critical error alert");
            }
        }

        private async Task SendHealthAlertAsync(double overallHealth, Dictionary<string, object> healthMetrics)
        {
            try
            {
                _logger.LogWarning("HEALTH ALERT: System health is {OverallHealth}%", overallHealth);

                // This would integrate with alerting systems
                _telemetryClient.TrackMetric("Alerts.Health", 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send health alert");
            }
        }
    }

    public enum ErrorSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class MonitoringStatistics
    {
        public long TotalRequests { get; set; }
        public long TotalErrors { get; set; }
        public double AverageResponseTime { get; set; }
        public double SystemHealth { get; set; }
        public int ActiveUsers { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }

    public class MonitoringOptions
    {
        public bool Enabled { get; set; } = true;
        public bool TrackPerformance { get; set; } = true;
        public bool TrackErrors { get; set; } = true;
        public bool TrackUserActivity { get; set; } = true;
        public bool TrackSystemHealth { get; set; } = true;
        public bool TrackApiUsage { get; set; } = true;
        public double HealthAlertThreshold { get; set; } = 80.0;
        public bool SendAlerts { get; set; } = true;
    }
}
