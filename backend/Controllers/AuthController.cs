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

        public AuthController(
            RunnersDbContext context,
            IAuthService authService,
            IEmailService emailService,
            ISmsService smsService)
        {
            _context = context;
            _authService = authService;
            _emailService = emailService;
            _smsService = smsService;
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
    }
} 