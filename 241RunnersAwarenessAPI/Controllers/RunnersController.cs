using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Models;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RunnersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RunnersController> _logger;

        public RunnersController(ApplicationDbContext context, ILogger<RunnersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all runners with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RunnerDto>>> GetRunners(
            [FromQuery] string? status = null,
            [FromQuery] string? city = null,
            [FromQuery] string? state = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var query = _context.Runners.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(r => r.Status == status);
                }

                if (!string.IsNullOrEmpty(city))
                {
                    query = query.Where(r => r.City == city);
                }

                if (!string.IsNullOrEmpty(state))
                {
                    query = query.Where(r => r.State == state);
                }

                // Only return active runners
                query = query.Where(r => r.IsActive);

                // Apply pagination
                var totalCount = await query.CountAsync();
                var runners = await query
                    .OrderByDescending(r => r.DateReported)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RunnerDto
                    {
                        Id = r.Id,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        FullName = r.FullName,
                        RunnerId = r.RunnerId,
                        Age = r.Age,
                        Gender = r.Gender,
                        Status = r.Status,
                        City = r.City,
                        State = r.State,
                        Address = r.Address,
                        Description = r.Description,
                        DateReported = r.DateReported,
                        DateFound = r.DateFound,
                        LastSeen = r.LastSeen,
                        DateOfBirth = r.DateOfBirth,
                        IsUrgent = r.IsUrgent,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        Height = r.Height,
                        Weight = r.Weight,
                        HairColor = r.HairColor,
                        EyeColor = r.EyeColor,
                        IdentifyingMarks = r.IdentifyingMarks
                    })
                    .ToListAsync();

                Response.Headers["X-Total-Count"] = totalCount.ToString();
                Response.Headers["X-Page"] = page.ToString();
                Response.Headers["X-Page-Size"] = pageSize.ToString();

                return Ok(runners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving runners");
                return StatusCode(500, new { message = "An error occurred while retrieving runners" });
            }
        }

        /// <summary>
        /// Initialize the Runners table if it doesn't exist
        /// </summary>
        [HttpPost("init-table")]
        public async Task<ActionResult> InitializeTable()
        {
            try
            {
                // Check if the Runners table exists
                var tableExists = await _context.Database
                    .SqlQueryRaw<int>($"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Runners'")
                    .FirstOrDefaultAsync();

                if (tableExists > 0)
                {
                    return Ok(new { message = "Runners table already exists" });
                }

                // Create the Runners table
                var createTableSql = @"
                    CREATE TABLE Runners (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        FirstName NVARCHAR(100) NOT NULL,
                        LastName NVARCHAR(100) NOT NULL,
                        RunnerId NVARCHAR(50) NOT NULL,
                        Age INT NOT NULL,
                        Gender NVARCHAR(50) NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'missing',
                        City NVARCHAR(100) NOT NULL,
                        State NVARCHAR(50) NOT NULL,
                        Address NVARCHAR(500) NOT NULL,
                        Description NVARCHAR(500) NOT NULL,
                        ContactInfo NVARCHAR(200) NOT NULL,
                        DateReported DATETIME2 NOT NULL,
                        DateFound DATETIME2 NULL,
                        LastSeen DATETIME2 NULL,
                        DateOfBirth DATETIME2 NULL,
                        Tags NVARCHAR(500) NOT NULL,
                        IsActive BIT NOT NULL DEFAULT 1,
                        IsUrgent BIT NOT NULL DEFAULT 0,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        Height NVARCHAR(50) NOT NULL,
                        Weight NVARCHAR(50) NOT NULL,
                        HairColor NVARCHAR(50) NOT NULL,
                        EyeColor NVARCHAR(50) NOT NULL,
                        IdentifyingMarks NVARCHAR(500) NOT NULL,
                        MedicalConditions NVARCHAR(1000) NOT NULL,
                        Medications NVARCHAR(500) NOT NULL,
                        Allergies NVARCHAR(500) NOT NULL,
                        EmergencyContacts NVARCHAR(500) NOT NULL,
                        ReportedByUserId INT NULL
                    )";

                await _context.Database.ExecuteSqlRawAsync(createTableSql);

                return Ok(new { message = "Runners table created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Runners table");
                return StatusCode(500, new { message = "An error occurred while initializing the table" });
            }
        }

        /// <summary>
        /// Create sample Houston NamUs cases for testing
        /// </summary>
        [HttpPost("create-sample-cases")]
        public async Task<ActionResult> CreateSampleCases()
        {
            try
            {
                // Check if we already have sample cases
                var existingCount = await _context.Runners.CountAsync();
                if (existingCount > 0)
                {
                    return Ok(new { message = "Sample cases already exist", count = existingCount });
                }

                // Create sample Houston NamUs cases
                var sampleCases = new List<Runner>
                {
                    new Runner
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        RunnerId = "MP12345",
                        Age = 25,
                        Gender = "Male",
                        Status = "missing",
                        City = "Houston",
                        State = "Texas",
                        Address = "Houston, Texas",
                        Description = "NamUs Case: MP12345 - Last seen at local park",
                        ContactInfo = "Houston Police Department",
                        DateReported = DateTime.UtcNow.AddDays(-30),
                        Tags = "NamUs,Houston,Texas",
                        IsActive = true,
                        IsUrgent = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Height = "5'10\"",
                        Weight = "180 lbs",
                        HairColor = "Brown",
                        EyeColor = "Blue",
                        IdentifyingMarks = "Tattoo on left forearm",
                        MedicalConditions = "None known",
                        Medications = "None",
                        Allergies = "None known",
                        EmergencyContacts = "Family: (555) 123-4567"
                    },
                    new Runner
                    {
                        FirstName = "Sarah",
                        LastName = "Johnson",
                        RunnerId = "MP12346",
                        Age = 32,
                        Gender = "Female",
                        Status = "missing",
                        City = "Houston",
                        State = "Texas",
                        Address = "Houston, Texas",
                        Description = "NamUs Case: MP12346 - Disappeared from workplace",
                        ContactInfo = "Harris County Sheriff's Office",
                        DateReported = DateTime.UtcNow.AddDays(-15),
                        Tags = "NamUs,Houston,Texas",
                        IsActive = true,
                        IsUrgent = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Height = "5'6\"",
                        Weight = "140 lbs",
                        HairColor = "Blonde",
                        EyeColor = "Green",
                        IdentifyingMarks = "Birthmark on right cheek",
                        MedicalConditions = "Asthma",
                        Medications = "Inhaler",
                        Allergies = "Dust, pollen",
                        EmergencyContacts = "Spouse: (555) 987-6543"
                    },
                    new Runner
                    {
                        FirstName = "Michael",
                        LastName = "Chen",
                        RunnerId = "MP12347",
                        Age = 19,
                        Gender = "Male",
                        Status = "found",
                        City = "Houston",
                        State = "Texas",
                        Address = "Houston, Texas",
                        Description = "NamUs Case: MP12347 - Found safe after 5 days",
                        ContactInfo = "Houston Police Department",
                        DateReported = DateTime.UtcNow.AddDays(-20),
                        DateFound = DateTime.UtcNow.AddDays(-15),
                        Tags = "NamUs,Houston,Texas,Resolved",
                        IsActive = true,
                        IsUrgent = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Height = "6'0\"",
                        Weight = "170 lbs",
                        HairColor = "Black",
                        EyeColor = "Brown",
                        IdentifyingMarks = "Scar on left knee",
                        MedicalConditions = "None known",
                        Medications = "None",
                        Allergies = "None known",
                        EmergencyContacts = "Parents: (555) 456-7890"
                    }
                };

                _context.Runners.AddRange(sampleCases);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Sample Houston NamUs cases created successfully", 
                    count = sampleCases.Count,
                    cases = sampleCases.Select(c => new { c.FirstName, c.LastName, c.RunnerId, c.Status })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sample cases");
                return StatusCode(500, new { message = "An error occurred while creating sample cases" });
            }
        }
    }
} 