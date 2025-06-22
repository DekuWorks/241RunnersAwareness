using System;
using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwareness.BackendAPI.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        public string FullName { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [Required]
        public string Role { get; set; } = "user"; // user, parent, caregiver, aba_therapist, adoptive_parent, admin
        
        // Role-specific fields
        public string? RelationshipToRunner { get; set; } // For parents, caregivers, adoptive parents
        public string? LicenseNumber { get; set; } // For ABA therapists
        public string? Organization { get; set; } // For therapists, caregivers
        public string? Credentials { get; set; } // For therapists
        public string? Specialization { get; set; } // For therapists
        public string? YearsOfExperience { get; set; } // For therapists, caregivers
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        
        public bool EmailVerified { get; set; } = false;
        public bool PhoneVerified { get; set; } = false;
        
        public string EmailVerificationToken { get; set; }
        public string PhoneVerificationCode { get; set; }
        
        public DateTime? EmailVerificationExpiry { get; set; }
        public DateTime? PhoneVerificationExpiry { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        
        // Navigation properties
        public Individual? Individual { get; set; }
        public Guid? IndividualId { get; set; }
    }
} 