using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using _241RunnersAwareness.BackendAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CaseController : ControllerBase
    {
        private readonly RunnersDbContext _context;

        public CaseController(RunnersDbContext context)
        {
            _context = context;
        }

        // GET: api/cases/mine
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<CaseDto>>> GetMyCases()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var cases = await _context.Cases
                    .Where(c => c.OwnerUserId == userId)
                    .Include(c => c.Individual)
                    .Include(c => c.Updates.OrderByDescending(u => u.CreatedAt).Take(1))
                    .OrderByDescending(c => c.LastUpdatedAt)
                    .Select(c => new CaseDto
                    {
                        Id = c.Id,
                        CaseNumber = c.CaseNumber,
                        PublicSlug = c.PublicSlug,
                        Title = c.Title,
                        Description = c.Description,
                        Status = c.Status,
                        Priority = c.Priority,
                        RiskLevel = c.RiskLevel,
                        LastSeenLocation = c.LastSeenLocation,
                        LastSeenDate = c.LastSeenDate,
                        IsPublic = c.IsPublic,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        LastUpdatedAt = c.LastUpdatedAt,
                        IndividualName = c.Individual != null ? c.Individual.FullName : null,
                        UpdateCount = c.Updates.Count,
                        LatestUpdate = c.Updates.FirstOrDefault() != null ? new CaseUpdateDto
                        {
                            Id = c.Updates.First().Id,
                            Title = c.Updates.First().Title,
                            UpdateType = c.Updates.First().UpdateType,
                            CreatedAt = c.Updates.First().CreatedAt
                        } : null
                    })
                    .ToListAsync();

                return Ok(cases);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving cases", error = ex.Message });
            }
        }

        // POST: api/cases
        [HttpPost]
        public async Task<ActionResult<CaseDto>> CreateCase(CreateCaseRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Validate required fields
                if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Description))
                {
                    return BadRequest(new { message = "Title and description are required" });
                }

                if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
                {
                    return BadRequest(new { message = "First name and last name are required" });
                }

                // Create the Individual first
                var individual = new Individual
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    MiddleName = request.MiddleName,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    LastKnownAddress = request.LastKnownAddress,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    ZipCode = request.ZipCode,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Height = request.Height,
                    Weight = request.Weight,
                    HairColor = request.HairColor,
                    EyeColor = request.EyeColor,
                    DistinguishingFeatures = request.DistinguishingFeatures,
                    PrimaryDisability = request.PrimaryDisability,
                    DisabilityDescription = request.DisabilityDescription,
                    CommunicationMethod = request.CommunicationMethod,
                    CommunicationNeeds = request.CommunicationNeeds,
                    IsNonVerbal = request.IsNonVerbal,
                    UsesAACDevice = request.UsesAACDevice,
                    AACDeviceType = request.AACDeviceType,
                    MobilityStatus = request.MobilityStatus,
                    UsesWheelchair = request.UsesWheelchair,
                    UsesMobilityDevice = request.UsesMobilityDevice,
                    MobilityDeviceType = request.MobilityDeviceType,
                    HasVisualImpairment = request.HasVisualImpairment,
                    HasHearingImpairment = request.HasHearingImpairment,
                    HasSensoryProcessingDisorder = request.HasSensoryProcessingDisorder,
                    SensoryTriggers = request.SensoryTriggers,
                    SensoryComforts = request.SensoryComforts,
                    BehavioralTriggers = request.BehavioralTriggers,
                    CalmingTechniques = request.CalmingTechniques,
                    MayWanderOrElope = request.MayWanderOrElope,
                    IsAttractedToWater = request.IsAttractedToWater,
                    IsAttractedToRoads = request.IsAttractedToRoads,
                    IsAttractedToBrightLights = request.IsAttractedToBrightLights,
                    WanderingPatterns = request.WanderingPatterns,
                    PreferredLocations = request.PreferredLocations,
                    MedicalConditions = request.MedicalConditions,
                    Medications = request.Medications,
                    Allergies = request.Allergies,
                    RequiresMedication = request.RequiresMedication,
                    MedicationSchedule = request.MedicationSchedule,
                    HasSeizureDisorder = request.HasSeizureDisorder,
                    SeizureTriggers = request.SeizureTriggers,
                    HasDiabetes = request.HasDiabetes,
                    HasAsthma = request.HasAsthma,
                    HasHeartCondition = request.HasHeartCondition,
                    EmergencyResponseInstructions = request.EmergencyResponseInstructions,
                    PreferredEmergencyContact = request.PreferredEmergencyContact,
                    ShouldCall911 = request.ShouldCall911,
                    SpecialInstructionsForFirstResponders = request.SpecialInstructionsForFirstResponders,
                    EnableRealTimeAlerts = request.EnableRealTimeAlerts,
                    EnableSMSAlerts = request.EnableSMSAlerts,
                    EnableEmailAlerts = request.EnableEmailAlerts,
                    EnablePushNotifications = request.EnablePushNotifications,
                    AlertRadius = request.AlertRadius,
                    AlertRadiusMiles = request.AlertRadiusMiles,
                    HasGPSDevice = request.HasGPSDevice,
                    GPSDeviceType = request.GPSDeviceType,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                };

                _context.Individuals.Add(individual);
                await _context.SaveChangesAsync();

                // Generate unique case number and slug
                var caseNumber = await GenerateUniqueCaseNumber();
                var publicSlug = await GenerateUniquePublicSlug(request.Title);

                var newCase = new Case
                {
                    CaseNumber = caseNumber,
                    PublicSlug = publicSlug,
                    IndividualId = individual.Id,
                    OwnerUserId = userId.Value,
                    Title = request.Title,
                    Description = request.Description,
                    Status = request.Status ?? "Active",
                    Priority = request.Priority ?? "Medium",
                    RiskLevel = request.RiskLevel ?? "Medium",
                    LastSeenLocation = request.LastSeenLocation,
                    LastSeenDate = request.LastSeenDate,
                    LawEnforcementCaseNumber = request.LawEnforcementCaseNumber,
                    InvestigatingAgency = request.InvestigatingAgency,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    IsPublic = request.IsPublic ?? false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserEmail(),
                    UpdatedBy = GetCurrentUserEmail()
                };

                _context.Cases.Add(newCase);
                await _context.SaveChangesAsync();

                // Return the created case
                var caseDto = new CaseDto
                {
                    Id = newCase.Id,
                    CaseNumber = newCase.CaseNumber,
                    PublicSlug = newCase.PublicSlug,
                    Title = newCase.Title,
                    Description = newCase.Description,
                    Status = newCase.Status,
                    Priority = newCase.Priority,
                    RiskLevel = newCase.RiskLevel,
                    LastSeenLocation = newCase.LastSeenLocation,
                    LastSeenDate = newCase.LastSeenDate,
                    IsPublic = newCase.IsPublic,
                    CreatedAt = newCase.CreatedAt,
                    UpdatedAt = newCase.UpdatedAt,
                    LastUpdatedAt = newCase.LastUpdatedAt,
                    IndividualName = individual.FullName,
                    UpdateCount = 0
                };

                return CreatedAtAction(nameof(GetCase), new { id = newCase.Id }, caseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the case", error = ex.Message });
            }
        }

        // GET: api/cases/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CaseDetailDto>> GetCase(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var caseItem = await _context.Cases
                    .Include(c => c.Individual)
                    .Include(c => c.OwnerUser)
                    .Include(c => c.Updates.OrderByDescending(u => u.CreatedAt))
                    .ThenInclude(u => u.CreatedByUser)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (caseItem == null)
                {
                    return NotFound(new { message = "Case not found" });
                }

                // Check if user has access to this case
                if (caseItem.OwnerUserId != userId && !IsAdmin())
                {
                    return Forbid();
                }

                var caseDetailDto = new CaseDetailDto
                {
                    Id = caseItem.Id,
                    CaseNumber = caseItem.CaseNumber,
                    PublicSlug = caseItem.PublicSlug,
                    Title = caseItem.Title,
                    Description = caseItem.Description,
                    Status = caseItem.Status,
                    Priority = caseItem.Priority,
                    RiskLevel = caseItem.RiskLevel,
                    LastSeenLocation = caseItem.LastSeenLocation,
                    LastSeenDate = caseItem.LastSeenDate,
                    LawEnforcementCaseNumber = caseItem.LawEnforcementCaseNumber,
                    InvestigatingAgency = caseItem.InvestigatingAgency,
                    Latitude = caseItem.Latitude,
                    Longitude = caseItem.Longitude,
                    IsPublic = caseItem.IsPublic,
                    CreatedAt = caseItem.CreatedAt,
                    UpdatedAt = caseItem.UpdatedAt,
                    LastUpdatedAt = caseItem.LastUpdatedAt,
                    Owner = caseItem.OwnerUser != null ? new UserDto
                    {
                        UserId = caseItem.OwnerUser.UserId,
                        FullName = caseItem.OwnerUser.FullName,
                        Email = caseItem.OwnerUser.Email
                    } : null,
                    Individual = caseItem.Individual != null ? new CaseIndividualDto
                    {
                        IndividualId = caseItem.Individual.IndividualId,
                        FullName = caseItem.Individual.FullName,
                        DateOfBirth = caseItem.Individual.DateOfBirth,
                        Gender = caseItem.Individual.Gender
                    } : null,
                    Updates = caseItem.Updates.Select(u => new CaseUpdateDto
                    {
                        Id = u.Id,
                        Title = u.Title,
                        Content = u.Content,
                        UpdateType = u.UpdateType,
                        IsPublic = u.IsPublic,
                        IsUrgent = u.IsUrgent,
                        Location = u.Location,
                        UpdateDate = u.UpdateDate ?? DateTime.UtcNow,
                        CreatedAt = u.CreatedAt,
                        CreatedBy = u.CreatedByUser != null ? new UserDto
                        {
                            UserId = u.CreatedByUser.UserId,
                            FullName = u.CreatedByUser.FullName,
                            Email = u.CreatedByUser.Email
                        } : null
                    }).ToList()
                };

                return Ok(caseDetailDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the case", error = ex.Message });
            }
        }

        // PATCH: api/cases/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<CaseDto>> UpdateCase(int id, UpdateCaseRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var caseItem = await _context.Cases.FindAsync(id);
                if (caseItem == null)
                {
                    return NotFound(new { message = "Case not found" });
                }

                // Check if user has permission to update this case
                if (caseItem.OwnerUserId != userId && !IsAdmin())
                {
                    return Forbid();
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.Title))
                    caseItem.Title = request.Title;
                
                if (!string.IsNullOrEmpty(request.Description))
                    caseItem.Description = request.Description;
                
                if (!string.IsNullOrEmpty(request.Status))
                    caseItem.Status = request.Status;
                
                if (!string.IsNullOrEmpty(request.Priority))
                    caseItem.Priority = request.Priority;
                
                if (!string.IsNullOrEmpty(request.RiskLevel))
                    caseItem.RiskLevel = request.RiskLevel;
                
                if (!string.IsNullOrEmpty(request.LastSeenLocation))
                    caseItem.LastSeenLocation = request.LastSeenLocation;
                
                if (request.LastSeenDate.HasValue)
                    caseItem.LastSeenDate = request.LastSeenDate;
                
                if (!string.IsNullOrEmpty(request.LawEnforcementCaseNumber))
                    caseItem.LawEnforcementCaseNumber = request.LawEnforcementCaseNumber;
                
                if (!string.IsNullOrEmpty(request.InvestigatingAgency))
                    caseItem.InvestigatingAgency = request.InvestigatingAgency;
                
                if (request.Latitude.HasValue)
                    caseItem.Latitude = request.Latitude;
                
                if (request.Longitude.HasValue)
                    caseItem.Longitude = request.Longitude;
                
                if (request.IsPublic.HasValue)
                    caseItem.IsPublic = request.IsPublic.Value;

                caseItem.UpdatedAt = DateTime.UtcNow;
                caseItem.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Return updated case
                var caseDto = new CaseDto
                {
                    Id = caseItem.Id,
                    CaseNumber = caseItem.CaseNumber,
                    PublicSlug = caseItem.PublicSlug,
                    Title = caseItem.Title,
                    Description = caseItem.Description,
                    Status = caseItem.Status,
                    Priority = caseItem.Priority,
                    RiskLevel = caseItem.RiskLevel,
                    LastSeenLocation = caseItem.LastSeenLocation,
                    LastSeenDate = caseItem.LastSeenDate,
                    IsPublic = caseItem.IsPublic,
                    CreatedAt = caseItem.CreatedAt,
                    UpdatedAt = caseItem.UpdatedAt,
                    LastUpdatedAt = caseItem.LastUpdatedAt
                };

                return Ok(caseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the case", error = ex.Message });
            }
        }

        // GET: api/cases/public/{slug}
        [HttpGet("public/{slug}")]
        [AllowAnonymous]
        public async Task<ActionResult<PublicCaseDto>> GetPublicCase(string slug)
        {
            try
            {
                var caseItem = await _context.Cases
                    .Include(c => c.Individual)
                    .Include(c => c.Updates.Where(u => u.IsPublic).OrderByDescending(u => u.CreatedAt))
                    .FirstOrDefaultAsync(c => c.PublicSlug == slug && c.IsPublic);

                if (caseItem == null)
                {
                    return NotFound(new { message = "Public case not found" });
                }

                var publicCaseDto = new PublicCaseDto
                {
                    Id = caseItem.Id,
                    CaseNumber = caseItem.CaseNumber,
                    Title = caseItem.Title,
                    Description = caseItem.Description,
                    Status = caseItem.Status,
                    RiskLevel = caseItem.RiskLevel,
                    LastSeenLocation = caseItem.LastSeenLocation,
                    LastSeenDate = caseItem.LastSeenDate,
                    InvestigatingAgency = caseItem.InvestigatingAgency,
                    CreatedAt = caseItem.CreatedAt,
                    LastUpdatedAt = caseItem.LastUpdatedAt,
                    Individual = caseItem.Individual != null ? new CaseIndividualDto
                    {
                        IndividualId = caseItem.Individual.IndividualId,
                        FullName = caseItem.Individual.FullName,
                        DateOfBirth = caseItem.Individual.DateOfBirth,
                        Gender = caseItem.Individual.Gender
                    } : null,
                    PublicUpdates = caseItem.Updates.Select(u => new PublicCaseUpdateDto
                    {
                        Id = u.Id,
                        Title = u.Title,
                        Content = u.Content,
                        UpdateType = u.UpdateType,
                        IsUrgent = u.IsUrgent,
                        Location = u.Location,
                        UpdateDate = u.UpdateDate ?? DateTime.UtcNow,
                        CreatedAt = u.CreatedAt
                    }).ToList()
                };

                return Ok(publicCaseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the public case", error = ex.Message });
            }
        }

        // GET: api/cases/{id}/updates
        [HttpGet("{id}/updates")]
        public async Task<ActionResult<IEnumerable<CaseUpdateDto>>> GetCaseUpdates(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var caseItem = await _context.Cases.FindAsync(id);
                if (caseItem == null)
                {
                    return NotFound(new { message = "Case not found" });
                }

                // Check if user has access to this case
                if (caseItem.OwnerUserId != userId && !IsAdmin())
                {
                    return Forbid();
                }

                var updates = await _context.CaseUpdates
                    .Where(u => u.CaseId == id)
                    .Include(u => u.CreatedByUser)
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(u => new CaseUpdateDto
                    {
                        Id = u.Id,
                        Title = u.Title,
                        Content = u.Content,
                        UpdateType = u.UpdateType,
                        IsPublic = u.IsPublic,
                        IsUrgent = u.IsUrgent,
                        Location = u.Location,
                        UpdateDate = u.UpdateDate ?? DateTime.UtcNow,
                        CreatedAt = u.CreatedAt,
                        CreatedBy = u.CreatedByUser != null ? new UserDto
                        {
                            UserId = u.CreatedByUser.UserId,
                            FullName = u.CreatedByUser.FullName,
                            Email = u.CreatedByUser.Email
                        } : null
                    })
                    .ToListAsync();

                return Ok(updates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving case updates", error = ex.Message });
            }
        }

        // POST: api/cases/{id}/updates
        [HttpPost("{id}/updates")]
        public async Task<ActionResult<CaseUpdateDto>> AddCaseUpdate(int id, CreateCaseUpdateRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var caseItem = await _context.Cases.FindAsync(id);
                if (caseItem == null)
                {
                    return NotFound(new { message = "Case not found" });
                }

                // Check if user has access to this case
                if (caseItem.OwnerUserId != userId && !IsAdmin())
                {
                    return Forbid();
                }

                // Validate required fields
                if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest(new { message = "Title and content are required" });
                }

                var update = new CaseUpdate
                {
                    CaseId = id,
                    CreatedByUserId = userId.Value,
                    Title = request.Title,
                    Content = request.Content,
                    UpdateType = request.UpdateType ?? "General",
                    IsPublic = request.IsPublic ?? false,
                    IsUrgent = request.IsUrgent ?? false,
                    Location = request.Location,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    UpdateDate = request.UpdateDate ?? DateTime.UtcNow,
                    RequiresNotification = request.RequiresNotification ?? false,
                    NotificationsSent = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.CaseUpdates.Add(update);

                // Update the case's LastUpdatedAt
                caseItem.LastUpdatedAt = DateTime.UtcNow;
                caseItem.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Return the created update
                var updateDto = new CaseUpdateDto
                {
                    Id = update.Id,
                    Title = update.Title,
                    Content = update.Content,
                    UpdateType = update.UpdateType,
                    IsPublic = update.IsPublic,
                    IsUrgent = update.IsUrgent,
                    Location = update.Location,
                    UpdateDate = update.UpdateDate ?? DateTime.UtcNow,
                    CreatedAt = update.CreatedAt
                };

                return CreatedAtAction(nameof(GetCaseUpdates), new { id = id }, updateDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the case update", error = ex.Message });
            }
        }

        // Helper methods
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
        }

        private bool IsAdmin()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return roleClaim == "admin";
        }

        private string GetCurrentUserEmail()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            return emailClaim ?? "unknown@example.com";
        }

        private async Task<string> GenerateUniqueCaseNumber()
        {
            var prefix = "CASE";
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random();
            var suffix = random.Next(1000, 9999).ToString();
            
            var caseNumber = $"{prefix}-{timestamp}-{suffix}";
            
            // Ensure uniqueness
            while (await _context.Cases.AnyAsync(c => c.CaseNumber == caseNumber))
            {
                suffix = random.Next(1000, 9999).ToString();
                caseNumber = $"{prefix}-{timestamp}-{suffix}";
            }
            
            return caseNumber;
        }

        private async Task<string> GenerateUniquePublicSlug(string title)
        {
            var baseSlug = title.ToLower()
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace("&", "and");
            
            // Remove any non-alphanumeric characters except hyphens
            baseSlug = System.Text.RegularExpressions.Regex.Replace(baseSlug, @"[^a-z0-9\-]", "");
            
            // Remove multiple consecutive hyphens
            baseSlug = System.Text.RegularExpressions.Regex.Replace(baseSlug, @"-+", "-");
            
            // Remove leading/trailing hyphens
            baseSlug = baseSlug.Trim('-');
            
            var slug = baseSlug;
            var counter = 1;
            
            // Ensure uniqueness
            while (await _context.Cases.AnyAsync(c => c.PublicSlug == slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }
            
            return slug;
        }
    }
}
