using Microsoft.AspNetCore.SignalR;
using _241RunnersAPI.Hubs;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for SignalR real-time communication
    /// </summary>
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<AlertsHub> _hubContext;
        private readonly IFirebaseNotificationService _notificationService;
        private readonly ILogger<SignalRService> _logger;

        public SignalRService(
            IHubContext<AlertsHub> hubContext,
            IFirebaseNotificationService notificationService,
            ILogger<SignalRService> logger)
        {
            _hubContext = hubContext;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Broadcast case update to relevant users
        /// </summary>
        public async Task<ServiceResult> BroadcastCaseUpdatedAsync(int caseId, object caseData)
        {
            try
            {
                var topic = Topics.GetCaseTopic(caseId);
                var payload = new { id = caseId, data = caseData };

                // Send via SignalR to connected clients subscribed to this case
                await _hubContext.Clients.Group($"topic:{topic}")
                    .SendAsync("caseUpdated", payload);

                // Send push notification to topic subscribers
                var notification = new CreateNotificationDto
                {
                    Title = "Case Updated",
                    Body = "A case you're following has been updated",
                    Type = "case_updated",
                    Topic = topic,
                    Data = new Dictionary<string, object>
                    {
                        ["caseId"] = caseId,
                        ["caseData"] = caseData
                    },
                    RelatedCaseId = caseId,
                    Priority = "normal"
                };

                var pushResult = await _notificationService.SendNotificationToTopicAsync(topic, notification);

                _logger.LogInformation("Case update broadcasted for case {CaseId}", caseId);

                return ServiceResult.CreateSuccess(
                    "Case update broadcasted successfully",
                    new { caseId = caseId, pushResult = pushResult.Success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting case update for case {CaseId}", caseId);
                return ServiceResult.CreateFailure($"Failed to broadcast case update: {ex.Message}");
            }
        }

        /// <summary>
        /// Broadcast new case to all users
        /// </summary>
        public async Task<ServiceResult> BroadcastNewCaseAsync(int caseId, object caseData)
        {
            try
            {
                var payload = new { id = caseId, data = caseData };

                // Send via SignalR to all connected clients
                await _hubContext.Clients.All.SendAsync("newCase", payload);

                // Send push notification to all users
                var notification = new CreateNotificationDto
                {
                    Title = "New Case Reported",
                    Body = "A new case has been reported in your area",
                    Type = "new_case",
                    Topic = Topics.OrgAll,
                    Data = new Dictionary<string, object>
                    {
                        ["caseId"] = caseId,
                        ["caseData"] = caseData
                    },
                    RelatedCaseId = caseId,
                    Priority = "high"
                };

                var pushResult = await _notificationService.SendNotificationToAllUsersAsync(notification);

                _logger.LogInformation("New case broadcasted for case {CaseId}", caseId);

                return ServiceResult.CreateSuccess(
                    "New case broadcasted successfully",
                    new { caseId = caseId, pushResult = pushResult.Success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting new case for case {CaseId}", caseId);
                return ServiceResult.CreateFailure($"Failed to broadcast new case: {ex.Message}");
            }
        }

        /// <summary>
        /// Broadcast admin notice to admin users
        /// </summary>
        public async Task<ServiceResult> BroadcastAdminNoticeAsync(string message, object? data = null)
        {
            try
            {
                var payload = new { message = message, data = data };

                // Send via SignalR to admin users
                await _hubContext.Clients.Group("role:admin")
                    .SendAsync("adminNotice", payload);

                // Send push notification to admin users
                var notification = new CreateNotificationDto
                {
                    Title = "Admin Notice",
                    Body = message,
                    Type = "admin_notice",
                    Topic = Topics.RoleAdmin,
                    Data = data != null ? new Dictionary<string, object> { ["data"] = data } : null,
                    Priority = "high"
                };

                var pushResult = await _notificationService.SendNotificationToAdminsAsync(notification);

                _logger.LogInformation("Admin notice broadcasted: {Message}", message);

                return ServiceResult.CreateSuccess(
                    "Admin notice broadcasted successfully",
                    new { message = message, pushResult = pushResult.Success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting admin notice: {Message}", message);
                return ServiceResult.CreateFailure($"Failed to broadcast admin notice: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification to specific user
        /// </summary>
        public async Task<ServiceResult> SendToUserAsync(int userId, string type, object data)
        {
            try
            {
                var payload = new { type = type, data = data, timestamp = DateTime.UtcNow };

                // Send via SignalR to specific user
                await _hubContext.Clients.Group($"user:{userId}")
                    .SendAsync("Notification", payload);

                _logger.LogInformation("Notification sent to user {UserId}: {Type}", userId, type);

                return ServiceResult.CreateSuccess("Notification sent to user successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                return ServiceResult.CreateFailure($"Failed to send notification to user: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification to all connected users
        /// </summary>
        public async Task<ServiceResult> SendToAllAsync(string type, object data)
        {
            try
            {
                var payload = new { type = type, data = data, timestamp = DateTime.UtcNow };

                // Send via SignalR to all connected clients
                await _hubContext.Clients.All.SendAsync("BroadcastNotification", payload);

                _logger.LogInformation("Broadcast notification sent: {Type}", type);

                return ServiceResult.CreateSuccess("Broadcast notification sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending broadcast notification: {Type}", type);
                return ServiceResult.CreateFailure($"Failed to send broadcast notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification to admin users
        /// </summary>
        public async Task<ServiceResult> SendToAdminsAsync(string type, object data)
        {
            try
            {
                var payload = new { type = type, data = data, timestamp = DateTime.UtcNow };

                // Send via SignalR to admin users
                await _hubContext.Clients.Group("role:admin")
                    .SendAsync("AdminNotification", payload);

                _logger.LogInformation("Admin notification sent: {Type}", type);

                return ServiceResult.CreateSuccess("Admin notification sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending admin notification: {Type}", type);
                return ServiceResult.CreateFailure($"Failed to send admin notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification to users subscribed to a topic
        /// </summary>
        public async Task<ServiceResult> SendToTopicAsync(string topic, string type, object data)
        {
            try
            {
                var payload = new { type = type, data = data, timestamp = DateTime.UtcNow };

                // Send via SignalR to topic subscribers
                await _hubContext.Clients.Group($"topic:{topic}")
                    .SendAsync("TopicNotification", payload);

                _logger.LogInformation("Topic notification sent to {Topic}: {Type}", topic, type);

                return ServiceResult.CreateSuccess($"Topic notification sent to {topic} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending topic notification to {Topic}: {Type}", topic, type);
                return ServiceResult.CreateFailure($"Failed to send topic notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Get connection statistics
        /// </summary>
        public async Task<object> GetConnectionStatsAsync()
        {
            try
            {
                // This would require access to the connection tracking
                // For now, return basic stats
                return new
                {
                    hubName = "AlertsHub",
                    endpoint = "/hubs/alerts",
                    status = "active",
                    lastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connection statistics");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Broadcast case deletion to relevant users
        /// </summary>
        public async Task<ServiceResult> BroadcastCaseDeletedAsync(int caseId, object caseData)
        {
            try
            {
                var payload = new { id = caseId, data = caseData };

                // Send via SignalR to all connected clients
                await _hubContext.Clients.All.SendAsync("caseDeleted", payload);

                // Send to admin group specifically
                await _hubContext.Clients.Group("role:admin")
                    .SendAsync("adminCaseDeleted", payload);

                _logger.LogInformation("Case deletion broadcasted for case {CaseId}", caseId);

                return ServiceResult.CreateSuccess(
                    "Case deletion broadcasted successfully",
                    new { caseId = caseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting case deletion for case {CaseId}", caseId);
                return ServiceResult.CreateFailure($"Failed to broadcast case deletion: {ex.Message}");
            }
        }
    }
}
