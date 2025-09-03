using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwarenessAPI.Models
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [RegularExpression("^(user|parent|caregiver|therapist|adoptiveparent|admin)$", 
            ErrorMessage = "Role must be one of: user, parent, caregiver, therapist, adoptiveparent, admin")]
        public string Role { get; set; } = "user";
        
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
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Specialization can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? Specialization { get; set; }
        
        [MaxLength(50)]
        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Years of experience can only contain numbers, spaces, plus signs, and hyphens")]
        public string? YearsOfExperience { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Organization { get; set; }
        public string? Title { get; set; }
        // Additional admin-specific fields
        public string? Credentials { get; set; }
        public string? Specialization { get; set; }
        public string? YearsOfExperience { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UserUpdateRequest
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        
        [Required]
        [RegularExpression("^(user|parent|caregiver|therapist|adoptiveparent|admin)$", 
            ErrorMessage = "Role must be one of: user, parent, caregiver, therapist, adoptiveparent, admin")]
        public string Role { get; set; } = "user";
    }

    public class ProfileUpdateRequest
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;
        
        [Phone]
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
    }

    public class PasswordChangeRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ProfileUpdateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    public class PasswordResetRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class AdminPasswordResetRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
} 