using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/cases")]
    [Authorize]
    public class CasesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CasesController> _logger;

        public CasesController(ApplicationDbContext context, ILogger<CasesController> logger)
        {
            _context = context;
            _logger = logger;
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
        /// Create a new case
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCase([FromBody] CreateCaseRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.IndividualId) || string.IsNullOrEmpty(request.Title))
                {
                    return ValidationErrorResponse(new Dictionary<string, string[]>
                    {
                        { "individualId", new[] { "Individual ID is required" } },
                        { "title", new[] { "Title is required" } }
                    });
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
        [Authorize(Roles = "Admin")]
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

                _context.Cases.Remove(caseEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Case deleted: {Title}", caseEntity.Title);

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