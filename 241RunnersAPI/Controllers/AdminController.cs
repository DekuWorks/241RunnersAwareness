using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using _241RunnersAPI.Services;
using System.Security.Claims;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly ValidationService _validationService;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, ValidationService validationService)
        {
            _context = context;
            _logger = logger;
            _validationService = validationService;
        }

        /// <summary>
        /// Create cases for all existing runners
        /// </summary>
        [HttpPost("create-cases-for-runners")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateCasesForRunners()
        {
            try
            {
                // Get all runners that don't have cases yet
                var runnersWithoutCases = await _context.Runners
                    .Where(r => !_context.Cases.Any(c => c.RunnerId == r.Id))
                    .Include(r => r.User)
                    .ToListAsync();

                if (!runnersWithoutCases.Any())
                {
                    return Ok(new { success = true, message = "All runners already have cases", count = 0 });
                }

                // Get the first admin user to be the reporter
                var adminUser = await _context.Users
                    .Where(u => u.Role == "admin")
                    .FirstOrDefaultAsync();

                if (adminUser == null)
                {
                    return BadRequest(new { success = false, message = "No admin user found to create cases" });
                }

                var cases = new List<Case>();

                foreach (var runner in runnersWithoutCases)
                {
                    var newCase = new Case
                    {
                        RunnerId = runner.Id,
                        ReportedByUserId = adminUser.Id,
                        Title = $"Missing Person Case - {runner.Name}",
                        Description = $"Case for missing person {runner.Name}. {runner.PhysicalDescription ?? "No additional description available."}",
                        LastSeenDate = DateTime.UtcNow.AddDays(-1), // Default to yesterday
                        LastSeenLocation = runner.LastKnownLocation ?? "Location unknown",
                        LastSeenTime = "Unknown",
                        LastSeenCircumstances = "Reported missing",
                        ClothingDescription = "Unknown",
                        PhysicalCondition = "Unknown",
                        MentalState = "Unknown",
                        AdditionalInformation = runner.MedicalConditions ?? "No additional information",
                        Status = "Active",
                        Priority = "Medium",
                        IsPublic = true,
                        IsApproved = true,
                        IsVerified = false,
                        ContactPersonName = adminUser.FirstName + " " + adminUser.LastName,
                        ContactPersonPhone = adminUser.PhoneNumber,
                        ContactPersonEmail = adminUser.Email,
                        EmergencyContactName = runner.User?.EmergencyContactName,
                        EmergencyContactPhone = runner.User?.EmergencyContactPhone,
                        EmergencyContactRelationship = runner.User?.EmergencyContactRelationship,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    cases.Add(newCase);
                }

                _context.Cases.AddRange(cases);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {Count} cases for existing runners", cases.Count);

                return Ok(new { 
                    success = true, 
                    message = $"Successfully created {cases.Count} cases for runners",
                    count = cases.Count,
                    cases = cases.Select(c => new { c.Id, c.Title, RunnerName = c.Runner.Name })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cases for runners");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }


        /// <summary>
        /// Get all admin users
        /// </summary>
        [HttpGet("admins")]
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
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
        /// Get all users (admin only) - for admin dashboard
        /// </summary>
        [HttpGet("users")]
        [Authorize(Roles = "admin")]
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
        /// Update user status (admin only)
        /// </summary>
        [HttpPut("users/{id}/status")]
        [Authorize(Roles = "admin")]
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

        /// <summary>
        /// Update user profile (admin only) with comprehensive validation
        /// </summary>
        [HttpPut("users/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto updateData)
        {
            try
            {
                // Validate the update data
                var validationResult = await _validationService.ValidateUserUpdate(id, updateData);
                
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Validation failed",
                        errors = validationResult.Errors.Select(e => new { e.Message, e.Code }),
                        warnings = validationResult.Warnings.Select(w => new { w.Message, w.Code })
                    });
                }

                // Find the user to update
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Update user fields
                user.FirstName = updateData.FirstName;
                user.LastName = updateData.LastName;
                user.PhoneNumber = updateData.PhoneNumber;
                user.Address = updateData.Address;
                user.City = updateData.City;
                user.State = updateData.State;
                user.ZipCode = updateData.ZipCode;
                user.Organization = updateData.Organization;
                user.Title = updateData.Title;
                user.Credentials = updateData.Credentials;
                user.Specialization = updateData.Specialization;
                user.YearsOfExperience = updateData.YearsOfExperience;
                user.EmergencyContactName = updateData.EmergencyContactName;
                user.EmergencyContactPhone = updateData.EmergencyContactPhone;
                user.EmergencyContactRelationship = updateData.EmergencyContactRelationship;
                user.Notes = updateData.Notes;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("User profile updated for user {UserId} ({Email})", user.Id, user.Email);

                // Return updated user data
                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    UpdatedAt = user.UpdatedAt,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    City = user.City,
                    State = user.State,
                    ZipCode = user.ZipCode,
                    Organization = user.Organization,
                    Title = user.Title,
                    Credentials = user.Credentials,
                    Specialization = user.Specialization,
                    YearsOfExperience = user.YearsOfExperience,
                    EmergencyContactName = user.EmergencyContactName,
                    EmergencyContactPhone = user.EmergencyContactPhone,
                    EmergencyContactRelationship = user.EmergencyContactRelationship,
                    IsEmailVerified = user.IsEmailVerified,
                    IsPhoneVerified = user.IsPhoneVerified
                };

                return Ok(new { 
                    success = true, 
                    message = "User updated successfully",
                    user = userResponse,
                    warnings = validationResult.Warnings.Select(w => new { w.Message, w.Code })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all users (temporary - no auth for debugging)
        /// </summary>
        [HttpGet("users-debug")]
        public async Task<IActionResult> GetUsersDebug()
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
                _logger.LogError(ex, "Error retrieving users for debug");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a user (admin only) with comprehensive validation
        /// </summary>
        [HttpDelete("users/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                // Get current admin user ID from JWT token
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized(new { success = false, message = "Invalid authentication token" });
                }

                // Validate the deletion operation
                var validationResult = await _validationService.ValidateUserDeletion(id, currentUserId);
                
                if (!validationResult.IsValid)
                {
                    var firstError = validationResult.Errors.First();
                    return BadRequest(new { 
                        success = false, 
                        message = firstError.Message,
                        code = firstError.Code,
                        errors = validationResult.Errors.Select(e => new { e.Message, e.Code }),
                        warnings = validationResult.Warnings.Select(w => new { w.Message, w.Code })
                    });
                }

                // Find the user to delete
                var userToDelete = await _context.Users.FindAsync(id);
                if (userToDelete == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Check if we should deactivate instead of delete
                var hasRelatedData = validationResult.Warnings.Any(w => w.Code == "HAS_RELATED_DATA");
                
                if (hasRelatedData)
                {
                    // Mark as inactive instead of deleting
                    userToDelete.IsActive = false;
                    userToDelete.UpdatedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("User {UserId} ({Email}) marked as inactive due to related data", 
                        userToDelete.Id, userToDelete.Email);
                    
                    return Ok(new { 
                        success = true, 
                        message = "User marked as inactive due to related data (cases or runners)",
                        action = "deactivated",
                        warnings = validationResult.Warnings.Select(w => new { w.Message, w.Code })
                    });
                }

                // Safe to delete - remove from database
                _context.Users.Remove(userToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} ({Email}) deleted by admin {AdminId}", 
                    userToDelete.Id, userToDelete.Email, currentUserId);

                return Ok(new { 
                    success = true, 
                    message = "User deleted successfully",
                    action = "deleted"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete an admin user (admin only) with comprehensive validation
        /// </summary>
        [HttpDelete("admins/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            try
            {
                // Get current admin user ID from JWT token
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized(new { success = false, message = "Invalid authentication token" });
                }

                // Validate the admin deletion operation
                var validationResult = await _validationService.ValidateAdminDeletion(id, currentUserId);
                
                if (!validationResult.IsValid)
                {
                    var firstError = validationResult.Errors.First();
                    return BadRequest(new { 
                        success = false, 
                        message = firstError.Message,
                        code = firstError.Code,
                        errors = validationResult.Errors.Select(e => new { e.Message, e.Code }),
                        warnings = validationResult.Warnings.Select(w => new { w.Message, w.Code })
                    });
                }

                // Find the admin to delete
                var adminToDelete = await _context.Users.FindAsync(id);
                if (adminToDelete == null)
                {
                    return NotFound(new { success = false, message = "Admin not found" });
                }

                // Check if we should deactivate instead of delete
                var hasRelatedData = validationResult.Warnings.Any(w => w.Code == "HAS_RELATED_DATA");
                
                if (hasRelatedData)
                {
                    // Mark as inactive instead of deleting
                    adminToDelete.IsActive = false;
                    adminToDelete.UpdatedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Admin {AdminId} ({Email}) marked as inactive due to related data", 
                        adminToDelete.Id, adminToDelete.Email);
                    
                    return Ok(new { 
                        success = true, 
                        message = "Admin marked as inactive due to related data (cases or runners)",
                        action = "deactivated",
                        warnings = validationResult.Warnings.Select(w => new { w.Message, w.Code })
                    });
                }

                // Safe to delete - remove from database
                _context.Users.Remove(adminToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin {AdminId} ({Email}) deleted by admin {CurrentAdminId}", 
                    adminToDelete.Id, adminToDelete.Email, currentUserId);

                return Ok(new { 
                    success = true, 
                    message = "Admin deleted successfully",
                    action = "deleted"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting admin {AdminId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Report client-side errors
        /// </summary>
        [HttpPost("errors")]
        public IActionResult ReportError([FromBody] ErrorReportDto errorReport)
        {
            try
            {
                _logger.LogWarning("Client Error Report: {Message} | URL: {Url} | Severity: {Severity}", 
                    errorReport.Message, errorReport.Url, errorReport.Severity);

                // Log additional context if available
                if (errorReport.Context != null)
                {
                    _logger.LogInformation("Error Context: {Context}", System.Text.Json.JsonSerializer.Serialize(errorReport.Context));
                }

                return Ok(new { success = true, message = "Error reported successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing error report");
                return StatusCode(500, new { success = false, message = "Failed to process error report" });
            }
        }
    }

    public class UpdateUserStatusDto
    {
        public bool IsActive { get; set; }
    }

    public class ErrorReportDto
    {
        public string Id { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Stack { get; set; }
        public string Url { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Severity { get; set; } = "medium";
        public object? Context { get; set; }
    }
}
