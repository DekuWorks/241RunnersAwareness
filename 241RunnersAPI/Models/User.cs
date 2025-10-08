using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAPI.Models
{
    /// <summary>
    /// User model representing all user types in the system
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Password hash cannot exceed 255 characters")]
        public string? PasswordHash { get; set; } // Nullable for OAuth users

        // OAuth Provider Information
        [MaxLength(50, ErrorMessage = "Auth provider cannot exceed 50 characters")]
        public string? AuthProvider { get; set; } // "email", "google", "apple", etc.

        [MaxLength(255, ErrorMessage = "Provider user ID cannot exceed 255 characters")]
        public string? ProviderUserId { get; set; } // External provider's user ID

        [MaxLength(500, ErrorMessage = "Provider access token cannot exceed 500 characters")]
        public string? ProviderAccessToken { get; set; } // Encrypted access token

        [MaxLength(500, ErrorMessage = "Provider refresh token cannot exceed 500 characters")]
        public string? ProviderRefreshToken { get; set; } // Encrypted refresh token

        public DateTime? ProviderTokenExpires { get; set; } // When the provider token expires

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required(ErrorMessage = "Role is required")]
        [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
        [RegularExpression("^(user|parent|caregiver|therapist|adoptiveparent|admin)$", 
            ErrorMessage = "Role must be one of: user, parent, caregiver, therapist, adoptiveparent, admin")]
        public string Role { get; set; } = "user";

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Contact Information
        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "Please enter a valid phone number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "City can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? City { get; set; }

        [MaxLength(50, ErrorMessage = "State cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "State can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? State { get; set; }

        [MaxLength(20, ErrorMessage = "Zip code cannot exceed 20 characters")]
        [RegularExpression(@"^[\d\-]+$", ErrorMessage = "Zip code can only contain numbers and hyphens")]
        public string? ZipCode { get; set; }

        // Professional Information
        [MaxLength(200, ErrorMessage = "Organization cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'\.&()]+$", ErrorMessage = "Organization can only contain letters, numbers, spaces, hyphens, apostrophes, periods, ampersands, and parentheses")]
        public string? Organization { get; set; }

        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Title can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? Title { get; set; }

        // Professional Credentials (for therapists, caregivers, etc.)
        [MaxLength(200, ErrorMessage = "Credentials cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'.,&()]+$", ErrorMessage = "Credentials can only contain letters, numbers, spaces, hyphens, apostrophes, commas, periods, ampersands, and parentheses")]
        public string? Credentials { get; set; }

        [MaxLength(200, ErrorMessage = "Specialization cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'.,&()]+$", ErrorMessage = "Specialization can only contain letters, numbers, spaces, hyphens, apostrophes, commas, periods, ampersands, and parentheses")]
        public string? Specialization { get; set; }

        [MaxLength(50, ErrorMessage = "Years of experience cannot exceed 50 characters")]
        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Years of experience can only contain numbers, spaces, plus signs, and hyphens")]
        public string? YearsOfExperience { get; set; }

        // Profile Information
        [MaxLength(500, ErrorMessage = "Profile image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Profile image URL must be a valid URL")]
        public string? ProfileImageUrl { get; set; }

        [MaxLength(1000, ErrorMessage = "Additional image URLs cannot exceed 1000 characters")]
        public string? AdditionalImageUrls { get; set; } // JSON array of image URLs

        [MaxLength(1000, ErrorMessage = "Document URLs cannot exceed 1000 characters")]
        public string? DocumentUrls { get; set; } // JSON array of document URLs

        // Emergency Contact Information
        [MaxLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Emergency contact name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Invalid emergency contact phone number format")]
        [MaxLength(20, ErrorMessage = "Emergency contact phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "Please enter a valid emergency contact phone number")]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(100, ErrorMessage = "Emergency contact relationship cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Emergency contact relationship can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? EmergencyContactRelationship { get; set; }

        // Additional Notes
        [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
        public string? Notes { get; set; }

        // Verification Status
        public bool IsEmailVerified { get; set; } = false;
        public bool IsPhoneVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? PhoneVerifiedAt { get; set; }

        // Security
        public string? EmailVerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockedUntil { get; set; }
    }

    /// <summary>
    /// DTO for user registration
    /// </summary>
    public class UserRegistrationDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string? Password { get; set; } // Optional for OAuth users

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; } // Optional for OAuth users

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(user|parent|caregiver|therapist|adoptiveparent|admin)$", 
            ErrorMessage = "Role must be one of: user, parent, caregiver, therapist, adoptiveparent, admin")]
        public string Role { get; set; } = "user";

        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "Please enter a valid phone number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "City can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? City { get; set; }

        [MaxLength(50, ErrorMessage = "State cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "State can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? State { get; set; }

        [MaxLength(20, ErrorMessage = "Zip code cannot exceed 20 characters")]
        [RegularExpression(@"^[\d\-]+$", ErrorMessage = "Zip code can only contain numbers and hyphens")]
        public string? ZipCode { get; set; }

        [MaxLength(200, ErrorMessage = "Organization cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'\.&()]+$", ErrorMessage = "Organization can only contain letters, numbers, spaces, hyphens, apostrophes, periods, ampersands, and parentheses")]
        public string? Organization { get; set; }

        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Title can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? Title { get; set; }

        [MaxLength(200, ErrorMessage = "Credentials cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'.,&()]+$", ErrorMessage = "Credentials can only contain letters, numbers, spaces, hyphens, apostrophes, commas, periods, ampersands, and parentheses")]
        public string? Credentials { get; set; }

        [MaxLength(200, ErrorMessage = "Specialization cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'.,&()]+$", ErrorMessage = "Specialization can only contain letters, numbers, spaces, hyphens, apostrophes, commas, periods, ampersands, and parentheses")]
        public string? Specialization { get; set; }

        [MaxLength(50, ErrorMessage = "Years of experience cannot exceed 50 characters")]
        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Years of experience can only contain numbers, spaces, plus signs, and hyphens")]
        public string? YearsOfExperience { get; set; }

        [MaxLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Emergency contact name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Invalid emergency contact phone number format")]
        [MaxLength(20, ErrorMessage = "Emergency contact phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "Please enter a valid emergency contact phone number")]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(100, ErrorMessage = "Emergency contact relationship cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Emergency contact relationship can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? EmergencyContactRelationship { get; set; }
    }

    /// <summary>
    /// DTO for user login
    /// </summary>
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for user profile update
    /// </summary>
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "Please enter a valid phone number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "City can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? City { get; set; }

        [MaxLength(50, ErrorMessage = "State cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "State can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? State { get; set; }

        [MaxLength(20, ErrorMessage = "Zip code cannot exceed 20 characters")]
        [RegularExpression(@"^[\d\-]+$", ErrorMessage = "Zip code can only contain numbers and hyphens")]
        public string? ZipCode { get; set; }

        [MaxLength(200, ErrorMessage = "Organization cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'\.&()]+$", ErrorMessage = "Organization can only contain letters, numbers, spaces, hyphens, apostrophes, periods, ampersands, and parentheses")]
        public string? Organization { get; set; }

        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Title can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? Title { get; set; }

        [MaxLength(200, ErrorMessage = "Credentials cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'.,&()]+$", ErrorMessage = "Credentials can only contain letters, numbers, spaces, hyphens, apostrophes, commas, periods, ampersands, and parentheses")]
        public string? Credentials { get; set; }

        [MaxLength(200, ErrorMessage = "Specialization cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'.,&()]+$", ErrorMessage = "Specialization can only contain letters, numbers, spaces, hyphens, apostrophes, commas, periods, ampersands, and parentheses")]
        public string? Specialization { get; set; }

        [MaxLength(50, ErrorMessage = "Years of experience cannot exceed 50 characters")]
        [RegularExpression(@"^[\d\s\+\-]+$", ErrorMessage = "Years of experience can only contain numbers, spaces, plus signs, and hyphens")]
        public string? YearsOfExperience { get; set; }

        [MaxLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Emergency contact name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Invalid emergency contact phone number format")]
        [MaxLength(20, ErrorMessage = "Emergency contact phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "Please enter a valid emergency contact phone number")]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(100, ErrorMessage = "Emergency contact relationship cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Emergency contact relationship can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? EmergencyContactRelationship { get; set; }

        [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for password change
    /// </summary>
    public class PasswordChangeDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long")]
        [MaxLength(100, ErrorMessage = "New password cannot exceed 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm new password is required")]
        [Compare("NewPassword", ErrorMessage = "New passwords do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for user response (without sensitive data)
    /// </summary>
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Organization { get; set; }
        public string? Title { get; set; }
        public string? Credentials { get; set; }
        public string? Specialization { get; set; }
        public string? YearsOfExperience { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? PhoneVerifiedAt { get; set; }
    }

    /// <summary>
    /// DTO for authentication response
    /// </summary>
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserResponseDto? User { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO for password reset (admin only)
    /// </summary>
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long")]
        [MaxLength(100, ErrorMessage = "New password cannot exceed 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for OAuth authentication (Google, Apple, etc.)
    /// </summary>
    public class OAuthLoginDto
    {
        [Required(ErrorMessage = "Provider is required")]
        [MaxLength(50, ErrorMessage = "Provider cannot exceed 50 characters")]
        [RegularExpression("^(google|apple|microsoft)$", ErrorMessage = "Provider must be one of: google, apple, microsoft")]
        public string Provider { get; set; } = string.Empty;

        [Required(ErrorMessage = "Access token is required")]
        [MaxLength(1000, ErrorMessage = "Access token cannot exceed 1000 characters")]
        public string AccessToken { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "ID token cannot exceed 1000 characters")]
        public string? IdToken { get; set; }

        [MaxLength(1000, ErrorMessage = "Refresh token cannot exceed 1000 characters")]
        public string? RefreshToken { get; set; }

        [MaxLength(255, ErrorMessage = "Provider user ID cannot exceed 255 characters")]
        public string? ProviderUserId { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }

        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string? FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string? LastName { get; set; }

        [MaxLength(500, ErrorMessage = "Profile image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Profile image URL must be a valid URL")]
        public string? ProfileImageUrl { get; set; }
    }

    /// <summary>
    /// DTO for OAuth user registration
    /// </summary>
    public class OAuthRegistrationDto
    {
        [Required(ErrorMessage = "Provider is required")]
        [MaxLength(50, ErrorMessage = "Provider cannot exceed 50 characters")]
        [RegularExpression("^(google|apple|microsoft)$", ErrorMessage = "Provider must be one of: google, apple, microsoft")]
        public string Provider { get; set; } = string.Empty;

        [Required(ErrorMessage = "Provider user ID is required")]
        [MaxLength(255, ErrorMessage = "Provider user ID cannot exceed 255 characters")]
        public string ProviderUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(user|parent|caregiver|therapist|adoptiveparent|admin)$", 
            ErrorMessage = "Role must be one of: user, parent, caregiver, therapist, adoptiveparent, admin")]
        public string Role { get; set; } = "user";

        [MaxLength(500, ErrorMessage = "Profile image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Profile image URL must be a valid URL")]
        public string? ProfileImageUrl { get; set; }

        [MaxLength(1000, ErrorMessage = "Access token cannot exceed 1000 characters")]
        public string? AccessToken { get; set; }

        [MaxLength(1000, ErrorMessage = "Refresh token cannot exceed 1000 characters")]
        public string? RefreshToken { get; set; }

        public DateTime? TokenExpires { get; set; }
    }
}
