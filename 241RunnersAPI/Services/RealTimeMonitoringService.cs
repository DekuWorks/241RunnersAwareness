using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using System.Collections.Concurrent;
using _241RunnersAPI.Hubs;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Real-time monitoring service for live application monitoring
    /// Provides real-time metrics, alerts, and dashboard updates
    /// </summary>
    public class RealTimeMonitoringService
    {
        private readonly IHubContext<AdminHub> _hubContext;
        private readonly ILogger<RealTimeMonitoringService> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly ConcurrentDictionary<string, MonitoringSession> _activeSessions;
        private readonly Timer _metricsTimer;

        public RealTimeMonitoringService(
            IHubContext<AdminHub> hubContext,
            ILogger<RealTimeMonitoringService> logger,
            TelemetryClient telemetryClient)
        {
            _hubContext = hubContext;
            _logger = logger;
            _telemetryClient = telemetryClient;
            _activeSessions = new ConcurrentDictionary<string, MonitoringSession>();
            
            // Start metrics collection timer
            _metricsTimer = new Timer(CollectAndBroadcastMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// Start monitoring session for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="connectionId">SignalR connection ID</param>
        public async Task StartMonitoringSessionAsync(string userId, string connectionId)
        {
            try
            {
                var session = new MonitoringSession
                {
                    UserId = userId,
                    ConnectionId = connectionId,
                    StartTime = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                _activeSessions.AddOrUpdate(connectionId, session, (key, existing) => session);

                _logger.LogInformation("Started monitoring session for user {UserId} with connection {ConnectionId}", 
                    userId, connectionId);

                // Send initial metrics
                await SendInitialMetricsAsync(connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start monitoring session for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Stop monitoring session for a user
        /// </summary>
        /// <param name="connectionId">SignalR connection ID</param>
        public async Task StopMonitoringSessionAsync(string connectionId)
        {
            try
            {
                if (_activeSessions.TryRemove(connectionId, out var session))
                {
                    session.IsActive = false;
                    session.EndTime = DateTimeOffset.UtcNow;

                    _logger.LogInformation("Stopped monitoring session for user {UserId}", session.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop monitoring session for connection {ConnectionId}", connectionId);
            }
        }

        /// <summary>
        /// Send real-time alert to monitoring users
        /// </summary>
        /// <param name="alert">Alert information</param>
        public async Task SendRealTimeAlertAsync(MonitoringAlert alert)
        {
            try
            {
                var alertMessage = new
                {
                    type = "alert",
                    alert = new
                    {
                        id = alert.Id,
                        severity = alert.Severity,
                        title = alert.Title,
                        message = alert.Message,
                        timestamp = alert.Timestamp,
                        category = alert.Category,
                        metadata = alert.Metadata
                    }
                };

                // Send to all active monitoring sessions
                await _hubContext.Clients.All.SendAsync("ReceiveAlert", alertMessage);

                _logger.LogInformation("Sent real-time alert: {Title} - {Severity}", alert.Title, alert.Severity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real-time alert: {Title}", alert.Title);
            }
        }

        /// <summary>
        /// Send real-time metrics update
        /// </summary>
        /// <param name="metrics">Metrics data</param>
        public async Task SendRealTimeMetricsAsync(RealTimeMetrics metrics)
        {
            try
            {
                var metricsMessage = new
                {
                    type = "metrics",
                    metrics = new
                    {
                        timestamp = metrics.Timestamp,
                        systemHealth = metrics.SystemHealth,
                        activeUsers = metrics.ActiveUsers,
                        totalRequests = metrics.TotalRequests,
                        errorRate = metrics.ErrorRate,
                        averageResponseTime = metrics.AverageResponseTime,
                        memoryUsage = metrics.MemoryUsage,
                        cpuUsage = metrics.CpuUsage,
                        databaseConnections = metrics.DatabaseConnections,
                        cacheHitRate = metrics.CacheHitRate
                    }
                };

                // Send to all active monitoring sessions
                await _hubContext.Clients.All.SendAsync("ReceiveMetrics", metricsMessage);

                _logger.LogDebug("Sent real-time metrics update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real-time metrics");
            }
        }

        /// <summary>
        /// Send real-time log entry
        /// </summary>
        /// <param name="logEntry">Log entry</param>
        public async Task SendRealTimeLogAsync(RealTimeLogEntry logEntry)
        {
            try
            {
                var logMessage = new
                {
                    type = "log",
                    log = new
                    {
                        id = logEntry.Id,
                        level = logEntry.Level,
                        message = logEntry.Message,
                        timestamp = logEntry.Timestamp,
                        source = logEntry.Source,
                        userId = logEntry.UserId,
                        ipAddress = logEntry.IpAddress,
                        metadata = logEntry.Metadata
                    }
                };

                // Send to all active monitoring sessions
                await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);

                _logger.LogDebug("Sent real-time log entry: {Level} - {Message}", logEntry.Level, logEntry.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real-time log entry");
            }
        }

        /// <summary>
        /// Get active monitoring sessions
        /// </summary>
        /// <returns>Active monitoring sessions</returns>
        public async Task<List<MonitoringSession>> GetActiveSessionsAsync()
        {
            try
            {
                var activeSessions = _activeSessions.Values
                    .Where(s => s.IsActive)
                    .ToList();

                return activeSessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active monitoring sessions");
                return new List<MonitoringSession>();
            }
        }

        private async void CollectAndBroadcastMetrics(object? state)
        {
            try
            {
                if (_activeSessions.IsEmpty)
                    return;

                var metrics = await CollectSystemMetricsAsync();
                await SendRealTimeMetricsAsync(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect and broadcast metrics");
            }
        }

        private async Task<RealTimeMetrics> CollectSystemMetricsAsync()
        {
            try
            {
                // Collect system metrics
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var memoryUsage = process.WorkingSet64 / 1024 / 1024; // MB
                var cpuUsage = await GetCpuUsageAsync();

                var metrics = new RealTimeMetrics
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    SystemHealth = CalculateSystemHealth(),
                    ActiveUsers = _activeSessions.Count,
                    TotalRequests = GetTotalRequests(), // Would be calculated from telemetry
                    ErrorRate = GetErrorRate(), // Would be calculated from telemetry
                    AverageResponseTime = GetAverageResponseTime(), // Would be calculated from telemetry
                    MemoryUsage = memoryUsage,
                    CpuUsage = cpuUsage,
                    DatabaseConnections = GetDatabaseConnections(), // Would be calculated from database
                    CacheHitRate = GetCacheHitRate() // Would be calculated from cache
                };

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect system metrics");
                return new RealTimeMetrics
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    SystemHealth = 0,
                    ActiveUsers = 0,
                    TotalRequests = 0,
                    ErrorRate = 0,
                    AverageResponseTime = 0,
                    MemoryUsage = 0,
                    CpuUsage = 0,
                    DatabaseConnections = 0,
                    CacheHitRate = 0
                };
            }
        }

        private async Task SendInitialMetricsAsync(string connectionId)
        {
            try
            {
                var metrics = await CollectSystemMetricsAsync();
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveInitialMetrics", metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send initial metrics to connection {ConnectionId}", connectionId);
            }
        }

        private double CalculateSystemHealth()
        {
            // Simple health calculation - would be more sophisticated in practice
            return 95.0; // Placeholder
        }

        private async Task<double> GetCpuUsageAsync()
        {
            try
            {
                // This would require more sophisticated CPU monitoring
                return 25.0; // Placeholder
            }
            catch
            {
                return 0.0;
            }
        }

        private long GetTotalRequests()
        {
            // This would be calculated from Application Insights or database
            return 0; // Placeholder
        }

        private double GetErrorRate()
        {
            // This would be calculated from Application Insights or database
            return 0.0; // Placeholder
        }

        private double GetAverageResponseTime()
        {
            // This would be calculated from Application Insights or database
            return 0.0; // Placeholder
        }

        private int GetDatabaseConnections()
        {
            // This would be calculated from database connection pool
            return 0; // Placeholder
        }

        private double GetCacheHitRate()
        {
            // This would be calculated from cache statistics
            return 0.0; // Placeholder
        }
    }

    public class MonitoringSession
    {
        public string UserId { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public bool IsActive { get; set; }
    }

    public class MonitoringAlert
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Severity { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public string Category { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class RealTimeMetrics
    {
        public DateTimeOffset Timestamp { get; set; }
        public double SystemHealth { get; set; }
        public int ActiveUsers { get; set; }
        public long TotalRequests { get; set; }
        public double ErrorRate { get; set; }
        public double AverageResponseTime { get; set; }
        public long MemoryUsage { get; set; }
        public double CpuUsage { get; set; }
        public int DatabaseConnections { get; set; }
        public double CacheHitRate { get; set; }
    }

    public class RealTimeLogEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public string Source { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? IpAddress { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
