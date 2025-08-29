using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwarenessAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public string FullName => $"{FirstName} {LastName}";
        
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "user";
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(100)]
        public string? City { get; set; }
        
        [MaxLength(50)]
        public string? State { get; set; }
        
        [MaxLength(20)]
        public string? ZipCode { get; set; }
        
        [MaxLength(200)]
        public string? Organization { get; set; }
        
        [MaxLength(100)]
        public string? Title { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
} 