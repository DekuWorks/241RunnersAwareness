using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Serilog;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin,superadmin")]
    public class AdminController : ControllerBase
    {
        private readonly RunnersDbContext _context;

        public AdminController(RunnersDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard-stats")]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            try
            {
                var totalCases = await _context.Individuals.CountAsync();
                var activeUsers = await _context.Users.Where(u => u.IsActive).CountAsync();
                var resolvedCases = await _context.Individuals.Where(i => i.CurrentStatus == "Found" || i.CurrentStatus == "Safe").CountAsync();
                var pendingCases = await _context.Individuals.Where(i => i.CurrentStatus == "Missing" || i.CurrentStatus == "Urgent").CountAsync();

                return Ok(new
                {
                    totalCases,
                    activeUsers,
                    resolvedCases,
                    pendingCases,
                    lastUpdated = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching dashboard statistics", error = ex.Message });
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
                        u.UserId,
                        u.Username,
                        u.Email,
                        u.FullName,
                        u.Role,
                        u.IsActive,
                        u.CreatedAt,
                        u.LastLoginAt
                    })
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching users", error = ex.Message });
            }
        }

        [HttpGet("cases")]
        public async Task<ActionResult<object>> GetCases()
        {
            try
            {
                var cases = await _context.Individuals
                    .Select(i => new
                    {
                        i.Id,
                        Name = $"{i.FirstName} {i.LastName}",
                        i.CurrentStatus,
                        i.LastSeenDate,
                        i.City,
                        i.State,
                        i.RiskLevel
                    })
                    .OrderByDescending(c => c.LastSeenDate)
                    .ToListAsync();

                return Ok(cases);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching cases", error = ex.Message });
            }
        }

        [HttpPost("users/{userId}/toggle-status")]
        public async Task<ActionResult<object>> ToggleUserStatus(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully",
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating user status", error = ex.Message });
            }
        }

        [HttpDelete("users/{userId}")]
        public async Task<ActionResult<object>> DeleteUser(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Don't allow deletion of admin users
                if (user.Role == "admin" || user.Role == "superadmin")
                {
                    return BadRequest(new { message = "Cannot delete admin users" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting user", error = ex.Message });
            }
        }

        [HttpPost("cases/{caseId}/update-status")]
        public async Task<ActionResult<object>> UpdateCaseStatus(int caseId, [FromBody] UpdateCaseStatusRequest request)
        {
            try
            {
                var individual = await _context.Individuals.FindAsync(caseId);
                if (individual == null)
                {
                    return NotFound(new { message = "Case not found" });
                }

                individual.CurrentStatus = request.Status;
                individual.LastSeenDate = request.LastSeenDate ?? individual.LastSeenDate;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Case status updated successfully",
                    status = individual.CurrentStatus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating case status", error = ex.Message });
            }
        }

        [HttpDelete("cases/{caseId}")]
        public async Task<ActionResult<object>> DeleteCase(int caseId)
        {
            try
            {
                var individual = await _context.Individuals.FindAsync(caseId);
                if (individual == null)
                {
                    return NotFound(new { message = "Case not found" });
                }

                _context.Individuals.Remove(individual);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Case deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting case", error = ex.Message });
            }
        }

        // Create new admin user
        [HttpPost("create-admin")]
        public async Task<ActionResult<object>> CreateAdmin([FromBody] CreateAdminRequest request)
        {
            try
            {
                // Validate ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();
                    
                    return BadRequest(new { 
                        message = "Validation failed", 
                        errors = errors 
                    });
                }

                // Log the incoming request for debugging
                Log.Information("Creating admin user: {Email}, {FirstName}, {LastName}", 
                    request.Email, request.FirstName, request.LastName);

                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    return BadRequest(new { message = "User with this email already exists" });
                }

                // Create new admin user
                var newAdmin = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = request.Username ?? request.Email.Split('@')[0],
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    FullName = $"{request.FirstName} {request.LastName}",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = request.Role ?? "admin",
                    Organization = request.Organization,
                    Credentials = request.Credentials,
                    Specialization = request.Specialization,
                    YearsOfExperience = request.YearsOfExperience,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailVerified = true
                };

                Log.Information("Adding admin user to context: {UserId}, {Email}, {FirstName}, {LastName}", 
                    newAdmin.UserId, newAdmin.Email, newAdmin.FirstName, newAdmin.LastName);

                _context.Users.Add(newAdmin);
                await _context.SaveChangesAsync();

                Log.Information("Admin user created successfully: {UserId}", newAdmin.UserId);

                return Ok(new { 
                    message = "Admin user created successfully",
                    userId = newAdmin.UserId,
                    email = newAdmin.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating admin user", error = ex.Message });
            }
        }

        // Reset admin password
        [HttpPost("reset-admin-password")]
        public async Task<ActionResult<object>> ResetAdminPassword([FromBody] AdminPasswordResetRequest request)
        {
            try
            {
                // Find the admin user
                var adminUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && 
                                            (u.Role == "admin" || u.Role == "superadmin"));

                if (adminUser == null)
                {
                    return NotFound(new { message = "Admin user not found" });
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, adminUser.PasswordHash))
                {
                    return BadRequest(new { message = "Current password is incorrect" });
                }

                // Validate new password strength
                if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 8)
                {
                    return BadRequest(new { message = "New password must be at least 8 characters long" });
                }

                // Update password
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                adminUser.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Admin password reset successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error resetting admin password", error = ex.Message });
            }
        }

        // Get all admin users
        [HttpGet("admins")]
        public async Task<ActionResult<object>> GetAdminUsers()
        {
            try
            {
                var adminUsers = await _context.Users
                    .Where(u => u.Role == "admin" || u.Role == "superadmin")
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.FullName,
                        u.Role,
                        u.Organization,
                        u.Credentials,
                        u.Specialization,
                        u.YearsOfExperience,
                        u.IsActive,
                        u.EmailVerified,
                        u.CreatedAt,
                        u.LastLoginAt
                    })
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                return Ok(adminUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching admin users", error = ex.Message });
            }
        }

        // Get admin user by ID
        [HttpGet("admins/{userId}")]
        public async Task<ActionResult<object>> GetAdminById(Guid userId)
        {
            try
            {
                var adminUser = await _context.Users
                    .Where(u => (u.Role == "admin" || u.Role == "superadmin") && u.UserId == userId)
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.FullName,
                        u.Role,
                        u.Organization,
                        u.Credentials,
                        u.Specialization,
                        u.YearsOfExperience,
                        u.IsActive,
                        u.EmailVerified,
                        u.CreatedAt,
                        u.LastLoginAt
                    })
                    .FirstOrDefaultAsync();

                if (adminUser == null)
                {
                    return NotFound(new { message = "Admin user not found" });
                }

                return Ok(adminUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching admin user", error = ex.Message });
            }
        }

        // Update admin user
        [HttpPut("update-admin/{userId}")]
        public async Task<ActionResult<object>> UpdateAdmin(Guid userId, [FromBody] UpdateAdminRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.FirstName) || 
                    string.IsNullOrEmpty(request.LastName))
                {
                    return BadRequest(new { message = "Email, first name, and last name are required" });
                }

                // Validate email format
                if (!System.Text.RegularExpressions.Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return BadRequest(new { message = "Invalid email format" });
                }

                var adminUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId && 
                                            (u.Role == "admin" || u.Role == "superadmin"));

                if (adminUser == null)
                {
                    return NotFound(new { message = "Admin user not found" });
                }

                // Check if email is already taken by another user
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.UserId != userId);

                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email is already taken by another user" });
                }

                // Update admin user
                adminUser.Email = request.Email;
                adminUser.FirstName = request.FirstName;
                adminUser.LastName = request.LastName;
                adminUser.FullName = $"{request.FirstName} {request.LastName}";
                adminUser.Username = request.Username ?? request.Email.Split('@')[0];
                adminUser.Role = request.Role ?? "admin";
                adminUser.Organization = request.Organization;
                adminUser.Credentials = request.Credentials;
                adminUser.Specialization = request.Specialization;
                adminUser.YearsOfExperience = request.YearsOfExperience;
                adminUser.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Admin user updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating admin user", error = ex.Message });
            }
        }

        // Delete admin user
        [HttpDelete("delete-admin/{userId}")]
        public async Task<ActionResult<object>> DeleteAdmin(Guid userId)
        {
            try
            {
                var adminUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId && 
                                            (u.Role == "admin" || u.Role == "superadmin"));

                if (adminUser == null)
                {
                    return NotFound(new { message = "Admin user not found" });
                }

                // Don't allow deletion of the last admin
                var adminCount = await _context.Users
                    .Where(u => u.Role == "admin" || u.Role == "superadmin")
                    .CountAsync();

                if (adminCount <= 1)
                {
                    return BadRequest(new { message = "Cannot delete the last admin user" });
                }

                // Don't allow deletion of superadmin users
                if (adminUser.Role == "superadmin")
                {
                    return BadRequest(new { message = "Cannot delete superadmin users" });
                }

                _context.Users.Remove(adminUser);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Admin user deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting admin user", error = ex.Message });
            }
        }

        // Reset admin password (by another admin)
        [HttpPost("reset-admin-password-by-admin")]
        public async Task<ActionResult<object>> ResetAdminPasswordByAdmin([FromBody] AdminPasswordResetByAdminRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { message = "User ID and new password are required" });
                }

                // Validate password strength
                if (request.NewPassword.Length < 8)
                {
                    return BadRequest(new { message = "Password must be at least 8 characters long" });
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(request.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]"))
                {
                    return BadRequest(new { message = "Password must contain uppercase, lowercase, number, and special character" });
                }

                var adminUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == Guid.Parse(request.UserId) && 
                                            (u.Role == "admin" || u.Role == "superadmin"));

                if (adminUser == null)
                {
                    return NotFound(new { message = "Admin user not found" });
                }

                // Update password
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                adminUser.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Admin password reset successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error resetting admin password", error = ex.Message });
            }
        }
    }

    public class UpdateCaseStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? LastSeenDate { get; set; }
    }

    public class CreateAdminRequest
    {
        public string? Username { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Role is required")]
        [RegularExpression(@"^(admin|superadmin)$", ErrorMessage = "Role must be either 'admin' or 'superadmin'")]
        public string Role { get; set; } = "admin";
        
        [StringLength(100, ErrorMessage = "Organization cannot exceed 100 characters")]
        public string? Organization { get; set; }
        
        [StringLength(100, ErrorMessage = "Credentials cannot exceed 100 characters")]
        public string? Credentials { get; set; }
        
        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
        public string? Specialization { get; set; }
        
        [StringLength(20, ErrorMessage = "Years of experience cannot exceed 20 characters")]
        public string? YearsOfExperience { get; set; }
    }

    public class AdminPasswordResetRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateAdminRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string? Username { get; set; }
        
        [Required(ErrorMessage = "Role is required")]
        [RegularExpression(@"^(admin|superadmin)$", ErrorMessage = "Role must be either 'admin' or 'superadmin'")]
        public string Role { get; set; } = "admin";
        
        [StringLength(100, ErrorMessage = "Organization cannot exceed 100 characters")]
        public string? Organization { get; set; }
        
        [StringLength(100, ErrorMessage = "Credentials cannot exceed 100 characters")]
        public string? Credentials { get; set; }
        
        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
        public string? Specialization { get; set; }
        
        [StringLength(20, ErrorMessage = "Years of experience cannot exceed 20 characters")]
        public string? YearsOfExperience { get; set; }
    }

    public class AdminPasswordResetByAdminRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
