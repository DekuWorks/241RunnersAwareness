using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Models;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CasesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CasesController> _logger;

        public CasesController(ApplicationDbContext context, ILogger<CasesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CaseDto>>> GetCases(
            [FromQuery] string? id,
            [FromQuery] string? name,
            [FromQuery] int? age,
            [FromQuery] string? status,
            [FromQuery] string? state,
            [FromQuery] string? tags)
        {
            try
            {
                var query = _context.Cases.Where(c => c.IsActive);

                // Apply filters
                if (!string.IsNullOrEmpty(id))
                    query = query.Where(c => c.CaseId.Contains(id));

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(c => c.Name.Contains(name));

                if (age.HasValue)
                    query = query.Where(c => c.Age == age.Value);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(c => c.Status == status);

                if (!string.IsNullOrEmpty(state))
                    query = query.Where(c => c.State.Contains(state));

                if (!string.IsNullOrEmpty(tags))
                    query = query.Where(c => c.Tags.Contains(tags));

                var cases = await query
                    .OrderByDescending(c => c.DateReported)
                    .ToListAsync();

                var caseDtos = cases.Select(c => new CaseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CaseId = c.CaseId,
                    Age = c.Age,
                    Status = c.Status,
                    City = c.City,
                    State = c.State,
                    Description = c.Description,
                    ContactInfo = c.ContactInfo,
                    DateReported = c.DateReported,
                    DateFound = c.DateFound,
                    LastSeen = c.LastSeen,
                    Tags = string.IsNullOrEmpty(c.Tags) ? new List<string>() : c.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = c.IsActive
                });

                return Ok(caseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cases");
                return StatusCode(500, new { message = "An error occurred while retrieving cases" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CaseDto>> GetCase(int id)
        {
            try
            {
                var caseItem = await _context.Cases
                    .Include(c => c.ReportedByUser)
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (caseItem == null)
                    return NotFound(new { message = "Case not found" });

                var caseDto = new CaseDto
                {
                    Id = caseItem.Id,
                    Name = caseItem.Name,
                    CaseId = caseItem.CaseId,
                    Age = caseItem.Age,
                    Status = caseItem.Status,
                    City = caseItem.City,
                    State = caseItem.State,
                    Description = caseItem.Description,
                    ContactInfo = caseItem.ContactInfo,
                    DateReported = caseItem.DateReported,
                    DateFound = caseItem.DateFound,
                    LastSeen = caseItem.LastSeen,
                    Tags = string.IsNullOrEmpty(caseItem.Tags) ? new List<string>() : caseItem.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = caseItem.IsActive
                };

                return Ok(caseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving case {CaseId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the case" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CaseDto>> CreateCase([FromBody] CreateCaseRequest request)
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

                // Check if case ID already exists
                var existingCase = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == request.CaseId);
                if (existingCase != null)
                {
                    return BadRequest(new { message = "A case with this ID already exists" });
                }

                var caseItem = new Case
                {
                    Name = request.Name,
                    CaseId = request.CaseId,
                    Age = request.Age,
                    Status = "missing",
                    City = request.City,
                    State = request.State,
                    Description = request.Description,
                    ContactInfo = request.ContactInfo,
                    LastSeen = request.LastSeen,
                    Tags = request.Tags,
                    DateReported = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Cases.Add(caseItem);
                await _context.SaveChangesAsync();

                var caseDto = new CaseDto
                {
                    Id = caseItem.Id,
                    Name = caseItem.Name,
                    CaseId = caseItem.CaseId,
                    Age = caseItem.Age,
                    Status = caseItem.Status,
                    City = caseItem.City,
                    State = caseItem.State,
                    Description = caseItem.Description,
                    ContactInfo = caseItem.ContactInfo,
                    DateReported = caseItem.DateReported,
                    DateFound = caseItem.DateFound,
                    LastSeen = caseItem.LastSeen,
                    Tags = string.IsNullOrEmpty(caseItem.Tags) ? new List<string>() : caseItem.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = caseItem.IsActive
                };

                _logger.LogInformation("Created new case: {CaseId}", caseItem.CaseId);
                return CreatedAtAction(nameof(GetCase), new { id = caseItem.Id }, caseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating case");
                return StatusCode(500, new { message = "An error occurred while creating the case" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CaseDto>> UpdateCase(int id, [FromBody] UpdateCaseRequest request)
        {
            try
            {
                var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
                if (caseItem == null)
                    return NotFound(new { message = "Case not found" });

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.Name))
                    caseItem.Name = request.Name;

                if (request.Age.HasValue)
                    caseItem.Age = request.Age.Value;

                if (!string.IsNullOrEmpty(request.Status))
                    caseItem.Status = request.Status;

                if (!string.IsNullOrEmpty(request.City))
                    caseItem.City = request.City;

                if (!string.IsNullOrEmpty(request.State))
                    caseItem.State = request.State;

                if (!string.IsNullOrEmpty(request.Description))
                    caseItem.Description = request.Description;

                if (!string.IsNullOrEmpty(request.ContactInfo))
                    caseItem.ContactInfo = request.ContactInfo;

                if (request.LastSeen.HasValue)
                    caseItem.LastSeen = request.LastSeen;

                if (!string.IsNullOrEmpty(request.Tags))
                    caseItem.Tags = request.Tags;

                // Update DateFound if status changed to found
                if (request.Status == "found" && caseItem.Status != "found")
                {
                    caseItem.DateFound = DateTime.UtcNow;
                }

                caseItem.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var caseDto = new CaseDto
                {
                    Id = caseItem.Id,
                    Name = caseItem.Name,
                    CaseId = caseItem.CaseId,
                    Age = caseItem.Age,
                    Status = caseItem.Status,
                    City = caseItem.City,
                    State = caseItem.State,
                    Description = caseItem.Description,
                    ContactInfo = caseItem.ContactInfo,
                    DateReported = caseItem.DateReported,
                    DateFound = caseItem.DateFound,
                    LastSeen = caseItem.LastSeen,
                    Tags = string.IsNullOrEmpty(caseItem.Tags) ? new List<string>() : caseItem.Tags.Split(',').Select(t => t.Trim()).ToList(),
                    IsActive = caseItem.IsActive
                };

                _logger.LogInformation("Updated case: {CaseId}", caseItem.CaseId);
                return Ok(caseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating case {CaseId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the case" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCase(int id)
        {
            try
            {
                var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
                if (caseItem == null)
                    return NotFound(new { message = "Case not found" });

                caseItem.IsActive = false;
                caseItem.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted case: {CaseId}", caseItem.CaseId);
                return Ok(new { message = "Case deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting case {CaseId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the case" });
            }
        }
    }
} 