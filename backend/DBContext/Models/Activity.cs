using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAwareness.BackendAPI.DBContext.Models
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }

        public int IndividualId { get; set; }

        [Required]
        [StringLength(50)]
        public string ActivityType { get; set; } // "CaseOpened", "CaseUpdated", "PhotoAdded", "PhotoPrimaryChanged", "ProfileUpdated", "LastSeenUpdated"

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Metadata for different activity types
        [StringLength(500)]
        public string? Metadata { get; set; } // JSON string for additional data

        // Related entity IDs
        public int? RelatedCaseId { get; set; }

        public int? RelatedPhotoId { get; set; }

        // Navigation properties
        public virtual Individual Individual { get; set; }

        public virtual Case? RelatedCase { get; set; }

        public virtual Photo? RelatedPhoto { get; set; }
    }
}
