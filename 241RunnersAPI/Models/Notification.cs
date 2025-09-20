using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAPI.Models
{
    /// <summary>
    /// Notification model for tracking sent notifications
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required")]
        [MaxLength(1000, ErrorMessage = "Body cannot exceed 1000 characters")]
        public string Body { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        [RegularExpression("^(case_updated|new_case|admin_notice|urgent_notification|system_maintenance)$", 
            ErrorMessage = "Type must be one of: case_updated, new_case, admin_notice, urgent_notification, system_maintenance")]
        public string Type { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Topic cannot exceed 100 characters")]
        public string? Topic { get; set; }

        [MaxLength(2000, ErrorMessage = "Data JSON cannot exceed 2000 characters")]
        public string? DataJson { get; set; } // JSON object with additional data

        [Required]
        public bool IsSent { get; set; } = false;

        public DateTime? SentAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Notification delivery tracking
        public bool IsDelivered { get; set; } = false;
        public DateTime? DeliveredAt { get; set; }

        public bool IsOpened { get; set; } = false;
        public DateTime? OpenedAt { get; set; }

        // Error tracking
        [MaxLength(500, ErrorMessage = "Error message cannot exceed 500 characters")]
        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; } = 0;

        // Related entity tracking
        public int? RelatedCaseId { get; set; }
        public int? RelatedUserId { get; set; }

        [MaxLength(100, ErrorMessage = "Priority cannot exceed 100 characters")]
        [RegularExpression("^(low|normal|high|urgent)$", ErrorMessage = "Priority must be one of: low, normal, high, urgent")]
        public string Priority { get; set; } = "normal";

        // Expiration
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO for creating notifications
    /// </summary>
    public class CreateNotificationDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required")]
        [MaxLength(1000, ErrorMessage = "Body cannot exceed 1000 characters")]
        public string Body { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        [RegularExpression("^(case_updated|new_case|admin_notice|urgent_notification|system_maintenance)$", 
            ErrorMessage = "Type must be one of: case_updated, new_case, admin_notice, urgent_notification, system_maintenance")]
        public string Type { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Topic cannot exceed 100 characters")]
        public string? Topic { get; set; }

        public Dictionary<string, object>? Data { get; set; }

        public int? RelatedCaseId { get; set; }
        public int? RelatedUserId { get; set; }

        [MaxLength(100, ErrorMessage = "Priority cannot exceed 100 characters")]
        [RegularExpression("^(low|normal|high|urgent)$", ErrorMessage = "Priority must be one of: low, normal, high, urgent")]
        public string Priority { get; set; } = "normal";

        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO for notification response
    /// </summary>
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Topic { get; set; }
        public Dictionary<string, object>? Data { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public bool IsOpened { get; set; }
        public DateTime? OpenedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public int? RelatedCaseId { get; set; }
        public int? RelatedUserId { get; set; }
        public string Priority { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO for bulk notification creation
    /// </summary>
    public class BulkNotificationDto
    {
        [Required(ErrorMessage = "User IDs are required")]
        [MinLength(1, ErrorMessage = "At least one user ID is required")]
        public List<int> UserIds { get; set; } = new();

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required")]
        [MaxLength(1000, ErrorMessage = "Body cannot exceed 1000 characters")]
        public string Body { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        [RegularExpression("^(case_updated|new_case|admin_notice|urgent_notification|system_maintenance)$", 
            ErrorMessage = "Type must be one of: case_updated, new_case, admin_notice, urgent_notification, system_maintenance")]
        public string Type { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Topic cannot exceed 100 characters")]
        public string? Topic { get; set; }

        public Dictionary<string, object>? Data { get; set; }

        public int? RelatedCaseId { get; set; }
        public int? RelatedUserId { get; set; }

        [MaxLength(100, ErrorMessage = "Priority cannot exceed 100 characters")]
        [RegularExpression("^(low|normal|high|urgent)$", ErrorMessage = "Priority must be one of: low, normal, high, urgent")]
        public string Priority { get; set; } = "normal";

        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO for topic-based notification
    /// </summary>
    public class TopicNotificationDto
    {
        [Required(ErrorMessage = "Topic is required")]
        [MaxLength(100, ErrorMessage = "Topic cannot exceed 100 characters")]
        public string Topic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required")]
        [MaxLength(1000, ErrorMessage = "Body cannot exceed 1000 characters")]
        public string Body { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        [RegularExpression("^(case_updated|new_case|admin_notice|urgent_notification|system_maintenance)$", 
            ErrorMessage = "Type must be one of: case_updated, new_case, admin_notice, urgent_notification, system_maintenance")]
        public string Type { get; set; } = string.Empty;

        public Dictionary<string, object>? Data { get; set; }

        public int? RelatedCaseId { get; set; }
        public int? RelatedUserId { get; set; }

        [MaxLength(100, ErrorMessage = "Priority cannot exceed 100 characters")]
        [RegularExpression("^(low|normal|high|urgent)$", ErrorMessage = "Priority must be one of: low, normal, high, urgent")]
        public string Priority { get; set; } = "normal";

        public DateTime? ExpiresAt { get; set; }
    }
}
