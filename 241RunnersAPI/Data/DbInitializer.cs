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
                var adminEmails = new[] { 
                    "dekuworks1@gmail.com", 
                    "danielcarey9770@yahoo.com", 
                    "lthomas3350@gmail.com", 
                    "tinaleggins@yahoo.com", 
                    "mmelasky@iplawconsulting.com", 
                    "ralphfrank900@gmail.com" 
                };
                var adminExists = await context.Users.AnyAsync(u => adminEmails.Contains(u.Email));
                logger.LogInformation("Admin user exists: {AdminExists}", adminExists);

                if (adminExists)
                {
                    logger.LogInformation("Admin user already exists, skipping seed data");
                    return;
                }

                // Create admin users - Only the 6 real admin users
                var adminUsers = new List<User>
                {
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
                        PhoneNumber = "+1-555-0123",
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
                        Email = "danielcarey9770@yahoo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Daniel2025!"),
                        FirstName = "Daniel",
                        LastName = "Carey",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
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
                        Email = "lthomas3350@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lisa2025!"),
                        FirstName = "Lisa",
                        LastName = "Thomas",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "+1-555-0125",
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
                        Email = "tinaleggins@yahoo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tina2025!"),
                        FirstName = "Tina",
                        LastName = "Matthews",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "+1-555-0126",
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
                        Email = "mmelasky@iplawconsulting.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mark2025!"),
                        FirstName = "Mark",
                        LastName = "Melasky",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "IP Law Consulting",
                        Title = "Administrator",
                        PhoneNumber = "+1-555-0127",
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
                        Email = "ralphfrank900@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Ralph2025!"),
                        FirstName = "Ralph",
                        LastName = "Frank",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "+1-555-0128",
                        Address = "123 Admin Street",
                        City = "Admin City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "+1-555-911",
                        EmergencyContactRelationship = "Emergency Contact"
                    }
                };

                context.Users.AddRange(adminUsers);
                await context.SaveChangesAsync();

                logger.LogInformation("Database seeded with {Count} initial users", adminUsers.Count);
                logger.LogInformation("Admin users created:");
                logger.LogInformation("- dekuworks1@gmail.com (Password: marcus2025)");
                logger.LogInformation("- danielcarey9770@yahoo.com (Password: Daniel2025!)");
                logger.LogInformation("- lthomas3350@gmail.com (Password: Lisa2025!)");
                logger.LogInformation("- tinaleggins@yahoo.com (Password: Tina2025!)");
                logger.LogInformation("- mmelasky@iplawconsulting.com (Password: Mark2025!)");
                logger.LogInformation("- ralphfrank900@gmail.com (Password: Ralph2025!)");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }
    }
}
