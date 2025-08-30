using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwarenessAPI.Models
{
    public class Case
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string CaseId { get; set; } = string.Empty;
        
        public int Age { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "missing"; // missing, found, safe
        
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string State { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string ContactInfo { get; set; } = string.Empty;
        
        public DateTime DateReported { get; set; } = DateTime.UtcNow;
        
        public DateTime? DateFound { get; set; }
        
        public DateTime? LastSeen { get; set; }
        
        [MaxLength(500)]
        public string Tags { get; set; } = string.Empty; // Comma-separated tags
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public int? ReportedByUserId { get; set; }
        public User? ReportedByUser { get; set; }
    }

    public class CaseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CaseId { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Status { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public DateTime? DateFound { get; set; }
        public DateTime? LastSeen { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public bool IsActive { get; set; }
    }

    public class CreateCaseRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string CaseId { get; set; } = string.Empty;
        
        [Range(0, 120)]
        public int Age { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string State { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string ContactInfo { get; set; } = string.Empty;
        
        public DateTime? LastSeen { get; set; }
        
        [MaxLength(500)]
        public string Tags { get; set; } = string.Empty;
    }

    public class UpdateCaseRequest
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        
        [Range(0, 120)]
        public int? Age { get; set; }
        
        [MaxLength(50)]
        public string? Status { get; set; }
        
        [MaxLength(100)]
        public string? City { get; set; }
        
        [MaxLength(50)]
        public string? State { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(200)]
        public string? ContactInfo { get; set; }
        
        public DateTime? LastSeen { get; set; }
        
        [MaxLength(500)]
        public string? Tags { get; set; }
    }
} 