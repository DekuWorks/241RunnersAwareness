using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;

namespace _241RunnersAwareness.BackendAPI.Services
{
    public interface IPasswordResetService
    {
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ValidateResetTokenAsync(string email, string token);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<int> GetRemainingResetsAsync(string email);
    }

    public class PasswordResetService : IPasswordResetService
    {
        private readonly RunnersDbContext _context;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private const int MAX_RESETS_PER_YEAR = 3;
        private const int TOKEN_EXPIRY_HOURS = 24;

        public PasswordResetService(
            RunnersDbContext context, 
            IAuthService authService, 
            IEmailService emailService)
        {
            _context = context;
            _authService = authService;
            _emailService = emailService;
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    // Don't reveal if user exists or not for security
                    return true;
                }

                // Check if user has exceeded reset limit for the year
                if (!CanResetPassword(user))
                {
                    return false;
                }

                // Generate reset token
                var token = GenerateResetToken();
                var expiry = DateTime.UtcNow.AddHours(TOKEN_EXPIRY_HOURS);

                // Update user with reset token
                user.PasswordResetToken = token;
                user.PasswordResetTokenExpiry = expiry;
                await _context.SaveChangesAsync();

                // Send reset email
                await SendPasswordResetEmail(user.Email, user.FullName, token);

                return true;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error requesting password reset: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ValidateResetTokenAsync(string email, string token)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null || user.PasswordResetToken != token)
                {
                    return false;
                }

                // Check if token is expired
                if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null || user.PasswordResetToken != token)
                {
                    return false;
                }

                // Check if token is expired
                if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    return false;
                }

                // Check if user can reset password
                if (!CanResetPassword(user))
                {
                    return false;
                }

                // Hash new password
                var newPasswordHash = _authService.HashPassword(newPassword);

                // Update user
                user.PasswordHash = newPasswordHash;
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.LastPasswordResetAt = DateTime.UtcNow;
                user.PasswordResetCount++;
                user.PasswordResetYear = DateTime.UtcNow.Year;

                await _context.SaveChangesAsync();

                // Send confirmation email
                await SendPasswordResetConfirmationEmail(user.Email, user.FullName);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetRemainingResetsAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return 0;
                }

                return GetRemainingResets(user);
            }
            catch
            {
                return 0;
            }
        }

        private bool CanResetPassword(User user)
        {
            var currentYear = DateTime.UtcNow.Year;
            
            // If it's a new year, reset the count
            if (user.PasswordResetYear != currentYear)
            {
                user.PasswordResetCount = 0;
                user.PasswordResetYear = currentYear;
            }

            return user.PasswordResetCount < MAX_RESETS_PER_YEAR;
        }

        private int GetRemainingResets(User user)
        {
            var currentYear = DateTime.UtcNow.Year;
            
            // If it's a new year, reset the count
            if (user.PasswordResetYear != currentYear)
            {
                return MAX_RESETS_PER_YEAR;
            }

            return Math.Max(0, MAX_RESETS_PER_YEAR - user.PasswordResetCount);
        }

        private string GenerateResetToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")
                .Substring(0, 32);
        }

        private async Task SendPasswordResetEmail(string email, string fullName, string token)
        {
            var resetUrl = $"https://241runnersawareness.org/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
            
            var subject = "Password Reset Request - 241 Runners Awareness";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {fullName},</p>
                <p>You have requested to reset your password for your 241 Runners Awareness account.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetUrl}' style='background-color: #dc2626; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;'>Reset Password</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't request this password reset, please ignore this email.</p>
                <p>Best regards,<br>241 Runners Awareness Team</p>
            ";

            await _emailService.SendEmailAsync(email, subject, body);
        }

        private async Task SendPasswordResetConfirmationEmail(string email, string fullName)
        {
            var subject = "Password Reset Successful - 241 Runners Awareness";
            var body = $@"
                <h2>Password Reset Successful</h2>
                <p>Hello {fullName},</p>
                <p>Your password has been successfully reset.</p>
                <p>If you didn't perform this action, please contact us immediately.</p>
                <p>Best regards,<br>241 Runners Awareness Team</p>
            ";

            await _emailService.SendEmailAsync(email, subject, body);
        }
    }
}
