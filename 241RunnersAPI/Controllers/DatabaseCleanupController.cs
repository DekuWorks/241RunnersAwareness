using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _241RunnersAPI.Services;

namespace _241RunnersAPI.Controllers
{
    /// <summary>
    /// Controller for database cleanup operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
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
        /// Get database statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDatabaseStats()
        {
            try
            {
                var stats = await _cleanupService.GetDatabaseStatsAsync();
                return Ok(new
                {
                    success = true,
                    data = stats,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database statistics");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving database statistics",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Run database cleanup
        /// </summary>
        [HttpPost("cleanup")]
        public async Task<IActionResult> RunCleanup()
        {
            try
            {
                await _cleanupService.CleanupExpiredDataAsync();
                return Ok(new
                {
                    success = true,
                    message = "Database cleanup completed successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database cleanup");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error during database cleanup",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
