using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using _241RunnersAwareness.BackendAPI.Services;
using Google.Apis.Auth;
using System.Collections.Generic;
using _241RunnersAwareness.BackendAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RunnersDbContext _context;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ITwoFactorService _twoFactorService;

        public AuthController(
            RunnersDbContext context,
            IAuthService authService,
            IEmailService emailService,
            ISmsService smsService,
            ITwoFactorService twoFactorService)
        {
            _context = context;
            _authService = authService;
            _emailService = emailService;
            _smsService = smsService;
            _twoFactorService = twoFactorService;
        }

        [HttpGet("test")]
        public ActionResult<object> Test()
        {
            return Ok(new { message = "Auth controller is working", timestamp = DateTime.UtcNow });
        }

        [HttpGet("test-db")]
        public async Task<ActionResult<object>> TestDb()
        {
            try
            {
                Log.Information("Testing database connection...");
                
                // Test basic connection
                var userCount = await _context.Users.CountAsync();
                Log.Information("Database connection successful. User count: {UserCount}", userCount);
                
                // Test creating a test user (will be deleted)
                var testUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "test_user",
                    Email = "test@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    FullName = "Test User",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                    Role = "user",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                Log.Information("Creating test user: {Email}, {FirstName}, {LastName}", 
                    testUser.Email, testUser.FirstName, testUser.LastName);
                
                _context.Users.Add(testUser);
                await _context.SaveChangesAsync();
                
                Log.Information("Test user created successfully. UserId: {UserId}", testUser.UserId);
                
                // Delete the test user
                _context.Users.Remove(testUser);
                await _context.SaveChangesAsync();
                
                Log.Information("Test user deleted successfully");
                
                return Ok(new { 
                    message = "Database connection and write operations working", 
                    userCount = userCount,
                    testWrite = "successful"
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database test failed");
                return StatusCode(500, new { message = "Database error", error = ex.Message });
            }
        }

        [HttpPost("register-simple")]
        public async Task<ActionResult<AuthResponse>> RegisterSimple(RegisterRequest request)
        {
            try
            {
                // Prevent admin role creation through regular registration
                if (request.Role?.ToLower() == "admin" || request.Role?.ToLower() == "superadmin")
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Admin roles cannot be created through regular registration."
                    });
                }

                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "User with this email already exists."
                    });
                }

                // Create new user with minimal fields
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = request.Email.Split('@')[0],
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    FullName = request.FullName ?? $"{request.FirstName} {request.LastName}",
                    PasswordHash = _authService.HashPassword(request.Password),
                    Role = request.Role ?? "user",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Only set phone number if provided
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    user.PhoneNumber = request.PhoneNumber;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create a simple user DTO without complex mapping
                var userDto = new Models.UserDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName ?? $"{user.FirstName} {user.LastName}",
                    Role = user.Role,
                    PhoneNumber = user.PhoneNumber,
                    EmailVerified = user.EmailVerified,
                    PhoneVerified = user.PhoneVerified,
                    CreatedAt = user.CreatedAt
                };

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful.",
                    User = userDto,
                    RequiresVerification = false
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = $"An error occurred during registration: {ex.Message}"
                });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            try
            {
                // Prevent admin role creation through regular registration
                if (request.Role?.ToLower() == "admin" || request.Role?.ToLower() == "superadmin")
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Admin roles cannot be created through regular registration."
                    });
                }

                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "User with this email already exists."
                    });
                }

                // Create new user
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = request.Email.Split('@')[0], // Generate username from email
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    FullName = request.FullName ?? $"{request.FirstName} {request.LastName}",
                    PasswordHash = _authService.HashPassword(request.Password),
                    EmailVerificationToken = _authService.GenerateVerificationToken(),
                    PhoneVerificationCode = _authService.GenerateVerificationCode(),
                    EmailVerificationExpiry = DateTime.UtcNow.AddHours(24),
                    PhoneVerificationExpiry = DateTime.UtcNow.AddMinutes(10),
                    Role = request.Role ?? "user"
                };

                // Individual creation logic temporarily disabled for testing
                // if (request.Role != null && request.Role != "user" && request.Individual != null)
                // {
                //     // Individual creation code here
                // }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Send verification emails and SMS (temporarily disabled for testing)
                try {
                    await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, user.EmailVerificationToken);
                    await _smsService.SendVerificationCodeAsync(user.PhoneNumber, user.PhoneVerificationCode);
                } catch (Exception ex) {
                    // Log the error but don't fail the registration
                    Console.WriteLine($"Verification sending failed: {ex.Message}");
                }

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful. Please check your email and phone for verification codes.",
                    User = _authService.MapToUserDto(user),
                    RequiresVerification = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during registration."
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    });
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = _authService.GenerateJwtToken(user);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    Token = token,
                    User = _authService.MapToUserDto(user),
                    RequiresVerification = !user.EmailVerified || !user.PhoneVerified
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login."
                });
            }
        }

        [HttpPost("google-login")]
        public async Task<ActionResult<AuthResponse>> GoogleLogin(GoogleLoginRequest request)
        {
            try
            {
                // Verify Google ID token
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
                
                if (payload == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid Google token."
                    });
                }

                // Check if user exists
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == payload.Email && u.IsActive);

                if (user == null)
                {
                    // Create new user from Google data
                    user = new User
                    {
                        UserId = Guid.NewGuid(),
                        Username = payload.Email.Split('@')[0], // Generate username from email
                        Email = payload.Email,
                        FullName = payload.Name,
                        EmailVerified = true, // Google accounts are pre-verified
                        PhoneVerified = false, // Will need phone verification
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Update last login for existing user
                    user.LastLoginAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                var token = _authService.GenerateJwtToken(user);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Google login successful.",
                    Token = token,
                    User = _authService.MapToUserDto(user),
                    RequiresVerification = !user.PhoneVerified
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during Google login."
                });
            }
        }

        [HttpPost("admin-login")]
        public async Task<ActionResult<AuthResponse>> AdminLogin(LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid admin credentials."
                    });
                }

                // Check if user is an admin
                if (user.Role != "admin" && user.Role != "superadmin")
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Access denied. Admin privileges required."
                    });
                }

                // Verify password
                if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid admin credentials."
                    });
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Account is deactivated. Please contact support."
                    });
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = _authService.GenerateJwtToken(user);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Admin login successful.",
                    Token = token,
                    User = _authService.MapToUserDto(user)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during admin login."
                });
            }
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult<AuthResponse>> VerifyEmail(VerifyEmailRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token);

                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid verification token."
                    });
                }

                if (user.EmailVerificationExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Verification token has expired."
                    });
                }

                user.EmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationExpiry = null;

                await _context.SaveChangesAsync();

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Email verified successfully.",
                    User = _authService.MapToUserDto(user)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during email verification."
                });
            }
        }

        [HttpPost("verify-phone")]
        public async Task<ActionResult<AuthResponse>> VerifyPhone(VerifyPhoneRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PhoneVerificationCode == request.Code);

                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid verification code."
                    });
                }

                if (user.PhoneVerificationExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Verification code has expired."
                    });
                }

                user.PhoneVerified = true;
                user.PhoneVerificationCode = null;
                user.PhoneVerificationExpiry = null;

                await _context.SaveChangesAsync();

                // Send welcome SMS
                await _smsService.SendWelcomeMessageAsync(user.PhoneNumber, user.FullName);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Phone verified successfully.",
                    User = _authService.MapToUserDto(user)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during phone verification."
                });
            }
        }

        [HttpPost("resend-verification")]
        public async Task<ActionResult<AuthResponse>> ResendVerification(ResendVerificationRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                if (request.Type.ToLower() == "email" && !user.EmailVerified)
                {
                    user.EmailVerificationToken = _authService.GenerateVerificationToken();
                    user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);
                    await _context.SaveChangesAsync();

                    await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, user.EmailVerificationToken);

                    return Ok(new AuthResponse
                    {
                        Success = true,
                        Message = "Verification email sent successfully."
                    });
                }
                else if (request.Type.ToLower() == "phone" && !user.PhoneVerified)
                {
                    user.PhoneVerificationCode = _authService.GenerateVerificationCode();
                    user.PhoneVerificationExpiry = DateTime.UtcNow.AddMinutes(10);
                    await _context.SaveChangesAsync();

                    await _smsService.SendVerificationCodeAsync(user.PhoneNumber, user.PhoneVerificationCode);

                    return Ok(new AuthResponse
                    {
                        Success = true,
                        Message = "Verification SMS sent successfully."
                    });
                }

                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid verification type or already verified."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while resending verification."
                });
            }
        }

        // 2FA Setup
        [HttpPost("2fa/setup")]
        public async Task<ActionResult<SetupTwoFactorResponse>> SetupTwoFactor(SetupTwoFactorRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                if (user == null)
                {
                    return BadRequest(new SetupTwoFactorResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                if (user.TwoFactorEnabled)
                {
                    return BadRequest(new SetupTwoFactorResponse
                    {
                        Success = false,
                        Message = "Two-factor authentication is already enabled."
                    });
                }

                var secret = _twoFactorService.GenerateSecret();
                var qrCodeUrl = _twoFactorService.GenerateQrCodeUrl(user.Email, secret);
                var backupCodes = _twoFactorService.GenerateBackupCodes();

                user.TwoFactorSecret = secret;
                user.TwoFactorBackupCodes = backupCodes;
                user.TwoFactorSetupDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var backupCodesList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(backupCodes);

                return Ok(new SetupTwoFactorResponse
                {
                    Success = true,
                    Message = "Two-factor authentication setup successful.",
                    QrCodeUrl = qrCodeUrl,
                    Secret = secret,
                    BackupCodes = backupCodesList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SetupTwoFactorResponse
                {
                    Success = false,
                    Message = "An error occurred during 2FA setup."
                });
            }
        }

        // 2FA Verification
        [HttpPost("2fa/verify")]
        public async Task<ActionResult<VerifyTwoFactorResponse>> VerifyTwoFactor(VerifyTwoFactorRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                if (user == null)
                {
                    return BadRequest(new VerifyTwoFactorResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                if (!user.TwoFactorEnabled)
                {
                    return BadRequest(new VerifyTwoFactorResponse
                    {
                        Success = false,
                        Message = "Two-factor authentication is not enabled."
                    });
                }

                if (string.IsNullOrEmpty(user.TwoFactorSecret))
                {
                    return BadRequest(new VerifyTwoFactorResponse
                    {
                        Success = false,
                        Message = "Two-factor authentication is not properly configured."
                    });
                }

                // Check if it's a backup code
                if (_twoFactorService.ValidateBackupCode(user.TwoFactorBackupCodes, request.Totp))
                {
                    // Remove used backup code
                    user.TwoFactorBackupCodes = _twoFactorService.RemoveUsedBackupCode(user.TwoFactorBackupCodes, request.Totp);
                    await _context.SaveChangesAsync();
                }
                else if (!_twoFactorService.ValidateTotp(user.TwoFactorSecret, request.Totp))
                {
                    return BadRequest(new VerifyTwoFactorResponse
                    {
                        Success = false,
                        Message = "Invalid two-factor authentication code."
                    });
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = _authService.GenerateJwtToken(user);

                return Ok(new VerifyTwoFactorResponse
                {
                    Success = true,
                    Message = "Two-factor authentication successful.",
                    Token = token,
                    User = _authService.MapToUserDto(user)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new VerifyTwoFactorResponse
                {
                    Success = false,
                    Message = "An error occurred during 2FA verification."
                });
            }
        }

        // Enable 2FA
        [HttpPost("2fa/enable")]
        public async Task<ActionResult<AuthResponse>> EnableTwoFactor(VerifyTwoFactorRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                if (string.IsNullOrEmpty(user.TwoFactorSecret))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Two-factor authentication is not set up."
                    });
                }

                if (!_twoFactorService.ValidateTotp(user.TwoFactorSecret, request.Totp))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid two-factor authentication code."
                    });
                }

                user.TwoFactorEnabled = true;
                await _context.SaveChangesAsync();

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Two-factor authentication enabled successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while enabling 2FA."
                });
            }
        }

        // Disable 2FA
        [HttpPost("2fa/disable")]
        public async Task<ActionResult<AuthResponse>> DisableTwoFactor(DisableTwoFactorRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                if (!user.TwoFactorEnabled)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Two-factor authentication is not enabled."
                    });
                }

                if (!_twoFactorService.ValidateTotp(user.TwoFactorSecret, request.Totp))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid two-factor authentication code."
                    });
                }

                user.TwoFactorEnabled = false;
                user.TwoFactorSecret = null;
                user.TwoFactorBackupCodes = null;
                user.TwoFactorSetupDate = null;
                await _context.SaveChangesAsync();

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Two-factor authentication disabled successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while disabling 2FA."
                });
            }
        }

        // Get 2FA Status
        [HttpGet("2fa/status/{email}")]
        public async Task<ActionResult<object>> GetTwoFactorStatus(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(new
                {
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    TwoFactorSetupDate = user.TwoFactorSetupDate,
                    HasSecret = !string.IsNullOrEmpty(user.TwoFactorSecret)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting 2FA status." });
            }
        }

        // Password Reset Request
        [HttpPost("forgot-password")]
        public async Task<ActionResult<AuthResponse>> ForgotPassword(ForgotPasswordRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                if (user == null)
                {
                    // Don't reveal if user exists or not for security
                    return Ok(new AuthResponse
                    {
                        Success = true,
                        Message = "If an account with this email exists, a password reset link has been sent."
                    });
                }

                // Generate password reset token
                user.PasswordResetToken = _authService.GenerateVerificationToken();
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                user.PasswordResetCount++;
                user.LastPasswordResetAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send password reset email
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, user.PasswordResetToken);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "If an account with this email exists, a password reset link has been sent."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while processing password reset request."
                });
            }
        }

        // Password Reset Verification
        [HttpPost("reset-password")]
        public async Task<ActionResult<AuthResponse>> ResetPassword(ResetPasswordRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token && u.IsActive);

                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid or expired password reset token."
                    });
                }

                if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Password reset token has expired."
                    });
                }

                // Update password
                user.PasswordHash = _authService.HashPassword(request.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.PasswordResetYear = DateTime.UtcNow.Year;

                await _context.SaveChangesAsync();

                // Send password change confirmation email
                await _emailService.SendPasswordChangeConfirmationAsync(user.Email, user.FullName);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Password has been reset successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while resetting password."
                });
            }
        }

        // Change Password (for authenticated users)
        [HttpPost("change-password")]
        public async Task<ActionResult<AuthResponse>> ChangePassword(ChangePasswordRequest request)
        {
            try
            {
                // Get user from JWT token
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "User not authenticated."
                    });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == userEmail && u.IsActive);

                if (user == null)
                {
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                // Verify current password
                if (!_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Current password is incorrect."
                    });
                }

                // Update password
                user.PasswordHash = _authService.HashPassword(request.NewPassword);
                user.PasswordResetYear = DateTime.UtcNow.Year;

                await _context.SaveChangesAsync();

                // Send password change confirmation email
                await _emailService.SendPasswordChangeConfirmationAsync(user.Email, user.FullName);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Password changed successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while changing password."
                });
            }
        }

        // Get current user profile with linked individuals
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<object>> GetCurrentUser()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == userEmail && u.IsActive);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Get linked individuals with case counts
                var linkedIndividuals = await _context.Individuals
                    .Where(i => i.OwnerUserId == user.UserId)
                    .Select(i => new
                    {
                        id = i.Id,
                        firstName = i.FirstName,
                        lastName = i.LastName,
                        status = i.Status,
                        casesCount = i.Cases.Count
                    })
                    .ToListAsync();

                return Ok(new
                {
                    id = user.UserId,
                    email = user.Email,
                    role = user.Role,
                    linkedIndividuals = linkedIndividuals
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching user profile." });
            }
        }

        // Update Phone Number
        [HttpPost("update-phone")]
        public async Task<ActionResult<AuthResponse>> UpdatePhoneNumber(UpdatePhoneRequest request)
        {
            try
            {
                // Get user from JWT token
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "User not authenticated."
                    });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == userEmail && u.IsActive);

                if (user == null)
                {
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                // Check if phone number is already in use by another user
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber && u.UserId != user.UserId);

                if (existingUser != null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Phone number is already in use by another account."
                    });
                }

                // Update phone number
                user.PhoneNumber = request.PhoneNumber;
                user.PhoneVerified = false; // Reset verification status

                // Generate new verification code
                user.PhoneVerificationCode = _authService.GenerateVerificationCode();
                user.PhoneVerificationExpiry = DateTime.UtcNow.AddMinutes(10);

                await _context.SaveChangesAsync();

                // Send verification SMS
                await _smsService.SendVerificationCodeAsync(user.PhoneNumber, user.PhoneVerificationCode);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Phone number updated. Please verify your new phone number.",
                    RequiresVerification = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while updating phone number."
                });
            }
        }
    }
} 