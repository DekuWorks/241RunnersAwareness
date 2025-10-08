using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using _241RunnersAPI.Services;
using Microsoft.Extensions.Logging;

namespace _241RunnersAPI.Controllers
{
    /// <summary>
    /// Database optimization and monitoring controller
    /// Provides endpoints for database health checks and optimization
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "admin")] // Only admins can access database optimization features
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseOptimizationService _databaseService;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(DatabaseOptimizationService databaseService, ILogger<DatabaseController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Get database health status
        /// </summary>
        /// <returns>Database health information</returns>
        [HttpGet("health")]
        public async Task<IActionResult> GetDatabaseHealth()
        {
            try
            {
                _logger.LogInformation("Database health check requested by {User}", User.Identity?.Name);
                
                var healthStatus = await _databaseService.GetDatabaseHealthAsync();
                
                return Ok(new
                {
                    success = true,
                    data = healthStatus,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database health status");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get database health status",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Get database performance metrics
        /// </summary>
        /// <returns>Database performance information</returns>
        [HttpGet("performance")]
        public async Task<IActionResult> GetDatabasePerformance()
        {
            try
            {
                _logger.LogInformation("Database performance metrics requested by {User}", User.Identity?.Name);
                
                var performanceMetrics = await _databaseService.GetPerformanceMetricsAsync();
                
                return Ok(new
                {
                    success = true,
                    data = performanceMetrics,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database performance metrics");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get database performance metrics",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Run database optimization analysis
        /// </summary>
        /// <returns>Database optimization recommendations</returns>
        [HttpPost("optimize")]
        public async Task<IActionResult> OptimizeDatabase()
        {
            try
            {
                _logger.LogInformation("Database optimization requested by {User}", User.Identity?.Name);
                
                var optimizationResult = await _databaseService.OptimizeDatabaseAsync();
                
                return Ok(new
                {
                    success = true,
                    data = optimizationResult,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to optimize database");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to optimize database",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Get database statistics summary
        /// </summary>
        /// <returns>Database statistics</returns>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDatabaseStats()
        {
            try
            {
                _logger.LogInformation("Database statistics requested by {User}", User.Identity?.Name);
                
                var healthStatus = await _databaseService.GetDatabaseHealthAsync();
                var performanceMetrics = await _databaseService.GetPerformanceMetricsAsync();
                
                var stats = new
                {
                    health = healthStatus,
                    performance = performanceMetrics,
                    summary = new
                    {
                        totalUsers = healthStatus.UserCount,
                        totalCases = healthStatus.CaseCount,
                        isHealthy = healthStatus.IsHealthy,
                        responseTime = healthStatus.ResponseTime,
                        lastChecked = healthStatus.Timestamp
                    }
                };
                
                return Ok(new
                {
                    success = true,
                    data = stats,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database statistics");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get database statistics",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }
    }
}
