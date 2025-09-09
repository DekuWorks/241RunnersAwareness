using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using System.Security.Claims;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
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
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = $"{u.FirstName} {u.LastName}",
                        Role = u.Role,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt,
                        UpdatedAt = u.UpdatedAt,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        City = u.City,
                        State = u.State,
                        ZipCode = u.ZipCode,
                        Organization = u.Organization,
                        Title = u.Title,
                        Credentials = u.Credentials,
                        Specialization = u.Specialization,
                        YearsOfExperience = u.YearsOfExperience,
                        ProfileImageUrl = u.ProfileImageUrl,
                        EmergencyContactName = u.EmergencyContactName,
                        EmergencyContactPhone = u.EmergencyContactPhone,
                        EmergencyContactRelationship = u.EmergencyContactRelationship,
                        IsEmailVerified = u.IsEmailVerified,
                        IsPhoneVerified = u.IsPhoneVerified,
                        EmailVerifiedAt = u.EmailVerifiedAt,
                        PhoneVerifiedAt = u.PhoneVerifiedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, users = users, count = users.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all admin users
        /// </summary>
        [HttpGet("admins")]
        public async Task<IActionResult> GetAdmins()
        {
            try
            {
                var admins = await _context.Users
                    .Where(u => u.Role == "admin")
                    .Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = $"{u.FirstName} {u.LastName}",
                        Role = u.Role,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt,
                        UpdatedAt = u.UpdatedAt,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        City = u.City,
                        State = u.State,
                        ZipCode = u.ZipCode,
                        Organization = u.Organization,
                        Title = u.Title,
                        Credentials = u.Credentials,
                        Specialization = u.Specialization,
                        YearsOfExperience = u.YearsOfExperience,
                        ProfileImageUrl = u.ProfileImageUrl,
                        EmergencyContactName = u.EmergencyContactName,
                        EmergencyContactPhone = u.EmergencyContactPhone,
                        EmergencyContactRelationship = u.EmergencyContactRelationship,
                        IsEmailVerified = u.IsEmailVerified,
                        IsPhoneVerified = u.IsPhoneVerified,
                        EmailVerifiedAt = u.EmailVerifiedAt,
                        PhoneVerifiedAt = u.PhoneVerifiedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, admins = admins, count = admins.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admins");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Seed admin users (development only)
        /// </summary>
        [HttpPost("seed-admins")]
        public async Task<IActionResult> SeedAdmins()
        {
            try
            {
                var adminUsers = new List<User>
                {
                    new User
                    {
                        Email = "dekuworks1@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("marcus2025"),
                        FirstName = "Marcus",
                        LastName = "Brown",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "System Administrator",
                        PhoneNumber = "15551234567",
                        Address = "123 Admin Street",
                        City = "Administration City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "danielcarey9770@yahoo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Daniel2025!"),
                        FirstName = "Daniel",
                        LastName = "Carey",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "15551234568",
                        Address = "124 Admin Street",
                        City = "Administration City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "lthomas3350@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lisa2025!"),
                        FirstName = "Lisa",
                        LastName = "Thomas",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "15551234569",
                        Address = "125 Admin Street",
                        City = "Administration City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "tinaleggins@yahoo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tina2025!"),
                        FirstName = "Tina",
                        LastName = "Matthews",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "15551234570",
                        Address = "126 Admin Street",
                        City = "Administration City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "mmelasky@iplawconsulting.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mark2025!"),
                        FirstName = "Mark",
                        LastName = "Melasky",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "IP Law Consulting",
                        Title = "Legal Administrator",
                        PhoneNumber = "15551234571",
                        Address = "127 Legal Street",
                        City = "Legal City",
                        State = "Legal State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "ralphfrank900@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Ralph2025!"),
                        FirstName = "Ralph",
                        LastName = "Frank",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "15551234572",
                        Address = "128 Admin Street",
                        City = "Administration City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    }
                };

                int addedCount = 0;
                int skippedCount = 0;

                foreach (var adminUser in adminUsers)
                {
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminUser.Email);
                    if (existingUser == null)
                    {
                        _context.Users.Add(adminUser);
                        addedCount++;
                        _logger.LogInformation("Added admin user: {Email} ({FirstName} {LastName})", 
                            adminUser.Email, adminUser.FirstName, adminUser.LastName);
                    }
                    else
                    {
                        skippedCount++;
                        _logger.LogInformation("Admin user already exists: {Email} ({FirstName} {LastName})", 
                            adminUser.Email, adminUser.FirstName, adminUser.LastName);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    success = true, 
                    message = "Admin users seeded successfully",
                    added = addedCount,
                    skipped = skippedCount,
                    total = adminUsers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding admin users");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalAdmins = await _context.Users.CountAsync(u => u.Role == "admin");
                var activeAdmins = await _context.Users.CountAsync(u => u.Role == "admin" && u.IsActive);
                var totalRunners = await _context.Runners.CountAsync();
                var activeRunners = await _context.Runners.CountAsync(r => r.IsActive);
                var totalCases = await _context.Cases.CountAsync();
                var openCases = await _context.Cases.CountAsync(c => c.Status == "Open");
                var publicCases = await _context.Cases.CountAsync(c => c.IsPublic && c.IsApproved);
                var verifiedCases = await _context.Cases.CountAsync(c => c.IsVerified);

                return Ok(new
                {
                    success = true,
                    stats = new
                    {
                        totalUsers,
                        totalAdmins,
                        activeAdmins,
                        totalRunners,
                        activeRunners,
                        totalCases,
                        openCases,
                        publicCases,
                        verifiedCases,
                        systemStatus = "healthy"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update user status (admin only)
        /// </summary>
        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                user.IsActive = dto.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "User status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status for user {UserId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    public class UpdateUserStatusDto
    {
        public bool IsActive { get; set; }
    }
}
