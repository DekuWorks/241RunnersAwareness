using _241RunnersAwareness.BackendAPI.DBContext.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace _241RunnersAwareness.BackendAPI.Services
{
    /// <summary>
    /// Service for cleaning and maintaining database data
    /// Handles test data removal, duplicate cleanup, and data anonymization
    /// </summary>
    public interface IDataCleanupService
    {
        Task<int> RemoveTestDataAsync();
        Task<int> RemoveDuplicateUsersAsync();
        Task<int> AnonymizeTestDataAsync();
        Task<int> CleanupOrphanedRecordsAsync();
        Task<CleanupReport> RunFullCleanupAsync();
    }

    public class DataCleanupService : IDataCleanupService
    {
        private readonly RunnersDbContext _context;
        private readonly ILogger<DataCleanupService> _logger;

        public DataCleanupService(RunnersDbContext context, ILogger<DataCleanupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Removes test data from the database
        /// </summary>
        public async Task<int> RemoveTestDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting test data cleanup...");

                // Remove test users
                var testUsers = await _context.Users
                    .Where(u => u.Email.Contains("@test.com") || 
                               u.Email.Contains("@example.com") ||
                               u.Email.Contains("+test"))
                    .ToListAsync();

                _context.Users.RemoveRange(testUsers);
                var userCount = testUsers.Count;

                // Remove test individuals
                var testIndividuals = await _context.Individuals
                    .Where(i => i.Email != null && 
                               (i.Email.Contains("@test.com") || 
                                i.Email.Contains("@example.com") ||
                                i.Email.Contains("+test")))
                    .ToListAsync();

                _context.Individuals.RemoveRange(testIndividuals);
                var individualCount = testIndividuals.Count;

                // Remove test products
                var testProducts = await _context.Products
                    .Where(p => p.Name.Contains("TEST") || 
                               (p.Description != null && p.Description.Contains("TEST")) ||
                               (p.SKU != null && p.SKU.Contains("TEST")))
                    .ToListAsync();

                _context.Products.RemoveRange(testProducts);
                var productCount = testProducts.Count;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Test data cleanup completed: {UserCount} users, {IndividualCount} individuals, {ProductCount} products removed", 
                    userCount, individualCount, productCount);

                return userCount + individualCount + productCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during test data cleanup");
                throw;
            }
        }

        /// <summary>
        /// Removes duplicate users based on email
        /// </summary>
        public async Task<int> RemoveDuplicateUsersAsync()
        {
            try
            {
                _logger.LogInformation("Starting duplicate user cleanup...");

                var duplicates = await _context.Users
                    .GroupBy(u => u.Email)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g.OrderBy(u => u.CreatedAt).Skip(1))
                    .ToListAsync();

                _context.Users.RemoveRange(duplicates);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Duplicate user cleanup completed: {Count} duplicates removed", duplicates.Count);
                return duplicates.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during duplicate user cleanup");
                throw;
            }
        }

        /// <summary>
        /// Anonymizes test data instead of removing it
        /// </summary>
        public async Task<int> AnonymizeTestDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting test data anonymization...");

                var testUsers = await _context.Users
                    .Where(u => u.Email.Contains("@test.com") || 
                               u.Email.Contains("@example.com") ||
                               u.Email.Contains("+test"))
                    .ToListAsync();

                foreach (var user in testUsers)
                {
                    user.Email = $"user_{user.UserId}@anonymized.com";
                    user.FullName = $"User {user.UserId}";
                }

                var testIndividuals = await _context.Individuals
                    .Where(i => i.Email != null && 
                               (i.Email.Contains("@test.com") || 
                                i.Email.Contains("@example.com") ||
                                i.Email.Contains("+test")))
                    .ToListAsync();

                foreach (var individual in testIndividuals)
                {
                    individual.Email = $"individual_{individual.Id}@anonymized.com";
                    individual.FirstName = $"Individual";
                    individual.LastName = individual.Id.ToString();
                    individual.PhoneNumber = "000-000-0000";
                    individual.SocialSecurityNumber = "000-00-0000";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Test data anonymization completed: {UserCount} users, {IndividualCount} individuals anonymized", 
                    testUsers.Count, testIndividuals.Count);

                return testUsers.Count + testIndividuals.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during test data anonymization");
                throw;
            }
        }

        /// <summary>
        /// Removes orphaned records that have no references
        /// </summary>
        public async Task<int> CleanupOrphanedRecordsAsync()
        {
            try
            {
                _logger.LogInformation("Starting orphaned records cleanup...");

                // Remove orphaned emergency contacts
                var orphanedContacts = await _context.EmergencyContacts
                    .Where(ec => !_context.Individuals.Any(i => i.Id == ec.IndividualId))
                    .ToListAsync();

                _context.EmergencyContacts.RemoveRange(orphanedContacts);

                // Remove orphaned case images
                var orphanedImages = await _context.CaseImages
                    .Where(ci => !_context.Individuals.Any(i => i.Id == ci.IndividualId))
                    .ToListAsync();

                _context.CaseImages.RemoveRange(orphanedImages);

                // Remove orphaned case documents
                var orphanedDocuments = await _context.CaseDocuments
                    .Where(cd => !_context.Individuals.Any(i => i.Id == cd.IndividualId))
                    .ToListAsync();

                _context.CaseDocuments.RemoveRange(orphanedDocuments);

                await _context.SaveChangesAsync();

                var totalOrphaned = orphanedContacts.Count + orphanedImages.Count + orphanedDocuments.Count;
                _logger.LogInformation("Orphaned records cleanup completed: {Count} records removed", totalOrphaned);

                return totalOrphaned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during orphaned records cleanup");
                throw;
            }
        }

        /// <summary>
        /// Runs a full cleanup operation and returns a detailed report
        /// </summary>
        public async Task<CleanupReport> RunFullCleanupAsync()
        {
            var report = new CleanupReport
            {
                StartedAt = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Starting full database cleanup...");

                report.TestDataRemoved = await RemoveTestDataAsync();
                report.DuplicatesRemoved = await RemoveDuplicateUsersAsync();
                report.OrphanedRecordsRemoved = await CleanupOrphanedRecordsAsync();

                report.CompletedAt = DateTime.UtcNow;
                report.Success = true;

                _logger.LogInformation("Full database cleanup completed successfully: {TotalRecordsRemoved} total records removed", 
                    report.TotalRecordsRemoved);

                return report;
            }
            catch (Exception ex)
            {
                report.CompletedAt = DateTime.UtcNow;
                report.Success = false;
                report.ErrorMessage = ex.Message;

                _logger.LogError(ex, "Full database cleanup failed");
                throw;
            }
        }
    }

    /// <summary>
    /// Report of cleanup operations
    /// </summary>
    public class CleanupReport
    {
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int TestDataRemoved { get; set; }
        public int DuplicatesRemoved { get; set; }
        public int OrphanedRecordsRemoved { get; set; }
        public int AnonymizedRecords { get; set; }

        public int TotalRecordsRemoved => TestDataRemoved + DuplicatesRemoved + OrphanedRecordsRemoved;
        public TimeSpan Duration => (CompletedAt ?? DateTime.UtcNow) - StartedAt;
    }
}
