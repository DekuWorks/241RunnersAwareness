using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Security.Cryptography;
using System.Text;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Security audit service for monitoring and logging security events
    /// Tracks authentication attempts, suspicious activities, and security violations
    /// </summary>
    public class SecurityAuditService
    {
        private readonly ILogger<SecurityAuditService> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly SecurityAuditOptions _options;

        public SecurityAuditService(ILogger<SecurityAuditService> logger, TelemetryClient telemetryClient, SecurityAuditOptions options)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _options = options;
        }

        /// <summary>
        /// Log authentication attempt
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="success">Whether authentication was successful</param>
        /// <param name="ipAddress">Client IP address</param>
        /// <param name="userAgent">Client user agent</param>
        /// <param name="additionalInfo">Additional information</param>
        public async Task LogAuthenticationAttemptAsync(string userId, bool success, string ipAddress, string userAgent, Dictionary<string, object>? additionalInfo = null)
        {
            try
            {
                var auditEvent = new SecurityAuditEvent
                {
                    EventType = "Authentication",
                    UserId = userId,
                    Success = success,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTimeOffset.UtcNow,
                    AdditionalInfo = additionalInfo ?? new Dictionary<string, object>()
                };

                await LogSecurityEventAsync(auditEvent);

                // Track metrics
                _telemetryClient.TrackMetric($"Security.Auth.{success}", 1);
                _telemetryClient.TrackMetric("Security.Auth.Total", 1);

                if (!success)
                {
                    _telemetryClient.TrackMetric("Security.Auth.Failed", 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log authentication attempt for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Log suspicious activity
        /// </summary>
        /// <param name="activityType">Type of suspicious activity</param>
        /// <param name="userId">User ID (if known)</param>
        /// <param name="ipAddress">Client IP address</param>
        /// <param name="details">Activity details</param>
        public async Task LogSuspiciousActivityAsync(string activityType, string? userId, string ipAddress, Dictionary<string, object> details)
        {
            try
            {
                var auditEvent = new SecurityAuditEvent
                {
                    EventType = "SuspiciousActivity",
                    ActivityType = activityType,
                    UserId = userId ?? "unknown",
                    IpAddress = ipAddress,
                    Timestamp = DateTimeOffset.UtcNow,
                    AdditionalInfo = details
                };

                await LogSecurityEventAsync(auditEvent);

                // Track suspicious activity metrics
                _telemetryClient.TrackMetric($"Security.Suspicious.{activityType}", 1);
                _telemetryClient.TrackMetric("Security.Suspicious.Total", 1);

                // Send alert for high-risk activities
                if (IsHighRiskActivity(activityType))
                {
                    await SendSecurityAlertAsync(auditEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log suspicious activity: {ActivityType}", activityType);
            }
        }

        /// <summary>
        /// Log security violation
        /// </summary>
        /// <param name="violationType">Type of security violation</param>
        /// <param name="userId">User ID (if known)</param>
        /// <param name="ipAddress">Client IP address</param>
        /// <param name="details">Violation details</param>
        public async Task LogSecurityViolationAsync(string violationType, string? userId, string ipAddress, Dictionary<string, object> details)
        {
            try
            {
                var auditEvent = new SecurityAuditEvent
                {
                    EventType = "SecurityViolation",
                    ViolationType = violationType,
                    UserId = userId ?? "unknown",
                    IpAddress = ipAddress,
                    Timestamp = DateTimeOffset.UtcNow,
                    AdditionalInfo = details
                };

                await LogSecurityEventAsync(auditEvent);

                // Track security violation metrics
                _telemetryClient.TrackMetric($"Security.Violation.{violationType}", 1);
                _telemetryClient.TrackMetric("Security.Violation.Total", 1);

                // Send immediate alert for security violations
                await SendSecurityAlertAsync(auditEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security violation: {ViolationType}", violationType);
            }
        }

        /// <summary>
        /// Log data access event
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="resourceType">Type of resource accessed</param>
        /// <param name="resourceId">Resource ID</param>
        /// <param name="action">Action performed</param>
        /// <param name="ipAddress">Client IP address</param>
        public async Task LogDataAccessAsync(string userId, string resourceType, string resourceId, string action, string ipAddress)
        {
            try
            {
                var auditEvent = new SecurityAuditEvent
                {
                    EventType = "DataAccess",
                    UserId = userId,
                    ResourceType = resourceType,
                    ResourceId = resourceId,
                    Action = action,
                    IpAddress = ipAddress,
                    Timestamp = DateTimeOffset.UtcNow
                };

                await LogSecurityEventAsync(auditEvent);

                // Track data access metrics
                _telemetryClient.TrackMetric($"Security.DataAccess.{action}", 1);
                _telemetryClient.TrackMetric("Security.DataAccess.Total", 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log data access for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Get security audit statistics
        /// </summary>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        /// <returns>Security audit statistics</returns>
        public async Task<SecurityAuditStatistics> GetSecurityStatisticsAsync(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            try
            {
                // This would typically query a database or audit log
                // For now, return basic statistics
                return new SecurityAuditStatistics
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalEvents = 0, // Would be calculated from audit log
                    AuthenticationAttempts = 0,
                    FailedAuthentications = 0,
                    SuspiciousActivities = 0,
                    SecurityViolations = 0,
                    DataAccessEvents = 0,
                    LastUpdated = DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get security statistics");
                return new SecurityAuditStatistics
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    LastUpdated = DateTimeOffset.UtcNow
                };
            }
        }

        private async Task LogSecurityEventAsync(SecurityAuditEvent auditEvent)
        {
            // Log to application insights
            var telemetry = new EventTelemetry("SecurityAudit")
            {
                Properties = {
                    ["EventType"] = auditEvent.EventType,
                    ["UserId"] = auditEvent.UserId,
                    ["IpAddress"] = auditEvent.IpAddress,
                    ["Timestamp"] = auditEvent.Timestamp.ToString("O"),
                    ["Success"] = auditEvent.Success?.ToString() ?? "N/A"
                }
            };

            if (!string.IsNullOrEmpty(auditEvent.ActivityType))
            {
                telemetry.Properties["ActivityType"] = auditEvent.ActivityType;
            }

            if (!string.IsNullOrEmpty(auditEvent.ViolationType))
            {
                telemetry.Properties["ViolationType"] = auditEvent.ViolationType;
            }

            if (!string.IsNullOrEmpty(auditEvent.ResourceType))
            {
                telemetry.Properties["ResourceType"] = auditEvent.ResourceType;
            }

            if (!string.IsNullOrEmpty(auditEvent.Action))
            {
                telemetry.Properties["Action"] = auditEvent.Action;
            }

            _telemetryClient.TrackEvent(telemetry);

            // Log to application logger
            _logger.LogInformation("Security audit event: {EventType} for user {UserId} from {IpAddress}", 
                auditEvent.EventType, auditEvent.UserId, auditEvent.IpAddress);
        }

        private bool IsHighRiskActivity(string activityType)
        {
            var highRiskActivities = new[]
            {
                "MultipleFailedLogins",
                "BruteForceAttack",
                "SQLInjectionAttempt",
                "XSSAttack",
                "UnauthorizedAccess",
                "DataExfiltration",
                "PrivilegeEscalation"
            };

            return highRiskActivities.Contains(activityType);
        }

        private async Task SendSecurityAlertAsync(SecurityAuditEvent auditEvent)
        {
            try
            {
                // This would integrate with alerting systems
                _logger.LogWarning("SECURITY ALERT: {EventType} for user {UserId} from {IpAddress}", 
                    auditEvent.EventType, auditEvent.UserId, auditEvent.IpAddress);

                // Track alert metrics
                _telemetryClient.TrackMetric("Security.Alert.Sent", 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send security alert");
            }
        }
    }

    public class SecurityAuditEvent
    {
        public string EventType { get; set; } = string.Empty;
        public string? ActivityType { get; set; }
        public string? ViolationType { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? ResourceType { get; set; }
        public string? ResourceId { get; set; }
        public string? Action { get; set; }
        public bool? Success { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; } = new();
    }

    public class SecurityAuditStatistics
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int TotalEvents { get; set; }
        public int AuthenticationAttempts { get; set; }
        public int FailedAuthentications { get; set; }
        public int SuspiciousActivities { get; set; }
        public int SecurityViolations { get; set; }
        public int DataAccessEvents { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }

    public class SecurityAuditOptions
    {
        public bool Enabled { get; set; } = true;
        public bool LogToApplicationInsights { get; set; } = true;
        public bool LogToFile { get; set; } = false;
        public bool SendAlerts { get; set; } = true;
        public string[] HighRiskActivities { get; set; } = Array.Empty<string>();
        public int AlertThreshold { get; set; } = 5;
        public TimeSpan AlertWindow { get; set; } = TimeSpan.FromMinutes(5);
    }
}
