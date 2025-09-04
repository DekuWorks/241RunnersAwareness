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
                        MiddleName = r.MiddleName,
                        LastName = r.LastName,
                        MaidenName = r.MaidenName,
                        ChosenName = r.ChosenName,
                        FullName = r.FullName,
                        RunnerId = r.RunnerId,
                        NcicNumber = r.NcicNumber,
                        ViCapNumber = r.ViCapNumber,
                        NcmecNumber = r.NcmecNumber,
                        Age = r.Age,
                        Gender = r.Gender,
                        PlaceOfBirth = r.PlaceOfBirth,
                        Tribe = r.Tribe,
                        Status = r.Status,
                        ResolutionStatus = r.ResolutionStatus,
                        DateFound = r.DateFound,
                        CityFound = r.CityFound,
                        StateFound = r.StateFound,
                        MannerOfDeath = r.MannerOfDeath,
                        City = r.City,
                        State = r.State,
                        County = r.County,
                        Address = r.Address,
                        Height = r.Height,
                        Weight = r.Weight,
                        HairColor = r.HairColor,
                        EyeColor = r.EyeColor,
                        LeftEyeColor = r.LeftEyeColor,
                        RightEyeColor = r.RightEyeColor,
                        IdentifyingMarks = r.IdentifyingMarks,
                        DistinctiveFeatures = r.DistinctiveFeatures,
                        DateReported = r.DateReported,
                        DateOfLastContact = r.DateOfLastContact,
                        LastSeen = r.LastSeen,
                        Description = r.Description,
                        Circumstances = r.Circumstances,
                        MedicalConditions = r.MedicalConditions,
                        Medications = r.Medications,
                        Allergies = r.Allergies,
                        DnaStatus = r.DnaStatus,
                        FingerprintStatus = r.FingerprintStatus,
                        DentalStatus = r.DentalStatus,
                        ContactInfo = r.ContactInfo,
                        InvestigatingAgency = r.InvestigatingAgency,
                        AgencyCaseNumber = r.AgencyCaseNumber,
                        AgencyCity = r.AgencyCity,
                        AgencyState = r.AgencyState,
                        ProfileImageUrl = r.ProfileImageUrl,
                        AdditionalImageUrls = r.AdditionalImageUrls,
                        DocumentUrls = r.DocumentUrls,
                        VehicleMake = r.VehicleMake,
                        VehicleModel = r.VehicleModel,
                        VehicleColor = r.VehicleColor,
                        VehicleYear = r.VehicleYear,
                        VehicleVin = r.VehicleVin,
                        ClothingDescription = r.ClothingDescription,
                        PersonalItems = r.PersonalItems,
                        Tags = r.Tags,
                        IsActive = r.IsActive,
                        IsUrgent = r.IsUrgent,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        ReportedByUserId = r.ReportedByUserId,
                        CurrentStatus = r.CurrentStatus,
                        DateAdded = r.DateAdded,
                        AgeInYears = r.AgeInYears,
                        DisplayHeight = r.DisplayHeight,
                        DisplayWeight = r.DisplayWeight
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

                // Create the Runners table with comprehensive NamUs fields
                var createTableSql = @"
                    CREATE TABLE Runners (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        FirstName NVARCHAR(100) NOT NULL,
                        MiddleName NVARCHAR(100) NULL,
                        LastName NVARCHAR(100) NOT NULL,
                        MaidenName NVARCHAR(100) NULL,
                        ChosenName NVARCHAR(100) NULL,
                        RunnerId NVARCHAR(50) NOT NULL,
                        NcicNumber NVARCHAR(50) NULL,
                        ViCapNumber NVARCHAR(50) NULL,
                        NcmecNumber NVARCHAR(50) NULL,
                        Age INT NOT NULL,
                        Gender NVARCHAR(50) NOT NULL,
                        PlaceOfBirth NVARCHAR(100) NULL,
                        Tribe NVARCHAR(100) NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'missing',
                        ResolutionStatus NVARCHAR(50) NULL,
                        DateFound DATETIME2 NULL,
                        CityFound NVARCHAR(100) NULL,
                        StateFound NVARCHAR(50) NULL,
                        MannerOfDeath NVARCHAR(100) NULL,
                        City NVARCHAR(100) NOT NULL,
                        State NVARCHAR(50) NOT NULL,
                        County NVARCHAR(100) NULL,
                        Address NVARCHAR(500) NOT NULL,
                        Height NVARCHAR(50) NOT NULL,
                        Weight NVARCHAR(50) NOT NULL,
                        HairColor NVARCHAR(50) NOT NULL,
                        EyeColor NVARCHAR(50) NOT NULL,
                        LeftEyeColor NVARCHAR(50) NULL,
                        RightEyeColor NVARCHAR(50) NULL,
                        IdentifyingMarks NVARCHAR(500) NOT NULL,
                        DistinctiveFeatures NVARCHAR(500) NULL,
                        DateReported DATETIME2 NOT NULL,
                        DateOfLastContact DATETIME2 NULL,
                        LastSeen DATETIME2 NULL,
                        Description NVARCHAR(1000) NOT NULL,
                        Circumstances NVARCHAR(1000) NULL,
                        MedicalConditions NVARCHAR(1000) NOT NULL,
                        Medications NVARCHAR(500) NOT NULL,
                        Allergies NVARCHAR(500) NOT NULL,
                        DnaStatus NVARCHAR(100) NULL,
                        FingerprintStatus NVARCHAR(100) NULL,
                        DentalStatus NVARCHAR(100) NULL,
                        ContactInfo NVARCHAR(200) NOT NULL,
                        InvestigatingAgency NVARCHAR(200) NULL,
                        AgencyCaseNumber NVARCHAR(100) NULL,
                        AgencyCity NVARCHAR(100) NULL,
                        AgencyState NVARCHAR(50) NULL,
                        ProfileImageUrl NVARCHAR(500) NULL,
                        AdditionalImageUrls NVARCHAR(1000) NULL,
                        DocumentUrls NVARCHAR(1000) NULL,
                        VehicleMake NVARCHAR(100) NULL,
                        VehicleModel NVARCHAR(100) NULL,
                        VehicleColor NVARCHAR(50) NULL,
                        VehicleYear NVARCHAR(50) NULL,
                        VehicleVin NVARCHAR(100) NULL,
                        ClothingDescription NVARCHAR(100) NULL,
                        PersonalItems NVARCHAR(500) NULL,
                        Tags NVARCHAR(500) NOT NULL,
                        IsActive BIT NOT NULL DEFAULT 1,
                        IsUrgent BIT NOT NULL DEFAULT 0,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt DATETIME2 NULL,
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
                        Allergies = "None known"
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
                        Allergies = "Dust, pollen"
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
                        Allergies = "None known"
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