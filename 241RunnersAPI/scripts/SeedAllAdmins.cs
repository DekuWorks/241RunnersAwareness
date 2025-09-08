using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace SeedAllAdmins
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

            Console.WriteLine("Starting comprehensive admin user seeding...");
            Console.WriteLine($"Database connection: {connectionString.Substring(0, 50)}...");

            // Check existing users
            var existingUsers = await context.Users.ToListAsync();
            Console.WriteLine($"Found {existingUsers.Count} existing users in database");

            // Define all admin users from the image
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
                    Organization = "241 Runners Awareness",
                    Title = "System Administrator",
                    PhoneNumber = "15551234567",
                    Address = "123 Admin Street",
                    City = "Administration City",
                    State = "Admin State",
                    ZipCode = "12345",
                    EmergencyContactName = "Emergency Services",
                    EmergencyContactPhone = "1555911",
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
                    PhoneNumber = "15551234568",
                    Address = "124 Admin Street",
                    City = "Administration City",
                    State = "Admin State",
                    ZipCode = "12345",
                    EmergencyContactName = "Emergency Services",
                    EmergencyContactPhone = "1555911",
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
                    PhoneNumber = "15551234569",
                    Address = "125 Admin Street",
                    City = "Administration City",
                    State = "Admin State",
                    ZipCode = "12345",
                    EmergencyContactName = "Emergency Services",
                    EmergencyContactPhone = "1555911",
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
                    PhoneNumber = "15551234570",
                    Address = "126 Admin Street",
                    City = "Administration City",
                    State = "Admin State",
                    ZipCode = "12345",
                    EmergencyContactName = "Emergency Services",
                    EmergencyContactPhone = "1555911",
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
                    Title = "Legal Administrator",
                    PhoneNumber = "15551234571",
                    Address = "127 Legal Street",
                    City = "Legal City",
                    State = "Legal State",
                    ZipCode = "12345",
                    EmergencyContactName = "Emergency Services",
                    EmergencyContactPhone = "1555911",
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
                    PhoneNumber = "15551234572",
                    Address = "128 Admin Street",
                    City = "Administration City",
                    State = "Admin State",
                    ZipCode = "12345",
                    EmergencyContactName = "Emergency Services",
                    EmergencyContactPhone = "1555911",
                    EmergencyContactRelationship = "Emergency Contact"
                }
            };

            int addedCount = 0;
            int skippedCount = 0;

            foreach (var adminUser in adminUsers)
            {
                var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminUser.Email);
                if (existingUser == null)
                {
                    context.Users.Add(adminUser);
                    addedCount++;
                    Console.WriteLine($"✅ Added admin user: {adminUser.Email} ({adminUser.FirstName} {adminUser.LastName})");
                }
                else
                {
                    skippedCount++;
                    Console.WriteLine($"⏭️  Admin user already exists: {adminUser.Email} ({adminUser.FirstName} {adminUser.LastName})");
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"\n📊 Seeding Summary:");
            Console.WriteLine($"   - Added: {addedCount} new admin users");
            Console.WriteLine($"   - Skipped: {skippedCount} existing users");
            Console.WriteLine($"   - Total processed: {adminUsers.Count} admin users");

            // Show all admin users
            var allAdmins = await context.Users.Where(u => u.Role == "admin").ToListAsync();
            Console.WriteLine($"\n👥 All Admin Users in Database ({allAdmins.Count} total):");
            foreach (var admin in allAdmins.OrderBy(a => a.FirstName))
            {
                Console.WriteLine($"   - {admin.FirstName} {admin.LastName} ({admin.Email}) - Active: {admin.IsActive}");
            }

            Console.WriteLine("\n✅ Admin user seeding completed successfully!");
        }
    }
}
