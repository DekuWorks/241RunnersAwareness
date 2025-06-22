using System;
using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwareness.BackendAPI.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        public string FullName { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public string Role { get; set; } = "user"; // admin, user, etc.
        
        public bool EmailVerified { get; set; } = false;
        public bool PhoneVerified { get; set; } = false;
        
        public string EmailVerificationToken { get; set; }
        public string PhoneVerificationCode { get; set; }
        
        public DateTime? EmailVerificationExpiry { get; set; }
        public DateTime? PhoneVerificationExpiry { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public Individual? Individual { get; set; }
        public Guid? IndividualId { get; set; }
    }
} 