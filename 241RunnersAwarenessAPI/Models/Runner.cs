using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAwarenessAPI.Models
{
    public class Runner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RunnerId { get; set; } = string.Empty; // NamUs Case Number

        [Required]
        public int Age { get; set; }

        [Required]
        [MaxLength(50)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "missing";

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ContactInfo { get; set; } = string.Empty;

        [Required]
        public DateTime DateReported { get; set; }

        public DateTime? DateFound { get; set; }

        public DateTime? LastSeen { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        [MaxLength(500)]
        public string Tags { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsUrgent { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string Height { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Weight { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string HairColor { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string EyeColor { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string IdentifyingMarks { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string MedicalConditions { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Medications { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Allergies { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string EmergencyContacts { get; set; } = string.Empty;

        public int? ReportedByUserId { get; set; }

        [ForeignKey("ReportedByUserId")]
        public User? ReportedByUser { get; set; }

        // Computed properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public string CurrentStatus => Status;

        [NotMapped]
        public DateTime DateAdded => DateReported;
    }

    // DTO for API responses
    public class RunnerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RunnerId { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public DateTime? DateFound { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsUrgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Height { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string HairColor { get; set; } = string.Empty;
        public string EyeColor { get; set; } = string.Empty;
        public string IdentifyingMarks { get; set; } = string.Empty;
    }
} 