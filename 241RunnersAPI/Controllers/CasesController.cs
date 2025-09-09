using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Controllers
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

        /// <summary>
        /// Get public cases (no authentication required)
        /// </summary>
        [HttpGet("publiccases")]
        public async Task<IActionResult> GetPublicCases([FromQuery] string? status = null, [FromQuery] string? priority = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Cases
                    .Include(c => c.Runner)
                    .Include(c => c.ReportedByUser)
                    .Where(c => c.IsPublic && c.IsApproved);

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(c => c.Status == status);
                }

                if (!string.IsNullOrEmpty(priority))
                {
                    query = query.Where(c => c.Priority == priority);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var cases = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CaseResponseDto
                    {
                        Id = c.Id,
                        RunnerId = c.RunnerId,
                        ReportedByUserId = c.ReportedByUserId,
                        Title = c.Title,
                        Description = c.Description,
                        LastSeenDate = c.LastSeenDate,
                        LastSeenLocation = c.LastSeenLocation,
                        LastSeenTime = c.LastSeenTime,
                        LastSeenCircumstances = c.LastSeenCircumstances,
                        ClothingDescription = c.ClothingDescription,
                        PhysicalCondition = c.PhysicalCondition,
                        MentalState = c.MentalState,
                        AdditionalInformation = c.AdditionalInformation,
                        Status = c.Status,
                        Priority = c.Priority,
                        IsPublic = c.IsPublic,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        ResolvedAt = c.ResolvedAt,
                        ResolutionNotes = c.ResolutionNotes,
                        ResolvedBy = c.ResolvedBy,
                        ContactPersonName = c.ContactPersonName,
                        ContactPersonPhone = c.ContactPersonPhone,
                        ContactPersonEmail = c.ContactPersonEmail,
                        LastSeenLatitude = c.LastSeenLatitude,
                        LastSeenLongitude = c.LastSeenLongitude,
                        CaseImageUrls = c.CaseImageUrls,
                        DocumentUrls = c.DocumentUrls,
                        EmergencyContactName = c.EmergencyContactName,
                        EmergencyContactPhone = c.EmergencyContactPhone,
                        EmergencyContactRelationship = c.EmergencyContactRelationship,
                        IsVerified = c.IsVerified,
                        VerifiedAt = c.VerifiedAt,
                        VerifiedBy = c.VerifiedBy,
                        IsApproved = c.IsApproved,
                        ApprovedAt = c.ApprovedAt,
                        ApprovedBy = c.ApprovedBy,
                        ViewCount = c.ViewCount,
                        ShareCount = c.ShareCount,
                        TipCount = c.TipCount,
                        Runner = new RunnerResponseDto
                        {
                            Id = c.Runner.Id,
                            UserId = c.Runner.UserId,
                            Name = c.Runner.Name,
                            DateOfBirth = c.Runner.DateOfBirth,
                            Gender = c.Runner.Gender,
                            PhysicalDescription = c.Runner.PhysicalDescription,
                            MedicalConditions = c.Runner.MedicalConditions,
                            Medications = c.Runner.Medications,
                            Allergies = c.Runner.Allergies,
                            EmergencyInstructions = c.Runner.EmergencyInstructions,
                            PreferredRunningLocations = c.Runner.PreferredRunningLocations,
                            TypicalRunningTimes = c.Runner.TypicalRunningTimes,
                            ExperienceLevel = c.Runner.ExperienceLevel,
                            SpecialNeeds = c.Runner.SpecialNeeds,
                            AdditionalNotes = c.Runner.AdditionalNotes,
                            IsActive = c.Runner.IsActive,
                            CreatedAt = c.Runner.CreatedAt,
                            UpdatedAt = c.Runner.UpdatedAt,
                            LastKnownLocation = c.Runner.LastKnownLocation,
                            LastLocationUpdate = c.Runner.LastLocationUpdate,
                            PreferredContactMethod = c.Runner.PreferredContactMethod,
                            ProfileImageUrl = c.Runner.ProfileImageUrl,
                            IsProfileComplete = c.Runner.IsProfileComplete,
                            IsVerified = c.Runner.IsVerified,
                            VerifiedAt = c.Runner.VerifiedAt,
                            VerifiedBy = c.Runner.VerifiedBy
                        },
                        ReportedByUserName = $"{c.ReportedByUser.FirstName} {c.ReportedByUser.LastName}",
                        ReportedByUserEmail = c.ReportedByUser.Email
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    cases = cases,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public cases");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get public case statistics
        /// </summary>
        [HttpGet("publiccases/stats/houston")]
        public async Task<IActionResult> GetPublicCaseStats()
        {
            try
            {
                var totalCases = await _context.Cases.CountAsync(c => c.IsPublic && c.IsApproved);
                var openCases = await _context.Cases.CountAsync(c => c.IsPublic && c.IsApproved && c.Status == "Open");
                var verifiedCases = await _context.Cases.CountAsync(c => c.IsPublic && c.IsApproved && c.IsVerified);

                return Ok(new
                {
                    success = true,
                    stats = new
                    {
                        totalCases = totalCases,
                        openCases = openCases,
                        verifiedCases = verifiedCases,
                        region = "Houston Area"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public case statistics");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new case (authenticated users)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCase([FromBody] CaseCreationDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }

                // Validate the request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify the runner exists and belongs to the user (or user is admin)
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var runner = await _context.Runners.FirstOrDefaultAsync(r => r.Id == request.RunnerId);
                if (runner == null)
                {
                    return BadRequest(new { success = false, message = "Runner not found" });
                }

                if (runner.UserId != userId && userRole != "admin")
                {
                    return Forbid("You can only create cases for your own runners");
                }

                // Create new case
                var caseEntity = new Case
                {
                    RunnerId = request.RunnerId,
                    ReportedByUserId = userId,
                    Title = request.Title,
                    Description = request.Description,
                    LastSeenDate = request.LastSeenDate,
                    LastSeenLocation = request.LastSeenLocation,
                    LastSeenTime = request.LastSeenTime,
                    LastSeenCircumstances = request.LastSeenCircumstances,
                    ClothingDescription = request.ClothingDescription,
                    PhysicalCondition = request.PhysicalCondition,
                    MentalState = request.MentalState,
                    AdditionalInformation = request.AdditionalInformation,
                    Priority = request.Priority,
                    IsPublic = request.IsPublic,
                    ContactPersonName = request.ContactPersonName,
                    ContactPersonPhone = request.ContactPersonPhone,
                    ContactPersonEmail = request.ContactPersonEmail,
                    LastSeenLatitude = request.LastSeenLatitude,
                    LastSeenLongitude = request.LastSeenLongitude,
                    EmergencyContactName = request.EmergencyContactName,
                    EmergencyContactPhone = request.EmergencyContactPhone,
                    EmergencyContactRelationship = request.EmergencyContactRelationship,
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Cases.Add(caseEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Case created with ID {CaseId} by user {UserId}", caseEntity.Id, userId);

                return Ok(new { success = true, message = "Case created successfully", caseId = caseEntity.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating case");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all cases (admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllCases([FromQuery] string? status = null, [FromQuery] string? priority = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var baseQuery = _context.Cases.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    baseQuery = baseQuery.Where(c => c.Status == status);
                }

                if (!string.IsNullOrEmpty(priority))
                {
                    baseQuery = baseQuery.Where(c => c.Priority == priority);
                }

                var query = baseQuery
                    .Include(c => c.Runner)
                    .Include(c => c.ReportedByUser);

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var cases = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CaseResponseDto
                    {
                        Id = c.Id,
                        RunnerId = c.RunnerId,
                        ReportedByUserId = c.ReportedByUserId,
                        Title = c.Title,
                        Description = c.Description,
                        LastSeenDate = c.LastSeenDate,
                        LastSeenLocation = c.LastSeenLocation,
                        LastSeenTime = c.LastSeenTime,
                        LastSeenCircumstances = c.LastSeenCircumstances,
                        ClothingDescription = c.ClothingDescription,
                        PhysicalCondition = c.PhysicalCondition,
                        MentalState = c.MentalState,
                        AdditionalInformation = c.AdditionalInformation,
                        Status = c.Status,
                        Priority = c.Priority,
                        IsPublic = c.IsPublic,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        ResolvedAt = c.ResolvedAt,
                        ResolutionNotes = c.ResolutionNotes,
                        ResolvedBy = c.ResolvedBy,
                        ContactPersonName = c.ContactPersonName,
                        ContactPersonPhone = c.ContactPersonPhone,
                        ContactPersonEmail = c.ContactPersonEmail,
                        LastSeenLatitude = c.LastSeenLatitude,
                        LastSeenLongitude = c.LastSeenLongitude,
                        CaseImageUrls = c.CaseImageUrls,
                        DocumentUrls = c.DocumentUrls,
                        EmergencyContactName = c.EmergencyContactName,
                        EmergencyContactPhone = c.EmergencyContactPhone,
                        EmergencyContactRelationship = c.EmergencyContactRelationship,
                        IsVerified = c.IsVerified,
                        VerifiedAt = c.VerifiedAt,
                        VerifiedBy = c.VerifiedBy,
                        IsApproved = c.IsApproved,
                        ApprovedAt = c.ApprovedAt,
                        ApprovedBy = c.ApprovedBy,
                        ViewCount = c.ViewCount,
                        ShareCount = c.ShareCount,
                        TipCount = c.TipCount,
                        Runner = new RunnerResponseDto
                        {
                            Id = c.Runner.Id,
                            UserId = c.Runner.UserId,
                            Name = c.Runner.Name,
                            DateOfBirth = c.Runner.DateOfBirth,
                            Gender = c.Runner.Gender,
                            PhysicalDescription = c.Runner.PhysicalDescription,
                            MedicalConditions = c.Runner.MedicalConditions,
                            Medications = c.Runner.Medications,
                            Allergies = c.Runner.Allergies,
                            EmergencyInstructions = c.Runner.EmergencyInstructions,
                            PreferredRunningLocations = c.Runner.PreferredRunningLocations,
                            TypicalRunningTimes = c.Runner.TypicalRunningTimes,
                            ExperienceLevel = c.Runner.ExperienceLevel,
                            SpecialNeeds = c.Runner.SpecialNeeds,
                            AdditionalNotes = c.Runner.AdditionalNotes,
                            IsActive = c.Runner.IsActive,
                            CreatedAt = c.Runner.CreatedAt,
                            UpdatedAt = c.Runner.UpdatedAt,
                            LastKnownLocation = c.Runner.LastKnownLocation,
                            LastLocationUpdate = c.Runner.LastLocationUpdate,
                            PreferredContactMethod = c.Runner.PreferredContactMethod,
                            ProfileImageUrl = c.Runner.ProfileImageUrl,
                            IsProfileComplete = c.Runner.IsProfileComplete,
                            IsVerified = c.Runner.IsVerified,
                            VerifiedAt = c.Runner.VerifiedAt,
                            VerifiedBy = c.Runner.VerifiedBy
                        },
                        ReportedByUserName = $"{c.ReportedByUser.FirstName} {c.ReportedByUser.LastName}",
                        ReportedByUserEmail = c.ReportedByUser.Email
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    cases = cases,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all cases");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update case status (admin only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateCaseStatus(int id, [FromBody] UpdateCaseStatusDto dto)
        {
            try
            {
                var caseEntity = await _context.Cases.FindAsync(id);
                if (caseEntity == null)
                {
                    return NotFound(new { success = false, message = "Case not found" });
                }

                caseEntity.Status = dto.Status;
                caseEntity.UpdatedAt = DateTime.UtcNow;

                if (dto.Status == "Found" || dto.Status == "Closed")
                {
                    caseEntity.ResolvedAt = DateTime.UtcNow;
                    caseEntity.ResolvedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Case {CaseId} status updated to {Status}", id, dto.Status);

                return Ok(new { success = true, message = "Case status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating case status for case {CaseId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Approve case for public viewing (admin only)
        /// </summary>
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ApproveCase(int id, [FromBody] ApproveCaseDto dto)
        {
            try
            {
                var caseEntity = await _context.Cases.FindAsync(id);
                if (caseEntity == null)
                {
                    return NotFound(new { success = false, message = "Case not found" });
                }

                caseEntity.IsApproved = dto.IsApproved;
                caseEntity.ApprovedAt = dto.IsApproved ? DateTime.UtcNow : null;
                caseEntity.ApprovedBy = dto.IsApproved ? User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown" : null;
                caseEntity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Case {CaseId} approval status updated to {Approved}", id, dto.IsApproved);

                return Ok(new { success = true, message = $"Case {(dto.IsApproved ? "approved" : "unapproved")} successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating case approval for case {CaseId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    public class UpdateCaseStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class ApproveCaseDto
    {
        public bool IsApproved { get; set; }
    }
}
