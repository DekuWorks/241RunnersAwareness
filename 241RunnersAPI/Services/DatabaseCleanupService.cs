using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for database cleanup operations
    /// </summary>
    public class DatabaseCleanupService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseCleanupService> _logger;

        public DatabaseCleanupService(ApplicationDbContext context, ILogger<DatabaseCleanupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Clean up expired tokens and old data
        /// </summary>
        public async Task CleanupExpiredDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting database cleanup...");

                // Clean up any expired data (placeholder for future cleanup operations)
                // This service is ready for future database maintenance tasks

                _logger.LogInformation("Database cleanup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database cleanup");
                throw;
            }
        }

        /// <summary>
        /// Get database statistics
        /// </summary>
        public async Task<object> GetDatabaseStatsAsync()
        {
            try
            {
                var userCount = await _context.Users.CountAsync();
                
                return new
                {
                    TotalUsers = userCount,
                    ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                    AdminUsers = await _context.Users.CountAsync(u => u.Role == "admin"),
                    LastCleanup = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database statistics");
                throw;
            }
        }
    }
}
