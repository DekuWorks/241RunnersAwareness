using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using _241RunnersAwareness.BackendAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using System.Security.Claims;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DNAController : ControllerBase
    {
        private readonly RunnersDbContext _context;
        private readonly ILogger<DNAController> _logger;

        public DNAController(RunnersDbContext context, ILogger<DNAController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all DNA reports
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DNAReport>>> GetDNAReports()
        {
            try
            {
                var reports = await _context.DNAReports
                    .Include(r => r.Reporter)
                    .Include(r => r.Individual)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DNA reports");
                return StatusCode(500, "An error occurred while retrieving DNA reports");
            }
        }

        /// <summary>
        /// Get DNA report by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DNAReport>> GetDNAReport(Guid id)
        {
            try
            {
                var report = await _context.DNAReports
                    .Include(r => r.Reporter)
                    .Include(r => r.Individual)
                    .FirstOrDefaultAsync(r => r.ReportId == id);

                if (report == null)
                {
                    return NotFound("DNA report not found");
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DNA report {ReportId}", id);
                return StatusCode(500, "An error occurred while retrieving the DNA report");
            }
        }

        /// <summary>
        /// Get DNA reports by individual ID
        /// </summary>
        [HttpGet("individual/{individualId}")]
        public async Task<ActionResult<IEnumerable<DNAReport>>> GetDNAReportsByIndividual(int individualId)
        {
            try
            {
                var reports = await _context.DNAReports
                    .Include(r => r.Reporter)
                    .Include(r => r.Individual)
                    .Where(r => r.IndividualId == individualId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DNA reports for individual {IndividualId}", individualId);
                return StatusCode(500, "An error occurred while retrieving DNA reports");
            }
        }

        /// <summary>
        /// Create a new DNA report
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DNAReport>> CreateDNAReport([FromBody] CreateDNAReportRequest request)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("User not authenticated");
                }

                // Verify individual exists
                var individual = await _context.Individuals.FindAsync(request.IndividualId);
                if (individual == null)
                {
                    return BadRequest("Individual not found");
                }

                var report = new DNAReport
                {
                    ReportId = Guid.NewGuid(),
                    ReporterUserId = userId,
                    IndividualId = request.IndividualId,
                    ReportTitle = request.ReportTitle,
                    Description = request.Description,
                    ReportDate = request.ReportDate ?? DateTime.UtcNow,
                    Location = request.Location,
                    Status = "Active",
                    DNASampleDescription = request.DNASampleDescription,
                    DNASampleType = request.DNASampleType,
                    DNASampleLocation = request.DNASampleLocation,
                    DNASampleCollectionDate = request.DNASampleCollectionDate,
                    DNALabReference = request.DNALabReference,
                    DNASequence = request.DNASequence,
                    DNASampleCollected = request.DNASampleCollected,
                    DNASampleProcessed = request.DNASampleProcessed,
                    DNASampleMatched = request.DNASampleMatched,
                    WeatherConditions = request.WeatherConditions,
                    ClothingDescription = request.ClothingDescription,
                    PhysicalDescription = request.PhysicalDescription,
                    BehaviorDescription = request.BehaviorDescription,
                    WitnessName = request.WitnessName,
                    WitnessPhone = request.WitnessPhone,
                    WitnessEmail = request.WitnessEmail,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DNAReports.Add(report);
                await _context.SaveChangesAsync();

                // Return the created report with navigation properties
                var createdReport = await _context.DNAReports
                    .Include(r => r.Reporter)
                    .Include(r => r.Individual)
                    .FirstAsync(r => r.ReportId == report.ReportId);

                return CreatedAtAction(nameof(GetDNAReport), new { id = report.ReportId }, createdReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating DNA report");
                return StatusCode(500, "An error occurred while creating the DNA report");
            }
        }

        /// <summary>
        /// Update DNA report
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDNAReport(Guid id, [FromBody] UpdateDNAReportRequest request)
        {
            try
            {
                var report = await _context.DNAReports.FindAsync(id);
                if (report == null)
                {
                    return NotFound("DNA report not found");
                }

                // Update fields
                if (!string.IsNullOrEmpty(request.ReportTitle))
                    report.ReportTitle = request.ReportTitle;
                
                if (!string.IsNullOrEmpty(request.Description))
                    report.Description = request.Description;
                
                if (!string.IsNullOrEmpty(request.Location))
                    report.Location = request.Location;
                
                if (!string.IsNullOrEmpty(request.Status))
                    report.Status = request.Status;
                
                if (!string.IsNullOrEmpty(request.DNASampleDescription))
                    report.DNASampleDescription = request.DNASampleDescription;
                
                if (!string.IsNullOrEmpty(request.DNASampleType))
                    report.DNASampleType = request.DNASampleType;
                
                if (!string.IsNullOrEmpty(request.DNASampleLocation))
                    report.DNASampleLocation = request.DNASampleLocation;
                
                if (request.DNASampleCollectionDate.HasValue)
                    report.DNASampleCollectionDate = request.DNASampleCollectionDate;
                
                if (!string.IsNullOrEmpty(request.DNALabReference))
                    report.DNALabReference = request.DNALabReference;
                
                if (!string.IsNullOrEmpty(request.DNASequence))
                    report.DNASequence = request.DNASequence;
                
                report.DNASampleCollected = request.DNASampleCollected ?? report.DNASampleCollected;
                report.DNASampleProcessed = request.DNASampleProcessed ?? report.DNASampleProcessed;
                report.DNASampleMatched = request.DNASampleMatched ?? report.DNASampleMatched;
                
                if (!string.IsNullOrEmpty(request.WeatherConditions))
                    report.WeatherConditions = request.WeatherConditions;
                
                if (!string.IsNullOrEmpty(request.ClothingDescription))
                    report.ClothingDescription = request.ClothingDescription;
                
                if (!string.IsNullOrEmpty(request.PhysicalDescription))
                    report.PhysicalDescription = request.PhysicalDescription;
                
                if (!string.IsNullOrEmpty(request.BehaviorDescription))
                    report.BehaviorDescription = request.BehaviorDescription;
                
                if (!string.IsNullOrEmpty(request.WitnessName))
                    report.WitnessName = request.WitnessName;
                
                if (!string.IsNullOrEmpty(request.WitnessPhone))
                    report.WitnessPhone = request.WitnessPhone;
                
                if (!string.IsNullOrEmpty(request.WitnessEmail))
                    report.WitnessEmail = request.WitnessEmail;
                
                if (!string.IsNullOrEmpty(request.ResolutionNotes))
                    report.ResolutionNotes = request.ResolutionNotes;
                
                if (!string.IsNullOrEmpty(request.ResolvedBy))
                    report.ResolvedBy = request.ResolvedBy;
                
                if (request.ResolutionDate.HasValue)
                    report.ResolutionDate = request.ResolutionDate;
                
                report.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DNA report {ReportId}", id);
                return StatusCode(500, "An error occurred while updating the DNA report");
            }
        }

        /// <summary>
        /// Delete DNA report
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDNAReport(Guid id)
        {
            try
            {
                var report = await _context.DNAReports.FindAsync(id);
                if (report == null)
                {
                    return NotFound("DNA report not found");
                }

                _context.DNAReports.Remove(report);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting DNA report {ReportId}", id);
                return StatusCode(500, "An error occurred while deleting the DNA report");
            }
        }

        /// <summary>
        /// Get DNA reports by status
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<DNAReport>>> GetDNAReportsByStatus(string status)
        {
            try
            {
                var reports = await _context.DNAReports
                    .Include(r => r.Reporter)
                    .Include(r => r.Individual)
                    .Where(r => r.Status == status)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DNA reports by status {Status}", status);
                return StatusCode(500, "An error occurred while retrieving DNA reports");
            }
        }
    }

    // Request DTOs
    public class CreateDNAReportRequest
    {
        public int IndividualId { get; set; }
        public string ReportTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? ReportDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? DNASampleDescription { get; set; }
        public string? DNASampleType { get; set; }
        public string? DNASampleLocation { get; set; }
        public DateTime? DNASampleCollectionDate { get; set; }
        public string? DNALabReference { get; set; }
        public string? DNASequence { get; set; }
        public bool DNASampleCollected { get; set; } = false;
        public bool DNASampleProcessed { get; set; } = false;
        public bool DNASampleMatched { get; set; } = false;
        public string? WeatherConditions { get; set; }
        public string? ClothingDescription { get; set; }
        public string? PhysicalDescription { get; set; }
        public string? BehaviorDescription { get; set; }
        public string? WitnessName { get; set; }
        public string? WitnessPhone { get; set; }
        public string? WitnessEmail { get; set; }
    }

    public class UpdateDNAReportRequest
    {
        public string? ReportTitle { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public string? DNASampleDescription { get; set; }
        public string? DNASampleType { get; set; }
        public string? DNASampleLocation { get; set; }
        public DateTime? DNASampleCollectionDate { get; set; }
        public string? DNALabReference { get; set; }
        public string? DNASequence { get; set; }
        public bool? DNASampleCollected { get; set; }
        public bool? DNASampleProcessed { get; set; }
        public bool? DNASampleMatched { get; set; }
        public string? WeatherConditions { get; set; }
        public string? ClothingDescription { get; set; }
        public string? PhysicalDescription { get; set; }
        public string? BehaviorDescription { get; set; }
        public string? WitnessName { get; set; }
        public string? WitnessPhone { get; set; }
        public string? WitnessEmail { get; set; }
        public string? ResolutionNotes { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolutionDate { get; set; }
    }
} 