using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using _241RunnersAwareness.BackendAPI.Services;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class UserManagementController : ControllerBase
    {
        private readonly RunnersDbContext _context;
        private readonly IAuthService _authService;

        public UserManagementController(RunnersDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: api/usermanagement/users
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserManagementDto>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new UserManagementDto
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Role = u.Role,
                        EmailVerified = u.EmailVerified,
                        PhoneVerified = u.PhoneVerified,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt,
                        IsActive = u.IsActive,
                        Organization = u.Organization,
                        Credentials = u.Credentials,
                        Specialization = u.Specialization,
                        YearsOfExperience = u.YearsOfExperience
                    })
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving users", error = ex.Message });
            }
        }

        // GET: api/usermanagement/users/{id}
        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserManagementDto>> GetUser(Guid id)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.UserId == id)
                    .Select(u => new UserManagementDto
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Role = u.Role,
                        EmailVerified = u.EmailVerified,
                        PhoneVerified = u.PhoneVerified,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt,
                        IsActive = u.IsActive,
                        Organization = u.Organization,
                        Credentials = u.Credentials,
                        Specialization = u.Specialization,
                        YearsOfExperience = u.YearsOfExperience,
                        Address = u.Address,
                        City = u.City,
                        State = u.State,
                        ZipCode = u.ZipCode,
                        EmergencyContactName = u.EmergencyContactName,
                        EmergencyContactPhone = u.EmergencyContactPhone,
                        EmergencyContactRelationship = u.EmergencyContactRelationship
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user", error = ex.Message });
            }
        }

        // POST: api/usermanagement/users
        [HttpPost("users")]
        public async Task<ActionResult<UserManagementDto>> CreateUser(CreateUserRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Username);

                if (existingUser != null)
                {
                    return BadRequest(new { message = "User with this email or username already exists" });
                }

                // Create new user
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = request.Username,
                    Email = request.Email,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = _authService.HashPassword(request.Password),
                    Role = request.Role ?? "user",
                    Organization = request.Organization,
                    Credentials = request.Credentials,
                    Specialization = request.Specialization,
                    YearsOfExperience = request.YearsOfExperience,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    ZipCode = request.ZipCode,
                    EmergencyContactName = request.EmergencyContactName,
                    EmergencyContactPhone = request.EmergencyContactPhone,
                    EmergencyContactRelationship = request.EmergencyContactRelationship,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userDto = new UserManagementDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    EmailVerified = user.EmailVerified,
                    PhoneVerified = user.PhoneVerified,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    IsActive = user.IsActive,
                    Organization = user.Organization,
                    Credentials = user.Credentials,
                    Specialization = user.Specialization,
                    YearsOfExperience = user.YearsOfExperience
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating user", error = ex.Message });
            }
        }

        // PUT: api/usermanagement/users/{id}
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Update user properties
                user.Username = request.Username ?? user.Username;
                user.Email = request.Email ?? user.Email;
                user.FullName = request.FullName ?? user.FullName;
                user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
                user.Role = request.Role ?? user.Role;
                user.Organization = request.Organization ?? user.Organization;
                user.Credentials = request.Credentials ?? user.Credentials;
                user.Specialization = request.Specialization ?? user.Specialization;
                user.YearsOfExperience = request.YearsOfExperience ?? user.YearsOfExperience;
                user.Address = request.Address ?? user.Address;
                user.City = request.City ?? user.City;
                user.State = request.State ?? user.State;
                user.ZipCode = request.ZipCode ?? user.ZipCode;
                user.EmergencyContactName = request.EmergencyContactName ?? user.EmergencyContactName;
                user.EmergencyContactPhone = request.EmergencyContactPhone ?? user.EmergencyContactPhone;
                user.EmergencyContactRelationship = request.EmergencyContactRelationship ?? user.EmergencyContactRelationship;
                user.IsActive = request.IsActive ?? user.IsActive;

                // Update password if provided
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordHash = _authService.HashPassword(request.Password);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating user", error = ex.Message });
            }
        }

        // DELETE: api/usermanagement/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Soft delete - set IsActive to false
                user.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "User deactivated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting user", error = ex.Message });
            }
        }

        // POST: api/usermanagement/users/{id}/activate
        [HttpPost("users/{id}/activate")]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.IsActive = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "User activated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error activating user", error = ex.Message });
            }
        }

        // GET: api/usermanagement/roles
        [HttpGet("roles")]
        public ActionResult<IEnumerable<string>> GetRoles()
        {
            var roles = new List<string>
            {
                "admin",
                "user",
                "therapist",
                "caregiver",
                "parent",
                "adoptive_parent"
            };

            return Ok(roles);
        }

        // GET: api/usermanagement/stats
        [HttpGet("stats")]
        public async Task<ActionResult<UserStats>> GetUserStats()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var adminUsers = await _context.Users.CountAsync(u => u.Role == "admin" && u.IsActive);
                var verifiedUsers = await _context.Users.CountAsync(u => u.EmailVerified && u.IsActive);

                var stats = new UserStats
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    AdminUsers = adminUsers,
                    VerifiedUsers = verifiedUsers,
                    InactiveUsers = totalUsers - activeUsers
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user stats", error = ex.Message });
            }
        }
    }

    // DTOs for user management
    public class UserManagementDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string Organization { get; set; }
        public string Credentials { get; set; }
        public string Specialization { get; set; }
        public string YearsOfExperience { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string EmergencyContactRelationship { get; set; }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; } // Optional, will be auto-generated
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Organization { get; set; }
        public string Credentials { get; set; }
        public string Specialization { get; set; }
        public string YearsOfExperience { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string EmergencyContactRelationship { get; set; }
    }

    public class UpdateUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; } // Optional, will be auto-generated
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Organization { get; set; }
        public string Credentials { get; set; }
        public string Specialization { get; set; }
        public string YearsOfExperience { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string EmergencyContactRelationship { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UserStats
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int InactiveUsers { get; set; }
    }
}
