using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace SeedDatabase
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = "Server=tcp:241runners-sql-2025.database.windows.net,1433;Initial Catalog=241RunnersAwarenessDB;Persist Security Info=False;User ID=sqladmin;Password=241RunnersAwareness2024!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            using var context = new ApplicationDbContext(options);

            Console.WriteLine("Checking existing users...");
            var existingUsers = await context.Users.ToListAsync();
            Console.WriteLine($"Found {existingUsers.Count} existing users");

            // Create admin user if it doesn't exist
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@241runnersawareness.org");
            if (adminUser == null)
            {
                adminUser = new User
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
                };
                context.Users.Add(adminUser);
                Console.WriteLine("Created admin user");
            }
            else
            {
                Console.WriteLine("Admin user already exists");
            }

            // Create support user if it doesn't exist
            var supportUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "support@241runnersawareness.org");
            if (supportUser == null)
            {
                supportUser = new User
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
                };
                context.Users.Add(supportUser);
                Console.WriteLine("Created support user");
            }
            else
            {
                Console.WriteLine("Support user already exists");
            }

            await context.SaveChangesAsync();
            Console.WriteLine("Database seeding completed successfully!");

            // Show all users
            var allUsers = await context.Users.ToListAsync();
            Console.WriteLine($"\nTotal users in database: {allUsers.Count}");
            foreach (var user in allUsers)
            {
                Console.WriteLine($"- {user.Email} ({user.Role}) - Active: {user.IsActive}");
            }
        }
    }
}
