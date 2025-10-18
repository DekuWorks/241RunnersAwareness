using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using _241RunnersAPI.Services;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/enhanced-runner")]
    [ApiVersion("1.0")]
    public class EnhancedRunnerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnhancedRunnerController> _logger;
        private readonly INotificationService _notificationService;

        public EnhancedRunnerController(
            ApplicationDbContext context, 
            ILogger<EnhancedRunnerController> logger,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Create enhanced runner profile with comprehensive fields
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEnhancedRunner([FromBody] EnhancedRunnerRegistrationDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }

                // Allow multiple runners per user - remove the existing runner check
                // Users can now create multiple runner profiles for family members, etc.

                // Validate the request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Create new enhanced runner
                var runner = new Runner
                {
                    UserId = userId,
                    Name = $"{request.FirstName} {request.LastName}",
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    Height = request.Height,
                    Weight = request.Weight,
                    EyeColor = request.EyeColor,
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
                    Status = "Active",
                    IsProfileComplete = !string.IsNullOrEmpty(request.FirstName) && 
                                      !string.IsNullOrEmpty(request.LastName) && 
                                      !string.IsNullOrEmpty(request.Gender) && 
                                      request.DateOfBirth != default,
                    // Set initial photo reminder for 6 months from now
                    NextPhotoReminder = DateTime.UtcNow.AddMonths(6),
                    PhotoUpdateReminderSent = false,
                    PhotoUpdateReminderCount = 0
                };

                _context.Runners.Add(runner);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Enhanced runner profile created for user {UserId} with ID {RunnerId}", userId, runner.Id);

                // Automatically create a case for the new runner
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    var newCase = new Case
                    {
                        RunnerId = runner.Id,
                        ReportedByUserId = userId,
                        Title = $"Runner Profile - {runner.Name}",
                        Description = $"Active runner profile for {runner.Name}. Height: {runner.Height ?? "Not specified"}, Weight: {runner.Weight ?? "Not specified"}, Eye Color: {runner.EyeColor ?? "Not specified"}. {runner.PhysicalDescription ?? "No additional description available."}",
                        LastSeenDate = DateTime.UtcNow,
                        LastSeenLocation = runner.LastKnownLocation ?? "Location not specified",
                        LastSeenTime = "Profile created",
                        LastSeenCircumstances = "Runner profile created",
                        ClothingDescription = "Not specified",
                        PhysicalCondition = "Good",
                        MentalState = "Stable",
                        AdditionalInformation = runner.MedicalConditions ?? "No medical conditions reported",
                        Status = "Active",
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

                    _logger.LogInformation("Case created for enhanced runner {RunnerId} with case ID {CaseId}", runner.Id, newCase.Id);
                }

                var runnerResponse = new EnhancedRunnerResponseDto
                {
                    Id = runner.Id,
                    UserId = runner.UserId,
                    Name = runner.Name,
                    FirstName = runner.FirstName ?? "",
                    LastName = runner.LastName ?? "",
                    DateOfBirth = runner.DateOfBirth,
                    Age = DateTime.UtcNow.Year - runner.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < runner.DateOfBirth.DayOfYear ? 1 : 0),
                    Gender = runner.Gender,
                    Height = runner.Height,
                    Weight = runner.Weight,
                    EyeColor = runner.EyeColor,
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
                    IsVerified = runner.IsVerified,
                    LastPhotoUpdate = runner.LastPhotoUpdate,
                    NextPhotoReminder = runner.NextPhotoReminder,
                    PhotoUpdateReminderSent = runner.PhotoUpdateReminderSent,
                    PhotoUpdateReminderCount = runner.PhotoUpdateReminderCount,
                    IsPhotoUpdateRequired = runner.NextPhotoReminder.HasValue && DateTime.UtcNow >= runner.NextPhotoReminder.Value
                };

                return Ok(new { success = true, message = "Enhanced runner profile created successfully", runner = runnerResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating enhanced runner profile");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update enhanced runner profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEnhancedRunner(int id, [FromBody] EnhancedRunnerUpdateDto request)
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
                runner.Name = $"{request.FirstName} {request.LastName}";
                runner.FirstName = request.FirstName;
                runner.LastName = request.LastName;
                runner.DateOfBirth = request.DateOfBirth;
                runner.Gender = request.Gender;
                runner.Height = request.Height;
                runner.Weight = request.Weight;
                runner.EyeColor = request.EyeColor;
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
                runner.IsProfileComplete = !string.IsNullOrEmpty(request.FirstName) && 
                                          !string.IsNullOrEmpty(request.LastName) && 
                                          !string.IsNullOrEmpty(request.Gender) && 
                                          request.DateOfBirth != default;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Enhanced runner profile updated for runner {RunnerId}", id);

                var runnerResponse = new EnhancedRunnerResponseDto
                {
                    Id = runner.Id,
                    UserId = runner.UserId,
                    Name = runner.Name,
                    FirstName = runner.FirstName ?? "",
                    LastName = runner.LastName ?? "",
                    DateOfBirth = runner.DateOfBirth,
                    Age = DateTime.UtcNow.Year - runner.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < runner.DateOfBirth.DayOfYear ? 1 : 0),
                    Gender = runner.Gender,
                    Height = runner.Height,
                    Weight = runner.Weight,
                    EyeColor = runner.EyeColor,
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
                    IsVerified = runner.IsVerified,
                    LastPhotoUpdate = runner.LastPhotoUpdate,
                    NextPhotoReminder = runner.NextPhotoReminder,
                    PhotoUpdateReminderSent = runner.PhotoUpdateReminderSent,
                    PhotoUpdateReminderCount = runner.PhotoUpdateReminderCount,
                    IsPhotoUpdateRequired = runner.NextPhotoReminder.HasValue && DateTime.UtcNow >= runner.NextPhotoReminder.Value
                };

                return Ok(new { success = true, message = "Enhanced runner profile updated successfully", runner = runnerResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating enhanced runner profile {RunnerId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Upload runner photo(s) with automatic 6-month reminder setup
        /// </summary>
        [HttpPost("{id}/photos")]
        [Authorize]
        public async Task<IActionResult> UploadRunnerPhotos(int id, [FromForm] List<IFormFile> photos, [FromForm] string photoType = "Additional")
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user token" });
                }

                var runner = await _context.Runners.FirstOrDefaultAsync(r => r.Id == id);
                if (runner == null)
                {
                    return NotFound(new { success = false, message = "Runner not found" });
                }

                // Check if user can upload photos for this runner (own profile or admin)
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (runner.UserId != userId && userRole != "admin")
                {
                    return Forbid("You can only upload photos for your own runner profile");
                }

                if (photos == null || photos.Count == 0)
                {
                    return BadRequest(new { success = false, message = "No photos provided" });
                }

                var uploadedUrls = new List<string>();

                foreach (var photo in photos)
                {
                    // Validate file
                    if (photo.Length == 0)
                        continue;

                    if (photo.Length > 10 * 1024 * 1024) // 10MB limit
                    {
                        return BadRequest(new { success = false, message = $"Photo {photo.FileName} is too large. Maximum size is 10MB." });
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new { success = false, message = $"Photo {photo.FileName} has an unsupported format. Allowed formats: JPG, JPEG, PNG, GIF, WEBP." });
                    }

                    // Generate unique filename
                    var fileName = $"runner_{id}_{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine("wwwroot", "uploads", "runners", fileName);

                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory!);
                    }

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }

                    var photoUrl = $"/uploads/runners/{fileName}";
                    uploadedUrls.Add(photoUrl);

                    _logger.LogInformation("Photo uploaded for runner {RunnerId}: {PhotoUrl}", id, photoUrl);
                }

                // Update runner with new photo URLs
                if (photoType == "Profile" && uploadedUrls.Count > 0)
                {
                    runner.ProfileImageUrl = uploadedUrls[0];
                }

                // Add to additional images
                var existingImages = new List<string>();
                if (!string.IsNullOrEmpty(runner.AdditionalImageUrls))
                {
                    try
                    {
                        existingImages = System.Text.Json.JsonSerializer.Deserialize<List<string>>(runner.AdditionalImageUrls) ?? new List<string>();
                    }
                    catch
                    {
                        existingImages = new List<string>();
                    }
                }

                existingImages.AddRange(uploadedUrls);
                runner.AdditionalImageUrls = System.Text.Json.JsonSerializer.Serialize(existingImages);

                // Update photo management fields
                runner.LastPhotoUpdate = DateTime.UtcNow;
                runner.NextPhotoReminder = DateTime.UtcNow.AddMonths(6); // 6 months from now
                runner.PhotoUpdateReminderSent = false;
                runner.PhotoUpdateReminderCount = 0;
                runner.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send notification about photo update
                await _notificationService.SendPhotoUpdateNotificationAsync(userId, runner.Id, uploadedUrls.Count);

                return Ok(new { 
                    success = true, 
                    message = $"{uploadedUrls.Count} photo(s) uploaded successfully",
                    uploadedUrls = uploadedUrls,
                    nextPhotoReminder = runner.NextPhotoReminder
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photos for runner {RunnerId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get runners that need photo updates (for notification system)
        /// </summary>
        [HttpGet("photo-reminders")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetPhotoUpdateReminders()
        {
            try
            {
                var runnersNeedingPhotoUpdate = await _context.Runners
                    .Where(r => r.NextPhotoReminder.HasValue && 
                               DateTime.UtcNow >= r.NextPhotoReminder.Value && 
                               !r.PhotoUpdateReminderSent)
                    .Select(r => new
                    {
                        r.Id,
                        r.UserId,
                        r.Name,
                        r.LastPhotoUpdate,
                        r.NextPhotoReminder,
                        r.PhotoUpdateReminderCount,
                        DaysOverdue = (DateTime.UtcNow - r.NextPhotoReminder.Value).Days
                    })
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    runners = runnersNeedingPhotoUpdate,
                    count = runnersNeedingPhotoUpdate.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving photo update reminders");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Mark photo update reminder as sent
        /// </summary>
        [HttpPut("{id}/photo-reminder-sent")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> MarkPhotoReminderSent(int id)
        {
            try
            {
                var runner = await _context.Runners.FindAsync(id);
                if (runner == null)
                {
                    return NotFound(new { success = false, message = "Runner not found" });
                }

                runner.PhotoUpdateReminderSent = true;
                runner.PhotoUpdateReminderCount++;
                runner.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Photo update reminder marked as sent for runner {RunnerId}", id);

                return Ok(new { success = true, message = "Photo update reminder marked as sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking photo reminder as sent for runner {RunnerId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get enhanced runner profile with photo management info
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEnhancedRunner(int id)
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

                var runnerResponse = new EnhancedRunnerResponseDto
                {
                    Id = runner.Id,
                    UserId = runner.UserId,
                    Name = runner.Name,
                    FirstName = runner.FirstName ?? "",
                    LastName = runner.LastName ?? "",
                    DateOfBirth = runner.DateOfBirth,
                    Age = DateTime.UtcNow.Year - runner.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < runner.DateOfBirth.DayOfYear ? 1 : 0),
                    Gender = runner.Gender,
                    Height = runner.Height,
                    Weight = runner.Weight,
                    EyeColor = runner.EyeColor,
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
                    AdditionalImageUrls = runner.AdditionalImageUrls,
                    LastPhotoUpdate = runner.LastPhotoUpdate,
                    NextPhotoReminder = runner.NextPhotoReminder,
                    PhotoUpdateReminderSent = runner.PhotoUpdateReminderSent,
                    PhotoUpdateReminderCount = runner.PhotoUpdateReminderCount,
                    IsPhotoUpdateRequired = runner.NextPhotoReminder.HasValue && DateTime.UtcNow >= runner.NextPhotoReminder.Value,
                    IsProfileComplete = runner.IsProfileComplete,
                    IsVerified = runner.IsVerified,
                    VerifiedAt = runner.VerifiedAt,
                    VerifiedBy = runner.VerifiedBy
                };

                return Ok(new { success = true, runner = runnerResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enhanced runner {RunnerId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Interface for notification service
    /// </summary>
    public interface INotificationService
    {
        Task SendPhotoUpdateNotificationAsync(int userId, int runnerId, int photoCount);
    }

    /// <summary>
    /// Implementation of notification service for photo updates
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly ApplicationDbContext _context;

        public NotificationService(ILogger<NotificationService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task SendPhotoUpdateNotificationAsync(int userId, int runnerId, int photoCount)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for photo update notification", userId);
                    return;
                }

                // Log the notification (in a real implementation, this would send email/push notification)
                _logger.LogInformation("Photo update notification sent to user {UserId} for runner {RunnerId}. {PhotoCount} photos uploaded.", 
                    userId, runnerId, photoCount);

                // TODO: Implement actual notification sending (email, push notification, etc.)
                // This could integrate with:
                // - Email service (SendGrid, SMTP)
                // - Push notification service (Firebase, Azure Notification Hub)
                // - SMS service (Twilio, Azure Communication Services)
                // - In-app notification system

                _logger.LogInformation("Photo update notification processed for user {UserEmail}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending photo update notification to user {UserId}", userId);
            }
        }
    }
}
