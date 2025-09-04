using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Models;
using System.Text;
using CsvHelper;
using System.Globalization;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicCasesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PublicCasesController> _logger;

        public PublicCasesController(ApplicationDbContext context, ILogger<PublicCasesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get public cases with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PublicCaseDto>>> GetPublicCases(
            [FromQuery] PublicCaseSearchDto? search = null)
        {
            try
            {
                // Set default values if search is null
                if (search == null)
                {
                    search = new PublicCaseSearchDto { Page = 1, PageSize = 20 };
                }
                
                // Validate search parameters
                if (search.Page < 1)
                {
                    return BadRequest(new { message = "Page number must be greater than 0" });
                }
                
                if (search.PageSize < 1 || search.PageSize > 100)
                {
                    return BadRequest(new { message = "Page size must be between 1 and 100" });
                }

                var query = _context.PublicCases.AsQueryable();

                // Apply region filter (Houston area)
                if (!string.IsNullOrEmpty(search.Region) && search.Region.ToLower() == "houston")
                {
                    query = query.Where(c => 
                        c.State == "Texas" && 
                        (c.City == "Houston" || c.County == "Harris"));
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(search.Status))
                {
                    query = query.Where(c => c.Status == search.Status);
                }

                // Apply city filter
                if (!string.IsNullOrEmpty(search.City))
                {
                    query = query.Where(c => c.City == search.City);
                }

                // Apply county filter
                if (!string.IsNullOrEmpty(search.County))
                {
                    query = query.Where(c => c.County == search.County);
                }

                // Only return missing and resolved cases for public view
                query = query.Where(c => c.Status == "missing" || 
                                       c.Status == "found" || 
                                       c.Status == "safe" || 
                                       c.Status == "deceased");

                // Apply pagination
                var totalCount = await query.CountAsync();
                var cases = await query
                    .OrderByDescending(c => c.DateMissing)
                    .Skip((search.Page - 1) * search.PageSize)
                    .Take(search.PageSize)
                    .Select(c => new PublicCaseDto
                    {
                        Id = c.Id,
                        NamusCaseNumber = c.NamusCaseNumber,
                        FullName = c.FullName,
                        Sex = c.Sex,
                        AgeAtMissing = c.AgeAtMissing,
                        DateMissing = c.DateMissing,
                        City = c.City,
                        County = c.County,
                        State = c.State,
                        Agency = c.Agency,
                        PhotoUrl = c.PhotoUrl,
                        Status = c.Status,
                        StatusNote = c.StatusNote,
                        SourceUrl = c.SourceUrl,
                        SourceLastChecked = c.SourceLastChecked,
                        VerificationSource = c.VerificationSource,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                Response.Headers["X-Total-Count"] = totalCount.ToString();
                Response.Headers["X-Page"] = search.Page.ToString();
                Response.Headers["X-Page-Size"] = search.PageSize.ToString();

                return Ok(cases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public cases");
                return StatusCode(500, new { message = "An error occurred while retrieving public cases" });
            }
        }

        /// <summary>
        /// Get a specific public case by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PublicCaseDto>> GetPublicCase(Guid id)
        {
            try
            {
                // Validate ID parameter
                if (id == Guid.Empty)
                {
                    return BadRequest(new { message = "Invalid case ID" });
                }

                var publicCase = await _context.PublicCases
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (publicCase == null)
                {
                    return NotFound(new { message = "Public case not found" });
                }

                var dto = new PublicCaseDto
                {
                    Id = publicCase.Id,
                    NamusCaseNumber = publicCase.NamusCaseNumber,
                    FullName = publicCase.FullName,
                    Sex = publicCase.Sex,
                    AgeAtMissing = publicCase.AgeAtMissing,
                    DateMissing = publicCase.DateMissing,
                    City = publicCase.City,
                    County = publicCase.County,
                    State = publicCase.State,
                    Agency = publicCase.Agency,
                    PhotoUrl = publicCase.PhotoUrl,
                    Status = publicCase.Status,
                    StatusNote = publicCase.StatusNote,
                    SourceUrl = publicCase.SourceUrl,
                    SourceLastChecked = publicCase.SourceLastChecked,
                    VerificationSource = publicCase.VerificationSource,
                    CreatedAt = publicCase.CreatedAt,
                    UpdatedAt = publicCase.UpdatedAt
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public case {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the public case" });
            }
        }

        /// <summary>
        /// Import NamUs CSV data (Admin only)
        /// </summary>
        [HttpPost("admin/namus/import")]
        public async Task<ActionResult> ImportNamusData([FromForm] NamusImportRequest request)
        {
            try
            {
                if (request.CsvFile == null || request.CsvFile.Length == 0)
                {
                    return BadRequest(new { message = "CSV file is required" });
                }

                // Validate file size (max 10MB)
                if (request.CsvFile.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "File size must be less than 10MB" });
                }

                if (!request.CsvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "File must be a CSV" });
                }

                // Validate file content type
                if (!request.CsvFile.ContentType.Equals("text/csv", StringComparison.OrdinalIgnoreCase) && 
                    !request.CsvFile.ContentType.Equals("application/csv", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "File must be a valid CSV file" });
                }

                var importedCount = 0;
                var updatedCount = 0;
                var errors = new List<string>();

                using var reader = new StreamReader(request.CsvFile.OpenReadStream());
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                // Read CSV header
                csv.Read();
                csv.ReadHeader();

                // Validate required headers
                var requiredHeaders = new[] { "Case Number", "Full Name", "Sex", "Age at Missing", "Date Missing", "City", "County", "State", "Agency" };
                if (csv.HeaderRecord == null)
                {
                    return BadRequest(new { message = "CSV file has no headers" });
                }
                var missingHeaders = requiredHeaders.Where(h => !csv.HeaderRecord.Contains(h)).ToList();

                if (missingHeaders.Any())
                {
                    return BadRequest(new { message = $"Missing required headers: {string.Join(", ", missingHeaders)}" });
                }

                // Read and process CSV data
                while (csv.Read())
                {
                    try
                    {
                        var caseNumber = csv.GetField("Case Number")?.Trim();
                        if (string.IsNullOrEmpty(caseNumber))
                        {
                            errors.Add($"Row {csv.Context.Parser.Row}: Missing case number");
                            continue;
                        }

                        // Parse date
                        DateTime? dateMissing = null;
                        var dateStr = csv.GetField("Date Missing");
                        if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var parsedDate))
                        {
                            dateMissing = parsedDate;
                        }

                        // Parse age
                        int ageAtMissing = 0;
                        var ageStr = csv.GetField("Age at Missing");
                        if (!string.IsNullOrEmpty(ageStr) && int.TryParse(ageStr, out var parsedAge))
                        {
                            ageAtMissing = parsedAge;
                        }

                        // Get location data
                        var city = csv.GetField("City")?.Trim();
                        var county = csv.GetField("County")?.Trim();
                        var state = csv.GetField("State")?.Trim();

                        // Filter to Texas and Houston area only
                        if (state?.ToUpper() != "TEXAS" || 
                            (city?.ToUpper() != "HOUSTON" && county?.ToUpper() != "HARRIS"))
                        {
                            continue; // Skip non-Houston area cases
                        }

                        var fullName = csv.GetField("Full Name")?.Trim();
                        if (string.IsNullOrEmpty(fullName))
                        {
                            errors.Add($"Row {csv.Context.Parser.Row}: Missing full name");
                            continue;
                        }

                        var sex = csv.GetField("Sex")?.Trim();

                        // Check if case already exists
                        var existingCase = await _context.PublicCases
                            .FirstOrDefaultAsync(c => c.NamusCaseNumber == caseNumber);

                        if (existingCase != null)
                        {
                            // Update existing case
                            existingCase.FullName = fullName;
                            existingCase.Sex = sex ?? existingCase.Sex;
                            existingCase.AgeAtMissing = ageAtMissing;
                            existingCase.City = city ?? existingCase.City;
                            existingCase.County = county ?? existingCase.County;
                            existingCase.State = state ?? existingCase.State;
                            existingCase.DateMissing = dateMissing ?? existingCase.DateMissing;
                            existingCase.UpdatedAt = DateTime.UtcNow;

                            updatedCount++;
                        }
                        else
                        {
                            // Create new case
                            var newCase = new PublicCase
                            {
                                NamusCaseNumber = caseNumber,
                                FullName = fullName,
                                Sex = sex ?? "Unknown",
                                AgeAtMissing = ageAtMissing,
                                DateMissing = dateMissing ?? DateTime.UtcNow,
                                City = city ?? "Houston",
                                County = county ?? "Harris",
                                State = state ?? "Texas",
                                Agency = csv.GetField("Agency")?.Trim(),
                                Status = "missing",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _context.PublicCases.Add(newCase);
                            importedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {csv.Context.Parser.Row}: {ex.Message}");
                    }
                }

                // Save changes
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "NamUs Houston data import completed",
                    imported = importedCount,
                    updated = updatedCount,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing NamUs Houston data");
                return StatusCode(500, new { message = "An error occurred while importing NamUs Houston data" });
            }
        }

        /// <summary>
        /// Update case status via TxDPS bulletin check (Admin only)
        /// </summary>
        [HttpPost("admin/txdps/check")]
        public async Task<ActionResult> CheckTxDpsBulletin([FromBody] TxDpsCheckRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new { message = $"Validation failed: {string.Join(", ", errors)}" });
                }

                // Validate case ID
                if (request.CaseId == Guid.Empty)
                {
                    return BadRequest(new { message = "Invalid case ID" });
                }

                var publicCase = await _context.PublicCases
                    .FirstOrDefaultAsync(c => c.Id == request.CaseId);

                if (publicCase == null)
                {
                    return NotFound(new { message = "Public case not found" });
                }

                // Update case status
                publicCase.Status = request.NewStatus;
                publicCase.VerificationSource = request.VerificationSource;
                publicCase.StatusNote = request.Notes;
                publicCase.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Case status updated successfully",
                    caseId = publicCase.Id,
                    newStatus = publicCase.Status,
                    verificationSource = publicCase.VerificationSource
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating case status via TxDPS check");
                return StatusCode(500, new { message = "An error occurred while updating case status" });
            }
        }

        /// <summary>
        /// Get case statistics for Houston area
        /// </summary>
        [HttpGet("stats/houston")]
        public async Task<ActionResult> GetHoustonStats()
        {
            try
            {
                var stats = await _context.PublicCases
                    .Where(c => c.State == "Texas" && 
                               (c.City == "Houston" || c.County == "Harris"))
                    .GroupBy(c => c.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var totalCases = stats.Sum(s => s.Count);
                var missingCases = stats.FirstOrDefault(s => s.Status == "missing")?.Count ?? 0;
                var resolvedCases = stats.Where(s => s.Status == "found" || s.Status == "safe" || s.Status == "deceased").Sum(s => s.Count);

                return Ok(new
                {
                    totalCases,
                    missingCases,
                    resolvedCases,
                    breakdown = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Houston area statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// Normalize location names (e.g., HOUSTON -> Houston, HARRIS -> Harris)
        /// </summary>
        private static string? NormalizeLocation(string? location)
        {
            if (string.IsNullOrEmpty(location))
                return null;

            var normalized = location.Trim().ToUpper();
            
            // Handle common variations
            if (normalized == "HOUSTON")
                return "Houston";
            if (normalized == "HARRIS")
                return "Harris";
            
            // General title case conversion
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(location.Trim().ToLower());
        }

        /// <summary>
        /// Get all runners with optional filtering
        /// </summary>
        [HttpGet("runners")]
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
                        DateOfBirth = r.DateOfLastContact,
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
        [HttpPost("runners/init-table")]
        public async Task<ActionResult> InitializeRunnersTable()
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
                        DateOfLastContact DATETIME2 NULL,
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
                        ContactInfo NVARCHAR(200) NOT NULL,
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
        [HttpPost("runners/create-sample-cases")]
        public async Task<ActionResult> CreateSampleRunners()
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
                    },
                    new Runner
                    {
                        FirstName = "Emily",
                        LastName = "Rodriguez",
                        RunnerId = "MP12348",
                        Age = 28,
                        Gender = "Female",
                        Status = "missing",
                        City = "Houston",
                        State = "Texas",
                        Address = "Houston, Texas",
                        Description = "NamUs Case: MP12348 - Last seen at shopping center",
                        ContactInfo = "Houston Police Department",
                        DateReported = DateTime.UtcNow.AddDays(-7),
                        Tags = "NamUs,Houston,Texas",
                        IsActive = true,
                        IsUrgent = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Height = "5'4\"",
                        Weight = "130 lbs",
                        HairColor = "Brown",
                        EyeColor = "Hazel",
                        IdentifyingMarks = "Small scar on chin",
                        MedicalConditions = "None known",
                        Medications = "None",
                        Allergies = "None known"
                    },
                    new Runner
                    {
                        FirstName = "David",
                        LastName = "Thompson",
                        RunnerId = "MP12349",
                        Age = 45,
                        Gender = "Male",
                        Status = "urgent",
                        City = "Houston",
                        State = "Texas",
                        Address = "Houston, Texas",
                        Description = "NamUs Case: MP12349 - High-risk missing person",
                        ContactInfo = "Harris County Sheriff's Office",
                        DateReported = DateTime.UtcNow.AddDays(-3),
                        Tags = "NamUs,Houston,Texas,HighRisk",
                        IsActive = true,
                        IsUrgent = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Height = "6'2\"",
                        Weight = "200 lbs",
                        HairColor = "Gray",
                        EyeColor = "Blue",
                        IdentifyingMarks = "Tattoo on right shoulder",
                        MedicalConditions = "Diabetes, Heart condition",
                        Medications = "Insulin, Blood pressure medication",
                        Allergies = "Penicillin"
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