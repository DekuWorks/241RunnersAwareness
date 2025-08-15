using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _241RunnersAwareness.BackendAPI.Services;
using Microsoft.Extensions.Logging;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    /// <summary>
    /// Controller for database cleanup and maintenance operations
    /// Requires admin authorization for all operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CleanupController : ControllerBase
    {
        private readonly IDataCleanupService _cleanupService;
        private readonly ILogger<CleanupController> _logger;

        public CleanupController(IDataCleanupService cleanupService, ILogger<CleanupController> logger)
        {
            _cleanupService = cleanupService;
            _logger = logger;
        }

        /// <summary>
        /// Removes test data from the database
        /// </summary>
        [HttpPost("remove-test-data")]
        public async Task<IActionResult> RemoveTestData()
        {
            try
            {
                _logger.LogInformation("Test data removal requested by user: {UserId}", User.Identity?.Name);
                
                var count = await _cleanupService.RemoveTestDataAsync();
                
                return Ok(new { 
                    success = true, 
                    message = $"Successfully removed {count} test records",
                    recordsRemoved = count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing test data");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error removing test data",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Removes duplicate users based on email
        /// </summary>
        [HttpPost("remove-duplicates")]
        public async Task<IActionResult> RemoveDuplicates()
        {
            try
            {
                _logger.LogInformation("Duplicate removal requested by user: {UserId}", User.Identity?.Name);
                
                var count = await _cleanupService.RemoveDuplicateUsersAsync();
                
                return Ok(new { 
                    success = true, 
                    message = $"Successfully removed {count} duplicate records",
                    recordsRemoved = count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing duplicates");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error removing duplicates",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Anonymizes test data instead of removing it
        /// </summary>
        [HttpPost("anonymize-test-data")]
        public async Task<IActionResult> AnonymizeTestData()
        {
            try
            {
                _logger.LogInformation("Test data anonymization requested by user: {UserId}", User.Identity?.Name);
                
                var count = await _cleanupService.AnonymizeTestDataAsync();
                
                return Ok(new { 
                    success = true, 
                    message = $"Successfully anonymized {count} test records",
                    recordsAnonymized = count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anonymizing test data");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error anonymizing test data",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Removes orphaned records that have no references
        /// </summary>
        [HttpPost("remove-orphaned")]
        public async Task<IActionResult> RemoveOrphanedRecords()
        {
            try
            {
                _logger.LogInformation("Orphaned records removal requested by user: {UserId}", User.Identity?.Name);
                
                var count = await _cleanupService.CleanupOrphanedRecordsAsync();
                
                return Ok(new { 
                    success = true, 
                    message = $"Successfully removed {count} orphaned records",
                    recordsRemoved = count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing orphaned records");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error removing orphaned records",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Runs a full cleanup operation and returns a detailed report
        /// </summary>
        [HttpPost("full-cleanup")]
        public async Task<IActionResult> FullCleanup()
        {
            try
            {
                _logger.LogInformation("Full cleanup requested by user: {UserId}", User.Identity?.Name);
                
                var report = await _cleanupService.RunFullCleanupAsync();
                
                return Ok(new { 
                    success = report.Success, 
                    message = report.Success ? "Full cleanup completed successfully" : "Full cleanup failed",
                    report = new
                    {
                        startedAt = report.StartedAt,
                        completedAt = report.CompletedAt,
                        duration = report.Duration.ToString(),
                        testDataRemoved = report.TestDataRemoved,
                        duplicatesRemoved = report.DuplicatesRemoved,
                        orphanedRecordsRemoved = report.OrphanedRecordsRemoved,
                        totalRecordsRemoved = report.TotalRecordsRemoved,
                        errorMessage = report.ErrorMessage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during full cleanup");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error during full cleanup",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Gets cleanup statistics without performing any operations
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetCleanupStats()
        {
            try
            {
                // This would typically query the database to get statistics
                // For now, returning a placeholder response
                return Ok(new { 
                    success = true, 
                    message = "Cleanup statistics retrieved",
                    stats = new
                    {
                        totalUsers = 0, // Would be populated from actual query
                        totalIndividuals = 0,
                        totalProducts = 0,
                        lastCleanupDate = DateTime.UtcNow.AddDays(-7), // Would be from actual data
                        estimatedTestData = 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cleanup statistics");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error getting cleanup statistics",
                    error = ex.Message 
                });
            }
        }
    }
}
