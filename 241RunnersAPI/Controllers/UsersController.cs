using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/users")]
    [ApiVersion("1.0")]
    public class UsersController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get current user profile (requires auth)
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }
                
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
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
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update current user profile (requires auth)
        /// </summary>
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserUpdateDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }
                
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
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
                if (!string.IsNullOrEmpty(request.YearsOfExperience))
                    user.YearsOfExperience = request.YearsOfExperience;
                if (!string.IsNullOrEmpty(request.EmergencyContactName))
                    user.EmergencyContactName = request.EmergencyContactName;
                if (!string.IsNullOrEmpty(request.EmergencyContactPhone))
                    user.EmergencyContactPhone = request.EmergencyContactPhone;
                if (!string.IsNullOrEmpty(request.EmergencyContactRelationship))
                    user.EmergencyContactRelationship = request.EmergencyContactRelationship;
                if (!string.IsNullOrEmpty(request.Notes))
                    user.Notes = request.Notes;

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
                _logger.LogError(ex, "Error updating current user");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all users with pagination and filtering (admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsers([FromQuery] UserQuery query)
        {
            try
            {
                var usersQuery = _context.Users.AsQueryable();

                // Apply role filter
                if (!string.IsNullOrEmpty(query.Role))
                {
                    usersQuery = usersQuery.Where(u => u.Role == query.Role);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(query.Q))
                {
                    var searchTerm = query.Q.ToLower();
                    usersQuery = usersQuery.Where(u => 
                        u.Email.ToLower().Contains(searchTerm) ||
                        u.FirstName.ToLower().Contains(searchTerm) ||
                        u.LastName.ToLower().Contains(searchTerm));
                }

                // Get total count
                var total = await usersQuery.CountAsync();

                // Apply pagination
                var users = await usersQuery
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(u => new
                    {
                        id = $"u_{u.Id}",
                        email = u.Email,
                        name = $"{u.FirstName} {u.LastName}",
                        role = u.Role,
                        createdAt = u.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        lastLoginAt = u.LastLoginAt.HasValue ? u.LastLoginAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null,
                        isActive = u.IsActive,
                        emailVerified = u.IsEmailVerified
                    })
                    .ToListAsync();

                return Ok(new
                {
                    data = users,
                    page = query.Page,
                    pageSize = query.PageSize,
                    total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while retrieving users"
                    }
                });
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // Comprehensive input validation
                var validationErrors = new Dictionary<string, string[]>();
                
                // Email validation
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    validationErrors["email"] = new[] { "Email is required" };
                }
                else if (!IsValidEmail(request.Email))
                {
                    validationErrors["email"] = new[] { "Email format is invalid" };
                }
                else if (request.Email.Length > 254)
                {
                    validationErrors["email"] = new[] { "Email is too long (max 254 characters)" };
                }
                
                // Password validation
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    validationErrors["password"] = new[] { "Password is required" };
                }
                else if (request.Password.Length < 8)
                {
                    validationErrors["password"] = new[] { "Password must be at least 8 characters long" };
                }
                else if (request.Password.Length > 128)
                {
                    validationErrors["password"] = new[] { "Password is too long (max 128 characters)" };
                }
                else if (!IsValidPassword(request.Password))
                {
                    validationErrors["password"] = new[] { "Password must contain at least one uppercase letter, one lowercase letter, and one number" };
                }
                
                // Name validation
                if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 100)
                {
                    validationErrors["name"] = new[] { "Name is too long (max 100 characters)" };
                }
                
                // Role validation
                if (!string.IsNullOrWhiteSpace(request.Role) && !IsValidRole(request.Role))
                {
                    validationErrors["role"] = new[] { "Invalid role. Must be 'admin', 'staff', or 'user'" };
                }
                
                if (validationErrors.Any())
                {
                    return ValidationErrorResponse(validationErrors);
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
                    FirstName = request.Name?.Split(' ').FirstOrDefault() ?? "",
                    LastName = request.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    Role = request.Role ?? "User",
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User created by admin: {Email} with role {Role}", user.Email, user.Role);

                return CreatedAtAction(nameof(CreateUser), new { id = user.Id }, new
                {
                    id = $"u_{user.Id}",
                    email = user.Email,
                    role = user.Role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while creating user"
                    }
                });
            }
        }

        /// <summary>
        /// Update a user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                // Parse user ID
                if (!int.TryParse(id.Replace("u_", ""), out var userId))
                {
                    return NotFoundResponse("User not found");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFoundResponse("User not found");
                }

                // Update user fields
                if (!string.IsNullOrEmpty(request.Name))
                {
                    var nameParts = request.Name.Split(' ');
                    user.FirstName = nameParts.FirstOrDefault() ?? "";
                    user.LastName = string.Join(" ", nameParts.Skip(1));
                }

                if (!string.IsNullOrEmpty(request.Role))
                {
                    user.Role = request.Role;
                }

                if (request.Disabled.HasValue)
                {
                    user.IsActive = !request.Disabled.Value;
                }

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("User updated: {Email}", user.Email);

                return Ok(new
                {
                    id = $"u_{user.Id}",
                    name = $"{user.FirstName} {user.LastName}",
                    role = user.Role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while updating user"
                    }
                });
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                // Parse user ID
                if (!int.TryParse(id.Replace("u_", ""), out var userId))
                {
                    return NotFoundResponse("User not found");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFoundResponse("User not found");
                }

                // Check if user has related data
                var hasRunners = await _context.Runners.AnyAsync(r => r.UserId == userId);
                var hasCases = await _context.Cases.AnyAsync(c => c.ReportedByUserId == userId);

                if (hasRunners || hasCases)
                {
                    // Mark as inactive instead of deleting
                    user.IsActive = false;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User marked as inactive due to related data: {Email}", user.Email);
                    return Ok(new { message = "User marked as inactive due to related data" });
                }

                // Safe to delete
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User deleted: {Email}", user.Email);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while deleting user"
                    }
                });
            }
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
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit);
        }

        /// <summary>
        /// Validates role
        /// </summary>
        private bool IsValidRole(string role)
        {
            var validRoles = new[] { "admin", "staff", "user" };
            return validRoles.Contains(role.ToLower());
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

        #endregion
    }

    public class UserQuery
    {
        public string? Role { get; set; }
        public string? Q { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Role { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Role { get; set; }
        public bool? Disabled { get; set; }
    }
}
