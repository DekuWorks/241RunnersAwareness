using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace _241RunnersAwareness.BackendAPI.Services
{
    public interface IAnalyticsService
    {
        Task TrackMapInteractionAsync(string userId, string interactionType, string location, string caseId);
        Task TrackAuthIssueAsync(string userId, string issueType, string details);
        Task TrackCaseSearchAsync(string userId, string searchTerm, string filters, int resultCount);
        Task TrackUrgentAlertAsync(string caseId, string individualName, string location, string sentBy);
        Task TrackUserActionAsync(string userId, string action, string details);
        Task TrackPerformanceAsync(string endpoint, long responseTime, bool success);
        Task TrackErrorAsync(string errorType, string errorMessage, string stackTrace);
        Task GetAnalyticsReportAsync(DateTime startDate, DateTime endDate);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly ILogger<AnalyticsService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _analyticsEndpoint;

        public AnalyticsService(ILogger<AnalyticsService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _analyticsEndpoint = _configuration["Analytics:Endpoint"] ?? "https://analytics.241runnersawareness.org";
        }

        /// <summary>
        /// Tracks map interactions for understanding user behavior
        /// </summary>
        public async Task TrackMapInteractionAsync(string userId, string interactionType, string location, string caseId)
        {
            try
            {
                var eventData = new
                {
                    EventType = "MapInteraction",
                    UserId = userId,
                    InteractionType = interactionType, // zoom, pan, click, search
                    Location = location,
                    CaseId = caseId,
                    Timestamp = DateTime.UtcNow,
                    SessionId = GetSessionId()
                };

                await SendAnalyticsEventAsync(eventData);
                _logger.LogInformation($"Map interaction tracked: {interactionType} at {location} by user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track map interaction");
            }
        }

        /// <summary>
        /// Tracks authentication issues for improving user experience
        /// </summary>
        public async Task TrackAuthIssueAsync(string userId, string issueType, string details)
        {
            try
            {
                var eventData = new
                {
                    EventType = "AuthIssue",
                    UserId = userId,
                    IssueType = issueType, // login_failed, password_reset, 2fa_issue, etc.
                    Details = details,
                    Timestamp = DateTime.UtcNow,
                    SessionId = GetSessionId(),
                    UserAgent = GetUserAgent(),
                    IPAddress = GetIPAddress()
                };

                await SendAnalyticsEventAsync(eventData);
                _logger.LogWarning($"Auth issue tracked: {issueType} for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track auth issue");
            }
        }

        /// <summary>
        /// Tracks case search patterns for improving search functionality
        /// </summary>
        public async Task TrackCaseSearchAsync(string userId, string searchTerm, string filters, int resultCount)
        {
            try
            {
                var eventData = new
                {
                    EventType = "CaseSearch",
                    UserId = userId,
                    SearchTerm = searchTerm,
                    Filters = filters,
                    ResultCount = resultCount,
                    Timestamp = DateTime.UtcNow,
                    SessionId = GetSessionId()
                };

                await SendAnalyticsEventAsync(eventData);
                _logger.LogInformation($"Case search tracked: '{searchTerm}' returned {resultCount} results for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track case search");
            }
        }

        /// <summary>
        /// Tracks urgent alerts for monitoring system usage
        /// </summary>
        public async Task TrackUrgentAlertAsync(string caseId, string individualName, string location, string sentBy)
        {
            try
            {
                var eventData = new
                {
                    EventType = "UrgentAlert",
                    CaseId = caseId,
                    IndividualName = individualName,
                    Location = location,
                    SentBy = sentBy,
                    Timestamp = DateTime.UtcNow,
                    AlertId = Guid.NewGuid().ToString()
                };

                await SendAnalyticsEventAsync(eventData);
                _logger.LogWarning($"Urgent alert tracked: Case {caseId} for {individualName} at {location}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track urgent alert");
            }
        }

        /// <summary>
        /// Tracks general user actions for understanding user behavior
        /// </summary>
        public async Task TrackUserActionAsync(string userId, string action, string details)
        {
            try
            {
                var eventData = new
                {
                    EventType = "UserAction",
                    UserId = userId,
                    Action = action, // case_create, case_update, profile_edit, etc.
                    Details = details,
                    Timestamp = DateTime.UtcNow,
                    SessionId = GetSessionId()
                };

                await SendAnalyticsEventAsync(eventData);
                _logger.LogInformation($"User action tracked: {action} by user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track user action");
            }
        }

        /// <summary>
        /// Tracks API performance metrics
        /// </summary>
        public async Task TrackPerformanceAsync(string endpoint, long responseTime, bool success)
        {
            try
            {
                var eventData = new
                {
                    EventType = "Performance",
                    Endpoint = endpoint,
                    ResponseTime = responseTime,
                    Success = success,
                    Timestamp = DateTime.UtcNow
                };

                await SendAnalyticsEventAsync(eventData);
                
                if (responseTime > 5000) // Log slow endpoints
                {
                    _logger.LogWarning($"Slow endpoint detected: {endpoint} took {responseTime}ms");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track performance");
            }
        }

        /// <summary>
        /// Tracks errors for monitoring system health
        /// </summary>
        public async Task TrackErrorAsync(string errorType, string errorMessage, string stackTrace)
        {
            try
            {
                var eventData = new
                {
                    EventType = "Error",
                    ErrorType = errorType,
                    ErrorMessage = errorMessage,
                    StackTrace = stackTrace,
                    Timestamp = DateTime.UtcNow
                };

                await SendAnalyticsEventAsync(eventData);
                _logger.LogError($"Error tracked: {errorType} - {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track error");
            }
        }

        /// <summary>
        /// Generates analytics report for the specified date range
        /// </summary>
        public async Task GetAnalyticsReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var reportData = new
                {
                    EventType = "AnalyticsReport",
                    StartDate = startDate,
                    EndDate = endDate,
                    ReportId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                };

                await SendAnalyticsEventAsync(reportData);
                _logger.LogInformation($"Analytics report requested for {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate analytics report");
            }
        }

        #region Private Methods

        private async Task SendAnalyticsEventAsync(object eventData)
        {
            try
            {
                // In a real implementation, this would send to an analytics service
                // For now, we'll log the event and potentially send to a queue
                var jsonData = JsonSerializer.Serialize(eventData);
                
                // Log the analytics event
                _logger.LogInformation($"Analytics Event: {jsonData}");
                
                // TODO: Send to analytics service (Plausible, PostHog, etc.)
                // await SendToAnalyticsServiceAsync(jsonData);
                
                // TODO: Store in database for local analytics
                // await StoreAnalyticsEventAsync(eventData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send analytics event");
            }
        }

        private string GetSessionId()
        {
            // In a real implementation, this would get the session ID from the current context
            return Guid.NewGuid().ToString();
        }

        private string GetUserAgent()
        {
            // In a real implementation, this would get the user agent from the HTTP context
            return "Unknown";
        }

        private string GetIPAddress()
        {
            // In a real implementation, this would get the IP address from the HTTP context
            return "Unknown";
        }

        #endregion
    }

    /// <summary>
    /// Analytics event types for tracking
    /// </summary>
    public static class AnalyticsEventTypes
    {
        // Map interactions
        public const string MapZoom = "map_zoom";
        public const string MapPan = "map_pan";
        public const string MapClick = "map_click";
        public const string MapSearch = "map_search";
        public const string MapFilter = "map_filter";

        // Authentication
        public const string LoginSuccess = "login_success";
        public const string LoginFailed = "login_failed";
        public const string PasswordReset = "password_reset";
        public const string TwoFactorSetup = "2fa_setup";
        public const string TwoFactorVerify = "2fa_verify";

        // Case management
        public const string CaseCreate = "case_create";
        public const string CaseUpdate = "case_update";
        public const string CaseDelete = "case_delete";
        public const string CaseSearch = "case_search";
        public const string CaseExport = "case_export";

        // Alerts
        public const string UrgentAlert = "urgent_alert";
        public const string LawEnforcementAlert = "law_enforcement_alert";
        public const string EmergencyContactAlert = "emergency_contact_alert";
        public const string MediaAlert = "media_alert";
        public const string FoundNotification = "found_notification";

        // User actions
        public const string ProfileEdit = "profile_edit";
        public const string SettingsChange = "settings_change";
        public const string NotificationPreference = "notification_preference";

        // Performance
        public const string SlowEndpoint = "slow_endpoint";
        public const string Timeout = "timeout";
        public const string Error = "error";
    }
} 