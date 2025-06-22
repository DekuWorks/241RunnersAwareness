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
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 