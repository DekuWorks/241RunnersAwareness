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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RunnerDto>>> GetRunners(
            [FromQuery] string? id,
            [FromQuery] string? name,
            [FromQuery] int? age,
            [FromQuery] string? status,
            [FromQuery] string? state,
            [FromQuery] string? tags,
            [FromQuery] bool? urgent)
        {
            try
            {
                var query = _context.Runners.Where(r => r.IsActive);

                // Apply filters
                if (!string.IsNullOrEmpty(id))
                    query = query.Where(r => r.RunnerId.Contains(id));

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(r => 
                        r.FirstName.Contains(name) || 
                        r.LastName.Contains(name));

                if (age.HasValue)
                    query = query.Where(r => r.Age == age.Value);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(r => r.Status == status);

                if (!string.IsNullOrEmpty(state))
                    query = query.Where(r => r.State.Contains(state));

                if (!string.IsNullOrEmpty(tags))
                    query = query.Where(r => r.Tags.Contains(tags));

                if (urgent.HasValue)
                    query = query.Where(r => r.IsUrgent == urgent.Value);

                var runners = await query
                    .OrderByDescending(r => r.DateReported)
                    .ToListAsync();

                var dtos = runners.Select(r => new RunnerDto
                {
                    Id = r.Id,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    FullName = $"{r.FirstName} {r.LastName}",
                    RunnerId = r.RunnerId,
                    Age = r.Age,
                    CalculatedAge = r.CalculatedAge,
                    Gender = r.Gender,
                    Status = r.Status,
                    City = r.City,
                    State = r.State,
                    Address = r.Address,
                    Description = r.Description,
                    ContactInfo = r.ContactInfo,
                    DateReported = r.DateReported,
                    DateFound = r.DateFound,
                    LastSeen = r.LastSeen,
                    DateOfBirth = r.DateOfBirth,
                    Tags = string.IsNullOrEmpty(r.Tags) ? new List<string>() : r.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = r.IsActive,
                    IsUrgent = r.IsUrgent,
                    Height = r.Height,
                    Weight = r.Weight,
                    HairColor = r.HairColor,
                    EyeColor = r.EyeColor,
                    IdentifyingMarks = r.IdentifyingMarks,
                    MedicalConditions = r.MedicalConditions,
                    Medications = r.Medications,
                    Allergies = r.Allergies,
                    EmergencyContacts = r.EmergencyContacts,
                    ReportedBy = "Anonymous"
                });

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving runners");
                return StatusCode(500, new { message = "An error occurred while retrieving runners" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RunnerSearchDto>>> SearchRunners(
            [FromQuery] string? query,
            [FromQuery] string? status,
            [FromQuery] string? state,
            [FromQuery] int? limit = 20)
        {
            try
            {
                var runnersQuery = _context.Runners.Where(r => r.IsActive);

                // Apply status filter if provided
                if (!string.IsNullOrEmpty(status))
                    runnersQuery = runnersQuery.Where(r => r.Status == status);

                // Apply state filter if provided
                if (!string.IsNullOrEmpty(state))
                    runnersQuery = runnersQuery.Where(r => r.State == state);

                // Apply search query if provided
                if (!string.IsNullOrEmpty(query))
                {
                    var searchTerm = query.ToLower();
                    runnersQuery = runnersQuery.Where(r =>
                        r.FirstName.ToLower().Contains(searchTerm) ||
                        r.LastName.ToLower().Contains(searchTerm) ||
                        r.RunnerId.ToLower().Contains(searchTerm) ||
                        r.City.ToLower().Contains(searchTerm) ||
                        r.State.ToLower().Contains(searchTerm) ||
                        (r.Tags != null && r.Tags.ToLower().Contains(searchTerm))
                    );
                }

                var runners = await runnersQuery
                    .OrderByDescending(r => r.IsUrgent)
                    .ThenByDescending(r => r.DateReported)
                    .Take(limit ?? 20)
                    .ToListAsync();

                var searchResults = runners.Select(r => new RunnerSearchDto
                {
                    Id = r.Id,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    FullName = $"{r.FirstName} {r.LastName}",
                    RunnerId = r.RunnerId,
                    Age = r.Age,
                    CalculatedAge = r.CalculatedAge,
                    Gender = r.Gender,
                    Status = r.Status,
                    City = r.City,
                    State = r.State,
                    Description = r.Description,
                    DateReported = r.DateReported,
                    LastSeen = r.LastSeen,
                    IsUrgent = r.IsUrgent,
                    Height = r.Height,
                    Weight = r.Weight,
                    HairColor = r.HairColor,
                    EyeColor = r.EyeColor,
                    IdentifyingMarks = r.IdentifyingMarks
                });

                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching runners");
                return StatusCode(500, new { message = "An error occurred while searching runners" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RunnerDto>> GetRunner(int id)
        {
            try
            {
                var runner = await _context.Runners
                    .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

                if (runner == null)
                    return NotFound(new { message = "Runner not found" });

                var dto = new RunnerDto
                {
                    Id = runner.Id,
                    FirstName = runner.FirstName,
                    LastName = runner.LastName,
                    FullName = $"{runner.FirstName} {runner.LastName}",
                    RunnerId = runner.RunnerId,
                    Age = runner.Age,
                    CalculatedAge = runner.CalculatedAge,
                    Gender = runner.Gender,
                    Status = runner.Status,
                    City = runner.City,
                    State = runner.State,
                    Address = runner.Address,
                    Description = runner.Description,
                    ContactInfo = runner.ContactInfo,
                    DateReported = runner.DateReported,
                    DateFound = runner.DateFound,
                    LastSeen = runner.LastSeen,
                    DateOfBirth = runner.DateOfBirth,
                    Tags = string.IsNullOrEmpty(runner.Tags) ? new List<string>() : runner.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = runner.IsActive,
                    IsUrgent = runner.IsUrgent,
                    Height = runner.Height,
                    Weight = runner.Weight,
                    HairColor = runner.HairColor,
                    EyeColor = runner.EyeColor,
                    IdentifyingMarks = runner.IdentifyingMarks,
                    MedicalConditions = runner.MedicalConditions,
                    Medications = runner.Medications,
                    Allergies = runner.Allergies,
                    EmergencyContacts = runner.EmergencyContacts,
                    ReportedBy = "Anonymous"
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving runner {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the runner" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<RunnerDto>> CreateRunner([FromBody] CreateRunnerRequest request)
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

                // Generate unique runner ID
                var runnerId = await GenerateUniqueRunnerId();

                var runner = new Runner
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    RunnerId = runnerId,
                    Age = request.Age,
                    Gender = request.Gender,
                    Status = "missing",
                    City = request.City,
                    State = request.State,
                    Address = request.Address,
                    Description = request.Description,
                    ContactInfo = request.ContactInfo,
                    LastSeen = request.LastSeen,
                    DateOfBirth = request.DateOfBirth,
                    Tags = request.Tags,
                    IsUrgent = request.IsUrgent,
                    Height = request.Height,
                    Weight = request.Weight,
                    HairColor = request.HairColor,
                    EyeColor = request.EyeColor,
                    IdentifyingMarks = request.IdentifyingMarks,
                    MedicalConditions = request.MedicalConditions,
                    Medications = request.Medications,
                    Allergies = request.Allergies,
                    EmergencyContacts = request.EmergencyContacts,
                    DateReported = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Runners.Add(runner);
                await _context.SaveChangesAsync();

                var dto = new RunnerDto
                {
                    Id = runner.Id,
                    FirstName = runner.FirstName,
                    LastName = runner.LastName,
                    FullName = runner.FullName,
                    RunnerId = runner.RunnerId,
                    Age = runner.Age,
                    CalculatedAge = runner.CalculatedAge,
                    Gender = runner.Gender,
                    Status = runner.Status,
                    City = runner.City,
                    State = runner.State,
                    Address = runner.Address,
                    Description = runner.Description,
                    ContactInfo = runner.ContactInfo,
                    DateReported = runner.DateReported,
                    DateFound = runner.DateFound,
                    LastSeen = runner.LastSeen,
                    DateOfBirth = runner.DateOfBirth,
                    Tags = string.IsNullOrEmpty(runner.Tags) ? new List<string>() : runner.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = runner.IsActive,
                    IsUrgent = runner.IsUrgent,
                    Height = runner.Height,
                    Weight = runner.Weight,
                    HairColor = runner.HairColor,
                    EyeColor = runner.EyeColor,
                    IdentifyingMarks = runner.IdentifyingMarks,
                    MedicalConditions = runner.MedicalConditions,
                    Medications = runner.Medications,
                    Allergies = runner.Allergies,
                    EmergencyContacts = runner.EmergencyContacts,
                    ReportedBy = "Anonymous"
                };

                _logger.LogInformation("Created new runner: {RunnerId}", runner.RunnerId);
                return CreatedAtAction(nameof(GetRunner), new { id = runner.Id }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating runner");
                return StatusCode(500, new { message = "An error occurred while creating the runner" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RunnerDto>> UpdateRunner(int id, [FromBody] UpdateRunnerRequest request)
        {
            try
            {
                var runner = await _context.Runners.FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
                if (runner == null)
                    return NotFound(new { message = "Runner not found" });

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.FirstName))
                    runner.FirstName = request.FirstName;

                if (!string.IsNullOrEmpty(request.LastName))
                    runner.LastName = request.LastName;

                if (request.Age.HasValue)
                    runner.Age = request.Age.Value;

                if (!string.IsNullOrEmpty(request.Gender))
                    runner.Gender = request.Gender;

                if (!string.IsNullOrEmpty(request.Status))
                    runner.Status = request.Status;

                if (!string.IsNullOrEmpty(request.City))
                    runner.City = request.City;

                if (!string.IsNullOrEmpty(request.State))
                    runner.State = request.State;

                if (!string.IsNullOrEmpty(request.Address))
                    runner.Address = request.Address;

                if (!string.IsNullOrEmpty(request.Description))
                    runner.Description = request.Description;

                if (!string.IsNullOrEmpty(request.ContactInfo))
                    runner.ContactInfo = request.ContactInfo;

                if (request.LastSeen.HasValue)
                    runner.LastSeen = request.LastSeen;

                if (request.DateOfBirth.HasValue)
                    runner.DateOfBirth = request.DateOfBirth;

                if (!string.IsNullOrEmpty(request.Tags))
                    runner.Tags = request.Tags;

                if (request.IsUrgent.HasValue)
                    runner.IsUrgent = request.IsUrgent.Value;

                if (!string.IsNullOrEmpty(request.Height))
                    runner.Height = request.Height;

                if (!string.IsNullOrEmpty(request.Weight))
                    runner.Weight = request.Weight;

                if (!string.IsNullOrEmpty(request.HairColor))
                    runner.HairColor = request.HairColor;

                if (!string.IsNullOrEmpty(request.EyeColor))
                    runner.EyeColor = request.EyeColor;

                if (!string.IsNullOrEmpty(request.IdentifyingMarks))
                    runner.IdentifyingMarks = request.IdentifyingMarks;

                if (!string.IsNullOrEmpty(request.MedicalConditions))
                    runner.MedicalConditions = request.MedicalConditions;

                if (!string.IsNullOrEmpty(request.Medications))
                    runner.Medications = request.Medications;

                if (!string.IsNullOrEmpty(request.Allergies))
                    runner.Allergies = request.Allergies;

                if (!string.IsNullOrEmpty(request.EmergencyContacts))
                    runner.EmergencyContacts = request.EmergencyContacts;

                // Update DateFound if status changed to found
                if (request.Status == "found" && runner.Status != "found")
                {
                    runner.DateFound = DateTime.UtcNow;
                }

                runner.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var dto = new RunnerDto
                {
                    Id = runner.Id,
                    FirstName = runner.FirstName,
                    LastName = runner.LastName,
                    FullName = runner.FullName,
                    RunnerId = runner.RunnerId,
                    Age = runner.Age,
                    CalculatedAge = runner.CalculatedAge,
                    Gender = runner.Gender,
                    Status = runner.Status,
                    City = runner.City,
                    State = runner.State,
                    Address = runner.Address,
                    Description = runner.Description,
                    ContactInfo = runner.ContactInfo,
                    DateReported = runner.DateReported,
                    DateFound = runner.DateFound,
                    LastSeen = runner.LastSeen,
                    DateOfBirth = runner.DateOfBirth,
                    Tags = string.IsNullOrEmpty(runner.Tags) ? new List<string>() : runner.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = runner.IsActive,
                    IsUrgent = runner.IsUrgent,
                    Height = runner.Height,
                    Weight = runner.Weight,
                    HairColor = runner.HairColor,
                    EyeColor = runner.EyeColor,
                    IdentifyingMarks = runner.IdentifyingMarks,
                    MedicalConditions = runner.MedicalConditions,
                    Medications = runner.Medications,
                    Allergies = runner.Allergies,
                    EmergencyContacts = runner.EmergencyContacts,
                    ReportedBy = "Anonymous"
                };

                _logger.LogInformation("Updated runner: {RunnerId}", runner.RunnerId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating runner {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating the runner" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRunner(int id)
        {
            try
            {
                var runner = await _context.Runners.FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
                if (runner == null)
                    return NotFound(new { message = "Runner not found" });

                runner.IsActive = false;
                runner.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted runner: {RunnerId}", runner.RunnerId);
                return Ok(new { message = "Runner deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting runner {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the runner" });
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetStats()
        {
            try
            {
                var stats = await _context.Runners
                    .Where(r => r.IsActive)
                    .GroupBy(r => r.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var total = await _context.Runners.CountAsync(r => r.IsActive);
                var urgent = await _context.Runners.CountAsync(r => r.IsActive && r.IsUrgent);
                var recent = await _context.Runners.CountAsync(r => r.IsActive && r.DateReported >= DateTime.UtcNow.AddDays(-30));

                return Ok(new
                {
                    total,
                    urgent,
                    recent,
                    byStatus = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats");
                return StatusCode(500, new { message = "An error occurred while retrieving stats" });
            }
        }

        private async Task<string> GenerateUniqueRunnerId()
        {
            var year = DateTime.UtcNow.Year;
            var existingIds = await _context.Runners
                .Where(r => r.RunnerId.StartsWith($"RUN-{year}-"))
                .Select(r => r.RunnerId)
                .ToListAsync();

            var maxNumber = 0;
            foreach (var id in existingIds)
            {
                if (int.TryParse(id.Split('-').Last(), out var number))
                {
                    maxNumber = Math.Max(maxNumber, number);
                }
            }

            return $"RUN-{year}-{(maxNumber + 1):D3}";
        }
    }
} 