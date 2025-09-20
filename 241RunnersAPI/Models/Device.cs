using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAPI.Models
{
    /// <summary>
    /// Device model for push notification registration
    /// </summary>
    public class Device
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Platform is required")]
        [MaxLength(10, ErrorMessage = "Platform cannot exceed 10 characters")]
        [RegularExpression("^(ios|android)$", ErrorMessage = "Platform must be 'ios' or 'android'")]
        public string Platform { get; set; } = string.Empty;

        [Required(ErrorMessage = "FCM token is required")]
        [MaxLength(500, ErrorMessage = "FCM token cannot exceed 500 characters")]
        public string FcmToken { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "App version cannot exceed 20 characters")]
        public string? AppVersion { get; set; }

        [Required]
        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

        [MaxLength(2000, ErrorMessage = "Topics JSON cannot exceed 2000 characters")]
        public string? TopicsJson { get; set; } // JSON array of subscribed topics

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Device information
        [MaxLength(100, ErrorMessage = "Device model cannot exceed 100 characters")]
        public string? DeviceModel { get; set; }

        [MaxLength(50, ErrorMessage = "OS version cannot exceed 50 characters")]
        public string? OsVersion { get; set; }

        [MaxLength(100, ErrorMessage = "App build number cannot exceed 100 characters")]
        public string? AppBuildNumber { get; set; }
    }

    /// <summary>
    /// DTO for device registration
    /// </summary>
    public class DeviceRegistrationDto
    {
        [Required(ErrorMessage = "Platform is required")]
        [RegularExpression("^(ios|android)$", ErrorMessage = "Platform must be 'ios' or 'android'")]
        public string Platform { get; set; } = string.Empty;

        [Required(ErrorMessage = "FCM token is required")]
        [MaxLength(500, ErrorMessage = "FCM token cannot exceed 500 characters")]
        public string FcmToken { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "App version cannot exceed 20 characters")]
        public string? AppVersion { get; set; }

        [MaxLength(100, ErrorMessage = "Device model cannot exceed 100 characters")]
        public string? DeviceModel { get; set; }

        [MaxLength(50, ErrorMessage = "OS version cannot exceed 50 characters")]
        public string? OsVersion { get; set; }

        [MaxLength(100, ErrorMessage = "App build number cannot exceed 100 characters")]
        public string? AppBuildNumber { get; set; }
    }

    /// <summary>
    /// DTO for device response
    /// </summary>
    public class DeviceResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string? AppVersion { get; set; }
        public DateTime LastSeenAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? DeviceModel { get; set; }
        public string? OsVersion { get; set; }
        public string? AppBuildNumber { get; set; }
        public List<string> Topics { get; set; } = new();
    }
}
