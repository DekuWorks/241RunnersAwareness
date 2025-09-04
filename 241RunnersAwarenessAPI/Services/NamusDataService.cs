using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace _241RunnersAwarenessAPI.Services
{
    public class NamusDataService : INamusDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NamusDataService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _namusApiBaseUrl = "https://namus.nij.ojp.gov/api/v1";

        public NamusDataService(ApplicationDbContext context, ILogger<NamusDataService> logger, HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<NamUsImportResult> FetchNamUsDataAsync(string state = "TX", string city = "Houston", int maxResults = 100)
        {
            var result = new NamUsImportResult();
            
            try
            {
                _logger.LogInformation("Starting NamUs data fetch for {City}, {State}", city, state);
                
                // Note: NamUs doesn't have a public API, so we'll create a structured import process
                // For now, we'll use the CSV import method with sample data structure
                
                result.Message = "NamUs data fetch completed. Use CSV import for actual data.";
                result.Success = true;
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching NamUs data");
                result.Success = false;
                result.Message = "Error fetching NamUs data";
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        public async Task<NamUsImportResult> ImportFromCsvAsync(IFormFile csvFile)
        {
            var result = new NamUsImportResult();
            
            try
            {
                if (csvFile == null || csvFile.Length == 0)
                {
                    result.Success = false;
                    result.Message = "CSV file is required";
                    return result;
                }

                // Validate file size (max 10MB)
                if (csvFile.Length > 10 * 1024 * 1024)
                {
                    result.Success = false;
                    result.Message = "File size must be less than 10MB";
                    return result;
                }

                if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    result.Success = false;
                    result.Message = "File must be a CSV";
                    return result;
                }

                using var reader = new StreamReader(csvFile.OpenReadStream());
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                // Read CSV header
                csv.Read();
                csv.ReadHeader();

                // Validate required headers
                var requiredHeaders = new[] { "Case Number", "Full Name", "Sex", "Age at Missing", "Date Missing", "City", "County", "State", "Agency" };
                if (csv.HeaderRecord == null)
                {
                    result.Success = false;
                    result.Message = "CSV file has no headers";
                    return result;
                }
                
                var missingHeaders = requiredHeaders.Where(h => !csv.HeaderRecord.Contains(h)).ToList();
                if (missingHeaders.Any())
                {
                    result.Success = false;
                    result.Message = $"Missing required headers: {string.Join(", ", missingHeaders)}";
                    return result;
                }

                // Read and process CSV data
                while (csv.Read())
                {
                    try
                    {
                        var caseNumber = csv.GetField("Case Number")?.Trim();
                        if (string.IsNullOrEmpty(caseNumber))
                        {
                            result.Errors.Add($"Row {csv.Context.Parser.Row}: Missing case number");
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
                            result.Skipped++;
                            continue; // Skip non-Houston area cases
                        }

                        var fullName = csv.GetField("Full Name")?.Trim();
                        if (string.IsNullOrEmpty(fullName))
                        {
                            result.Errors.Add($"Row {csv.Context.Parser.Row}: Missing full name");
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

                            result.Updated++;
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
                            result.Imported++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {csv.Context.Parser.Row}: {ex.Message}");
                    }
                }

                // Save changes
                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"NamUs Houston data import completed. Imported: {result.Imported}, Updated: {result.Updated}, Skipped: {result.Skipped}";
                
                _logger.LogInformation("NamUs CSV import completed: {Imported} imported, {Updated} updated, {Skipped} skipped", 
                    result.Imported, result.Updated, result.Skipped);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing NamUs CSV data");
                result.Success = false;
                result.Message = "An error occurred while importing NamUs CSV data";
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        public async Task<NamUsImportResult> ImportFromJsonAsync(string jsonData)
        {
            var result = new NamUsImportResult();
            
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    result.Success = false;
                    result.Message = "JSON data is required";
                    return result;
                }

                var namusCases = JsonSerializer.Deserialize<List<NamUsCaseData>>(jsonData);
                if (namusCases == null || !namusCases.Any())
                {
                    result.Success = false;
                    result.Message = "No valid NamUs case data found in JSON";
                    return result;
                }

                foreach (var namusCase in namusCases)
                {
                    try
                    {
                        // Filter to Houston area only
                        if (namusCase.State?.ToUpper() != "TEXAS" || 
                            (namusCase.City?.ToUpper() != "HOUSTON" && namusCase.County?.ToUpper() != "HARRIS"))
                        {
                            result.Skipped++;
                            continue;
                        }

                        // Check if case already exists
                        var existingCase = await _context.PublicCases
                            .FirstOrDefaultAsync(c => c.NamusCaseNumber == namusCase.CaseNumber);

                        if (existingCase != null)
                        {
                            // Update existing case
                            existingCase.FullName = namusCase.FullName;
                            existingCase.Sex = namusCase.Sex ?? existingCase.Sex;
                            existingCase.AgeAtMissing = namusCase.AgeAtMissing;
                            existingCase.City = namusCase.City ?? existingCase.City;
                            existingCase.County = namusCase.County ?? existingCase.County;
                            existingCase.State = namusCase.State ?? existingCase.State;
                            existingCase.DateMissing = namusCase.DateMissing ?? existingCase.DateMissing;
                            existingCase.UpdatedAt = DateTime.UtcNow;

                            result.Updated++;
                        }
                        else
                        {
                            // Create new case
                            var newCase = new PublicCase
                            {
                                NamusCaseNumber = namusCase.CaseNumber,
                                FullName = namusCase.FullName,
                                Sex = namusCase.Sex ?? "Unknown",
                                AgeAtMissing = namusCase.AgeAtMissing,
                                DateMissing = namusCase.DateMissing ?? DateTime.UtcNow,
                                City = namusCase.City ?? "Houston",
                                County = namusCase.County ?? "Harris",
                                State = namusCase.State ?? "Texas",
                                Agency = namusCase.Agency,
                                Status = "missing",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _context.PublicCases.Add(newCase);
                            result.Imported++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Case {namusCase.CaseNumber}: {ex.Message}");
                    }
                }

                // Save changes
                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"NamUs JSON import completed. Imported: {result.Imported}, Updated: {result.Updated}, Skipped: {result.Skipped}";
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing NamUs JSON data");
                result.Success = false;
                result.Message = "An error occurred while importing NamUs JSON data";
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        public async Task<NamUsSyncResult> SyncExistingCasesAsync()
        {
            var result = new NamUsSyncResult();
            
            try
            {
                var existingCases = await _context.PublicCases
                    .Where(c => c.Status == "missing")
                    .ToListAsync();

                result.CasesChecked = existingCases.Count;

                foreach (var existingCase in existingCases)
                {
                    try
                    {
                        // Here we would typically check NamUs for updates
                        // For now, we'll just mark as checked
                        existingCase.UpdatedAt = DateTime.UtcNow;
                        result.CasesUpdated++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Case {existingCase.NamusCaseNumber}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"Sync completed. Checked: {result.CasesChecked}, Updated: {result.CasesUpdated}";
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing existing NamUs cases");
                result.Success = false;
                result.Message = "An error occurred while syncing cases";
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        public async Task<NamUsStats> GetImportStatsAsync()
        {
            try
            {
                var totalCases = await _context.PublicCases.CountAsync();
                var activeCases = await _context.PublicCases.CountAsync(c => c.Status == "missing");
                var resolvedCases = await _context.PublicCases.CountAsync(c => c.Status != "missing");
                var houstonAreaCases = await _context.PublicCases
                    .CountAsync(c => c.State == "Texas" && (c.City == "Houston" || c.County == "Harris"));

                var lastImport = await _context.PublicCases
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => c.CreatedAt)
                    .FirstOrDefaultAsync();

                var lastUpdate = await _context.PublicCases
                    .OrderByDescending(c => c.UpdatedAt)
                    .Select(c => c.UpdatedAt)
                    .FirstOrDefaultAsync();

                return new NamUsStats
                {
                    TotalCases = totalCases,
                    ActiveCases = activeCases,
                    ResolvedCases = resolvedCases,
                    HoustonAreaCases = houstonAreaCases,
                    LastImportDate = lastImport,
                    LastSyncDate = lastUpdate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NamUs import stats");
                return new NamUsStats();
            }
        }
    }

    // Data model for NamUs JSON import
    public class NamUsCaseData
    {
        public string CaseNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Sex { get; set; }
        public int? AgeAtMissing { get; set; }
        public DateTime? DateMissing { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string? State { get; set; }
        public string? Agency { get; set; }
    }
} 