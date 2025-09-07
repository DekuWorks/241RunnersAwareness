using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Models;
using _241RunnersAwarenessAPI.Services;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all users (admin only)
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<object>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.PhoneNumber,
                        u.Organization,
                        u.Title,
                        u.Role,
                        u.IsActive,
                        u.CreatedAt,
                        u.UpdatedAt
                    })
                    .OrderBy(u => u.CreatedAt)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    users = users,
                    total = users.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving users."
                });
            }
        }

        /// <summary>
        /// Get user by ID (admin only)
        /// </summary>
        [HttpGet("users/{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            try
            {
                // Validate ID parameter
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "User ID must be greater than 0."
                    });
                }

                var user = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.PhoneNumber,
                        u.Organization,
                        u.Title,
                        u.Role,
                        u.IsActive,
                        u.CreatedAt,
                        u.UpdatedAt
                    })
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    user = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the user."
                });
            }
        }

        /// <summary>
        /// Update user (admin only)
        /// </summary>
        [HttpPut("users/{id}")]
        public async Task<ActionResult<object>> UpdateUser(int id, [FromBody] AdminUserUpdateRequest request)
        {
            try
            {
                // Validate ID parameter
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "User ID must be greater than 0."
                    });
                }

                // Validate input
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found."
                    });
                }

                // Update user fields
                if (!string.IsNullOrEmpty(request.FirstName))
                    user.FirstName = request.FirstName;
                
                if (!string.IsNullOrEmpty(request.LastName))
                    user.LastName = request.LastName;
                
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                    user.PhoneNumber = request.PhoneNumber;
                
                if (!string.IsNullOrEmpty(request.Organization))
                    user.Organization = request.Organization;
                
                if (!string.IsNullOrEmpty(request.Title))
                    user.Title = request.Title;
                
                if (!string.IsNullOrEmpty(request.Role))
                    user.Role = request.Role;
                
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin updated user {UserId}", id);

                return Ok(new
                {
                    success = true,
                    message = "User updated successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the user."
                });
            }
        }

        /// <summary>
        /// Disable user (admin only)
        /// </summary>
        [HttpPost("users/{id}/disable")]
        public async Task<ActionResult<object>> DisableUser(int id)
        {
            try
            {
                // Validate ID parameter
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "User ID must be greater than 0."
                    });
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found."
                    });
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin disabled user {UserId}", id);

                return Ok(new
                {
                    success = true,
                    message = "User disabled successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling user {UserId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while disabling the user."
                });
            }
        }

        /// <summary>
        /// Enable user (admin only)
        /// </summary>
        [HttpPost("users/{id}/enable")]
        public async Task<ActionResult<object>> EnableUser(int id)
        {
            try
            {
                // Validate ID parameter
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "User ID must be greater than 0."
                    });
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found."
                    });
                }

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin enabled user {UserId}", id);

                return Ok(new
                {
                    success = true,
                    message = "User enabled successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling user {UserId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while enabling the user."
                });
            }
        }

        /// <summary>
        /// Delete user (admin only)
        /// </summary>
        [HttpDelete("users/{id}")]
        public async Task<ActionResult<object>> DeleteUser(int id)
        {
            try
            {
                // Validate ID parameter
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "User ID must be greater than 0."
                    });
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found."
                    });
                }

                // Prevent deletion of the last admin
                if (user.Role.ToLower() == "admin")
                {
                    var adminCount = await _context.Users.CountAsync(u => u.Role.ToLower() == "admin");
                    if (adminCount <= 1)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Cannot delete the last admin user."
                        });
                    }
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin deleted user {UserId}", id);

                return Ok(new
                {
                    success = true,
                    message = "User deleted successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the user."
                });
            }
        }

        /// <summary>
        /// Get system statistics (admin only)
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetStats()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var adminUsers = await _context.Users.CountAsync(u => u.Role.ToLower() == "admin");
                var moderatorUsers = await _context.Users.CountAsync(u => u.Role.ToLower() == "moderator");
                var regularUsers = await _context.Users.CountAsync(u => u.Role.ToLower() == "user");

                return Ok(new
                {
                    success = true,
                    stats = new
                    {
                        totalUsers,
                        activeUsers,
                        adminUsers,
                        moderatorUsers,
                        regularUsers,
                        inactiveUsers = totalUsers - activeUsers
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system statistics");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving system statistics."
                });
            }
        }
    }

    /// <summary>
    /// Request model for admin user updates
    /// </summary>
    public class AdminUserUpdateRequest
    {
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string? FirstName { get; set; }
        
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string? LastName { get; set; }
        
        [Phone]
        [MaxLength(20)]
        [RegularExpression(@"^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "Please enter a valid phone number")]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(200)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'\.&]+$", ErrorMessage = "Organization can only contain letters, numbers, spaces, hyphens, apostrophes, periods, and ampersands")]
        public string? Organization { get; set; }
        
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Title can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string? Title { get; set; }
        
        [RegularExpression("^(user|parent|caregiver|therapist|adoptiveparent|admin)$", 
            ErrorMessage = "Role must be one of: user, parent, caregiver, therapist, adoptiveparent, admin")]
        public string? Role { get; set; }
    }
} 