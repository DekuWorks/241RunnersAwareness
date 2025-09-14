using _241RunnersAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace _241RunnersAPI.Data
{
    /// <summary>
    /// Database initializer for seeding initial data
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Initialize the database with seed data
        /// </summary>
        public static async Task Initialize(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database ensured created");

                // Check if we already have users
                var userCount = await context.Users.CountAsync();
                logger.LogInformation("Current user count in database: {UserCount}", userCount);

                // Always ensure we have at least one admin user
                var adminExists = await context.Users.AnyAsync(u => u.Email == "admin@241runnersawareness.org");
                logger.LogInformation("Admin user exists: {AdminExists}", adminExists);

                if (adminExists)
                {
                    logger.LogInformation("Admin user already exists, skipping seed data");
                    return;
                }

                // Create admin users
                var adminUsers = new List<User>
                {
                    new User
                    {
                        Email = "admin@241runnersawareness.org",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@241Runners2024!"),
                        FirstName = "System",
                        LastName = "Administrator",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "System Administrator",
                        PhoneNumber = "+1-555-0123",
                        Address = "123 Safety Street",
                        City = "Awareness City",
                        State = "Safety State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "+1-555-911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
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
                        Organization = "DekuWorks",
                        Title = "Administrator",
                        PhoneNumber = "+1-555-0124",
                        Address = "123 Admin Street",
                        City = "Admin City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "+1-555-911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "support@241runnersawareness.org",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Support@241Runners2024!"),
                        FirstName = "Support",
                        LastName = "Team",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Support Administrator",
                        PhoneNumber = "+1-555-0124",
                        Address = "123 Support Avenue",
                        City = "Help City",
                        State = "Support State",
                        ZipCode = "12346",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "+1-555-911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "test@241runnersawareness.org",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@241Runners2024!"),
                        FirstName = "Test",
                        LastName = "User",
                        Role = "user",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "Test Organization",
                        Title = "Test User",
                        PhoneNumber = "+1-555-0125",
                        Address = "123 Test Street",
                        City = "Test City",
                        State = "Test State",
                        ZipCode = "12347",
                        EmergencyContactName = "Test Emergency Contact",
                        EmergencyContactPhone = "+1-555-0126",
                        EmergencyContactRelationship = "Family"
                    }
                };

                context.Users.AddRange(adminUsers);
                await context.SaveChangesAsync();

                logger.LogInformation("Database seeded with {Count} initial users", adminUsers.Count);
                logger.LogInformation("Admin users created:");
                logger.LogInformation("- admin@241runnersawareness.org (Password: Admin@241Runners2024!)");
                logger.LogInformation("- dekuworks1@gmail.com (Password: marcus2025)");
                logger.LogInformation("- support@241runnersawareness.org (Password: Support@241Runners2024!)");
                logger.LogInformation("- test@241runnersawareness.org (Password: Test@241Runners2024!)");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }
    }
}
