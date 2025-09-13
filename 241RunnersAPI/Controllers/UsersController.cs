using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]
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
        /// Get all users with pagination and filtering
        /// </summary>
        [HttpGet]
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
                // Validate request
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return ValidationErrorResponse(new Dictionary<string, string[]>
                    {
                        { "email", new[] { "Email is required" } },
                        { "password", new[] { "Password is required" } }
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
