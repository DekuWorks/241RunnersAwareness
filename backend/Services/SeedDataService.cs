using Microsoft.EntityFrameworkCore;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using _241RunnersAwareness.BackendAPI.Services;

namespace _241RunnersAwareness.BackendAPI.Services
{
    public class SeedDataService
    {
        private readonly RunnersDbContext _context;
        private readonly IAuthService _authService;

        public SeedDataService(RunnersDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                // Seed Users
                await SeedUsersAsync();
                
                // Seed Products
                await SeedProductsAsync();
                
                // Seed Sample Cases
                await SeedSampleCasesAsync();

                Console.WriteLine("Database seeded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
            }
        }

        private async Task SeedUsersAsync()
        {
            if (await _context.Users.AnyAsync())
                return;

            var users = new List<User>
            {
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "marcus",
                    Email = "dekuworks1@gmail.com",
                    FullName = "Marcus Brown",
                    PhoneNumber = "+1234567893",
                    PasswordHash = _authService.HashPassword("marcus2025"),
                    Role = "admin",
                    EmailVerified = true,
                    PhoneVerified = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "daniel",
                    Email = "danielcarey9770@gmail.com",
                    FullName = "Daniel Carey",
                    PhoneNumber = "+1234567894",
                    PasswordHash = _authService.HashPassword("daniel2025"),
                    Role = "admin",
                    EmailVerified = true,
                    PhoneVerified = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "testuser",
                    Email = "test@example.com",
                    FullName = "Test User",
                    PhoneNumber = "+1234567891",
                    PasswordHash = _authService.HashPassword("password123"),
                    Role = "GeneralUser",
                    EmailVerified = true,
                    PhoneVerified = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        private async Task SeedProductsAsync()
        {
            if (await _context.Products.AnyAsync())
                return;

            var products = new List<Product>
            {
                new Product
                {
                    Name = "241 Runners Awareness T-Shirt",
                    Description = "Premium cotton t-shirt featuring the 241 Runners Awareness logo. Support our mission with every purchase.",
                    Price = 25.00m,
                    Category = "Apparel",
                    Brand = "241 Runners",
                    SKU = "241-TSHIRT-001",
                    StockQuantity = 100,
                    IsActive = true
                },
                new Product
                {
                    Name = "241 Runners Awareness Hoodie",
                    Description = "Comfortable hoodie perfect for raising awareness. Features embroidered logo and mission statement.",
                    Price = 45.00m,
                    Category = "Apparel",
                    Brand = "241 Runners",
                    SKU = "241-HOODIE-001",
                    StockQuantity = 50,
                    IsActive = true
                },
                new Product
                {
                    Name = "Awareness Wristband",
                    Description = "Silicone wristband with 241 Runners Awareness branding. Show your support daily.",
                    Price = 5.00m,
                    Category = "Accessories",
                    Brand = "241 Runners",
                    SKU = "241-WRISTBAND-001",
                    StockQuantity = 200,
                    IsActive = true
                },
                new Product
                {
                    Name = "DNA Collection Kit",
                    Description = "Professional DNA collection kit for missing persons cases. Includes instructions and secure packaging.",
                    Price = 75.00m,
                    Category = "Equipment",
                    Brand = "241 Runners",
                    SKU = "241-DNA-KIT-001",
                    StockQuantity = 25,
                    IsActive = true
                }
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();
        }

        private async Task SeedSampleCasesAsync()
        {
            if (await _context.Individuals.AnyAsync())
                return;

            var individuals = new List<Individual>
            {
                new Individual
                {
                    FirstName = "Sample",
                    LastName = "Case",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = "Unknown",
                    Height = "5'8\"",
                    Weight = "150 lbs",
                    HairColor = "Brown",
                    EyeColor = "Blue",
                    DistinguishingFeatures = "Tattoo on left wrist",
                    SpecialNeeds = "None known"
                }
            };

            await _context.Individuals.AddRangeAsync(individuals);
            await _context.SaveChangesAsync();
        }
    }
}
