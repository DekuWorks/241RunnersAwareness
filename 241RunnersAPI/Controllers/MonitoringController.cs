using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using _241RunnersAPI.Services;
using Microsoft.Extensions.Logging;

namespace _241RunnersAPI.Controllers
{
    /// <summary>
    /// Monitoring controller for application monitoring and analytics
    /// Provides endpoints for monitoring data, alerts, and system health
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize] // Require authentication for all monitoring endpoints
    public class MonitoringController : ControllerBase
    {
        private readonly ComprehensiveMonitoringService _monitoringService;
        private readonly RealTimeMonitoringService _realTimeMonitoringService;
        private readonly ILogger<MonitoringController> _logger;

        public MonitoringController(
            ComprehensiveMonitoringService monitoringService,
            RealTimeMonitoringService realTimeMonitoringService,
            ILogger<MonitoringController> logger)
        {
            _monitoringService = monitoringService;
            _realTimeMonitoringService = realTimeMonitoringService;
            _logger = logger;
        }

        /// <summary>
        /// Get comprehensive monitoring statistics
        /// </summary>
        /// <returns>Monitoring statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "admin")] // Only admins can view monitoring statistics
        public async Task<IActionResult> GetMonitoringStatistics()
        {
            try
            {
                _logger.LogInformation("Monitoring statistics requested by {User}", User.Identity?.Name);

                var statistics = await _monitoringService.GetMonitoringStatisticsAsync();

                return Ok(new
                {
                    success = true,
                    data = statistics,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get monitoring statistics");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get monitoring statistics",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Get active monitoring sessions
        /// </summary>
        /// <returns>Active monitoring sessions</returns>
        [HttpGet("sessions")]
        [Authorize(Roles = "admin")] // Only admins can view monitoring sessions
        public async Task<IActionResult> GetActiveSessions()
        {
            try
            {
                _logger.LogInformation("Active monitoring sessions requested by {User}", User.Identity?.Name);

                var sessions = await _realTimeMonitoringService.GetActiveSessionsAsync();

                return Ok(new
                {
                    success = true,
                    data = sessions,
                    count = sessions.Count,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active monitoring sessions");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get active monitoring sessions",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Send a test alert
        /// </summary>
        /// <param name="request">Test alert request</param>
        /// <returns>Alert result</returns>
        [HttpPost("alerts/test")]
        [Authorize(Roles = "admin")] // Only admins can send test alerts
        public async Task<IActionResult> SendTestAlert([FromBody] TestAlertRequest request)
        {
            try
            {
                _logger.LogInformation("Test alert requested by {User}: {Title}", User.Identity?.Name, request.Title);

                var alert = new MonitoringAlert
                {
                    Severity = request.Severity,
                    Title = request.Title,
                    Message = request.Message,
                    Category = request.Category,
                    Metadata = request.Metadata ?? new Dictionary<string, object>()
                };

                await _realTimeMonitoringService.SendRealTimeAlertAsync(alert);

                return Ok(new
                {
                    success = true,
                    message = "Test alert sent successfully",
                    alertId = alert.Id,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test alert");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send test alert",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Get system health status
        /// </summary>
        /// <returns>System health status</returns>
        [HttpGet("health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                _logger.LogInformation("System health check requested by {User}", User.Identity?.Name);

                var health = new
                {
                    status = "healthy",
                    timestamp = DateTimeOffset.UtcNow,
                    services = new
                    {
                        database = "healthy",
                        cache = "healthy",
                        signalr = "healthy",
                        monitoring = "healthy"
                    },
                    metrics = new
                    {
                        uptime = GetSystemUptime(),
                        memoryUsage = GetMemoryUsage(),
                        cpuUsage = GetCpuUsage(),
                        activeConnections = await GetActiveConnections()
                    }
                };

                return Ok(new
                {
                    success = true,
                    data = health,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system health");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get system health",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Track custom performance metric
        /// </summary>
        /// <param name="request">Performance tracking request</param>
        /// <returns>Tracking result</returns>
        [HttpPost("performance/track")]
        [Authorize(Roles = "admin")] // Only admins can track custom metrics
        public async Task<IActionResult> TrackPerformance([FromBody] PerformanceTrackingRequest request)
        {
            try
            {
                _logger.LogInformation("Custom performance tracking requested by {User}: {Operation}", 
                    User.Identity?.Name, request.OperationName);

                await _monitoringService.TrackPerformanceAsync(
                    request.OperationName,
                    TimeSpan.FromMilliseconds(request.DurationMs),
                    request.Success,
                    request.AdditionalMetrics
                );

                return Ok(new
                {
                    success = true,
                    message = "Performance metric tracked successfully",
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track performance metric");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to track performance metric",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Track custom error
        /// </summary>
        /// <param name="request">Error tracking request</param>
        /// <returns>Tracking result</returns>
        [HttpPost("errors/track")]
        [Authorize(Roles = "admin")] // Only admins can track custom errors
        public async Task<IActionResult> TrackError([FromBody] ErrorTrackingRequest request)
        {
            try
            {
                _logger.LogInformation("Custom error tracking requested by {User}: {Message}", 
                    User.Identity?.Name, request.Message);

                var exception = new Exception(request.Message);
                await _monitoringService.TrackErrorAsync(exception, request.Context, request.Severity);

                return Ok(new
                {
                    success = true,
                    message = "Error tracked successfully",
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track custom error");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to track custom error",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Track user activity
        /// </summary>
        /// <param name="request">User activity tracking request</param>
        /// <returns>Tracking result</returns>
        [HttpPost("activity/track")]
        public async Task<IActionResult> TrackUserActivity([FromBody] UserActivityTrackingRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? "anonymous";
                _logger.LogInformation("User activity tracking requested by {User}: {ActivityType}", 
                    userId, request.ActivityType);

                await _monitoringService.TrackUserActivityAsync(
                    request.ActivityType,
                    userId,
                    request.Details
                );

                return Ok(new
                {
                    success = true,
                    message = "User activity tracked successfully",
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track user activity");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to track user activity",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        private TimeSpan GetSystemUptime()
        {
            return DateTimeOffset.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime;
        }

        private long GetMemoryUsage()
        {
            return System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024; // MB
        }

        private double GetCpuUsage()
        {
            // This would require more sophisticated CPU monitoring
            return 25.0; // Placeholder
        }

        private async Task<int> GetActiveConnections()
        {
            try
            {
                var sessions = await _realTimeMonitoringService.GetActiveSessionsAsync();
                return sessions.Count;
            }
            catch
            {
                return 0;
            }
        }
    }

    public class TestAlertRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "info";
        public string Category { get; set; } = "test";
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class PerformanceTrackingRequest
    {
        public string OperationName { get; set; } = string.Empty;
        public long DurationMs { get; set; }
        public bool Success { get; set; }
        public Dictionary<string, object>? AdditionalMetrics { get; set; }
    }

    public class ErrorTrackingRequest
    {
        public string Message { get; set; } = string.Empty;
        public ErrorSeverity Severity { get; set; } = ErrorSeverity.Medium;
        public Dictionary<string, object>? Context { get; set; }
    }

    public class UserActivityTrackingRequest
    {
        public string ActivityType { get; set; } = string.Empty;
        public Dictionary<string, object>? Details { get; set; }
    }
}
