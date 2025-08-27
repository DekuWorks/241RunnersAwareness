using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using _241RunnersAwareness.BackendAPI.DBContext.Models;

namespace _241RunnersAwareness.BackendAPI.DBContext.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        
        [StringLength(100)]
        public string? FullName { get; set; } // Keep for backward compatibility
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Role { get; set; }
        
        public bool EmailVerified { get; set; } = false;
        
        public bool PhoneVerified { get; set; } = false;
        
        // Email verification fields
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }
        
        // Phone verification fields
        public string? PhoneVerificationCode { get; set; }
        public DateTime? PhoneVerificationExpiry { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        // Password reset tracking
        public int PasswordResetCount { get; set; } = 0;
        
        public DateTime? LastPasswordResetAt { get; set; }
        
        public int PasswordResetYear { get; set; } = DateTime.UtcNow.Year;
        
        // Password reset token
        public string? PasswordResetToken { get; set; }
        
        public DateTime? PasswordResetTokenExpiry { get; set; }
        
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
        
        public bool IsActive { get; set; } = true;
        
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        
        // 2FA fields
        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecret { get; set; }
        public string? TwoFactorBackupCodes { get; set; } // JSON array of backup codes
        public DateTime? TwoFactorSetupDate { get; set; }
        
        // Navigation properties
        public Individual? Individual { get; set; }
        public int? IndividualId { get; set; }
    }
} 