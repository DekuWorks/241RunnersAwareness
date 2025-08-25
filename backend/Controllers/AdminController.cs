using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
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
    }

    public class UpdateCaseStatusRequest
    {
        public string Status { get; set; }
        public DateTime? LastSeenDate { get; set; }
    }
}
