using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using _241RunnersAwareness.BackendAPI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IndividualController : ControllerBase
    {
        private readonly RunnersDbContext _context;
        private readonly ICsvExportService _csvExportService;

        public IndividualController(RunnersDbContext context, ICsvExportService csvExportService)
        {
            _context = context;
            _csvExportService = csvExportService;
        }

        // GET: api/individual
        [HttpGet]
        public ActionResult<IEnumerable<Individual>> GetIndividuals()
        {
            return _context.Individuals.ToList();
        }

        // POST: api/individual
        [HttpPost]
        public async Task<ActionResult<Individual>> PostIndividual(Individual individual)
        {
            _context.Individuals.Add(individual);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetIndividuals), new { id = individual.IndividualId }, individual);
        }

        // GET: api/individual/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportIndividualsToCsv()
        {
            try
            {
                var individuals = await _context.Individuals.ToListAsync();
                var csvBytes = _csvExportService.ExportIndividualsToCsv(individuals);
                
                var fileName = $"runners_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while exporting data." });
            }
        }

        // GET: api/individual/export/filtered
        [HttpGet("export/filtered")]
        public async Task<IActionResult> ExportFilteredIndividualsToCsv(
            [FromQuery] string? status = null,
            [FromQuery] string? state = null,
            [FromQuery] string? gender = null)
        {
            try
            {
                var query = _context.Individuals.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(i => i.CurrentStatus == status);
                }

                if (!string.IsNullOrEmpty(state))
                {
                    query = query.Where(i => i.State == state);
                }

                if (!string.IsNullOrEmpty(gender))
                {
                    query = query.Where(i => i.Gender == gender);
                }

                var individuals = await query.ToListAsync();
                var csvBytes = _csvExportService.ExportIndividualsToCsv(individuals);
                
                var fileName = $"runners_filtered_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while exporting filtered data." });
            }
        }
    }
}
