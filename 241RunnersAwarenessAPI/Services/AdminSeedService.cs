using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Models;

namespace _241RunnersAwarenessAPI.Services
{
    /// <summary>
    /// Service for seeding initial admin users in the database
    /// </summary>
    public class AdminSeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminSeedService> _logger;

        public AdminSeedService(ApplicationDbContext context, ILogger<AdminSeedService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seeds the database with initial admin users if none exist
        /// </summary>
        public async Task SeedAdminUsersAsync()
        {
            try
            {
                // Check if any admin users already exist
                var adminExists = await _context.Users.AnyAsync(u => u.Role.ToLower() == "admin");
                
                if (adminExists)
                {
                    _logger.LogInformation("Admin users already exist, skipping seed operation");
                    return;
                }

                _logger.LogInformation("No admin users found. Creating initial admin users...");

                var adminUsers = new List<User>
                {
                    new User
                    {
                        Email = "dekuworks1@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("marcus2025"),
                        FirstName = "Marcus",
                        LastName = "Brown",
                        Role = "admin",
                        PhoneNumber = "(555) 345-6789",
                        Organization = "241 Runners Awareness",
                        Title = "Co-Founder",
                        Credentials = "Co-Founder",
                        Specialization = "Operations",
                        YearsOfExperience = "3+",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "danielcarey9770@yahoo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("daniel2025"),
                        FirstName = "Daniel",
                        LastName = "Carey",
                        Role = "admin",
                        PhoneNumber = "(555) 456-7890",
                        Organization = "241 Runners Awareness",
                        Title = "Co-Founder",
                        Credentials = "Co-Founder",
                        Specialization = "Technology",
                        YearsOfExperience = "4+",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "lthomas3350@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("lisa2025"),
                        FirstName = "Lisa",
                        LastName = "Thomas",
                        Role = "admin",
                        PhoneNumber = "(555) 567-8901",
                        Organization = "241 Runners Awareness",
                        Title = "Founder",
                        Credentials = "Founder",
                        Specialization = "Leadership & Strategy",
                        YearsOfExperience = "5+",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "tinaleggins@yahoo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("tina2025"),
                        FirstName = "Tina",
                        LastName = "Matthews",
                        Role = "admin",
                        PhoneNumber = "(555) 678-9012",
                        Organization = "241 Runners Awareness",
                        Title = "Program Director",
                        Credentials = "Program Director",
                        Specialization = "Program Management",
                        YearsOfExperience = "4+",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "mmelasky@iplawconsulting.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("mark2025"),
                        FirstName = "Mark",
                        LastName = "Melasky",
                        Role = "admin",
                        PhoneNumber = "(555) 789-0123",
                        Organization = "IP Law Consulting",
                        Title = "Intellectual Property Lawyer",
                        Credentials = "Attorney at Law",
                        Specialization = "Intellectual Property Law",
                        YearsOfExperience = "10+",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "ralphfrank900@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("ralph2025"),
                        FirstName = "Ralph",
                        LastName = "Frank",
                        Role = "admin",
                        PhoneNumber = "(555) 890-1234",
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        Credentials = "System Administrator",
                        Specialization = "System Administration",
                        YearsOfExperience = "2+",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                // Add all admin users
                foreach (var adminUser in adminUsers)
                {
                    // Check if user already exists by email
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == adminUser.Email.ToLower());
                    if (existingUser == null)
                    {
                        _context.Users.Add(adminUser);
                        _logger.LogInformation($"Added admin user: {adminUser.Email}");
                    }
                    else
                    {
                        _logger.LogInformation($"Admin user already exists: {adminUser.Email}");
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin user seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding admin users");
                throw;
            }
        }

        /// <summary>
        /// Gets all admin users from the database
        /// </summary>
        public async Task<List<User>> GetAllAdminUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Role.ToLower() == "admin" && u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing admin user's information
        /// </summary>
        public async Task<bool> UpdateAdminUserAsync(int userId, UserUpdateRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.Role.ToLower() != "admin")
                {
                    return false;
                }

                // Update user information
                user.FirstName = request.FirstName.Trim();
                user.LastName = request.LastName.Trim();
                user.Email = request.Email.ToLower().Trim();
                user.PhoneNumber = request.PhoneNumber?.Trim();
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated admin user: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating admin user {userId}");
                return false;
            }
        }
    }
} 