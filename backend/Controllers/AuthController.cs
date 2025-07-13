using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _241RunnersAwareness.BackendAPI.Data;
using _241RunnersAwareness.BackendAPI.Models;
using _241RunnersAwareness.BackendAPI.Services;
using Google.Apis.Auth;
using System.Collections.Generic;

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

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            try
            {
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
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    FullName = request.FullName,
                    PasswordHash = _authService.HashPassword(request.Password),
                    EmailVerificationToken = _authService.GenerateVerificationToken(),
                    PhoneVerificationCode = _authService.GenerateVerificationCode(),
                    EmailVerificationExpiry = DateTime.UtcNow.AddHours(24),
                    PhoneVerificationExpiry = DateTime.UtcNow.AddMinutes(10),
                    Role = request.Role ?? "user",
                    
                    // Role-specific fields
                    RelationshipToRunner = request.RelationshipToRunner,
                    LicenseNumber = request.LicenseNumber,
                    Organization = request.Organization,
                    Credentials = request.Credentials,
                    Specialization = request.Specialization,
                    YearsOfExperience = request.YearsOfExperience,
                    
                    // Common fields
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    ZipCode = request.ZipCode,
                    EmergencyContactName = request.EmergencyContactName,
                    EmergencyContactPhone = request.EmergencyContactPhone,
                    EmergencyContactRelationship = request.EmergencyContactRelationship
                };

                // If role is not 'user' and Individual info is provided, create and link Individual
                if (request.Role != null && request.Role != "user" && request.Individual != null)
                {
                    var individual = new Individual
                    {
                        IndividualId = Guid.NewGuid(),
                        FullName = request.Individual.FullName,
                        DateOfBirth = request.Individual.DateOfBirth ?? DateTime.MinValue,
                        Gender = request.Individual.Gender,
                        DateAdded = DateTime.UtcNow,
                        EmergencyContacts = new List<EmergencyContact>()
                    };

                    // Add emergency contact if provided
                    if (request.Individual.EmergencyContact != null)
                    {
                        individual.EmergencyContacts.Add(new EmergencyContact
                        {
                            Name = request.Individual.EmergencyContact.Name,
                            Phone = request.Individual.EmergencyContact.Phone
                        });
                    }

                    _context.Individuals.Add(individual);
                    await _context.SaveChangesAsync();

                    user.IndividualId = individual.IndividualId;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Send verification emails and SMS
                await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, user.EmailVerificationToken);
                await _smsService.SendVerificationCodeAsync(user.PhoneNumber, user.PhoneVerificationCode);

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
    }
} 