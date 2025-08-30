using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Models;
using _241RunnersAwarenessAPI.Services;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, JwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validate input
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                // Additional validation for admin role creation
                if (request.Role.ToLower() == "admin")
                {
                    // Check if the request is coming from an authenticated admin
                    var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                    {
                        return Unauthorized(new AuthResponse
                        {
                            Success = false,
                            Message = "Admin creation requires authentication."
                        });
                    }

                    var token = authHeader.Substring("Bearer ".Length);
                    var currentUser = _jwtService.ValidateToken(token);
                    if (currentUser == null || currentUser.Role.ToLower() != "admin")
                    {
                        return Forbid(new AuthResponse
                        {
                            Success = false,
                            Message = "Only existing admins can create new admin users."
                        });
                    }
                }

                // Validate email format more strictly
                if (!IsValidEmail(request.Email))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email format."
                    });
                }

                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (existingUser != null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "A user with this email already exists."
                    });
                }

                // Validate password strength
                if (!IsPasswordStrong(request.Password))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character."
                    });
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Create new user
                var user = new User
                {
                    Email = request.Email.ToLower().Trim(),
                    PasswordHash = passwordHash,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Role = request.Role.ToLower(),
                    PhoneNumber = request.PhoneNumber?.Trim(),
                    Address = request.Address?.Trim(),
                    City = request.City?.Trim(),
                    State = request.State?.Trim(),
                    ZipCode = request.ZipCode?.Trim(),
                    Organization = request.Organization?.Trim(),
                    Title = request.Title?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate token
                var token = _jwtService.GenerateToken(user);

                // Return success response
                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "User registered successfully.",
                    Token = token,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        City = user.City,
                        State = user.State,
                        ZipCode = user.ZipCode,
                        Organization = user.Organization,
                        Title = user.Title
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during registration. Please try again."
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate input
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                // Find user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password."
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

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
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

                // Generate token
                var token = _jwtService.GenerateToken(user);

                // Return success response
                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    Token = token,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        City = user.City,
                        State = user.State,
                        ZipCode = user.ZipCode,
                        Organization = user.Organization,
                        Title = user.Title
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again."
                });
            }
        }

        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok(new { message = "API is working!", timestamp = DateTime.UtcNow, status = "healthy" });
        }

        [HttpGet("health")]
        public ActionResult<string> Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        [HttpPost("test-reset")]
        public ActionResult<string> TestReset()
        {
            return Ok(new { message = "Test reset endpoint working", timestamp = DateTime.UtcNow });
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<AuthResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                // Validate input
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
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

                // Validate new password
                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "New password is required."
                    });
                }

                if (request.NewPassword.Length < 8)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Password must be at least 8 characters long."
                    });
                }

                // Check if password contains at least one uppercase, one lowercase, one number, and one special character
                if (!System.Text.RegularExpressions.Regex.IsMatch(request.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character."
                    });
                }

                // Hash new password
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                // Update user password
                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset successful for user: {user.Email}");

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Password reset successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during password reset. Please try again."
                });
            }
        }

        // Helper methods for validation
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

        private bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            // Check for at least one uppercase letter
            if (!password.Any(char.IsUpper))
                return false;

            // Check for at least one lowercase letter
            if (!password.Any(char.IsLower))
                return false;

            // Check for at least one digit
            if (!password.Any(char.IsDigit))
                return false;

            // Check for at least one special character
            var specialChars = @"@$!%*?&";
            if (!password.Any(c => specialChars.Contains(c)))
                return false;

            return true;
        }
    }
} 