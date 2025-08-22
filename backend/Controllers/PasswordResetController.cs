using Microsoft.AspNetCore.Mvc;
using _241RunnersAwareness.BackendAPI.Services;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordResetController : ControllerBase
    {
        private readonly IPasswordResetService _passwordResetService;

        public PasswordResetController(IPasswordResetService passwordResetService)
        {
            _passwordResetService = passwordResetService;
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            var success = await _passwordResetService.RequestPasswordResetAsync(request.Email);
            
            if (success)
            {
                return Ok(new { 
                    message = "If an account with this email exists, a password reset link has been sent.",
                    remainingResets = await _passwordResetService.GetRemainingResetsAsync(request.Email)
                });
            }
            else
            {
                return BadRequest(new { 
                    message = "Password reset limit exceeded for this year. You can reset your password up to 3 times per year.",
                    remainingResets = 0
                });
            }
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateResetToken([FromBody] ValidateTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(new { message = "Email and token are required" });
            }

            var isValid = await _passwordResetService.ValidateResetTokenAsync(request.Email, request.Token);
            
            if (isValid)
            {
                return Ok(new { message = "Token is valid" });
            }
            else
            {
                return BadRequest(new { message = "Invalid or expired token" });
            }
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || 
                string.IsNullOrEmpty(request.Token) || 
                string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { message = "Email, token, and new password are required" });
            }

            // Validate password strength
            if (request.NewPassword.Length < 8)
            {
                return BadRequest(new { message = "Password must be at least 8 characters long" });
            }

            var success = await _passwordResetService.ResetPasswordAsync(
                request.Email, 
                request.Token, 
                request.NewPassword
            );

            if (success)
            {
                return Ok(new { message = "Password has been reset successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to reset password. Please check your token and try again." });
            }
        }

        [HttpGet("remaining/{email}")]
        public async Task<IActionResult> GetRemainingResets(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            var remainingResets = await _passwordResetService.GetRemainingResetsAsync(email);
            
            return Ok(new { 
                remainingResets = remainingResets,
                maxResetsPerYear = 3
            });
        }
    }

    public class PasswordResetRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ValidateTokenRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
