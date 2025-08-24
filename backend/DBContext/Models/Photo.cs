using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAwareness.BackendAPI.DBContext.Models
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }

        public int IndividualId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        [StringLength(200)]
        public string? Caption { get; set; }

        [StringLength(100)]
        public string? ImageType { get; set; } // "Profile", "Identification", "Recent", "Medical", "Other"

        public bool IsPrimary { get; set; } = false;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? UploadedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        // File metadata
        [StringLength(100)]
        public string? FileName { get; set; }

        public long? FileSize { get; set; }

        [StringLength(50)]
        public string? ContentType { get; set; }

        // Navigation property
        public virtual Individual Individual { get; set; }
    }
}
