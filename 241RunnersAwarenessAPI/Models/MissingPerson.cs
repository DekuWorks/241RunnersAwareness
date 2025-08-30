using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwarenessAPI.Models
{
    public class MissingPerson
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string CaseId { get; set; } = string.Empty; // e.g., "RUN-2024-001"
        
        public int Age { get; set; }
        
        [MaxLength(50)]
        public string Gender { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "missing"; // missing, found, safe, urgent
        
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string State { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string ContactInfo { get; set; } = string.Empty;
        
        public DateTime DateReported { get; set; } = DateTime.UtcNow;
        
        public DateTime? DateFound { get; set; }
        
        public DateTime? LastSeen { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(500)]
        public string Tags { get; set; } = string.Empty; // Comma-separated tags
        
        public bool IsActive { get; set; } = true;
        
        public bool IsUrgent { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public int? ReportedByUserId { get; set; }
        public User? ReportedByUser { get; set; }
        
        // Computed properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        public int? CalculatedAge
        {
            get
            {
                if (DateOfBirth.HasValue)
                {
                    var today = DateTime.Today;
                    var age = today.Year - DateOfBirth.Value.Year;
                    if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
                    return age;
                }
                return Age > 0 ? Age : null;
            }
        }
    }

    public class MissingPersonDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string CaseId { get; set; } = string.Empty;
        public int Age { get; set; }
        public int? CalculatedAge { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public DateTime? DateFound { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public bool IsUrgent { get; set; }
        public string ReportedBy { get; set; } = string.Empty;
    }

    public class CreateMissingPersonRequest
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Range(0, 120)]
        public int Age { get; set; }
        
        [MaxLength(50)]
        public string Gender { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string State { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string ContactInfo { get; set; } = string.Empty;
        
        public DateTime? LastSeen { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(500)]
        public string Tags { get; set; } = string.Empty;
        
        public bool IsUrgent { get; set; } = false;
    }

    public class UpdateMissingPersonRequest
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }
        
        [MaxLength(100)]
        public string? LastName { get; set; }
        
        [Range(0, 120)]
        public int? Age { get; set; }
        
        [MaxLength(50)]
        public string? Gender { get; set; }
        
        [MaxLength(50)]
        public string? Status { get; set; }
        
        [MaxLength(100)]
        public string? City { get; set; }
        
        [MaxLength(50)]
        public string? State { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(200)]
        public string? ContactInfo { get; set; }
        
        public DateTime? LastSeen { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(500)]
        public string? Tags { get; set; }
        
        public bool? IsUrgent { get; set; }
    }
} 