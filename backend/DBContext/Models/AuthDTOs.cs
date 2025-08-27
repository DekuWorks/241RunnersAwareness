using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwareness.BackendAPI.Models
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string? PhoneNumber { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        
        public string? FullName { get; set; } // Optional, will be auto-generated if not provided
        
        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        // Role and role-specific fields
        [Required]
        public string Role { get; set; } = "user"; // admin, user, therapist, caregiver, parent, adoptive_parent
        
        // Common fields
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        
        // Role-specific fields
        public string? RelationshipToRunner { get; set; } // For parents, caregivers, adoptive parents
        public string? LicenseNumber { get; set; } // For ABA therapists
        public string? Organization { get; set; } // For therapists, caregivers
        public string? Credentials { get; set; } // For therapists
        public string? Specialization { get; set; } // For therapists
        public string? YearsOfExperience { get; set; } // For therapists, caregivers
        
        // Optional individual information
        public IndividualDto? Individual { get; set; }
    }
    
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
    
    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
    
    public class VerifyPhoneRequest
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }
    
    public class ResendVerificationRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Type { get; set; } = string.Empty; // "email" or "phone"
    }
    
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
        public bool RequiresVerification { get; set; }
    }
    
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty; // Computed field
        public string Role { get; set; } = string.Empty;
        public string? RelationshipToRunner { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Organization { get; set; }
        public string? Credentials { get; set; }
        public string? Specialization { get; set; }
        public string? YearsOfExperience { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Supporting DTOs
    public class IndividualDto
    {
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public EmergencyContactDto EmergencyContact { get; set; }
    }

    public class EmergencyContactDto
    {
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    // 2FA DTOs
    public class SetupTwoFactorRequest
    {
        [Required]
        public string Email { get; set; }
    }

    public class SetupTwoFactorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string QrCodeUrl { get; set; }
        public string Secret { get; set; }
        public List<string> BackupCodes { get; set; }
    }

    public class VerifyTwoFactorRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Totp { get; set; }
    }

    public class VerifyTwoFactorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public UserDto User { get; set; }
    }

    public class DisableTwoFactorRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Totp { get; set; }
    }

    public class BackupCodeRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string BackupCode { get; set; }
    }

    // Password Reset DTOs
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; }
        
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }
        
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }

    // Phone Number Update DTO
    public class UpdatePhoneRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
} 