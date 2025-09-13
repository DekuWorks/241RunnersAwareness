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
        [Authorize(Roles = "Admin")]
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
}
