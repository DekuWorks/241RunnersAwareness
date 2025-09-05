using Microsoft.AspNetCore.Mvc;
using _241RunnersAwarenessAPI.Services;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseCleanupController : ControllerBase
    {
        private readonly DatabaseCleanupService _cleanupService;
        private readonly ILogger<DatabaseCleanupController> _logger;

        public DatabaseCleanupController(DatabaseCleanupService cleanupService, ILogger<DatabaseCleanupController> logger)
        {
            _cleanupService = cleanupService;
            _logger = logger;
        }

        /// <summary>
        /// Cleans up the database by removing all non-admin users and all runners
        /// </summary>
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupDatabase()
        {
            try
            {
                _logger.LogInformation("Database cleanup requested");
                
                var result = await _cleanupService.CleanupDatabaseAsync();
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Database cleanup completed successfully",
                        data = result
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Database cleanup failed",
                        error = result.ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database cleanup");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error during cleanup",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Validates that admin credentials are intact
        /// </summary>
        [HttpGet("validate-admins")]
        public async Task<IActionResult> ValidateAdminCredentials()
        {
            try
            {
                _logger.LogInformation("Admin validation requested");
                
                var result = await _cleanupService.ValidateAdminCredentialsAsync();
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Admin validation completed",
                        data = result
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Admin validation failed",
                        error = result.ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin validation");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error during validation",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets current database statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDatabaseStats()
        {
            try
            {
                var validation = await _cleanupService.ValidateAdminCredentialsAsync();
                
                return Ok(new
                {
                    success = true,
                    message = "Database statistics retrieved",
                    data = new
                    {
                        adminCount = validation.AdminCount,
                        admins = validation.ValidAdmins.Select(a => new
                        {
                            id = a.Id,
                            email = a.Email,
                            name = $"{a.FirstName} {a.LastName}",
                            role = a.Role,
                            isActive = a.IsActive,
                            hasPassword = a.HasPasswordHash,
                            createdAt = a.CreatedAt
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database stats");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }
    }
}
