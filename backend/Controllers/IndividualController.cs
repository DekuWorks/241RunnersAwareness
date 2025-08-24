using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using _241RunnersAwareness.BackendAPI.Services;
using _241RunnersAwareness.BackendAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IndividualController : ControllerBase
    {
        private readonly RunnersDbContext _context;
        private readonly ICsvExportService _csvExportService;

        public IndividualController(RunnersDbContext context, ICsvExportService csvExportService)
        {
            _context = context;
            _csvExportService = csvExportService;
        }

        // GET: api/individuals - Search and filter individuals
        [HttpGet]
        public async Task<ActionResult<IndividualSearchResponse>> GetIndividuals(
            [FromQuery] string? q = null,
            [FromQuery] string? runnerId = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "createdAt",
            [FromQuery] string? sortOrder = "desc")
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsAdmin();

                // Start with base query
                var query = _context.Individuals
                    .Include(i => i.Photos.Where(p => p.IsPrimary))
                    .Include(i => i.Cases)
                    .Include(i => i.OwnerUser)
                    .AsQueryable();

                // Apply ownership filter - non-admins can only see their own individuals
                if (!isAdmin)
                {
                    query = query.Where(i => i.OwnerUserId == userId);
                }

                // Apply search filters
                if (!string.IsNullOrEmpty(q))
                {
                    var searchTerm = q.ToLower();
                    query = query.Where(i => 
                        i.FirstName.ToLower().Contains(searchTerm) ||
                        i.LastName.ToLower().Contains(searchTerm) ||
                        i.FullName.ToLower().Contains(searchTerm) ||
                        (i.RunnerId != null && i.RunnerId.ToLower().Contains(searchTerm))
                    );
                }

                if (!string.IsNullOrEmpty(runnerId))
                {
                    query = query.Where(i => i.RunnerId == runnerId);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(i => i.Status == status);
                }

                // Apply sorting
                query = sortBy?.ToLower() switch
                {
                    "name" => sortOrder?.ToLower() == "asc" 
                        ? query.OrderBy(i => i.FirstName).ThenBy(i => i.LastName)
                        : query.OrderByDescending(i => i.FirstName).ThenByDescending(i => i.LastName),
                    "lastseendate" => sortOrder?.ToLower() == "asc"
                        ? query.OrderBy(i => i.LastSeenDate)
                        : query.OrderByDescending(i => i.LastSeenDate),
                    _ => sortOrder?.ToLower() == "asc"
                        ? query.OrderBy(i => i.CreatedAt)
                        : query.OrderByDescending(i => i.CreatedAt)
                };

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var individuals = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(i => new RunnerProfileDto
                    {
                        Id = i.Id,
                        RunnerId = i.RunnerId,
                        FullName = i.FullName,
                        FirstName = i.FirstName,
                        LastName = i.LastName,
                        MiddleName = i.MiddleName,
                        DateOfBirth = i.DateOfBirth,
                        Age = i.Age,
                        Gender = i.Gender,
                        Status = i.Status,
                        City = i.City,
                        State = i.State,
                        LastSeenDate = i.LastSeenDate,
                        LastSeenLocation = i.LastSeenLocation,
                        CreatedAt = i.CreatedAt,
                        UpdatedAt = i.UpdatedAt,
                        CasesCount = i.Cases.Count,
                        PrimaryPhoto = i.Photos.FirstOrDefault(p => p.IsPrimary) != null ? new PhotoDto
                        {
                            Id = i.Photos.First(p => p.IsPrimary).Id,
                            ImageUrl = i.Photos.First(p => p.IsPrimary).ImageUrl,
                            Caption = i.Photos.First(p => p.IsPrimary).Caption,
                            IsPrimary = true,
                            UploadedAt = i.Photos.First(p => p.IsPrimary).UploadedAt
                        } : null,
                        OwnerUserId = i.OwnerUserId,
                        OwnerName = i.OwnerUser != null ? i.OwnerUser.FullName : null
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new IndividualSearchResponse
                {
                    Individuals = individuals,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving individuals", error = ex.Message });
            }
        }

        // GET: api/individuals/{id} - Get individual details
        [HttpGet("{id}")]
        public async Task<ActionResult<IndividualDetailDto>> GetIndividual(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsAdmin();

                var individual = await _context.Individuals
                    .Include(i => i.Photos.OrderByDescending(p => p.IsPrimary).ThenByDescending(p => p.UploadedAt))
                    .Include(i => i.Activities.OrderByDescending(a => a.CreatedAt).Take(10))
                    .Include(i => i.Cases.OrderByDescending(c => c.CreatedAt))
                    .Include(i => i.OwnerUser)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (individual == null)
                {
                    return NotFound(new { message = "Individual not found" });
                }

                // Check access permissions
                if (!isAdmin && individual.OwnerUserId != userId)
                {
                    return Forbid();
                }

                var individualDto = new IndividualDetailDto
                {
                    Id = individual.Id,
                    RunnerId = individual.RunnerId,
                    FullName = individual.FullName,
                    FirstName = individual.FirstName,
                    LastName = individual.LastName,
                    MiddleName = individual.MiddleName,
                    DateOfBirth = individual.DateOfBirth,
                    Age = individual.Age,
                    Gender = individual.Gender,
                    Status = individual.Status,
                    City = individual.City,
                    State = individual.State,
                    LastSeenDate = individual.LastSeenDate,
                    LastSeenLocation = individual.LastSeenLocation,
                    CreatedAt = individual.CreatedAt,
                    UpdatedAt = individual.UpdatedAt,
                    CasesCount = individual.Cases.Count,
                    Address = individual.Address,
                    ZipCode = individual.ZipCode,
                    PhoneNumber = individual.PhoneNumber,
                    Email = individual.Email,
                    Height = individual.Height,
                    Weight = individual.Weight,
                    HairColor = individual.HairColor,
                    EyeColor = individual.EyeColor,
                    DistinguishingFeatures = individual.DistinguishingFeatures,
                    PrimaryDisability = individual.PrimaryDisability,
                    DisabilityDescription = individual.DisabilityDescription,
                    MedicalConditions = individual.MedicalConditions,
                    Medications = individual.Medications,
                    Allergies = individual.Allergies,
                    EmergencyResponseInstructions = individual.EmergencyResponseInstructions,
                    PreferredEmergencyContact = individual.PreferredEmergencyContact,
                    Latitude = individual.Latitude,
                    Longitude = individual.Longitude,
                    OwnerUserId = individual.OwnerUserId,
                    OwnerName = individual.OwnerUser?.FullName,
                    Photos = individual.Photos.Select(p => new PhotoDto
                    {
                        Id = p.Id,
                        ImageUrl = p.ImageUrl,
                        Caption = p.Caption,
                        ImageType = p.ImageType,
                        IsPrimary = p.IsPrimary,
                        UploadedAt = p.UploadedAt,
                        UploadedBy = p.UploadedBy,
                        FileName = p.FileName,
                        FileSize = p.FileSize
                    }).ToList(),
                    RecentActivities = individual.Activities.Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        ActivityType = a.ActivityType,
                        Title = a.Title,
                        Description = a.Description,
                        Location = a.Location,
                        Latitude = a.Latitude,
                        Longitude = a.Longitude,
                        CreatedAt = a.CreatedAt,
                        CreatedBy = a.CreatedBy,
                        RelatedCaseId = a.RelatedCaseId,
                        RelatedPhotoId = a.RelatedPhotoId
                    }).ToList(),
                    Cases = individual.Cases.Select(c => new CaseSummaryDto
                    {
                        Id = c.Id,
                        CaseNumber = c.CaseNumber,
                        Title = c.Title,
                        Status = c.Status,
                        Priority = c.Priority,
                        RiskLevel = c.RiskLevel,
                        CreatedAt = c.CreatedAt,
                        LastUpdatedAt = c.LastUpdatedAt,
                        UpdateCount = c.Updates.Count
                    }).ToList()
                };

                return Ok(individualDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the individual", error = ex.Message });
            }
        }

        // POST: api/individuals - Create new individual
        [HttpPost]
        public async Task<ActionResult<RunnerProfileDto>> CreateIndividual(CreateIndividualRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Validate required fields
                if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
                {
                    return BadRequest(new { message = "First name and last name are required" });
                }

                // Generate unique RunnerId if not provided
                var runnerId = request.RunnerId;
                if (string.IsNullOrEmpty(runnerId))
                {
                    runnerId = await GenerateUniqueRunnerId();
                }
                else
                {
                    // Check if RunnerId already exists
                    var existingRunner = await _context.Individuals.FirstOrDefaultAsync(i => i.RunnerId == runnerId);
                    if (existingRunner != null)
                    {
                        return BadRequest(new { message = "Runner ID already exists" });
                    }
                }

                var individual = new Individual
                {
                    RunnerId = runnerId,
                    OwnerUserId = userId.Value,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    MiddleName = request.MiddleName,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    ZipCode = request.ZipCode,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    PrimaryDisability = request.PrimaryDisability,
                    DisabilityDescription = request.DisabilityDescription,
                    MedicalConditions = request.MedicalConditions,
                    EmergencyResponseInstructions = request.EmergencyResponseInstructions,
                    PreferredEmergencyContact = request.PreferredEmergencyContact,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserEmail(),
                    UpdatedBy = GetCurrentUserEmail()
                };

                _context.Individuals.Add(individual);
                await _context.SaveChangesAsync();

                // Create activity record
                var activity = new Activity
                {
                    IndividualId = individual.Id,
                    ActivityType = "ProfileCreated",
                    Title = "Profile Created",
                    Description = $"Profile created for {individual.FullName}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserEmail()
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();

                var individualDto = new RunnerProfileDto
                {
                    Id = individual.Id,
                    RunnerId = individual.RunnerId,
                    FullName = individual.FullName,
                    FirstName = individual.FirstName,
                    LastName = individual.LastName,
                    MiddleName = individual.MiddleName,
                    DateOfBirth = individual.DateOfBirth,
                    Age = individual.Age,
                    Gender = individual.Gender,
                    Status = individual.Status,
                    City = individual.City,
                    State = individual.State,
                    CreatedAt = individual.CreatedAt,
                    UpdatedAt = individual.UpdatedAt,
                    CasesCount = 0,
                    OwnerUserId = individual.OwnerUserId
                };

                return CreatedAtAction(nameof(GetIndividual), new { id = individual.Id }, individualDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the individual", error = ex.Message });
            }
        }

        // PUT: api/individuals/{id} - Update individual
        [HttpPut("{id}")]
        public async Task<ActionResult<RunnerProfileDto>> UpdateIndividual(int id, UpdateIndividualRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsAdmin();

                var individual = await _context.Individuals.FindAsync(id);
                if (individual == null)
                {
                    return NotFound(new { message = "Individual not found" });
                }

                // Check access permissions
                if (!isAdmin && individual.OwnerUserId != userId)
                {
                    return Forbid();
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.FirstName))
                    individual.FirstName = request.FirstName;
                
                if (!string.IsNullOrEmpty(request.LastName))
                    individual.LastName = request.LastName;
                
                if (request.MiddleName != null)
                    individual.MiddleName = request.MiddleName;
                
                if (request.DateOfBirth.HasValue)
                    individual.DateOfBirth = request.DateOfBirth.Value;
                
                if (request.Gender != null)
                    individual.Gender = request.Gender;
                
                if (request.RunnerId != null)
                    individual.RunnerId = request.RunnerId;
                
                if (request.Address != null)
                    individual.Address = request.Address;
                
                if (request.City != null)
                    individual.City = request.City;
                
                if (request.State != null)
                    individual.State = request.State;
                
                if (request.ZipCode != null)
                    individual.ZipCode = request.ZipCode;
                
                if (request.PhoneNumber != null)
                    individual.PhoneNumber = request.PhoneNumber;
                
                if (request.Email != null)
                    individual.Email = request.Email;
                
                if (request.Height != null)
                    individual.Height = request.Height;
                
                if (request.Weight != null)
                    individual.Weight = request.Weight;
                
                if (request.HairColor != null)
                    individual.HairColor = request.HairColor;
                
                if (request.EyeColor != null)
                    individual.EyeColor = request.EyeColor;
                
                if (request.DistinguishingFeatures != null)
                    individual.DistinguishingFeatures = request.DistinguishingFeatures;
                
                if (request.PrimaryDisability != null)
                    individual.PrimaryDisability = request.PrimaryDisability;
                
                if (request.DisabilityDescription != null)
                    individual.DisabilityDescription = request.DisabilityDescription;
                
                if (request.MedicalConditions != null)
                    individual.MedicalConditions = request.MedicalConditions;
                
                if (request.Medications != null)
                    individual.Medications = request.Medications;
                
                if (request.Allergies != null)
                    individual.Allergies = request.Allergies;
                
                if (request.EmergencyResponseInstructions != null)
                    individual.EmergencyResponseInstructions = request.EmergencyResponseInstructions;
                
                if (request.PreferredEmergencyContact != null)
                    individual.PreferredEmergencyContact = request.PreferredEmergencyContact;
                
                if (request.Status != null)
                    individual.Status = request.Status;
                
                if (request.LastSeenDate.HasValue)
                    individual.LastSeenDate = request.LastSeenDate.Value;
                
                if (request.LastSeenLocation != null)
                    individual.LastSeenLocation = request.LastSeenLocation;
                
                if (request.Latitude.HasValue)
                    individual.Latitude = request.Latitude.Value;
                
                if (request.Longitude.HasValue)
                    individual.Longitude = request.Longitude.Value;

                individual.UpdatedAt = DateTime.UtcNow;
                individual.UpdatedBy = GetCurrentUserEmail();

                await _context.SaveChangesAsync();

                // Create activity record
                var activity = new Activity
                {
                    IndividualId = individual.Id,
                    ActivityType = "ProfileUpdated",
                    Title = "Profile Updated",
                    Description = $"Profile updated for {individual.FullName}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserEmail()
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();

                var individualDto = new RunnerProfileDto
                {
                    Id = individual.Id,
                    RunnerId = individual.RunnerId,
                    FullName = individual.FullName,
                    FirstName = individual.FirstName,
                    LastName = individual.LastName,
                    MiddleName = individual.MiddleName,
                    DateOfBirth = individual.DateOfBirth,
                    Age = individual.Age,
                    Gender = individual.Gender,
                    Status = individual.Status,
                    City = individual.City,
                    State = individual.State,
                    LastSeenDate = individual.LastSeenDate,
                    LastSeenLocation = individual.LastSeenLocation,
                    CreatedAt = individual.CreatedAt,
                    UpdatedAt = individual.UpdatedAt,
                    CasesCount = individual.Cases.Count,
                    OwnerUserId = individual.OwnerUserId
                };

                return Ok(individualDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the individual", error = ex.Message });
            }
        }

        // GET: api/individuals/{id}/photos - Get individual photos
        [HttpGet("{id}/photos")]
        public async Task<ActionResult<List<PhotoDto>>> GetIndividualPhotos(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsAdmin();

                var individual = await _context.Individuals
                    .Include(i => i.Photos.OrderByDescending(p => p.IsPrimary).ThenByDescending(p => p.UploadedAt))
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (individual == null)
                {
                    return NotFound(new { message = "Individual not found" });
                }

                // Check access permissions
                if (!isAdmin && individual.OwnerUserId != userId)
                {
                    return Forbid();
                }

                var photos = individual.Photos.Select(p => new PhotoDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl,
                    Caption = p.Caption,
                    ImageType = p.ImageType,
                    IsPrimary = p.IsPrimary,
                    UploadedAt = p.UploadedAt,
                    UploadedBy = p.UploadedBy,
                    FileName = p.FileName,
                    FileSize = p.FileSize
                }).ToList();

                return Ok(photos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving photos", error = ex.Message });
            }
        }

        // POST: api/individuals/{id}/photos - Upload photo
        [HttpPost("{id}/photos")]
        public async Task<ActionResult<PhotoDto>> UploadPhoto(int id, UploadPhotoRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsAdmin();

                var individual = await _context.Individuals.FindAsync(id);
                if (individual == null)
                {
                    return NotFound(new { message = "Individual not found" });
                }

                // Check access permissions
                if (!isAdmin && individual.OwnerUserId != userId)
                {
                    return Forbid();
                }

                // If setting as primary, unset other primary photos
                if (request.SetAsPrimary)
                {
                    var existingPrimaryPhotos = await _context.Photos
                        .Where(p => p.IndividualId == id && p.IsPrimary)
                        .ToListAsync();

                    foreach (var existingPhoto in existingPrimaryPhotos)
                    {
                        existingPhoto.IsPrimary = false;
                        existingPhoto.UpdatedAt = DateTime.UtcNow;
                        existingPhoto.UpdatedBy = GetCurrentUserEmail();
                    }
                }

                var photo = new Photo
                {
                    IndividualId = id,
                    ImageUrl = request.ImageUrl,
                    Caption = request.Caption,
                    ImageType = request.ImageType ?? "Profile",
                    IsPrimary = request.SetAsPrimary,
                    UploadedAt = DateTime.UtcNow,
                    UploadedBy = GetCurrentUserEmail(),
                    FileName = request.FileName,
                    FileSize = request.FileSize,
                    ContentType = request.ContentType
                };

                _context.Photos.Add(photo);
                await _context.SaveChangesAsync();

                // Create activity record
                var activity = new Activity
                {
                    IndividualId = id,
                    ActivityType = "PhotoAdded",
                    Title = "Photo Added",
                    Description = $"Photo added to {individual.FullName}'s profile",
                    RelatedPhotoId = photo.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserEmail()
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();

                var photoDto = new PhotoDto
                {
                    Id = photo.Id,
                    ImageUrl = photo.ImageUrl,
                    Caption = photo.Caption,
                    ImageType = photo.ImageType,
                    IsPrimary = photo.IsPrimary,
                    UploadedAt = photo.UploadedAt,
                    UploadedBy = photo.UploadedBy,
                    FileName = photo.FileName,
                    FileSize = photo.FileSize
                };

                return CreatedAtAction(nameof(GetIndividualPhotos), new { id = id }, photoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading the photo", error = ex.Message });
            }
        }

        // PUT: api/individuals/{id}/photos/{photoId} - Update photo
        [HttpPut("{id}/photos/{photoId}")]
        public async Task<ActionResult<PhotoDto>> UpdatePhoto(int id, int photoId, UpdatePhotoRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsAdmin();

                var individual = await _context.Individuals.FindAsync(id);
                if (individual == null)
                {
                    return NotFound(new { message = "Individual not found" });
                }

                // Check access permissions
                if (!isAdmin && individual.OwnerUserId != userId)
                {
                    return Forbid();
                }

                var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId && p.IndividualId == id);
                if (photo == null)
                {
                    return NotFound(new { message = "Photo not found" });
                }

                // Update fields
                if (request.Caption != null)
                    photo.Caption = request.Caption;
                
                if (request.ImageType != null)
                    photo.ImageType = request.ImageType;

                // Handle primary photo change
                if (request.SetAsPrimary.HasValue && request.SetAsPrimary.Value && !photo.IsPrimary)
                {
                    // Unset other primary photos
                    var existingPrimaryPhotos = await _context.Photos
                        .Where(p => p.IndividualId == id && p.IsPrimary && p.Id != photoId)
                        .ToListAsync();

                    foreach (var existingPhoto in existingPrimaryPhotos)
                    {
                        existingPhoto.IsPrimary = false;
                        existingPhoto.UpdatedAt = DateTime.UtcNow;
                        existingPhoto.UpdatedBy = GetCurrentUserEmail();
                    }

                    photo.IsPrimary = true;

                    // Create activity for primary photo change
                    var primaryActivity = new Activity
                    {
                        IndividualId = id,
                        ActivityType = "PhotoPrimaryChanged",
                        Title = "Primary Photo Changed",
                        Description = $"Primary photo changed for {individual.FullName}",
                        RelatedPhotoId = photo.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserEmail()
                    };

                    _context.Activities.Add(primaryActivity);
                }

                photo.UpdatedAt = DateTime.UtcNow;
                photo.UpdatedBy = GetCurrentUserEmail();

                await _context.SaveChangesAsync();

                var photoDto = new PhotoDto
                {
                    Id = photo.Id,
                    ImageUrl = photo.ImageUrl,
                    Caption = photo.Caption,
                    ImageType = photo.ImageType,
                    IsPrimary = photo.IsPrimary,
                    UploadedAt = photo.UploadedAt,
                    UploadedBy = photo.UploadedBy,
                    FileName = photo.FileName,
                    FileSize = photo.FileSize
                };

                return Ok(photoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the photo", error = ex.Message });
            }
        }

        // DELETE: api/individuals/{id}/photos/{photoId} - Delete photo
        [HttpDelete("{id}/photos/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int id, int photoId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsAdmin();

                var individual = await _context.Individuals.FindAsync(id);
                if (individual == null)
                {
                    return NotFound(new { message = "Individual not found" });
                }

                // Check access permissions
                if (!isAdmin && individual.OwnerUserId != userId)
                {
                    return Forbid();
                }

                var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId && p.IndividualId == id);
                if (photo == null)
                {
                    return NotFound(new { message = "Photo not found" });
                }

                _context.Photos.Remove(photo);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the photo", error = ex.Message });
            }
        }

        // GET: api/individuals/{id}/activities - Get individual activities
        [HttpGet("{id}/activities")]
        public async Task<ActionResult<List<ActivityDto>>> GetIndividualActivities(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsAdmin();

                var individual = await _context.Individuals.FindAsync(id);
                if (individual == null)
                {
                    return NotFound(new { message = "Individual not found" });
                }

                // Check access permissions
                if (!isAdmin && individual.OwnerUserId != userId)
                {
                    return Forbid();
                }

                var activities = await _context.Activities
                    .Where(a => a.IndividualId == id)
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        ActivityType = a.ActivityType,
                        Title = a.Title,
                        Description = a.Description,
                        Location = a.Location,
                        Latitude = a.Latitude,
                        Longitude = a.Longitude,
                        CreatedAt = a.CreatedAt,
                        CreatedBy = a.CreatedBy,
                        RelatedCaseId = a.RelatedCaseId,
                        RelatedPhotoId = a.RelatedPhotoId
                    })
                    .ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving activities", error = ex.Message });
            }
        }

        // GET: api/individuals/export - Export individuals to CSV
        [HttpGet("export")]
        public async Task<IActionResult> ExportIndividualsToCsv()
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsAdmin();

                var query = _context.Individuals.AsQueryable();

                // Apply ownership filter for non-admins
                if (!isAdmin)
                {
                    query = query.Where(i => i.OwnerUserId == userId);
                }

                var individuals = await query.ToListAsync();
                var csvBytes = _csvExportService.ExportIndividualsToCsv(individuals);
                
                var fileName = $"runners_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while exporting data." });
            }
        }

        // GET: api/individuals/export/filtered
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

        private async Task<string> GenerateUniqueRunnerId()
        {
            var prefix = "RUNNER";
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random();
            var suffix = random.Next(1000, 9999).ToString();
            
            var runnerId = $"{prefix}-{timestamp}-{suffix}";
            
            // Ensure uniqueness
            while (await _context.Individuals.AnyAsync(i => i.RunnerId == runnerId))
            {
                suffix = random.Next(1000, 9999).ToString();
                runnerId = $"{prefix}-{timestamp}-{suffix}";
            }
            
            return runnerId;
        }
    }
}
