using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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

                    var authToken = authHeader.Substring("Bearer ".Length);
                    var currentUser = _jwtService.ValidateToken(authToken);
                    if (currentUser == null || currentUser.FindFirst(ClaimTypes.Role)?.Value.ToLower() != "admin")
                    {
                        return StatusCode(403, new AuthResponse
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
                    // Organization and Title will be null for now until we fix the database schema
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

        [HttpPost("create-first-admin")]
        public async Task<ActionResult<AuthResponse>> CreateFirstAdmin([FromBody] RegisterRequest request)
        {
            try
            {
                // Check if any admin users exist
                var adminExists = await _context.Users.AnyAsync(u => u.Role.ToLower() == "admin");
                if (adminExists)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Admin users already exist. Use the regular admin creation endpoint."
                    });
                }

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

                // Validate email format
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

                // Create new admin user
                var user = new User
                {
                    Email = request.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = "admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    ZipCode = request.ZipCode
                    // Organization and Title will be null for now until we fix the database schema
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate token
                var token = _jwtService.GenerateToken(user);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "First admin user created successfully.",
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
                _logger.LogError(ex, "Error creating first admin user");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while creating the admin user. Please try again."
                });
            }
        }

        [HttpPost("create-admin")]
        public async Task<ActionResult<AuthResponse>> CreateAdmin([FromBody] RegisterRequest request)
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

                // Validate email format
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

                // Create new admin user
                var user = new User
                {
                    Email = request.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = "admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    ZipCode = request.ZipCode
                    // Organization and Title will be null for now until we fix the database schema
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate token
                var token = _jwtService.GenerateToken(user);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Admin user created successfully.",
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
                _logger.LogError(ex, "Error creating admin user");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while creating the admin user. Please try again."
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<AuthResponse>> ResetPassword([FromBody] PasswordResetRequest request)
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

                // Get current user from JWT token
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Authentication required."
                    });
                }

                var token = authHeader.Substring("Bearer ".Length);
                var currentUser = _jwtService.ValidateToken(token);
                if (currentUser == null)
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid or expired token."
                    });
                }

                // Get user ID from claims
                var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid user token."
                    });
                }

                // Get user from database
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Current password is incorrect."
                    });
                }

                // Validate new password strength
                if (!IsPasswordStrong(request.NewPassword))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character."
                    });
                }

                // Hash new password
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                // Update user password
                user.PasswordHash = newPasswordHash;
                // user.UpdatedAt = DateTime.UtcNow; // This column doesn't exist yet
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



        [HttpGet("verify")]
        public async Task<ActionResult<AuthResponse>> VerifyToken()
        {
            try
            {
                // Get current user from JWT token
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Authentication required."
                    });
                }

                var token = authHeader.Substring("Bearer ".Length);
                var currentUser = _jwtService.ValidateToken(token);
                if (currentUser == null)
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid or expired token."
                    });
                }

                // Get user ID from claims
                var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid user token."
                    });
                }

                // Get user from database
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Token is valid.",
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
                _logger.LogError(ex, "Error verifying token");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while verifying the token."
                });
            }
        }

        [HttpGet("health")]
        public async Task<ActionResult> Health()
        {
            try
            {
                // Test database connection
                var userCount = await _context.Users.CountAsync();
                var runnerCount = await _context.Runners.CountAsync();
                
                return Ok(new { 
                    status = "healthy", 
                    timestamp = DateTime.UtcNow,
                    database = "connected",
                    users = userCount,
                    runners = runnerCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { 
                    status = "unhealthy", 
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<object>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.Role,
                        u.IsActive,
                        u.CreatedAt,
                        u.LastLoginAt,
                        u.PhoneNumber,
                        u.Address,
                        u.City,
                        u.State,
                        u.ZipCode
                    })
                    .ToListAsync();
                
                return Ok(new
                {
                    success = true,
                    users = users,
                    count = users.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving users.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("users/{id}")]
        public async Task<ActionResult<AuthResponse>> UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            try
            {
                // Check if user is authenticated and is admin
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Authentication required."
                    });
                }

                var authToken = authHeader.Substring("Bearer ".Length);
                var currentUser = _jwtService.ValidateToken(authToken);
                if (currentUser == null || currentUser.FindFirst(ClaimTypes.Role)?.Value.ToLower() != "admin")
                {
                    return StatusCode(403, new AuthResponse
                    {
                        Success = false,
                        Message = "Admin access required."
                    });
                }

                // Find the user to update
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                // Check if email is being changed and if it's already taken
                if (request.Email.ToLower() != user.Email.ToLower())
                {
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                    if (existingUser != null)
                    {
                        return BadRequest(new AuthResponse
                        {
                            Success = false,
                            Message = "A user with this email already exists."
                        });
                    }
                }

                // Update user information
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;
                user.PhoneNumber = request.PhoneNumber;
                user.Role = request.Role;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"User updated by admin: {user.Email}");

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "User updated successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while updating the user. Please try again."
                });
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<ActionResult<AuthResponse>> DeleteUser(int id)
        {
            try
            {
                // Check if user is authenticated and is admin
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Authentication required."
                    });
                }

                var authToken = authHeader.Substring("Bearer ".Length);
                var currentUser = _jwtService.ValidateToken(authToken);
                if (currentUser == null || currentUser.FindFirst(ClaimTypes.Role)?.Value.ToLower() != "admin")
                {
                    return StatusCode(403, new AuthResponse
                    {
                        Success = false,
                        Message = "Admin access required."
                    });
                }

                // Find the user to delete
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                // Prevent admin from deleting themselves
                if (currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value == id.ToString())
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "You cannot delete your own account."
                    });
                }

                // Remove user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User deleted by admin: {user.Email}");

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "User deleted successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting the user. Please try again."
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

        [HttpPost("reset-admin-password")]
        public async Task<ActionResult<AuthResponse>> ResetAdminPassword([FromBody] AdminPasswordResetRequest request)
        {
            try
            {
                // This is a temporary endpoint for testing - should be removed in production
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (user == null)
                {
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin password reset for user: {user.Email}");

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Admin password reset successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin password reset");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during password reset. Please try again."
                });
            }
        }
    }
} 