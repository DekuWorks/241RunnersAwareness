using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAPI.Models
{
    /// <summary>
    /// Topic subscription model for push notifications
    /// </summary>
    public class TopicSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Topic is required")]
        [MaxLength(100, ErrorMessage = "Topic cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Topic can only contain letters, numbers, and underscores")]
        public string Topic { get; set; } = string.Empty;

        [Required]
        public bool IsSubscribed { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Subscription metadata
        [MaxLength(200, ErrorMessage = "Subscription reason cannot exceed 200 characters")]
        public string? SubscriptionReason { get; set; } // e.g., "auto_subscribed", "user_requested", "case_follow"

        public DateTime? LastNotificationSent { get; set; }

        public int NotificationCount { get; set; } = 0;
    }

    /// <summary>
    /// DTO for topic subscription request
    /// </summary>
    public class TopicSubscriptionDto
    {
        [Required(ErrorMessage = "Topic is required")]
        [MaxLength(100, ErrorMessage = "Topic cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Topic can only contain letters, numbers, and underscores")]
        public string Topic { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Subscription reason cannot exceed 200 characters")]
        public string? SubscriptionReason { get; set; }
    }

    /// <summary>
    /// DTO for topic subscription response
    /// </summary>
    public class TopicSubscriptionResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public bool IsSubscribed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? SubscriptionReason { get; set; }
        public DateTime? LastNotificationSent { get; set; }
        public int NotificationCount { get; set; }
    }

    /// <summary>
    /// DTO for bulk topic subscription
    /// </summary>
    public class BulkTopicSubscriptionDto
    {
        [Required(ErrorMessage = "Topics are required")]
        [MinLength(1, ErrorMessage = "At least one topic is required")]
        public List<string> Topics { get; set; } = new();

        [MaxLength(200, ErrorMessage = "Subscription reason cannot exceed 200 characters")]
        public string? SubscriptionReason { get; set; }
    }

    /// <summary>
    /// DTO for topic subscription status
    /// </summary>
    public class TopicSubscriptionStatusDto
    {
        public string Topic { get; set; } = string.Empty;
        public bool IsSubscribed { get; set; }
        public DateTime? SubscribedAt { get; set; }
        public int NotificationCount { get; set; }
        public DateTime? LastNotificationSent { get; set; }
    }

    /// <summary>
    /// Predefined topic constants
    /// </summary>
    public static class Topics
    {
        // Global topics
        public const string OrgAll = "org_all";
        public const string OrgSystem = "org_system";

        // Role-based topics
        public const string RoleAdmin = "role_admin";
        public const string RoleParent = "role_parent";
        public const string RoleModerator = "role_moderator";

        // Case-specific topics
        public static string GetCaseTopic(int caseId) => $"case_{caseId}";

        // Geographic topics
        public const string RegionTxHouston = "region_tx_houston";
        public const string RegionTxDallas = "region_tx_dallas";

        // Priority topics
        public const string PriorityHigh = "priority_high";
        public const string PriorityCritical = "priority_critical";

        /// <summary>
        /// Get all predefined topics
        /// </summary>
        public static List<string> GetAllPredefinedTopics()
        {
            return new List<string>
            {
                OrgAll,
                OrgSystem,
                RoleAdmin,
                RoleParent,
                RoleModerator,
                RegionTxHouston,
                RegionTxDallas,
                PriorityHigh,
                PriorityCritical
            };
        }

        /// <summary>
        /// Get role-based topics for a user role
        /// </summary>
        public static List<string> GetRoleBasedTopics(string role)
        {
            return role.ToLower() switch
            {
                "admin" => new List<string> { RoleAdmin },
                "parent" => new List<string> { RoleParent },
                "moderator" => new List<string> { RoleModerator },
                _ => new List<string>()
            };
        }

        /// <summary>
        /// Get default topics for a user
        /// </summary>
        public static List<string> GetDefaultTopics(string role)
        {
            var topics = new List<string> { OrgAll, OrgSystem };
            topics.AddRange(GetRoleBasedTopics(role));
            return topics;
        }
    }
}
