using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for monitoring and tracking performance metrics
    /// </summary>
    public class PerformanceMonitoringService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<PerformanceMonitoringService> _logger;

        public PerformanceMonitoringService(TelemetryClient telemetryClient, ILogger<PerformanceMonitoringService> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        /// <summary>
        /// Track a custom metric
        /// </summary>
        public void TrackMetric(string metricName, double value, IDictionary<string, string>? properties = null)
        {
            _telemetryClient.TrackMetric(metricName, value, properties);
            _logger.LogDebug("Metric tracked: {MetricName} = {Value}", metricName, value);
        }

        /// <summary>
        /// Track a custom event
        /// </summary>
        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _telemetryClient.TrackEvent(eventName, properties, metrics);
            _logger.LogDebug("Event tracked: {EventName}", eventName);
        }

        /// <summary>
        /// Track a dependency call
        /// </summary>
        public void TrackDependency(string dependencyTypeName, string target, string dependencyName, DateTime startTime, TimeSpan duration, bool success)
        {
            _telemetryClient.TrackDependency(dependencyTypeName, target, dependencyName, startTime, duration, success);
            _logger.LogDebug("Dependency tracked: {DependencyName} to {Target} - {Success} in {Duration}ms", 
                dependencyName, target, success ? "Success" : "Failed", duration.TotalMilliseconds);
        }

        /// <summary>
        /// Track an exception
        /// </summary>
        public void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _telemetryClient.TrackException(exception, properties, metrics);
            _logger.LogError(exception, "Exception tracked: {ExceptionMessage}", exception.Message);
        }

        /// <summary>
        /// Track a request
        /// </summary>
        public void TrackRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
        {
            _telemetryClient.TrackRequest(name, startTime, duration, responseCode, success);
            _logger.LogDebug("Request tracked: {Name} - {ResponseCode} in {Duration}ms", 
                name, responseCode, duration.TotalMilliseconds);
        }

        /// <summary>
        /// Track database query performance
        /// </summary>
        public void TrackDatabaseQuery(string queryName, TimeSpan duration, bool success, int? recordCount = null)
        {
            var properties = new Dictionary<string, string>
            {
                ["QueryName"] = queryName,
                ["RecordCount"] = recordCount?.ToString() ?? "Unknown"
            };

            var metrics = new Dictionary<string, double>
            {
                ["Duration"] = duration.TotalMilliseconds,
                ["RecordCount"] = recordCount ?? 0
            };

            TrackDependency("SQL", "Database", queryName, DateTime.UtcNow - duration, duration, success);
            TrackMetric("Database.Query.Duration", duration.TotalMilliseconds, properties);
            
            if (recordCount.HasValue)
            {
                TrackMetric("Database.Query.RecordCount", recordCount.Value, properties);
            }
        }

        /// <summary>
        /// Track API endpoint performance
        /// </summary>
        public void TrackApiEndpoint(string endpoint, string method, TimeSpan duration, int statusCode, bool success)
        {
            var properties = new Dictionary<string, string>
            {
                ["Endpoint"] = endpoint,
                ["Method"] = method,
                ["StatusCode"] = statusCode.ToString()
            };

            var metrics = new Dictionary<string, double>
            {
                ["Duration"] = duration.TotalMilliseconds,
                ["StatusCode"] = statusCode
            };

            TrackRequest($"{method} {endpoint}", DateTime.UtcNow - duration, duration, statusCode.ToString(), success);
            TrackMetric("API.Endpoint.Duration", duration.TotalMilliseconds, properties);
            TrackEvent("API.Endpoint.Called", properties, metrics);
        }

        /// <summary>
        /// Track memory usage
        /// </summary>
        public void TrackMemoryUsage()
        {
            var memory = GC.GetTotalMemory(false);
            var gen0Collections = GC.CollectionCount(0);
            var gen1Collections = GC.CollectionCount(1);
            var gen2Collections = GC.CollectionCount(2);

            TrackMetric("Memory.Total", memory);
            TrackMetric("Memory.TotalMB", memory / 1024.0 / 1024.0);
            TrackMetric("GC.Gen0Collections", gen0Collections);
            TrackMetric("GC.Gen1Collections", gen1Collections);
            TrackMetric("GC.Gen2Collections", gen2Collections);
        }

        /// <summary>
        /// Track authentication events
        /// </summary>
        public void TrackAuthentication(string eventType, string userId, bool success, string? errorMessage = null)
        {
            var properties = new Dictionary<string, string>
            {
                ["EventType"] = eventType,
                ["UserId"] = userId,
                ["Success"] = success.ToString()
            };

            if (!string.IsNullOrEmpty(errorMessage))
            {
                properties["ErrorMessage"] = errorMessage;
            }

            TrackEvent("Authentication", properties);
        }

        /// <summary>
        /// Track user registration events
        /// </summary>
        public void TrackUserRegistration(string email, string role, bool success, string? errorMessage = null)
        {
            var properties = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Role"] = role,
                ["Success"] = success.ToString()
            };

            if (!string.IsNullOrEmpty(errorMessage))
            {
                properties["ErrorMessage"] = errorMessage;
            }

            TrackEvent("User.Registration", properties);
        }

        /// <summary>
        /// Flush telemetry data
        /// </summary>
        public void Flush()
        {
            _telemetryClient.Flush();
        }
    }
}
