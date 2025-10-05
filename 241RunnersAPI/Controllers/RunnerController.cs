using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class RunnerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RunnerController> _logger;

        public RunnerController(ApplicationDbContext context, ILogger<RunnerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all runners (admin only) or user's own runners
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRunners([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }

                var query = _context.Runners.AsQueryable();

                // Apply role-based filtering
                if (userRole != "admin")
                {
                    // Regular users can only see their own runners
                    query = query.Where(r => r.UserId == userId);
                }

                // Apply status filter if provided
                if (!string.IsNullOrEmpty(status) && (status == "Missing" || status == "Found" || status == "Resolved"))
                {
                    query = query.Where(r => r.Status == status);
                }

                // Get total count
                var total = await query.CountAsync();

                // Apply pagination
                var runners = await query
                    .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RunnerResponseDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        Name = r.Name,
                        DateOfBirth = r.DateOfBirth,
                        Age = DateTime.UtcNow.Year - r.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < r.DateOfBirth.DayOfYear ? 1 : 0),
                        Gender = r.Gender,
                        Status = r.Status,
                        PhysicalDescription = r.PhysicalDescription,
                        MedicalConditions = r.MedicalConditions,
                        Medications = r.Medications,
                        Allergies = r.Allergies,
                        EmergencyInstructions = r.EmergencyInstructions,
                        PreferredRunningLocations = r.PreferredRunningLocations,
                        TypicalRunningTimes = r.TypicalRunningTimes,
                        ExperienceLevel = r.ExperienceLevel,
                        SpecialNeeds = r.SpecialNeeds,
                        AdditionalNotes = r.AdditionalNotes,
                        IsActive = r.IsActive,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        LastKnownLocation = r.LastKnownLocation,
                        LastLocationUpdate = r.LastLocationUpdate,
                        PreferredContactMethod = r.PreferredContactMethod,
                        ProfileImageUrl = r.ProfileImageUrl,
                        IsProfileComplete = r.IsProfileComplete,
                        IsVerified = r.IsVerified,
                        VerifiedAt = r.VerifiedAt,
                        VerifiedBy = r.VerifiedBy,
                        UserEmail = null, // Will be populated separately if needed
                        UserPhoneNumber = null, // Will be populated separately if needed
                        UserEmergencyContactName = null, // Will be populated separately if needed
                        UserEmergencyContactPhone = null // Will be populated separately if needed
                    })
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    runners = runners, 
                    count = runners.Count,
                    total = total,
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving runners");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get runner by ID (owner or admin only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetRunner(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }

                var runner = await _context.Runners
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (runner == null)
                {
                    return NotFound(new { success = false, message = "Runner not found" });
                }

                // Check ownership - user can only access their own runners unless they're admin
                if (runner.UserId != userId && userRole != "admin")
                {
                    return Forbid("You can only access your own runner profiles");
                }

                var runnerResponse = new RunnerResponseDto
                {
                    Id = runner.Id,
                    UserId = runner.UserId,
                    Name = runner.Name,
                    DateOfBirth = runner.DateOfBirth,
                    Age = DateTime.UtcNow.Year - runner.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < runner.DateOfBirth.DayOfYear ? 1 : 0),
                    Gender = runner.Gender,
                    Status = runner.Status,
                    PhysicalDescription = runner.PhysicalDescription,
                    MedicalConditions = runner.MedicalConditions,
                    Medications = runner.Medications,
                    Allergies = runner.Allergies,
                    EmergencyInstructions = runner.EmergencyInstructions,
                    PreferredRunningLocations = runner.PreferredRunningLocations,
                    TypicalRunningTimes = runner.TypicalRunningTimes,
                    ExperienceLevel = runner.ExperienceLevel,
                    SpecialNeeds = runner.SpecialNeeds,
                    AdditionalNotes = runner.AdditionalNotes,
                    IsActive = runner.IsActive,
                    CreatedAt = runner.CreatedAt,
                    UpdatedAt = runner.UpdatedAt,
                    LastKnownLocation = runner.LastKnownLocation,
                    LastLocationUpdate = runner.LastLocationUpdate,
                    PreferredContactMethod = runner.PreferredContactMethod,
                    ProfileImageUrl = runner.ProfileImageUrl,
                    IsProfileComplete = runner.IsProfileComplete,
                    IsVerified = runner.IsVerified,
                    VerifiedAt = runner.VerifiedAt,
                    VerifiedBy = runner.VerifiedBy,
                    UserEmail = null, // Will be populated separately if needed
                    UserPhoneNumber = null, // Will be populated separately if needed
                    UserEmergencyContactName = null, // Will be populated separately if needed
                    UserEmergencyContactPhone = null // Will be populated separately if needed
                };

                return Ok(new { success = true, runner = runnerResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving runner {RunnerId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current user's runner profile status (authenticated users) - Simple version without database dependency
        /// </summary>
        [HttpGet("my-profile")]
        [Authorize]
        public IActionResult GetMyRunnerProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }

                // Simple response without database dependency
                if (userRole?.ToLower() == "admin")
                {
                    return Ok(new { 
                        success = true, 
                        hasProfile = false,
                        message = "Admin users typically don't need runner profiles. You have administrative access to manage all runner profiles.",
                        userRole = userRole,
                        userEmail = userEmail,
                        userName = userName,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return Ok(new { 
                        success = true, 
                        hasProfile = false,
                        message = "No runner profile found. Create one using POST /api/runner to register as a runner.",
                        userRole = userRole,
                        userEmail = userEmail,
                        userName = userName,
                        instructions = "To create a runner profile, send a POST request to /api/runner with your runner information.",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving runner profile status for user");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create runner profile (authenticated users)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateRunner([FromBody] RunnerRegistrationDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }

                // Check if user already has a runner profile
                var existingRunner = await _context.Runners.FirstOrDefaultAsync(r => r.UserId == userId);
                if (existingRunner != null)
                {
                    return Conflict(new { success = false, message = "Runner profile already exists for this user" });
                }

                // Validate the request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Create new runner
                var runner = new Runner
                {
                    UserId = userId,
                    Name = request.Name,
                    DateOfBirth = request.DateOfBirth,
                    Status = request.Status,
                    Gender = request.Gender,
                    PhysicalDescription = request.PhysicalDescription,
                    MedicalConditions = request.MedicalConditions,
                    Medications = request.Medications,
                    Allergies = request.Allergies,
                    EmergencyInstructions = request.EmergencyInstructions,
                    PreferredRunningLocations = request.PreferredRunningLocations,
                    TypicalRunningTimes = request.TypicalRunningTimes,
                    ExperienceLevel = request.ExperienceLevel,
                    SpecialNeeds = request.SpecialNeeds,
                    AdditionalNotes = request.AdditionalNotes,
                    PreferredContactMethod = request.PreferredContactMethod,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    IsProfileComplete = !string.IsNullOrEmpty(request.Name) && 
                                      !string.IsNullOrEmpty(request.Gender) && 
                                      request.DateOfBirth != default
                };

                _context.Runners.Add(runner);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Runner profile created for user {UserId} with ID {RunnerId}", userId, runner.Id);

                // Automatically create a case for the new runner
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    var newCase = new Case
                    {
                        RunnerId = runner.Id,
                        ReportedByUserId = userId,
                        Title = $"Runner Profile - {runner.Name}",
                        Description = $"Active runner profile for {runner.Name}. {runner.PhysicalDescription ?? "No additional description available."}",
                        LastSeenDate = DateTime.UtcNow,
                        LastSeenLocation = runner.LastKnownLocation ?? "Location not specified",
                        LastSeenTime = "Profile created",
                        LastSeenCircumstances = "Runner profile created",
                        ClothingDescription = "Not specified",
                        PhysicalCondition = "Good",
                        MentalState = "Stable",
                        AdditionalInformation = runner.MedicalConditions ?? "No medical conditions reported",
                        Status = "Active", // Default to Active status for new runners
                        Priority = "Low",
                        IsPublic = true,
                        IsApproved = true,
                        IsVerified = false,
                        ContactPersonName = user.FirstName + " " + user.LastName,
                        ContactPersonPhone = user.PhoneNumber,
                        ContactPersonEmail = user.Email,
                        EmergencyContactName = user.EmergencyContactName,
                        EmergencyContactPhone = user.EmergencyContactPhone,
                        EmergencyContactRelationship = user.EmergencyContactRelationship,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Cases.Add(newCase);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Case created for runner {RunnerId} with case ID {CaseId}", runner.Id, newCase.Id);
                }

                var runnerResponse = new RunnerResponseDto
                {
                    Id = runner.Id,
                    UserId = runner.UserId,
                    Name = runner.Name,
                    DateOfBirth = runner.DateOfBirth,
                    Age = DateTime.UtcNow.Year - runner.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < runner.DateOfBirth.DayOfYear ? 1 : 0),
                    Gender = runner.Gender,
                    PhysicalDescription = runner.PhysicalDescription,
                    MedicalConditions = runner.MedicalConditions,
                    Medications = runner.Medications,
                    Allergies = runner.Allergies,
                    EmergencyInstructions = runner.EmergencyInstructions,
                    PreferredRunningLocations = runner.PreferredRunningLocations,
                    TypicalRunningTimes = runner.TypicalRunningTimes,
                    ExperienceLevel = runner.ExperienceLevel,
                    SpecialNeeds = runner.SpecialNeeds,
                    AdditionalNotes = runner.AdditionalNotes,
                    IsActive = runner.IsActive,
                    CreatedAt = runner.CreatedAt,
                    PreferredContactMethod = runner.PreferredContactMethod,
                    IsProfileComplete = runner.IsProfileComplete,
                    IsVerified = runner.IsVerified
                };

                return Ok(new { success = true, message = "Runner profile created successfully", runner = runnerResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating runner profile");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update runner profile (authenticated users - own profile only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateRunner(int id, [FromBody] RunnerUpdateDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var runner = await _context.Runners.FirstOrDefaultAsync(r => r.Id == id);
                if (runner == null)
                {
                    return NotFound(new { success = false, message = "Runner not found" });
                }

                // Check if user can update this runner (own profile or admin)
                if (runner.UserId != userId && userRole != "admin")
                {
                    return Forbid("You can only update your own runner profile");
                }

                // Validate the request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Update runner fields
                runner.Name = request.Name;
                runner.DateOfBirth = request.DateOfBirth;
                runner.Status = request.Status;
                runner.Gender = request.Gender;
                runner.PhysicalDescription = request.PhysicalDescription;
                runner.MedicalConditions = request.MedicalConditions;
                runner.Medications = request.Medications;
                runner.Allergies = request.Allergies;
                runner.EmergencyInstructions = request.EmergencyInstructions;
                runner.PreferredRunningLocations = request.PreferredRunningLocations;
                runner.TypicalRunningTimes = request.TypicalRunningTimes;
                runner.ExperienceLevel = request.ExperienceLevel;
                runner.SpecialNeeds = request.SpecialNeeds;
                runner.AdditionalNotes = request.AdditionalNotes;
                runner.PreferredContactMethod = request.PreferredContactMethod;
                runner.UpdatedAt = DateTime.UtcNow;
                runner.IsProfileComplete = !string.IsNullOrEmpty(request.Name) && 
                                          !string.IsNullOrEmpty(request.Gender) && 
                                          request.DateOfBirth != default;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Runner profile updated for runner {RunnerId}", id);

                var runnerResponse = new RunnerResponseDto
                {
                    Id = runner.Id,
                    UserId = runner.UserId,
                    Name = runner.Name,
                    DateOfBirth = runner.DateOfBirth,
                    Age = DateTime.UtcNow.Year - runner.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < runner.DateOfBirth.DayOfYear ? 1 : 0),
                    Gender = runner.Gender,
                    PhysicalDescription = runner.PhysicalDescription,
                    MedicalConditions = runner.MedicalConditions,
                    Medications = runner.Medications,
                    Allergies = runner.Allergies,
                    EmergencyInstructions = runner.EmergencyInstructions,
                    PreferredRunningLocations = runner.PreferredRunningLocations,
                    TypicalRunningTimes = runner.TypicalRunningTimes,
                    ExperienceLevel = runner.ExperienceLevel,
                    SpecialNeeds = runner.SpecialNeeds,
                    AdditionalNotes = runner.AdditionalNotes,
                    IsActive = runner.IsActive,
                    CreatedAt = runner.CreatedAt,
                    UpdatedAt = runner.UpdatedAt,
                    PreferredContactMethod = runner.PreferredContactMethod,
                    IsProfileComplete = runner.IsProfileComplete,
                    IsVerified = runner.IsVerified
                };

                return Ok(new { success = true, message = "Runner profile updated successfully", runner = runnerResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating runner profile {RunnerId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update runner status (admin only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateRunnerStatus(int id, [FromBody] UpdateRunnerStatusDto dto)
        {
            try
            {
                var runner = await _context.Runners.FindAsync(id);
                if (runner == null)
                {
                    return NotFound(new { success = false, message = "Runner not found" });
                }

                runner.IsActive = dto.IsActive;
                runner.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Runner status updated for runner {RunnerId} to {Status}", id, dto.IsActive ? "Active" : "Inactive");

                return Ok(new { success = true, message = "Runner status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating runner status for runner {RunnerId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Verify runner profile (admin only)
        /// </summary>
        [HttpPut("{id}/verify")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> VerifyRunner(int id, [FromBody] VerifyRunnerDto dto)
        {
            try
            {
                var runner = await _context.Runners.FindAsync(id);
                if (runner == null)
                {
                    return NotFound(new { success = false, message = "Runner not found" });
                }

                var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

                runner.IsVerified = dto.IsVerified;
                runner.VerifiedAt = dto.IsVerified ? DateTime.UtcNow : null;
                runner.VerifiedBy = dto.IsVerified ? adminEmail : null;
                runner.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Runner verification updated for runner {RunnerId} to {Status} by {AdminEmail}", 
                    id, dto.IsVerified ? "Verified" : "Unverified", adminEmail);

                return Ok(new { success = true, message = $"Runner {(dto.IsVerified ? "verified" : "unverified")} successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating runner verification for runner {RunnerId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current user's cases (authenticated users)
        /// </summary>
        [HttpGet("my-cases")]
        [Authorize]
        public async Task<IActionResult> GetMyCases()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }

                var runner = await _context.Runners.FirstOrDefaultAsync(r => r.UserId == userId);
                if (runner == null)
                {
                    return Ok(new { success = true, cases = new List<object>(), message = "No runner profile found" });
                }

                var cases = await _context.Cases
                    .Where(c => c.RunnerId == runner.Id)
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
                        TipCount = c.TipCount
                    })
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return Ok(new { success = true, cases = cases, count = cases.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user's cases");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete runner profile (owner or admin only - soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRunner(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }

                var runner = await _context.Runners.FindAsync(id);
                if (runner == null)
                {
                    return NotFound(new { success = false, message = "Runner not found" });
                }

                // Check ownership - user can only delete their own runners unless they're admin
                if (runner.UserId != userId && userRole != "admin")
                {
                    return Forbid("You can only delete your own runner profiles");
                }

                // Soft delete - mark as inactive instead of removing
                runner.IsActive = false;
                runner.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Runner profile soft deleted for runner {RunnerId} by user {UserId}", id, userId);

                return Ok(new { success = true, message = "Runner profile deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting runner profile {RunnerId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    public class UpdateRunnerStatusDto
    {
        public bool IsActive { get; set; }
    }

    public class VerifyRunnerDto
    {
        public bool IsVerified { get; set; }
    }
}
