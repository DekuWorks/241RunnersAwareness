using Microsoft.EntityFrameworkCore;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Models;

namespace _241RunnersAwarenessAPI.Services
{
    /// <summary>
    /// Service for cleaning up database by removing non-admin users and all runners
    /// while preserving admin accounts and their credentials
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
        /// Cleans up the database by removing all non-admin users and all runners
        /// </summary>
        public async Task<CleanupResult> CleanupDatabaseAsync()
        {
            var result = new CleanupResult();
            
            try
            {
                _logger.LogInformation("Starting database cleanup...");

                // Get admin users before cleanup
                var adminUsers = await _context.Users
                    .Where(u => u.Role.ToLower() == "admin")
                    .ToListAsync();

                result.AdminUsersPreserved = adminUsers.Count;
                _logger.LogInformation($"Found {adminUsers.Count} admin users to preserve");

                // Remove all non-admin users
                var nonAdminUsers = await _context.Users
                    .Where(u => u.Role.ToLower() != "admin")
                    .ToListAsync();

                if (nonAdminUsers.Any())
                {
                    _context.Users.RemoveRange(nonAdminUsers);
                    result.NonAdminUsersRemoved = nonAdminUsers.Count;
                    _logger.LogInformation($"Removing {nonAdminUsers.Count} non-admin users");
                }

                // Remove all runners
                var allRunners = await _context.Runners.ToListAsync();
                if (allRunners.Any())
                {
                    _context.Runners.RemoveRange(allRunners);
                    result.RunnersRemoved = allRunners.Count;
                    _logger.LogInformation($"Removing {allRunners.Count} runners");
                }

                // Remove all public cases
                var allPublicCases = await _context.PublicCases.ToListAsync();
                if (allPublicCases.Any())
                {
                    _context.PublicCases.RemoveRange(allPublicCases);
                    result.PublicCasesRemoved = allPublicCases.Count;
                    _logger.LogInformation($"Removing {allPublicCases.Count} public cases");
                }

                // Save changes
                await _context.SaveChangesAsync();

                // Verify admin users are still there
                var remainingAdmins = await _context.Users
                    .Where(u => u.Role.ToLower() == "admin")
                    .ToListAsync();

                result.AdminUsersAfterCleanup = remainingAdmins.Count;
                result.Success = true;

                _logger.LogInformation($"Database cleanup completed successfully. {remainingAdmins.Count} admin users preserved.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database cleanup");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Validates that admin users and their credentials are intact
        /// </summary>
        public async Task<ValidationResult> ValidateAdminCredentialsAsync()
        {
            var result = new ValidationResult();
            
            try
            {
                var adminUsers = await _context.Users
                    .Where(u => u.Role.ToLower() == "admin")
                    .ToListAsync();

                result.AdminCount = adminUsers.Count;
                result.ValidAdmins = new List<AdminValidation>();

                foreach (var admin in adminUsers)
                {
                    var validation = new AdminValidation
                    {
                        Id = admin.Id,
                        Email = admin.Email,
                        FirstName = admin.FirstName,
                        LastName = admin.LastName,
                        Role = admin.Role,
                        IsActive = admin.IsActive,
                        HasPasswordHash = !string.IsNullOrEmpty(admin.PasswordHash),
                        CreatedAt = admin.CreatedAt
                    };

                    result.ValidAdmins.Add(validation);
                }

                result.Success = true;
                _logger.LogInformation($"Admin validation completed. Found {adminUsers.Count} valid admin accounts.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating admin credentials");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
    }

    public class CleanupResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int AdminUsersPreserved { get; set; }
        public int AdminUsersAfterCleanup { get; set; }
        public int NonAdminUsersRemoved { get; set; }
        public int RunnersRemoved { get; set; }
        public int PublicCasesRemoved { get; set; }
    }

    public class ValidationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int AdminCount { get; set; }
        public List<AdminValidation> ValidAdmins { get; set; } = new();
    }

    public class AdminValidation
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool HasPasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
