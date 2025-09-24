using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using _241RunnersAPI.Services;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/cases")]
    [ApiVersion("1.0")]
    [Authorize]
    public class CasesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CasesController> _logger;
        private readonly CoordinateValidationService _coordinateValidation;
        private readonly ContentSecurityService _contentSecurity;
        private readonly ISignalRService _signalRService;

        public CasesController(
            ApplicationDbContext context, 
            ILogger<CasesController> logger, 
            CoordinateValidationService coordinateValidation, 
            ContentSecurityService contentSecurity,
            ISignalRService signalRService)
        {
            _context = context;
            _logger = logger;
            _coordinateValidation = coordinateValidation;
            _contentSecurity = contentSecurity;
            _signalRService = signalRService;
        }

        /// <summary>
        /// Get all cases with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCases([FromQuery] CaseQuery query)
        {
            try
            {
                var casesQuery = _context.Cases.AsQueryable();

                // Apply status filter
                if (!string.IsNullOrEmpty(query.Status))
                {
                    casesQuery = casesQuery.Where(c => c.Status == query.Status);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(query.Q))
                {
                    var searchTerm = query.Q.ToLower();
                    casesQuery = casesQuery.Where(c => 
                        c.Title.ToLower().Contains(searchTerm) ||
                        c.Description.ToLower().Contains(searchTerm));
                }

                // Apply owner filter for regular users
                if (!IsStaff())
                {
                    var currentUserId = GetCurrentUserId();
                    if (int.TryParse(currentUserId, out var userId))
                    {
                        casesQuery = casesQuery.Where(c => c.ReportedByUserId == userId);
                    }
                }

                // Get total count
                var total = await casesQuery.CountAsync();

                // Apply pagination
                var casesData = await casesQuery
                    .Include(c => c.Runner)
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var cases = casesData.Select(c => new
                {
                    id = $"case_{c.Id}",
                    individualId = $"ind_{c.RunnerId}",
                    title = c.Title,
                    status = c.Status,
                    createdAt = c.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    updatedAt = c.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }).ToList();

                return Ok(new
                {
                    data = cases,
                    page = query.Page,
                    pageSize = query.PageSize,
                    total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cases");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while retrieving cases"
                    }
                });
            }
        }

        /// <summary>
        /// Get case by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCase(string id)
        {
            try
            {
                if (!int.TryParse(id.Replace("case_", ""), out var caseId))
                {
                    return NotFoundResponse("Case not found");
                }

                var caseEntity = await _context.Cases
                    .Include(c => c.Runner)
                    .FirstOrDefaultAsync(c => c.Id == caseId);

                if (caseEntity == null)
                {
                    return NotFoundResponse("Case not found");
                }

                // Check permissions for regular users
                if (!IsStaff())
                {
                    var currentUserId = GetCurrentUserId();
                    if (int.TryParse(currentUserId, out var userId) && caseEntity.ReportedByUserId != userId)
                    {
                        return UnauthorizedResponse("Access denied");
                    }
                }

                var result = new
                {
                    id = $"case_{caseEntity.Id}",
                    individualId = $"ind_{caseEntity.RunnerId}",
                    status = caseEntity.Status,
                    events = new[]
                    {
                        new
                        {
                            type = "Created",
                            at = caseEntity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                        },
                        caseEntity.UpdatedAt.HasValue ? new
                        {
                            type = "Updated",
                            at = caseEntity.UpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")
                        } : null
                    }.Where(e => e != null)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving case {CaseId}", id);
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while retrieving case"
                    }
                });
            }
        }

        /// <summary>
        /// Create a new case with enhanced validation
        /// </summary>
        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB request size limit
        public async Task<IActionResult> CreateCase([FromBody] CreateCaseRequest request)
        {
            try
            {
                // Comprehensive input validation
                var validationErrors = new Dictionary<string, string[]>();
                
                // Individual ID validation
                if (string.IsNullOrWhiteSpace(request.IndividualId))
                {
                    validationErrors["individualId"] = new[] { "Individual ID is required" };
                }
                else if (!IsValidId(request.IndividualId, "ind_"))
                {
                    validationErrors["individualId"] = new[] { "Invalid individual ID format" };
                }
                
                // Title validation
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    validationErrors["title"] = new[] { "Title is required" };
                }
                else if (request.Title.Length > 200)
                {
                    validationErrors["title"] = new[] { "Title is too long (max 200 characters)" };
                }
                
                // Description validation
                if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 2000)
                {
                    validationErrors["description"] = new[] { "Description is too long (max 2000 characters)" };
                }
                
                // Status validation
                if (!string.IsNullOrWhiteSpace(request.Status) && !IsValidCaseStatus(request.Status))
                {
                    validationErrors["status"] = new[] { "Invalid status. Must be 'open', 'investigating', 'resolved', or 'closed'" };
                }

                // Location validation
                if (string.IsNullOrWhiteSpace(request.LastSeenLocation))
                {
                    validationErrors["lastSeenLocation"] = new[] { "Last seen location is required" };
                }
                else if (!_coordinateValidation.ValidateLocationString(request.LastSeenLocation))
                {
                    validationErrors["lastSeenLocation"] = new[] { "Invalid location format or content" };
                }

                // Coordinate validation (if provided)
                if (request.Latitude.HasValue || request.Longitude.HasValue)
                {
                    var latitude = request.Latitude.HasValue ? (decimal?)request.Latitude.Value : null;
                    var longitude = request.Longitude.HasValue ? (decimal?)request.Longitude.Value : null;
                    
                    if (!_coordinateValidation.ValidateCoordinates(latitude, longitude))
                    {
                        validationErrors["coordinates"] = new[] { "Invalid coordinates. Latitude must be between -90 and 90, longitude between -180 and 180" };
                    }
                }

                // Content security validation
                var titleValidation = _contentSecurity.ValidateContent(request.Title, "title");
                if (!titleValidation.IsValid)
                {
                    validationErrors["title"] = titleValidation.Errors.ToArray();
                }

                if (!string.IsNullOrWhiteSpace(request.Description))
                {
                    var descriptionValidation = _contentSecurity.ValidateContent(request.Description, "description");
                    if (!descriptionValidation.IsValid)
                    {
                        validationErrors["description"] = descriptionValidation.Errors.ToArray();
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.AdditionalInfo))
                {
                    var additionalInfoValidation = _contentSecurity.ValidateContent(request.AdditionalInfo, "text");
                    if (!additionalInfoValidation.IsValid)
                    {
                        validationErrors["additionalInfo"] = additionalInfoValidation.Errors.ToArray();
                    }
                }
                
                if (validationErrors.Any())
                {
                    return ValidationErrorResponse(validationErrors);
                }

                // Parse individual ID
                if (!int.TryParse(request.IndividualId.Replace("ind_", ""), out var individualId))
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "INVALID_INDIVIDUAL_ID",
                            message = "Invalid individual ID"
                        }
                    });
                }

                // Verify individual exists
                var individual = await _context.Runners.FindAsync(individualId);
                if (individual == null)
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "INDIVIDUAL_NOT_FOUND",
                            message = "Individual not found"
                        }
                    });
                }

                var currentUserId = GetCurrentUserId();
                if (!int.TryParse(currentUserId, out var userId))
                {
                    return UnauthorizedResponse("Invalid user");
                }

                // Create new case
                var caseEntity = new Case
                {
                    RunnerId = individualId,
                    ReportedByUserId = userId,
                    Title = request.Title,
                    Description = request.Description,
                    Status = request.Status ?? "Missing",
                    Priority = "Medium",
                    IsPublic = true,
                    IsApproved = IsStaff(),
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Cases.Add(caseEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Case created: {Title} for individual {IndividualId} by user {UserId}", 
                    caseEntity.Title, individualId, userId);

                // Broadcast new case notification
                try
                {
                    var caseData = new
                    {
                        id = caseEntity.Id,
                        title = caseEntity.Title,
                        status = caseEntity.Status,
                        priority = caseEntity.Priority,
                        createdAt = caseEntity.CreatedAt,
                        runnerId = caseEntity.RunnerId
                    };
                    await _signalRService.BroadcastNewCaseAsync(caseEntity.Id, caseData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error broadcasting new case notification for case {CaseId}", caseEntity.Id);
                }

                return CreatedAtAction(nameof(GetCase), new { id = caseEntity.Id }, new
                {
                    id = $"case_{caseEntity.Id}",
                    individualId = request.IndividualId,
                    title = caseEntity.Title,
                    description = caseEntity.Description,
                    status = caseEntity.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating case");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while creating case"
                    }
                });
            }
        }

        /// <summary>
        /// Update a case
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCase(string id, [FromBody] UpdateCaseRequest request)
        {
            try
            {
                if (!int.TryParse(id.Replace("case_", ""), out var caseId))
                {
                    return NotFoundResponse("Case not found");
                }

                var caseEntity = await _context.Cases.FindAsync(caseId);
                if (caseEntity == null)
                {
                    return NotFoundResponse("Case not found");
                }

                // Check permissions
                if (!IsStaff())
                {
                    var currentUserId = GetCurrentUserId();
                    if (int.TryParse(currentUserId, out var userId) && caseEntity.ReportedByUserId != userId)
                    {
                        return UnauthorizedResponse("Access denied");
                    }
                }

                // Update case fields
                if (request.Status != null)
                {
                    caseEntity.Status = request.Status;
                }

                if (request.Description != null)
                {
                    caseEntity.Description = request.Description;
                }

                if (request.Title != null)
                {
                    caseEntity.Title = request.Title;
                }

                caseEntity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Case updated: {Title}", caseEntity.Title);

                // Broadcast case update notification
                try
                {
                    var caseData = new
                    {
                        id = caseEntity.Id,
                        title = caseEntity.Title,
                        status = caseEntity.Status,
                        priority = caseEntity.Priority,
                        updatedAt = caseEntity.UpdatedAt,
                        runnerId = caseEntity.RunnerId
                    };
                    await _signalRService.BroadcastCaseUpdatedAsync(caseEntity.Id, caseData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error broadcasting case update notification for case {CaseId}", caseEntity.Id);
                }

                return Ok(new
                {
                    id = $"case_{caseEntity.Id}",
                    individualId = $"ind_{caseEntity.RunnerId}",
                    title = caseEntity.Title,
                    description = caseEntity.Description,
                    status = caseEntity.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating case {CaseId}", id);
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while updating case"
                    }
                });
            }
        }

        /// <summary>
        /// Get public cases (for public consumption)
        /// </summary>
        [HttpGet("publiccases")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicCases([FromQuery] PublicCaseQuery query)
        {
            try
            {
                var casesQuery = _context.Cases
                    .Where(c => c.IsPublic == true && c.Status != "Closed")
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(query.Search))
                {
                    var searchTerm = query.Search.ToLower();
                    casesQuery = casesQuery.Where(c => 
                        c.Runner.FirstName.ToLower().Contains(searchTerm) ||
                        c.Runner.LastName.ToLower().Contains(searchTerm) ||
                        c.Description.ToLower().Contains(searchTerm) ||
                        c.Location.ToLower().Contains(searchTerm));
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(query.Status))
                {
                    casesQuery = casesQuery.Where(c => c.Status == query.Status);
                }

                // Apply region filter (simplified - in production, use proper geographic filtering)
                if (!string.IsNullOrEmpty(query.Region))
                {
                    var region = query.Region.ToLower();
                    if (region == "houston")
                    {
                        casesQuery = casesQuery.Where(c => 
                            c.Location.ToLower().Contains("houston") ||
                            c.Location.ToLower().Contains("texas") ||
                            c.Location.ToLower().Contains("tx"));
                    }
                }

                // Get total count
                var totalCount = await casesQuery.CountAsync();

                // Apply pagination
                var cases = await casesQuery
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(c => new
                    {
                        id = c.Id,
                        runnerId = c.RunnerId,
                        runnerName = $"{c.Runner.FirstName} {c.Runner.LastName}",
                        runnerAge = c.Runner.Age,
                        runnerGender = c.Runner.Gender,
                        runnerPhoto = c.Runner.ProfileImageUrl,
                        description = c.Description,
                        location = c.Location,
                        status = c.Status,
                        lastSeen = c.LastSeenAt,
                        createdAt = c.CreatedAt,
                        isPublic = c.IsPublic
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    cases = cases,
                    pagination = new
                    {
                        page = query.Page,
                        pageSize = query.PageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public cases");
                return InternalServerErrorResponse("Failed to get public cases");
            }
        }

        /// <summary>
        /// Get public cases statistics for a region
        /// </summary>
        [HttpGet("publiccases/stats/{region}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicCasesStats(string region)
        {
            try
            {
                var casesQuery = _context.Cases
                    .Where(c => c.IsPublic == true)
                    .AsQueryable();

                // Apply region filter
                if (!string.IsNullOrEmpty(region))
                {
                    var regionLower = region.ToLower();
                    if (regionLower == "houston")
                    {
                        casesQuery = casesQuery.Where(c => 
                            c.Location.ToLower().Contains("houston") ||
                            c.Location.ToLower().Contains("texas") ||
                            c.Location.ToLower().Contains("tx"));
                    }
                }

                var totalCases = await casesQuery.CountAsync();
                var activeCases = await casesQuery.Where(c => c.Status == "Active").CountAsync();
                var resolvedCases = await casesQuery.Where(c => c.Status == "Resolved").CountAsync();
                var closedCases = await casesQuery.Where(c => c.Status == "Closed").CountAsync();

                var recentCases = await casesQuery
                    .Where(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                    .CountAsync();

                return Ok(new
                {
                    success = true,
                    stats = new
                    {
                        totalCases = totalCases,
                        activeCases = activeCases,
                        resolvedCases = resolvedCases,
                        closedCases = closedCases,
                        recentCases = recentCases,
                        region = region
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public cases stats");
                return InternalServerErrorResponse("Failed to get public cases statistics");
            }
        }

        /// <summary>
        /// Delete a case
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCase(string id)
        {
            try
            {
                if (!int.TryParse(id.Replace("case_", ""), out var caseId))
                {
                    return NotFoundResponse("Case not found");
                }

                var caseEntity = await _context.Cases.FindAsync(caseId);
                if (caseEntity == null)
                {
                    return NotFoundResponse("Case not found");
                }

                // Store case data before deletion for SignalR notification
                var caseData = new
                {
                    id = caseEntity.Id,
                    title = caseEntity.Title,
                    status = caseEntity.Status,
                    priority = caseEntity.Priority,
                    deletedAt = DateTime.UtcNow,
                    runnerId = caseEntity.RunnerId
                };

                _context.Cases.Remove(caseEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Case deleted: {Title}", caseEntity.Title);

                // Broadcast case deletion notification
                try
                {
                    await _signalRService.BroadcastCaseDeletedAsync(caseEntity.Id, caseData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error broadcasting case deletion notification for case {CaseId}", caseEntity.Id);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting case {CaseId}", id);
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while deleting case"
                    }
                });
            }
        }

        #region Validation Helper Methods

        /// <summary>
        /// Validates ID format
        /// </summary>
        private bool IsValidId(string id, string prefix)
        {
            if (string.IsNullOrWhiteSpace(id) || !id.StartsWith(prefix))
                return false;

            var numericPart = id.Substring(prefix.Length);
            return int.TryParse(numericPart, out _) && int.Parse(numericPart) > 0;
        }

        /// <summary>
        /// Validates case status
        /// </summary>
        private bool IsValidCaseStatus(string status)
        {
            var validStatuses = new[] { "open", "investigating", "resolved", "closed" };
            return validStatuses.Contains(status.ToLower());
        }

        /// <summary>
        /// Sanitizes input to prevent XSS
        /// </summary>
        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return System.Web.HttpUtility.HtmlEncode(input.Trim());
        }

        /// <summary>
        /// Validates and sanitizes text input
        /// </summary>
        private string ValidateAndSanitizeText(string input, string fieldName, int maxLength = 255)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var sanitized = SanitizeInput(input);
            
            if (sanitized.Length > maxLength)
                throw new ArgumentException($"{fieldName} is too long (max {maxLength} characters)");

            return sanitized;
        }

        #endregion
    }

    public class CaseQuery
    {
        public string? Status { get; set; }
        public string? Q { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateCaseRequest
    {
        public string IndividualId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? LastSeenLocation { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AdditionalInfo { get; set; }
    }

    public class UpdateCaseRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }

    public class PublicCaseQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? Region { get; set; }
    }
}