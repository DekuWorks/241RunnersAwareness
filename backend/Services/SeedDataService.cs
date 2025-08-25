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
                
                // Seed Houston Area Individuals and Cases
                await SeedHoustonCasesAsync();

                Console.WriteLine("Database seeded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw;
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
                    Username = "admin",
                    Email = "admin@example.com",
                    FullName = "System Admin",
                    PhoneNumber = "(555) 123-4567",
                    PasswordHash = _authService.HashPassword("ChangeMe123!"),
                    Role = "admin",
                    EmailVerified = true,
                    PhoneVerified = true,
                    Organization = "241 Runners Awareness",
                    Credentials = "System Administrator",
                    Specialization = "Platform Management",
                    YearsOfExperience = "5+",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "marcus",
                    Email = "dekuworks1@gmail.com",
                    FullName = "Marcus Brown",
                    PhoneNumber = "(555) 345-6789",
                    PasswordHash = _authService.HashPassword("marcus2025"),
                    Role = "admin",
                    EmailVerified = true,
                    PhoneVerified = true,
                    Organization = "241 Runners Awareness",
                    Credentials = "Co-Founder",
                    Specialization = "Operations",
                    YearsOfExperience = "3+",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "daniel",
                    Email = "danielcarey9770@gmail.com",
                    FullName = "Daniel Carey",
                    PhoneNumber = "(555) 456-7890",
                    PasswordHash = _authService.HashPassword("daniel2025"),
                    Role = "admin",
                    EmailVerified = true,
                    PhoneVerified = true,
                    Organization = "241 Runners Awareness",
                    Credentials = "Co-Founder",
                    Specialization = "Technology",
                    YearsOfExperience = "4+",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "testuser",
                    Email = "test@example.com",
                    FullName = "Test User",
                    PhoneNumber = "(555) 234-5678",
                    PasswordHash = _authService.HashPassword("password123"),
                    Role = "user",
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

        private async Task SeedHoustonCasesAsync()
        {
            if (await _context.Individuals.AnyAsync())
                return;

            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (adminUser == null)
            {
                throw new InvalidOperationException("Admin user not found for seeding cases");
            }

            var individuals = new List<Individual>
            {
                new Individual
                {
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    DateOfBirth = new DateTime(2005, 3, 15),
                    Gender = "Female",
                    Height = "5'4\"",
                    Weight = "120 lbs",
                    HairColor = "Brown",
                    EyeColor = "Blue",
                    DistinguishingFeatures = "Small birthmark on left cheek",
                    SpecialNeeds = "Autism spectrum disorder, may be disoriented",
                    CurrentStatus = "Missing",
                    LastSeenDate = DateTime.UtcNow.AddDays(-10),
                    Latitude = 29.7604,
                    Longitude = -95.3698,
                    Address = "1234 Main St",
                    City = "Houston",
                    State = "TX",
                    ZipCode = "77002",
                    Circumstances = "Last seen wearing blue jeans and white shirt near downtown Houston. Has special needs and may be disoriented.",
                    PhoneNumber = "713-555-0101",
                    CaregiverName = "Mary Johnson",
                    CaregiverPhone = "(713) 555-0101",
                    CaregiverEmail = "mary.johnson@email.com",
                    RiskLevel = "Medium",
                    IsAtImmediateRisk = false,
                    MayWanderOrElope = true,
                    PrimaryDisability = "Autism",
                    CommunicationMethod = "Verbal",
                    EnableRealTimeAlerts = true
                },
                new Individual
                {
                    FirstName = "Michael",
                    LastName = "Rodriguez",
                    DateOfBirth = new DateTime(1998, 7, 22),
                    Gender = "Male",
                    Height = "5'10\"",
                    Weight = "170 lbs",
                    HairColor = "Black",
                    EyeColor = "Brown",
                    DistinguishingFeatures = "Tattoo of cross on right forearm",
                    SpecialNeeds = "Diabetes, requires medication",
                    CurrentStatus = "Urgent",
                    LastSeenDate = DateTime.UtcNow.AddDays(-5),
                    Latitude = 29.7604,
                    Longitude = -95.3698,
                    Address = "5678 Oak Ave",
                    City = "Houston",
                    State = "TX",
                    ZipCode = "77006",
                    Circumstances = "Medical condition requires immediate attention. Last seen near Memorial Park.",
                    PhoneNumber = "713-555-0202",
                    CaregiverName = "Carlos Rodriguez",
                    CaregiverPhone = "(713) 555-0202",
                    CaregiverEmail = "carlos.rodriguez@email.com",
                    RiskLevel = "High",
                    IsAtImmediateRisk = true,
                    HasDiabetes = true,
                    RequiresMedication = true,
                    EnableRealTimeAlerts = true
                },
                new Individual
                {
                    FirstName = "Emily",
                    LastName = "Davis",
                    DateOfBirth = new DateTime(2002, 11, 8),
                    Gender = "Female",
                    Height = "5'6\"",
                    Weight = "130 lbs",
                    HairColor = "Blonde",
                    EyeColor = "Green",
                    DistinguishingFeatures = "Scar on right knee",
                    SpecialNeeds = "None known",
                    CurrentStatus = "Missing",
                    LastSeenDate = DateTime.UtcNow.AddDays(-15),
                    Latitude = 29.7604,
                    Longitude = -95.3698,
                    Address = "9012 Pine St",
                    City = "Houston",
                    State = "TX",
                    ZipCode = "77008",
                    Circumstances = "Last seen leaving work at Medical Center. Car found abandoned.",
                    PhoneNumber = "713-555-0303",
                    CaregiverName = "Robert Davis",
                    CaregiverPhone = "(713) 555-0303",
                    CaregiverEmail = "robert.davis@email.com",
                    RiskLevel = "Medium",
                    IsAtImmediateRisk = false,
                    EnableRealTimeAlerts = true
                },
                new Individual
                {
                    FirstName = "David",
                    LastName = "Wilson",
                    DateOfBirth = new DateTime(1985, 4, 12),
                    Gender = "Male",
                    Height = "6'0\"",
                    Weight = "180 lbs",
                    HairColor = "Brown",
                    EyeColor = "Hazel",
                    DistinguishingFeatures = "Glasses, beard",
                    SpecialNeeds = "Depression, may be in crisis",
                    CurrentStatus = "Missing",
                    LastSeenDate = DateTime.UtcNow.AddDays(-20),
                    Latitude = 29.7604,
                    Longitude = -95.3698,
                    Address = "3456 Elm Dr",
                    City = "Houston",
                    State = "TX",
                    ZipCode = "77005",
                    Circumstances = "Veteran with PTSD. Last seen near Veterans Affairs hospital.",
                    PhoneNumber = "713-555-0404",
                    CaregiverName = "Jennifer Wilson",
                    CaregiverPhone = "(713) 555-0404",
                    CaregiverEmail = "jennifer.wilson@email.com",
                    RiskLevel = "High",
                    IsAtImmediateRisk = true,
                    EnableRealTimeAlerts = true
                },
                new Individual
                {
                    FirstName = "Lisa",
                    LastName = "Thomas",
                    DateOfBirth = new DateTime(1992, 9, 3),
                    Gender = "Female",
                    Height = "5'5\"",
                    Weight = "140 lbs",
                    HairColor = "Red",
                    EyeColor = "Blue",
                    DistinguishingFeatures = "Freckles across nose",
                    SpecialNeeds = "None known",
                    CurrentStatus = "Missing",
                    LastSeenDate = DateTime.UtcNow.AddDays(-8),
                    Latitude = 29.7604,
                    Longitude = -95.3698,
                    Address = "7890 Maple Ln",
                    City = "Houston",
                    State = "TX",
                    ZipCode = "77007",
                    Circumstances = "Last seen at local coffee shop. Phone and wallet found in car.",
                    PhoneNumber = "713-555-0505",
                    CaregiverName = "James Thomas",
                    CaregiverPhone = "(713) 555-0505",
                    CaregiverEmail = "james.thomas@email.com",
                    RiskLevel = "Medium",
                    IsAtImmediateRisk = false,
                    EnableRealTimeAlerts = true
                }
            };

            await _context.Individuals.AddRangeAsync(individuals);
            await _context.SaveChangesAsync();

            // Create cases for each individual
            var cases = new List<Case>();
            foreach (var individual in individuals)
            {
                var caseNumber = $"CASE-{individual.Id:D6}";
                var publicSlug = $"{individual.FirstName.ToLower()}-{individual.LastName.ToLower()}-{DateTime.UtcNow:yyyyMMdd}";
                
                cases.Add(new Case
                {
                    CaseNumber = caseNumber,
                    PublicSlug = publicSlug,
                    Title = $"Missing: {individual.FirstName} {individual.LastName}",
                    Description = individual.Circumstances,
                    Status = individual.CurrentStatus,
                    Priority = individual.CurrentStatus == "Urgent" ? "High" : "Medium",
                    RiskLevel = individual.CurrentStatus == "Urgent" ? "High" : "Medium",
                    LastSeenLocation = individual.Address,
                    LastSeenDate = individual.LastSeenDate,
                    Circumstances = $"Last seen on {individual.LastSeenDate:MMM dd, yyyy} at {individual.Address}",
                    ResolutionNotes = "",
                    LawEnforcementCaseNumber = $"LE-{individual.Id:D6}",
                    InvestigatingAgency = "Houston Police Department",
                    InvestigatorName = "Detective Smith",
                    InvestigatorContact = "713-555-0000",
                    Tags = "Houston,Missing Person,Community Alert",
                    SocialMediaHandles = "",
                    MediaContacts = "",
                    CreatedBy = adminUser.UserId.ToString(),
                    UpdatedBy = adminUser.UserId.ToString(),
                    CreatedAt = individual.LastSeenDate ?? DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IndividualId = individual.Id,
                    OwnerUserId = adminUser.UserId
                });
            }

            await _context.Cases.AddRangeAsync(cases);
            await _context.SaveChangesAsync();
        }
    }
}
