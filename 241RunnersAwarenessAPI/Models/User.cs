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
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;
        
        public string FullName => $"{FirstName} {LastName}";
        
        [Required]
        [MaxLength(50)]
        [RegularExpression("^(user|parent|caregiver|therapist|adoptiveparent|admin)$", 
            ErrorMessage = "Role must be one of: user, parent, caregiver, therapist, adoptiveparent, admin")]
        public string Role { get; set; } = "user";
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        [Phone]
        [MaxLength(20)]
        [RegularExpression(@"^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "Please enter a valid phone number")]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "City can only contain letters, spaces, hyphens, and apostrophes")]
        public string? City { get; set; }
        
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "State can only contain letters, spaces, hyphens, and apostrophes")]
        public string? State { get; set; }
        
        [MaxLength(20)]
        [RegularExpression(@"^[\d\-]+$", ErrorMessage = "Zip code can only contain numbers and hyphens")]
        public string? ZipCode { get; set; }
        
        [MaxLength(200)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'\.&]+$", ErrorMessage = "Organization can only contain letters, numbers, spaces, hyphens, apostrophes, periods, and ampersands")]
        public string? Organization { get; set; }
        
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Title can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? Title { get; set; }
        
        // Additional admin-specific fields
        [MaxLength(200)]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Credentials can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? Credentials { get; set; }
        
        [MaxLength(200)]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Specialization can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? Specialization { get; set; }
        
        [MaxLength(50)]
        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Years of experience can only contain numbers, spaces, plus signs, and hyphens")]
        public string? YearsOfExperience { get; set; }
        
        // Image and Media Support
        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }
        
        [MaxLength(1000)]
        public string? AdditionalImageUrls { get; set; } // JSON array of image URLs
        
        [MaxLength(1000)]
        public string? DocumentUrls { get; set; } // JSON array of document URLs
        
        public DateTime? UpdatedAt { get; set; }
    }
} 