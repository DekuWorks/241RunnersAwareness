using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAwareness.BackendAPI.DBContext.Models
{
    /// <summary>
    /// Represents a missing person case with enhanced functionality
    /// Extends the Individual model with case-specific features
    /// </summary>
    public class Case
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique identifier for the case
        /// </summary>
        [Required]
        [StringLength(50)]
        public string CaseNumber { get; set; }

        /// <summary>
        /// Public slug for sharing cases
        /// </summary>
        [Required]
        [StringLength(100)]
        public string PublicSlug { get; set; }

        /// <summary>
        /// Reference to the individual this case is about
        /// </summary>
        [Required]
        public int IndividualId { get; set; }

        /// <summary>
        /// User who created/owns this case
        /// </summary>
        [Required]
        public Guid OwnerUserId { get; set; }

        /// <summary>
        /// Whether this case is publicly viewable
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Current status of the case
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // "Active", "Resolved", "Closed", "Suspended"

        /// <summary>
        /// Priority level of the case
        /// </summary>
        [StringLength(20)]
        public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High", "Critical"

        /// <summary>
        /// Category of the case
        /// </summary>
        [StringLength(50)]
        public string Category { get; set; } = "Missing Person"; // "Missing Person", "Wandering", "Medical Emergency", "Abduction"

        /// <summary>
        /// Brief title/description of the case
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Detailed description of the case
        /// </summary>
        [StringLength(2000)]
        public string Description { get; set; }

        /// <summary>
        /// Location where the person was last seen
        /// </summary>
        [StringLength(500)]
        public string LastSeenLocation { get; set; }

        /// <summary>
        /// Date and time when the person was last seen
        /// </summary>
        public DateTime? LastSeenDate { get; set; }

        /// <summary>
        /// Circumstances surrounding the disappearance
        /// </summary>
        [StringLength(1000)]
        public string Circumstances { get; set; }

        /// <summary>
        /// Risk assessment for the case
        /// </summary>
        [StringLength(50)]
        public string RiskLevel { get; set; } = "Medium"; // "Low", "Medium", "High", "Critical"

        /// <summary>
        /// Whether this case requires immediate attention
        /// </summary>
        public bool IsUrgent { get; set; } = false;

        /// <summary>
        /// Whether this case is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the case was resolved (if applicable)
        /// </summary>
        public DateTime? ResolvedDate { get; set; }

        /// <summary>
        /// How the case was resolved
        /// </summary>
        [StringLength(500)]
        public string ResolutionNotes { get; set; }

        /// <summary>
        /// Law enforcement case number
        /// </summary>
        [StringLength(100)]
        public string LawEnforcementCaseNumber { get; set; }

        /// <summary>
        /// Investigating agency
        /// </summary>
        [StringLength(200)]
        public string InvestigatingAgency { get; set; }

        /// <summary>
        /// Name of the primary investigator
        /// </summary>
        [StringLength(100)]
        public string InvestigatorName { get; set; }

        /// <summary>
        /// Contact information for the investigator
        /// </summary>
        [StringLength(100)]
        public string InvestigatorContact { get; set; }

        /// <summary>
        /// Tags for categorizing and searching cases
        /// </summary>
        [StringLength(500)]
        public string Tags { get; set; }

        /// <summary>
        /// Geographic coordinates for mapping
        /// </summary>
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>
        /// Search radius in miles
        /// </summary>
        public int? SearchRadiusMiles { get; set; } = 10;

        /// <summary>
        /// Whether to enable real-time alerts for this case
        /// </summary>
        public bool EnableAlerts { get; set; } = true;

        /// <summary>
        /// Whether to enable public sharing for this case
        /// </summary>
        public bool EnablePublicSharing { get; set; } = false;

        /// <summary>
        /// Whether to enable media outreach for this case
        /// </summary>
        public bool EnableMediaOutreach { get; set; } = false;

        /// <summary>
        /// Social media handles to monitor
        /// </summary>
        [StringLength(500)]
        public string SocialMediaHandles { get; set; }

        /// <summary>
        /// Media contacts for outreach
        /// </summary>
        [StringLength(1000)]
        public string MediaContacts { get; set; }

        /// <summary>
        /// System timestamps
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who created the case
        /// </summary>
        [StringLength(100)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// User who last updated the case
        /// </summary>
        [StringLength(100)]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Navigation properties
        /// </summary>
        public virtual Individual Individual { get; set; }
        public virtual User OwnerUser { get; set; }
        public virtual ICollection<CaseUpdate> Updates { get; set; } = new List<CaseUpdate>();

        /// <summary>
        /// Computed properties
        /// </summary>
        [NotMapped]
        public bool IsResolved => Status == "Resolved" || Status == "Closed";

        [NotMapped]
        public bool IsCritical => RiskLevel == "Critical" || IsUrgent;

        [NotMapped]
        public bool IsPubliclyShareable => IsPublic && EnablePublicSharing;

        [NotMapped]
        public TimeSpan? TimeSinceLastSeen => LastSeenDate.HasValue ? DateTime.UtcNow - LastSeenDate.Value : null;

        [NotMapped]
        public string FullTitle => $"{Title} - {Individual?.FullName}";
    }

    /// <summary>
    /// Represents updates and notes for a case
    /// </summary>
    public class CaseUpdate
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Reference to the case this update belongs to
        /// </summary>
        [Required]
        public int CaseId { get; set; }

        /// <summary>
        /// User who created this update
        /// </summary>
        [Required]
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Type of update
        /// </summary>
        [Required]
        [StringLength(50)]
        public string UpdateType { get; set; } // "Status Change", "Sighting", "Investigation Update", "Media Update", "Public Update", "Internal Note"

        /// <summary>
        /// Title of the update
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Detailed content of the update
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Content { get; set; }

        /// <summary>
        /// Whether this update is public
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Whether this update is urgent
        /// </summary>
        public bool IsUrgent { get; set; } = false;

        /// <summary>
        /// Location associated with this update
        /// </summary>
        [StringLength(500)]
        public string Location { get; set; }

        /// <summary>
        /// Geographic coordinates
        /// </summary>
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>
        /// Date and time of the update
        /// </summary>
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// Whether this update requires notification
        /// </summary>
        public bool RequiresNotification { get; set; } = false;

        /// <summary>
        /// Whether notifications have been sent
        /// </summary>
        public bool NotificationsSent { get; set; } = false;

        /// <summary>
        /// System timestamps
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who created the update
        /// </summary>
        [StringLength(100)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// User who last updated the update
        /// </summary>
        [StringLength(100)]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Navigation properties
        /// </summary>
        public virtual Case Case { get; set; }
        public virtual User CreatedByUser { get; set; }
        public virtual ICollection<CaseUpdateMedia> Media { get; set; } = new List<CaseUpdateMedia>();

        /// <summary>
        /// Computed properties
        /// </summary>
        [NotMapped]
        public bool IsRecent => CreatedAt > DateTime.UtcNow.AddDays(7);

        [NotMapped]
        public bool IsLocationBased => !string.IsNullOrEmpty(Location) || (Latitude.HasValue && Longitude.HasValue);
    }

    /// <summary>
    /// Represents media attachments for case updates
    /// </summary>
    public class CaseUpdateMedia
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Reference to the case update
        /// </summary>
        [Required]
        public int CaseUpdateId { get; set; }

        /// <summary>
        /// Type of media
        /// </summary>
        [Required]
        [StringLength(50)]
        public string MediaType { get; set; } // "Image", "Video", "Document", "Audio"

        /// <summary>
        /// URL or path to the media file
        /// </summary>
        [Required]
        [StringLength(500)]
        public string MediaUrl { get; set; }

        /// <summary>
        /// Original filename
        /// </summary>
        [StringLength(200)]
        public string OriginalFilename { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// MIME type of the file
        /// </summary>
        [StringLength(100)]
        public string MimeType { get; set; }

        /// <summary>
        /// Description or caption for the media
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Whether this media is public
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// System timestamps
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who uploaded the media
        /// </summary>
        [StringLength(100)]
        public string UploadedBy { get; set; }

        /// <summary>
        /// Navigation properties
        /// </summary>
        public virtual CaseUpdate CaseUpdate { get; set; }

        /// <summary>
        /// Computed properties
        /// </summary>
        [NotMapped]
        public string FileSizeFormatted => FileSize.HasValue ? FormatFileSize(FileSize.Value) : "Unknown";

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }
}
