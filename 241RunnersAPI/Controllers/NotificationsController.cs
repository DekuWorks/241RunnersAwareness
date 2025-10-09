using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(ApplicationDbContext context, ILogger<NotificationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get notifications for the current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] NotificationQuery query)
        {
            try
            {
                // For now, return mock notifications
                // In a real implementation, you'd have a Notifications table
                var notifications = new[]
                {
                    new
                    {
                        id = "n_1",
                        type = "CASE_UPDATE",
                        title = "Case updated",
                        read = false,
                        createdAt = DateTime.UtcNow.AddHours(-2).ToString("yyyy-MM-ddTHH:mm:ssZ")
                    },
                    new
                    {
                        id = "n_2",
                        type = "ADMIN_ALERT",
                        title = "System maintenance scheduled",
                        read = true,
                        createdAt = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
                    }
                };

                var filteredNotifications = notifications.AsQueryable();

                if (query.Read.HasValue)
                {
                    filteredNotifications = filteredNotifications.Where(n => n.read == query.Read.Value);
                }

                var total = filteredNotifications.Count();
                var pagedNotifications = filteredNotifications
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToList();

                return Ok(new
                {
                    data = pagedNotifications,
                    page = query.Page,
                    pageSize = query.PageSize,
                    total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while retrieving notifications"
                    }
                });
            }
        }

        /// <summary>
        /// Create a notification (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            try
            {
                // For now, just return success
                // In a real implementation, you'd create a notification record
                _logger.LogInformation("Notification created: {Title} for user {UserId}", request.Title, request.UserId);

                return CreatedAtAction(nameof(CreateNotification), new { id = "n_new" }, new
                {
                    id = "n_new",
                    type = request.Type,
                    title = request.Title,
                    body = request.Body,
                    userId = request.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while creating notification"
                    }
                });
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            try
            {
                // For now, just return success
                // In a real implementation, you'd update the notification record
                _logger.LogInformation("Notification marked as read: {NotificationId}", id);

                return Ok(new
                {
                    id,
                    read = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while marking notification as read"
                    }
                });
            }
        }

        /// <summary>
        /// Get user's notification preferences
        /// </summary>
        [HttpGet("preferences")]
        public async Task<IActionResult> GetNotificationPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // For now, return default preferences
                // In a real implementation, you'd store these in the database
                var preferences = new
                {
                    userId,
                    emergency = true, // Always enabled
                    caseUpdates = true,
                    profileUpdates = true,
                    safetyChecks = true,
                    weatherAlerts = true,
                    system = true,
                    pushEnabled = false,
                    updatedAt = DateTime.UtcNow
                };

                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification preferences");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while retrieving notification preferences"
                    }
                });
            }
        }

        /// <summary>
        /// Update user's notification preferences
        /// </summary>
        [HttpPost("preferences")]
        public async Task<IActionResult> UpdateNotificationPreferences([FromBody] NotificationPreferencesRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // For now, just log the preferences
                // In a real implementation, you'd save these to the database
                _logger.LogInformation("Notification preferences updated for user {UserId}: {Preferences}", 
                    userId, System.Text.Json.JsonSerializer.Serialize(request));

                var preferences = new
                {
                    userId,
                    emergency = true, // Always enabled
                    caseUpdates = request.CaseUpdates,
                    profileUpdates = request.ProfileUpdates,
                    safetyChecks = request.SafetyChecks,
                    weatherAlerts = request.WeatherAlerts,
                    system = request.System,
                    pushEnabled = request.PushEnabled,
                    updatedAt = DateTime.UtcNow
                };

                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while updating notification preferences"
                    }
                });
            }
        }

        /// <summary>
        /// Subscribe to push notifications
        /// </summary>
        [HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeToPushNotifications([FromBody] PushSubscriptionRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // For now, just log the subscription
                // In a real implementation, you'd store the subscription in the database
                _logger.LogInformation("Push notification subscription created for user {UserId}: {Endpoint}", 
                    userId, request.Subscription?.Endpoint);

                return Ok(new
                {
                    userId,
                    subscribed = true,
                    subscription = request.Subscription,
                    subscribedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to push notifications");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while subscribing to push notifications"
                    }
                });
            }
        }

        /// <summary>
        /// Unsubscribe from push notifications
        /// </summary>
        [HttpPost("unsubscribe")]
        public async Task<IActionResult> UnsubscribeFromPushNotifications()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // For now, just log the unsubscription
                // In a real implementation, you'd remove the subscription from the database
                _logger.LogInformation("Push notification subscription removed for user {UserId}", userId);

                return Ok(new
                {
                    userId,
                    subscribed = false,
                    unsubscribedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from push notifications");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while unsubscribing from push notifications"
                    }
                });
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        }
    }

    public class NotificationQuery
    {
        public bool? Read { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateNotificationRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
    }

    public class NotificationPreferencesRequest
    {
        public bool CaseUpdates { get; set; } = true;
        public bool ProfileUpdates { get; set; } = true;
        public bool SafetyChecks { get; set; } = true;
        public bool WeatherAlerts { get; set; } = true;
        public bool System { get; set; } = true;
        public bool PushEnabled { get; set; } = false;
    }

    public class PushSubscriptionRequest
    {
        public object? Subscription { get; set; }
        public string? UserId { get; set; }
    }
}
