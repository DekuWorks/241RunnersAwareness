using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/individuals")]
    [Authorize]
    public class IndividualsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndividualsController> _logger;

        public IndividualsController(ApplicationDbContext context, ILogger<IndividualsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all individuals with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetIndividuals([FromQuery] IndividualQuery query)
        {
            try
            {
                var individualsQuery = _context.Runners.AsQueryable();

                // Apply status filter (using IsActive for now)
                if (!string.IsNullOrEmpty(query.Status))
                {
                    if (query.Status == "Active")
                        individualsQuery = individualsQuery.Where(r => r.IsActive);
                    else if (query.Status == "Inactive")
                        individualsQuery = individualsQuery.Where(r => !r.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(query.Q))
                {
                    var searchTerm = query.Q.ToLower();
                    individualsQuery = individualsQuery.Where(r => 
                        r.Name.ToLower().Contains(searchTerm) ||
                        (r.PhysicalDescription != null && r.PhysicalDescription.ToLower().Contains(searchTerm)));
                }

                // Apply mine filter for regular users
                if (query.Mine == true && !IsStaff())
                {
                    var currentUserId = GetCurrentUserId();
                    if (int.TryParse(currentUserId, out var userId))
                    {
                        individualsQuery = individualsQuery.Where(r => r.UserId == userId);
                    }
                }

                // Get total count
                var total = await individualsQuery.CountAsync();

                // Apply pagination
                var individualsData = await individualsQuery
                    .Include(r => r.User)
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var individuals = individualsData.Select(r => new
                {
                    id = $"ind_{r.Id}",
                    firstName = r.Name?.Split(' ').FirstOrDefault() ?? "",
                    lastName = r.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    dob = r.DateOfBirth.ToString("yyyy-MM-dd"),
                    status = r.IsActive ? "Active" : "Inactive",
                    lastSeen = new
                    {
                        city = r.LastKnownLocation?.Split(',').FirstOrDefault()?.Trim(),
                        state = r.LastKnownLocation?.Split(',').Skip(1).FirstOrDefault()?.Trim(),
                        time = r.LastLocationUpdate?.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    },
                    photoUrl = r.ProfileImageUrl
                }).ToList();

                return Ok(new
                {
                    data = individuals,
                    page = query.Page,
                    pageSize = query.PageSize,
                    total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving individuals");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while retrieving individuals"
                    }
                });
            }
        }

        /// <summary>
        /// Get individual by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIndividual(string id)
        {
            try
            {
                if (!int.TryParse(id.Replace("ind_", ""), out var individualId))
                {
                    return NotFoundResponse("Individual not found");
                }

                var individual = await _context.Runners
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == individualId);

                if (individual == null)
                {
                    return NotFoundResponse("Individual not found");
                }

                // Check permissions for regular users
                if (!IsStaff())
                {
                    var currentUserId = GetCurrentUserId();
                    if (int.TryParse(currentUserId, out var userId) && individual.UserId != userId)
                    {
                        return UnauthorizedResponse("Access denied");
                    }
                }

                var result = new
                {
                    id = $"ind_{individual.Id}",
                    firstName = individual.Name?.Split(' ').FirstOrDefault() ?? "",
                    lastName = individual.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    dob = individual.DateOfBirth.ToString("yyyy-MM-dd"),
                    address = individual.PreferredRunningLocations,
                    diagnosis = individual.MedicalConditions,
                    notes = individual.AdditionalNotes,
                    status = individual.IsActive ? "Active" : "Inactive",
                    photos = new[]
                    {
                        new
                        {
                            url = individual.ProfileImageUrl,
                            uploadedAt = individual.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                        }
                    },
                    contacts = new[]
                    {
                        new
                        {
                            name = individual.User?.EmergencyContactName,
                            phone = individual.User?.EmergencyContactPhone
                        }
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving individual {IndividualId}", id);
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while retrieving individual"
                    }
                });
            }
        }

        /// <summary>
        /// Create a new individual
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateIndividual([FromBody] CreateIndividualRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
                {
                    return ValidationErrorResponse(new Dictionary<string, string[]>
                    {
                        { "firstName", new[] { "First name is required" } },
                        { "lastName", new[] { "Last name is required" } }
                    });
                }

                var currentUserId = GetCurrentUserId();
                if (!int.TryParse(currentUserId, out var userId))
                {
                    return UnauthorizedResponse("Invalid user");
                }

                // Create new individual
                var individual = new Runner
                {
                    Name = $"{request.FirstName} {request.LastName}",
                    DateOfBirth = request.Dob != null ? DateTime.Parse(request.Dob) : DateTime.UtcNow,
                    PreferredRunningLocations = request.Address,
                    MedicalConditions = request.Diagnosis,
                    AdditionalNotes = request.Notes,
                    ProfileImageUrl = request.PhotoUrl,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Runners.Add(individual);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Individual created: {Name} by user {UserId}", individual.Name, userId);

                return CreatedAtAction(nameof(GetIndividual), new { id = individual.Id }, new
                {
                    id = $"ind_{individual.Id}",
                    firstName = request.FirstName,
                    lastName = request.LastName,
                    dob = request.Dob,
                    address = request.Address,
                    diagnosis = request.Diagnosis,
                    notes = request.Notes,
                    status = request.Status,
                    photoUrl = request.PhotoUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating individual");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while creating individual"
                    }
                });
            }
        }

        /// <summary>
        /// Update an individual
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIndividual(string id, [FromBody] UpdateIndividualRequest request)
        {
            try
            {
                if (!int.TryParse(id.Replace("ind_", ""), out var individualId))
                {
                    return NotFoundResponse("Individual not found");
                }

                var individual = await _context.Runners.FindAsync(individualId);
                if (individual == null)
                {
                    return NotFoundResponse("Individual not found");
                }

                // Check permissions
                if (!IsStaff())
                {
                    var currentUserId = GetCurrentUserId();
                    if (int.TryParse(currentUserId, out var userId) && individual.UserId != userId)
                    {
                        return UnauthorizedResponse("Access denied");
                    }
                }

                // Update individual fields
                if (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName))
                {
                    var firstName = request.FirstName ?? individual.Name?.Split(' ').FirstOrDefault() ?? "";
                    var lastName = request.LastName ?? individual.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "";
                    individual.Name = $"{firstName} {lastName}".Trim();
                }

                if (request.Dob != null)
                {
                    individual.DateOfBirth = DateTime.Parse(request.Dob);
                }

                if (request.Address != null)
                {
                    individual.PreferredRunningLocations = request.Address;
                }

                if (request.Diagnosis != null)
                {
                    individual.MedicalConditions = request.Diagnosis;
                }

                if (request.Notes != null)
                {
                    individual.AdditionalNotes = request.Notes;
                }

                if (request.Status != null)
                {
                    individual.IsActive = request.Status == "Active";
                }

                if (request.PhotoUrl != null)
                {
                    individual.ProfileImageUrl = request.PhotoUrl;
                }

                individual.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Individual updated: {Name}", individual.Name);

                return Ok(new
                {
                    id = $"ind_{individual.Id}",
                    firstName = individual.Name?.Split(' ').FirstOrDefault() ?? "",
                    lastName = individual.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    dob = individual.DateOfBirth.ToString("yyyy-MM-dd"),
                    address = individual.PreferredRunningLocations,
                    diagnosis = individual.MedicalConditions,
                    notes = individual.AdditionalNotes,
                    status = individual.IsActive ? "Active" : "Inactive",
                    photoUrl = individual.ProfileImageUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating individual {IndividualId}", id);
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while updating individual"
                    }
                });
            }
        }

        /// <summary>
        /// Delete an individual
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteIndividual(string id)
        {
            try
            {
                if (!int.TryParse(id.Replace("ind_", ""), out var individualId))
                {
                    return NotFoundResponse("Individual not found");
                }

                var individual = await _context.Runners.FindAsync(individualId);
                if (individual == null)
                {
                    return NotFoundResponse("Individual not found");
                }

                // Check if individual has related cases
                var hasCases = await _context.Cases.AnyAsync(c => c.RunnerId == individualId);
                if (hasCases)
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "HAS_RELATED_DATA",
                            message = "Cannot delete individual with related cases"
                        }
                    });
                }

                _context.Runners.Remove(individual);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Individual deleted: {Name}", individual.Name);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting individual {IndividualId}", id);
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while deleting individual"
                    }
                });
            }
        }
    }

    public class IndividualQuery
    {
        public string? Status { get; set; }
        public string? Q { get; set; }
        public bool? Mine { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateIndividualRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Dob { get; set; }
        public string? Address { get; set; }
        public string? Diagnosis { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class UpdateIndividualRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Dob { get; set; }
        public string? Address { get; set; }
        public string? Diagnosis { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
