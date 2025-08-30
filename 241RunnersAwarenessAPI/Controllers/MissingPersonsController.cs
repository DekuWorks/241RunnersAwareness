using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Models;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MissingPersonsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MissingPersonsController> _logger;

        public MissingPersonsController(ApplicationDbContext context, ILogger<MissingPersonsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MissingPersonDto>>> GetMissingPersons(
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
                var query = _context.MissingPersons.Where(mp => mp.IsActive);

                // Apply filters
                if (!string.IsNullOrEmpty(id))
                    query = query.Where(mp => mp.CaseId.Contains(id));

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(mp => 
                        mp.FirstName.Contains(name) || 
                        mp.LastName.Contains(name) || 
                        mp.FullName.Contains(name));

                if (age.HasValue)
                    query = query.Where(mp => mp.Age == age.Value);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(mp => mp.Status == status);

                if (!string.IsNullOrEmpty(state))
                    query = query.Where(mp => mp.State.Contains(state));

                if (!string.IsNullOrEmpty(tags))
                    query = query.Where(mp => mp.Tags.Contains(tags));

                if (urgent.HasValue)
                    query = query.Where(mp => mp.IsUrgent == urgent.Value);

                var missingPersons = await query
                    .Include(mp => mp.ReportedByUser)
                    .OrderByDescending(mp => mp.DateReported)
                    .ToListAsync();

                var dtos = missingPersons.Select(mp => new MissingPersonDto
                {
                    Id = mp.Id,
                    FirstName = mp.FirstName,
                    LastName = mp.LastName,
                    FullName = mp.FullName,
                    CaseId = mp.CaseId,
                    Age = mp.Age,
                    CalculatedAge = mp.CalculatedAge,
                    Gender = mp.Gender,
                    Status = mp.Status,
                    City = mp.City,
                    State = mp.State,
                    Address = mp.Address,
                    Description = mp.Description,
                    ContactInfo = mp.ContactInfo,
                    DateReported = mp.DateReported,
                    DateFound = mp.DateFound,
                    LastSeen = mp.LastSeen,
                    DateOfBirth = mp.DateOfBirth,
                    Tags = string.IsNullOrEmpty(mp.Tags) ? new List<string>() : mp.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = mp.IsActive,
                    IsUrgent = mp.IsUrgent,
                    ReportedBy = mp.ReportedByUser?.FullName ?? "Anonymous"
                });

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving missing persons");
                return StatusCode(500, new { message = "An error occurred while retrieving missing persons" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MissingPersonDto>> GetMissingPerson(int id)
        {
            try
            {
                var missingPerson = await _context.MissingPersons
                    .Include(mp => mp.ReportedByUser)
                    .FirstOrDefaultAsync(mp => mp.Id == id && mp.IsActive);

                if (missingPerson == null)
                    return NotFound(new { message = "Missing person not found" });

                var dto = new MissingPersonDto
                {
                    Id = missingPerson.Id,
                    FirstName = missingPerson.FirstName,
                    LastName = missingPerson.LastName,
                    FullName = missingPerson.FullName,
                    CaseId = missingPerson.CaseId,
                    Age = missingPerson.Age,
                    CalculatedAge = missingPerson.CalculatedAge,
                    Gender = missingPerson.Gender,
                    Status = missingPerson.Status,
                    City = missingPerson.City,
                    State = missingPerson.State,
                    Address = missingPerson.Address,
                    Description = missingPerson.Description,
                    ContactInfo = missingPerson.ContactInfo,
                    DateReported = missingPerson.DateReported,
                    DateFound = missingPerson.DateFound,
                    LastSeen = missingPerson.LastSeen,
                    DateOfBirth = missingPerson.DateOfBirth,
                    Tags = string.IsNullOrEmpty(missingPerson.Tags) ? new List<string>() : missingPerson.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = missingPerson.IsActive,
                    IsUrgent = missingPerson.IsUrgent,
                    ReportedBy = missingPerson.ReportedByUser?.FullName ?? "Anonymous"
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving missing person {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the missing person" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<MissingPersonDto>> CreateMissingPerson([FromBody] CreateMissingPersonRequest request)
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

                // Generate unique case ID
                var caseId = await GenerateUniqueCaseId();

                var missingPerson = new MissingPerson
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CaseId = caseId,
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
                    DateReported = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MissingPersons.Add(missingPerson);
                await _context.SaveChangesAsync();

                var dto = new MissingPersonDto
                {
                    Id = missingPerson.Id,
                    FirstName = missingPerson.FirstName,
                    LastName = missingPerson.LastName,
                    FullName = missingPerson.FullName,
                    CaseId = missingPerson.CaseId,
                    Age = missingPerson.Age,
                    CalculatedAge = missingPerson.CalculatedAge,
                    Gender = missingPerson.Gender,
                    Status = missingPerson.Status,
                    City = missingPerson.City,
                    State = missingPerson.State,
                    Address = missingPerson.Address,
                    Description = missingPerson.Description,
                    ContactInfo = missingPerson.ContactInfo,
                    DateReported = missingPerson.DateReported,
                    DateFound = missingPerson.DateFound,
                    LastSeen = missingPerson.LastSeen,
                    DateOfBirth = missingPerson.DateOfBirth,
                    Tags = string.IsNullOrEmpty(missingPerson.Tags) ? new List<string>() : missingPerson.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = missingPerson.IsActive,
                    IsUrgent = missingPerson.IsUrgent,
                    ReportedBy = "Anonymous"
                };

                _logger.LogInformation("Created new missing person: {CaseId}", missingPerson.CaseId);
                return CreatedAtAction(nameof(GetMissingPerson), new { id = missingPerson.Id }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating missing person");
                return StatusCode(500, new { message = "An error occurred while creating the missing person" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MissingPersonDto>> UpdateMissingPerson(int id, [FromBody] UpdateMissingPersonRequest request)
        {
            try
            {
                var missingPerson = await _context.MissingPersons.FirstOrDefaultAsync(mp => mp.Id == id && mp.IsActive);
                if (missingPerson == null)
                    return NotFound(new { message = "Missing person not found" });

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.FirstName))
                    missingPerson.FirstName = request.FirstName;

                if (!string.IsNullOrEmpty(request.LastName))
                    missingPerson.LastName = request.LastName;

                if (request.Age.HasValue)
                    missingPerson.Age = request.Age.Value;

                if (!string.IsNullOrEmpty(request.Gender))
                    missingPerson.Gender = request.Gender;

                if (!string.IsNullOrEmpty(request.Status))
                    missingPerson.Status = request.Status;

                if (!string.IsNullOrEmpty(request.City))
                    missingPerson.City = request.City;

                if (!string.IsNullOrEmpty(request.State))
                    missingPerson.State = request.State;

                if (!string.IsNullOrEmpty(request.Address))
                    missingPerson.Address = request.Address;

                if (!string.IsNullOrEmpty(request.Description))
                    missingPerson.Description = request.Description;

                if (!string.IsNullOrEmpty(request.ContactInfo))
                    missingPerson.ContactInfo = request.ContactInfo;

                if (request.LastSeen.HasValue)
                    missingPerson.LastSeen = request.LastSeen;

                if (request.DateOfBirth.HasValue)
                    missingPerson.DateOfBirth = request.DateOfBirth;

                if (!string.IsNullOrEmpty(request.Tags))
                    missingPerson.Tags = request.Tags;

                if (request.IsUrgent.HasValue)
                    missingPerson.IsUrgent = request.IsUrgent.Value;

                // Update DateFound if status changed to found
                if (request.Status == "found" && missingPerson.Status != "found")
                {
                    missingPerson.DateFound = DateTime.UtcNow;
                }

                missingPerson.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var dto = new MissingPersonDto
                {
                    Id = missingPerson.Id,
                    FirstName = missingPerson.FirstName,
                    LastName = missingPerson.LastName,
                    FullName = missingPerson.FullName,
                    CaseId = missingPerson.CaseId,
                    Age = missingPerson.Age,
                    CalculatedAge = missingPerson.CalculatedAge,
                    Gender = missingPerson.Gender,
                    Status = missingPerson.Status,
                    City = missingPerson.City,
                    State = missingPerson.State,
                    Address = missingPerson.Address,
                    Description = missingPerson.Description,
                    ContactInfo = missingPerson.ContactInfo,
                    DateReported = missingPerson.DateReported,
                    DateFound = missingPerson.DateFound,
                    LastSeen = missingPerson.LastSeen,
                    DateOfBirth = missingPerson.DateOfBirth,
                    Tags = string.IsNullOrEmpty(missingPerson.Tags) ? new List<string>() : missingPerson.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = missingPerson.IsActive,
                    IsUrgent = missingPerson.IsUrgent,
                    ReportedBy = "Anonymous"
                };

                _logger.LogInformation("Updated missing person: {CaseId}", missingPerson.CaseId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating missing person {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating the missing person" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMissingPerson(int id)
        {
            try
            {
                var missingPerson = await _context.MissingPersons.FirstOrDefaultAsync(mp => mp.Id == id && mp.IsActive);
                if (missingPerson == null)
                    return NotFound(new { message = "Missing person not found" });

                missingPerson.IsActive = false;
                missingPerson.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted missing person: {CaseId}", missingPerson.CaseId);
                return Ok(new { message = "Missing person deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting missing person {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the missing person" });
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetStats()
        {
            try
            {
                var stats = await _context.MissingPersons
                    .Where(mp => mp.IsActive)
                    .GroupBy(mp => mp.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var total = await _context.MissingPersons.CountAsync(mp => mp.IsActive);
                var urgent = await _context.MissingPersons.CountAsync(mp => mp.IsActive && mp.IsUrgent);
                var recent = await _context.MissingPersons.CountAsync(mp => mp.IsActive && mp.DateReported >= DateTime.UtcNow.AddDays(-30));

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

        private async Task<string> GenerateUniqueCaseId()
        {
            var year = DateTime.UtcNow.Year;
            var existingIds = await _context.MissingPersons
                .Where(mp => mp.CaseId.StartsWith($"RUN-{year}-"))
                .Select(mp => mp.CaseId)
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