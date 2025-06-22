using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwareness.BackendAPI.Models
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        public string FullName { get; set; }
        
        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        // Role and role-specific fields
        [Required]
        public string Role { get; set; } = "user"; // user, parent, caregiver, aba_therapist, adoptive_parent
        
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
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
    
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; }
    }
    
    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; }
    }
    
    public class VerifyPhoneRequest
    {
        [Required]
        public string Code { get; set; }
    }
    
    public class ResendVerificationRequest
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Type { get; set; } // "email" or "phone"
    }
    
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public UserDto User { get; set; }
        public bool RequiresVerification { get; set; }
    }
    
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
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
} 