using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using _241RunnersAPI.Services;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly PerformanceMonitoringService _performanceService;

        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger, IConfiguration configuration, PerformanceMonitoringService performanceService)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _performanceService = performanceService;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                // Comprehensive input validation
                var validationErrors = new List<string>();
                
                // Email validation
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    validationErrors.Add("Email is required");
                }
                else if (!IsValidEmail(request.Email))
                {
                    validationErrors.Add("Email format is invalid");
                }
                else if (request.Email.Length > 254)
                {
                    validationErrors.Add("Email is too long (max 254 characters)");
                }
                
                // Password validation
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    validationErrors.Add("Password is required");
                }
                else if (request.Password.Length < 8)
                {
                    validationErrors.Add("Password must be at least 8 characters long");
                }
                else if (request.Password.Length > 128)
                {
                    validationErrors.Add("Password is too long (max 128 characters)");
                }
                else if (!IsValidPassword(request.Password))
                {
                    validationErrors.Add("Password must contain at least one uppercase letter, one lowercase letter, and one number");
                }
                
                // First name validation
                if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName.Length > 50)
                {
                    validationErrors.Add("First name is too long (max 50 characters)");
                }
                
                // Last name validation
                if (!string.IsNullOrWhiteSpace(request.LastName) && request.LastName.Length > 50)
                {
                    validationErrors.Add("Last name is too long (max 50 characters)");
                }
                
                // Phone number validation
                if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !IsValidPhoneNumber(request.PhoneNumber))
                {
                    validationErrors.Add("Phone number format is invalid");
                }
                
                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "VALIDATION_FAILED",
                            message = "Input validation failed",
                            details = validationErrors
                        }
                    });
                }

                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "USER_EXISTS",
                            message = "User with this email already exists"
                        }
                    });
                }

                // Create new user
                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = request.Role ?? "User",
                    IsActive = true,
                    IsEmailVerified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user registered: {Email} with role {Role}", user.Email, user.Role);

                // Track successful registration
                var duration = DateTime.UtcNow - startTime;
                _performanceService.TrackUserRegistration(user.Email, user.Role, true);
                _performanceService.TrackApiEndpoint("/api/auth/register", "POST", duration, 201, true);

                return CreatedAtAction(nameof(Register), new { id = user.Id }, new
                {
                    id = $"u_{user.Id}",
                    email = user.Email,
                    role = user.Role,
                    emailVerified = user.IsEmailVerified
                });
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _performanceService.TrackUserRegistration(request.Email, request.Role ?? "unknown", false, ex.Message);
                _performanceService.TrackApiEndpoint("/api/auth/register", "POST", duration, 500, false);
                _performanceService.TrackException(ex);
                
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred during registration"
                    }
                });
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                // Input validation for login
                var validationErrors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    validationErrors.Add("Email is required");
                }
                else if (!IsValidEmail(request.Email))
                {
                    validationErrors.Add("Email format is invalid");
                }
                
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    validationErrors.Add("Password is required");
                }
                
                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "VALIDATION_FAILED",
                            message = "Input validation failed",
                            details = validationErrors
                        }
                    });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Unauthorized(new
                    {
                        error = new
                        {
                            code = "INVALID_CREDENTIALS",
                            message = "Invalid email or password"
                        }
                    });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new
                    {
                        error = new
                        {
                            code = "ACCOUNT_DISABLED",
                            message = "Account is disabled"
                        }
                    });
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(user);

                _logger.LogInformation("User logged in: {Email}", user.Email);

                // Track successful login
                var duration = DateTime.UtcNow - startTime;
                _performanceService.TrackAuthentication("Login", user.Id.ToString(), true);
                _performanceService.TrackApiEndpoint("/api/auth/login", "POST", duration, 200, true);
                
                return Ok(new
                {
                    accessToken = token,
                    expiresIn = 3600,
                    user = new
                    {
                        id = $"u_{user.Id}",
                        email = user.Email,
                        role = user.Role,
                        name = $"{user.FirstName} {user.LastName}"
                    }
                });
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _performanceService.TrackAuthentication("Login", request.Email, false, ex.Message);
                _performanceService.TrackApiEndpoint("/api/auth/login", "POST", duration, 500, false);
                _performanceService.TrackException(ex);
                
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred during login"
                    }
                });
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // In a stateless JWT system, logout is handled client-side
            // This endpoint exists for consistency and future token blacklisting
            return Ok(new { ok = true });
        }

        /// <summary>
        /// Verify JWT token
        /// </summary>
        [HttpPost("verify")]
        [Authorize]
        public async Task<IActionResult> VerifyToken()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null)
                {
                    return UnauthorizedResponse("User not found");
                }

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        role = user.Role,
                        isActive = user.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying token");
                return InternalServerErrorResponse("Failed to verify token");
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null)
                {
                    return UnauthorizedResponse("User not found");
                }

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        fullName = $"{user.FirstName} {user.LastName}".Trim(),
                        role = user.Role,
                        isActive = user.IsActive,
                        phoneNumber = user.PhoneNumber,
                        address = user.Address,
                        city = user.City,
                        state = user.State,
                        zipCode = user.ZipCode,
                        organization = user.Organization,
                        title = user.Title,
                        credentials = user.Credentials,
                        specialization = user.Specialization,
                        yearsOfExperience = user.YearsOfExperience,
                        profileImageUrl = user.ProfileImageUrl,
                        emergencyContactName = user.EmergencyContactName,
                        emergencyContactPhone = user.EmergencyContactPhone,
                        emergencyContactRelationship = user.EmergencyContactRelationship,
                        isEmailVerified = user.IsEmailVerified,
                        isPhoneVerified = user.IsPhoneVerified,
                        createdAt = user.CreatedAt,
                        lastLoginAt = user.LastLoginAt,
                        updatedAt = user.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return InternalServerErrorResponse("Failed to get user profile");
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null)
                {
                    return UnauthorizedResponse("User not found");
                }

                // Update user fields
                if (!string.IsNullOrEmpty(request.FirstName))
                    user.FirstName = request.FirstName;
                if (!string.IsNullOrEmpty(request.LastName))
                    user.LastName = request.LastName;
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                    user.PhoneNumber = request.PhoneNumber;
                if (!string.IsNullOrEmpty(request.Address))
                    user.Address = request.Address;
                if (!string.IsNullOrEmpty(request.City))
                    user.City = request.City;
                if (!string.IsNullOrEmpty(request.State))
                    user.State = request.State;
                if (!string.IsNullOrEmpty(request.ZipCode))
                    user.ZipCode = request.ZipCode;
                if (!string.IsNullOrEmpty(request.Organization))
                    user.Organization = request.Organization;
                if (!string.IsNullOrEmpty(request.Title))
                    user.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Credentials))
                    user.Credentials = request.Credentials;
                if (!string.IsNullOrEmpty(request.Specialization))
                    user.Specialization = request.Specialization;
                if (request.YearsOfExperience.HasValue)
                    user.YearsOfExperience = request.YearsOfExperience.ToString();
                if (!string.IsNullOrEmpty(request.ProfileImageUrl))
                    user.ProfileImageUrl = request.ProfileImageUrl;
                if (!string.IsNullOrEmpty(request.EmergencyContactName))
                    user.EmergencyContactName = request.EmergencyContactName;
                if (!string.IsNullOrEmpty(request.EmergencyContactPhone))
                    user.EmergencyContactPhone = request.EmergencyContactPhone;
                if (!string.IsNullOrEmpty(request.EmergencyContactRelationship))
                    user.EmergencyContactRelationship = request.EmergencyContactRelationship;

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Profile updated successfully",
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        fullName = $"{user.FirstName} {user.LastName}".Trim(),
                        role = user.Role,
                        isActive = user.IsActive,
                        phoneNumber = user.PhoneNumber,
                        address = user.Address,
                        city = user.City,
                        state = user.State,
                        zipCode = user.ZipCode,
                        organization = user.Organization,
                        title = user.Title,
                        credentials = user.Credentials,
                        specialization = user.Specialization,
                        yearsOfExperience = user.YearsOfExperience,
                        profileImageUrl = user.ProfileImageUrl,
                        emergencyContactName = user.EmergencyContactName,
                        emergencyContactPhone = user.EmergencyContactPhone,
                        emergencyContactRelationship = user.EmergencyContactRelationship,
                        isEmailVerified = user.IsEmailVerified,
                        isPhoneVerified = user.IsPhoneVerified,
                        updatedAt = user.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return InternalServerErrorResponse("Failed to update profile");
            }
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null)
                {
                    return UnauthorizedResponse("User not found");
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return BadRequestResponse("Current password is incorrect");
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Password changed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return InternalServerErrorResponse("Failed to change password");
            }
        }

        /// <summary>
        /// Reset password
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                
                if (user == null)
                {
                    // Don't reveal if email exists
                    return Ok(new
                    {
                        success = true,
                        message = "If the email exists, a password reset link has been sent"
                    });
                }

                // Generate reset token (simplified - in production, use proper token system)
                var resetToken = Guid.NewGuid().ToString();
                user.ResetToken = resetToken;
                user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // In production, send email with reset link
                _logger.LogInformation($"Password reset token for {user.Email}: {resetToken}");

                return Ok(new
                {
                    success = true,
                    message = "If the email exists, a password reset link has been sent"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return InternalServerErrorResponse("Failed to reset password");
            }
        }

        /// <summary>
        /// Verify email address
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            try
            {
                // For now, just mark as verified (in production, validate the token)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Token);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "INVALID_TOKEN",
                            message = "Invalid verification token"
                        }
                    });
                }

                user.IsEmailVerified = true;
                user.EmailVerifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { emailVerified = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred during email verification"
                    }
                });
            }
        }

        /// <summary>
        /// Create admin user (TEMPORARY - for development only)
        /// </summary>
        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                // Check if admin already exists
                var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingAdmin != null)
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "USER_EXISTS",
                            message = "Admin user already exists"
                        }
                    });
                }

                // Create new admin user
                var adminUser = new User
                {
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FirstName = request.FirstName ?? "Admin",
                    LastName = request.LastName ?? "User",
                    Role = "admin",
                    IsActive = true,
                    IsEmailVerified = true,
                    IsPhoneVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailVerifiedAt = DateTime.UtcNow,
                    PhoneVerifiedAt = DateTime.UtcNow,
                    Organization = "241 Runners Awareness",
                    Title = "Administrator",
                    PhoneNumber = "+1-555-0123",
                    Address = "123 Admin Street",
                    City = "Admin City",
                    State = "Admin State",
                    ZipCode = "12345",
                    EmergencyContactName = "Emergency Services",
                    EmergencyContactPhone = "+1-555-911",
                    EmergencyContactRelationship = "Emergency Contact"
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin user created: {Email}", request.Email);

                return Ok(new
                {
                    success = true,
                    message = "Admin user created successfully",
                    data = new
                    {
                        email = adminUser.Email,
                        role = adminUser.Role,
                        createdAt = adminUser.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin user {Email}", request.Email);
                return InternalServerErrorResponse("An error occurred while creating admin user");
            }
            finally
            {
                // Performance tracking removed for now
            }
        }

        /// <summary>
        /// Reset password for Marcus admin account (TEMPORARY - for development only)
        /// </summary>
        [HttpPost("reset-marcus-password")]
        public async Task<IActionResult> ResetMarcusPassword()
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "dekuworks1@gmail.com");
                if (user == null)
                {
                    return NotFound(new
                    {
                        error = new
                        {
                            code = "USER_NOT_FOUND",
                            message = "Marcus admin user not found"
                        }
                    });
                }

                // Reset password to marcus2025
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("marcus2025");
                user.IsActive = true;
                user.IsEmailVerified = true;
                user.IsPhoneVerified = true;
                user.Role = "admin";
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Marcus admin password reset successfully");

                return Ok(new
                {
                    success = true,
                    message = "Marcus admin password reset successfully",
                    data = new
                    {
                        email = user.Email,
                        role = user.Role,
                        isActive = user.IsActive,
                        updatedAt = user.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting Marcus admin password");
                return InternalServerErrorResponse("An error occurred while resetting password");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners";
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"] ?? "241RunnersAwareness";
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"] ?? "241RunnersAwareness";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #region Validation Helper Methods

        /// <summary>
        /// Validates email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates password strength
        /// </summary>
        private bool IsValidPassword(string password)
        {
            // At least 8 characters, one uppercase, one lowercase, one number
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit);
        }

        /// <summary>
        /// Validates phone number format
        /// </summary>
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters
            var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Check if it's a valid length (10-15 digits)
            return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
        }

        /// <summary>
        /// Sanitizes input to prevent XSS
        /// </summary>
        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return System.Web.HttpUtility.HtmlEncode(input.Trim());
        }

        /// <summary>
        /// Validates and sanitizes text input
        /// </summary>
        private string ValidateAndSanitizeText(string input, string fieldName, int maxLength = 255)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var sanitized = SanitizeInput(input);
            
            if (sanitized.Length > maxLength)
                throw new ArgumentException($"{fieldName} is too long (max {maxLength} characters)");

            return sanitized;
        }

        #endregion
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyEmailRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class CreateAdminRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}