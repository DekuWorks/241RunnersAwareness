using _241RunnersAwareness.BackendAPI.Models;
using _241RunnersAwareness.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CasesController : ControllerBase
{
    private readonly RunnersDbContext _context;

    public CasesController(RunnersDbContext context)
    {
        _context = context;
    }

    // GET: api/cases/me
    [HttpGet("me")]
    public async Task<ActionResult<CaseDetailsDto>> GetMyCase()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var individual = await _context.Individuals
            .FirstOrDefaultAsync(i => i.Id.ToString() == userId);

        if (individual == null)
        {
            return NotFound("No case file found for the current user.");
        }

        var caseDetails = new CaseDetailsDto
        {
            Id = individual.Id.ToString(),
            Name = individual.FullName,
            Status = individual.CurrentStatus,
            UpdatedAt = individual.LastPhotoUpdate,
            Image = individual.PhotoUrl
        };

        return caseDetails;
    }

    // PATCH: api/cases/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCaseImage(string id, [FromBody] CaseImageUpdateDto caseUpdate)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id != userId)
        {
            return Forbid("Users can only update their own case.");
        }

        var individual = await _context.Individuals.FindAsync(System.Guid.Parse(id));

        if (individual == null)
        {
            return NotFound();
        }
        
        individual.PhotoUrl = caseUpdate.Image;
        individual.LastPhotoUpdate = System.DateTime.UtcNow;

        _context.Entry(individual).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }
} 