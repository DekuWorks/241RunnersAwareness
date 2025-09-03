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
            [FromQuery] PublicCaseSearchDto search)
        {
            try
            {
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

                Response.Headers.Add("X-Total-Count", totalCount.ToString());
                Response.Headers.Add("X-Page", search.Page.ToString());
                Response.Headers.Add("X-Page-Size", search.PageSize.ToString());

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

                if (!request.CsvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "File must be a CSV" });
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
                        int? ageAtMissing = null;
                        var ageStr = csv.GetField("Age at Missing");
                        if (!string.IsNullOrEmpty(ageStr) && int.TryParse(ageStr, out var parsedAge))
                        {
                            ageAtMissing = parsedAge;
                        }

                        // Normalize city and county names
                        var city = NormalizeLocation(csv.GetField("City"));
                        var county = NormalizeLocation(csv.GetField("County"));
                        var state = csv.GetField("State")?.Trim();

                        // Filter to Texas and Houston area only
                        if (state?.ToUpper() != "TEXAS" || 
                            (city?.ToUpper() != "HOUSTON" && county?.ToUpper() != "HARRIS"))
                        {
                            continue; // Skip non-Houston area cases
                        }

                        var fullName = csv.GetField("Full Name")?.Trim();
                        var sex = csv.GetField("Sex")?.Trim();
                        var agency = csv.GetField("Agency")?.Trim();

                        // Check if case already exists
                        var existingCase = await _context.PublicCases
                            .FirstOrDefaultAsync(c => c.NamusCaseNumber == caseNumber);

                        if (existingCase != null)
                        {
                            // Update existing case
                            existingCase.FullName = fullName ?? existingCase.FullName;
                            existingCase.Sex = sex ?? existingCase.Sex;
                            existingCase.AgeAtMissing = ageAtMissing ?? existingCase.AgeAtMissing;
                            existingCase.DateMissing = dateMissing ?? existingCase.DateMissing;
                            existingCase.City = city ?? existingCase.City;
                            existingCase.County = county ?? existingCase.County;
                            existingCase.State = state ?? existingCase.State;
                            existingCase.Agency = agency ?? existingCase.Agency;
                            existingCase.SourceLastChecked = DateTime.UtcNow;
                            existingCase.UpdatedAt = DateTime.UtcNow;

                            updatedCount++;
                        }
                        else
                        {
                            // Create new case
                            var newCase = new PublicCase
                            {
                                Id = Guid.NewGuid(),
                                NamusCaseNumber = caseNumber,
                                FullName = fullName ?? "Unknown",
                                Sex = sex,
                                AgeAtMissing = ageAtMissing,
                                DateMissing = dateMissing,
                                City = city,
                                County = county,
                                State = state ?? "Texas",
                                Agency = agency,
                                Status = "missing",
                                SourceLastChecked = DateTime.UtcNow,
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

                // Check for cases that were previously missing but not in current export
                var missingCases = await _context.PublicCases
                    .Where(c => c.Status == "missing" && 
                               c.State == "Texas" && 
                               (c.City == "Houston" || c.County == "Harris"))
                    .ToListAsync();

                foreach (var missingCase in missingCases)
                {
                    if (missingCase.SourceLastChecked < DateTime.UtcNow.AddHours(-1)) // Not updated in this import
                    {
                        missingCase.Status = "resolved_pending_verify";
                        missingCase.StatusNote = "Missing from latest NamUs export; verify via TxDPS/HPD.";
                        missingCase.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "NamUs data import completed",
                    imported = importedCount,
                    updated = updatedCount,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing NamUs data");
                return StatusCode(500, new { message = "An error occurred while importing NamUs data" });
            }
        }

        /// <summary>
        /// Check TxDPS bulletin for case resolution (Admin only)
        /// </summary>
        [HttpPost("admin/txdps/check")]
        public async Task<ActionResult> CheckTxDpsBulletin([FromBody] TxDpsCheckRequest request)
        {
            try
            {
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
    }
} 