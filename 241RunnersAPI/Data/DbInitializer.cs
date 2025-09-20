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
                
                // Check each admin user individually and create missing ones
                var existingAdmins = await context.Users.Where(u => adminEmails.Contains(u.Email)).ToListAsync();
                logger.LogInformation("Found {Count} existing admin users", existingAdmins.Count);
                
                var existingEmails = existingAdmins.Select(u => u.Email).ToHashSet();
                var missingEmails = adminEmails.Where(email => !existingEmails.Contains(email)).ToList();
                
                if (missingEmails.Count == 0)
                {
                    logger.LogInformation("All admin users already exist, skipping seed data");
                    return;
                }
                
                logger.LogInformation("Missing admin users: {MissingEmails}", string.Join(", ", missingEmails));

                // Create only missing admin users
                var adminUsersToCreate = new List<User>();
                
                if (missingEmails.Contains("dekuworks1@gmail.com"))
                {
                    adminUsersToCreate.Add(new User
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
                    });
                }
                
                if (missingEmails.Contains("danielcarey9770@yahoo.com"))
                {
                    adminUsersToCreate.Add(new User
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
                    });
                }
                
                if (missingEmails.Contains("lthomas3350@gmail.com"))
                {
                    adminUsersToCreate.Add(new User
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
                    });
                }
                
                if (missingEmails.Contains("tinaleggins@yahoo.com"))
                {
                    adminUsersToCreate.Add(new User
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
                    });
                }
                
                if (missingEmails.Contains("mmelasky@iplawconsulting.com"))
                {
                    adminUsersToCreate.Add(new User
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
                    });
                }
                
                if (missingEmails.Contains("ralphfrank900@gmail.com"))
                {
                    adminUsersToCreate.Add(new User
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
                    });
                }

                if (adminUsersToCreate.Count > 0)
                {
                    context.Users.AddRange(adminUsersToCreate);
                    await context.SaveChangesAsync();

                    logger.LogInformation("Database seeded with {Count} new admin users", adminUsersToCreate.Count);
                    foreach (var user in adminUsersToCreate)
                    {
                        logger.LogInformation("- {Email} created successfully", user.Email);
                    }
                }
                else
                {
                    logger.LogInformation("No new admin users needed - all already exist");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database: {Message}", ex.Message);
                logger.LogWarning("Database initialization failed, but application will continue to start");
                // Don't throw - let the application start even if initialization fails
                // This allows the API to be accessible even if there are database issues
            }
        }
    }
}
