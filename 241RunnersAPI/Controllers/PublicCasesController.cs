using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using _241RunnersAPI.Services;

namespace _241RunnersAPI.Controllers
{
    /// <summary>
    /// Public, unauthenticated endpoints for privacy-safe case data and map data.
    /// Follows the contract defined in PUBLIC_API_SPEC.md.
    /// </summary>
    [ApiController]
    [Route("api/public/cases")]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class PublicCasesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PublicCasesController> _logger;

        public PublicCasesController(ApplicationDbContext context, ILogger<PublicCasesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/public/cases
        /// Returns a paged list of public cases using PublicCaseDto.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPublicCases([FromQuery] PublicCaseQuery query)
        {
            try
            {
                var casesQuery = _context.Cases
                    .Include(c => c.Runner)
                    .Where(c => c.IsPublic && c.Status != "Closed")
                    .AsQueryable();

                // Search by runner name, description, or location
                if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    var searchTerm = query.Search.ToLower();
                    casesQuery = casesQuery.Where(c =>
                        (c.Runner.FirstName != null && c.Runner.FirstName.ToLower().Contains(searchTerm)) ||
                        (c.Runner.LastName != null && c.Runner.LastName.ToLower().Contains(searchTerm)) ||
                        (c.Description != null && c.Description.ToLower().Contains(searchTerm)) ||
                        (c.Location != null && c.Location.ToLower().Contains(searchTerm)) ||
                        (c.LastSeenLocation != null && c.LastSeenLocation.ToLower().Contains(searchTerm)));
                }

                // Status filter (e.g., Missing, Found, Safe, Resolved)
                if (!string.IsNullOrWhiteSpace(query.Status))
                {
                    casesQuery = casesQuery.Where(c => c.Status == query.Status);
                }

                // Region filter (e.g., "houston") based on location text
                if (!string.IsNullOrWhiteSpace(query.Region))
                {
                    var region = query.Region.ToLower();
                    if (region == "houston")
                    {
                        casesQuery = casesQuery.Where(c =>
                            (c.Location != null && (c.Location.ToLower().Contains("houston") ||
                                                    c.Location.ToLower().Contains("texas") ||
                                                    c.Location.ToLower().Contains("tx"))) ||
                            (c.LastSeenLocation != null && (c.LastSeenLocation.ToLower().Contains("houston") ||
                                                            c.LastSeenLocation.ToLower().Contains("texas") ||
                                                            c.LastSeenLocation.ToLower().Contains("tx"))));
                    }
                }

                var totalCount = await casesQuery.CountAsync();

                // Project to anonymous (DateOfBirth for age; Runner.Age is NotMapped and not translatable)
                var raw = await casesQuery
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(c => new
                    {
                        Id = c.Id.ToString(),
                        PublicDisplayName =
                            c.Runner.FirstName != null && c.Runner.LastName != null && c.Runner.LastName != string.Empty
                                ? c.Runner.FirstName + " " + c.Runner.LastName.Substring(0, 1) + "."
                                : c.Runner.Name,
                        DateOfBirth = c.Runner.DateOfBirth,
                        Status = c.Status,
                        LastSeenCityRaw = c.Location ?? c.LastSeenLocation,
                        LastSeenAt = c.LastSeenAt ?? c.LastSeenDate,
                        PhotoUrl = c.Runner.ProfileImageUrl,
                        DescriptionShort = c.Description.Length > 400
                            ? c.Description.Substring(0, 397) + "..."
                            : c.Description,
                        Latitude = c.LastSeenLatitude.HasValue
                            ? Math.Round(c.LastSeenLatitude.Value, 3)
                            : (decimal?)null,
                        Longitude = c.LastSeenLongitude.HasValue
                            ? Math.Round(c.LastSeenLongitude.Value, 3)
                            : (decimal?)null,
                        UpdatedAt = c.UpdatedAt ?? c.CreatedAt
                    })
                    .ToListAsync();

                // Build DTOs with parsed city/state and age range (EF cannot translate these helpers)
                var items = raw.Select(c =>
                {
                    var age = PublicCaseHelpers.ComputeAge(c.DateOfBirth);
                    var (city, state) = PublicCaseHelpers.ParseCityState(c.LastSeenCityRaw);
                    return new PublicCaseDto
                    {
                        Id = c.Id,
                        PublicDisplayName = c.PublicDisplayName,
                        Age = age,
                        AgeRange = PublicCaseHelpers.GetAgeRange(age),
                        Status = c.Status,
                        LastSeenCity = city,
                        LastSeenState = state,
                        LastSeenAt = c.LastSeenAt,
                        PhotoUrl = c.PhotoUrl,
                        DescriptionShort = c.DescriptionShort,
                        Latitude = c.Latitude,
                        Longitude = c.Longitude,
                        UpdatedAt = c.UpdatedAt
                    };
                }).ToList();

                return Ok(new
                {
                    items,
                    totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public cases");
                return InternalServerErrorResponse("Failed to get public cases");
            }
        }

        /// <summary>
        /// GET /api/public/cases/{id}
        /// Returns a single public case by id or 404 if not found.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPublicCase(string id)
        {
            try
            {
                if (!int.TryParse(id, out var caseId))
                {
                    return BadRequestResponse("Invalid case id");
                }

                var c = await _context.Cases
                    .Include(ca => ca.Runner)
                    .Where(ca => ca.Id == caseId && ca.IsPublic && ca.Status != "Closed")
                    .FirstOrDefaultAsync();

                if (c == null)
                {
                    return NotFoundResponse("Case not found");
                }

                var locationRaw = c.Location ?? c.LastSeenLocation;
                var (city, state) = PublicCaseHelpers.ParseCityState(locationRaw);

                var dto = new PublicCaseDto
                {
                    Id = c.Id.ToString(),
                    PublicDisplayName =
                        c.Runner.FirstName != null && c.Runner.LastName != null && c.Runner.LastName != string.Empty
                            ? c.Runner.FirstName + " " + c.Runner.LastName.Substring(0, 1) + "."
                            : c.Runner.Name,
                    Age = c.Runner.Age,
                    AgeRange = PublicCaseHelpers.GetAgeRange(c.Runner.Age),
                    Status = c.Status,
                    LastSeenCity = city,
                    LastSeenState = state,
                    LastSeenAt = c.LastSeenAt ?? c.LastSeenDate,
                    PhotoUrl = c.Runner.ProfileImageUrl,
                    DescriptionShort = c.Description.Length > 400
                        ? c.Description.Substring(0, 397) + "..."
                        : c.Description,
                    Latitude = c.LastSeenLatitude.HasValue
                        ? Math.Round(c.LastSeenLatitude.Value, 3)
                        : null,
                    Longitude = c.LastSeenLongitude.HasValue
                        ? Math.Round(c.LastSeenLongitude.Value, 3)
                        : null,
                    UpdatedAt = c.UpdatedAt ?? c.CreatedAt
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public case by id");
                return InternalServerErrorResponse("Failed to get public case");
            }
        }

        /// <summary>
        /// GET /api/public/map/missing
        /// Returns only Missing cases that have coordinates, for the public map.
        /// </summary>
        [HttpGet("~/api/public/map/missing")]
        public async Task<IActionResult> GetMissingCasesForMap()
        {
            try
            {
                var raw = await _context.Cases
                    .Include(c => c.Runner)
                    .Where(c =>
                        c.IsPublic &&
                        c.Status == "Missing" &&
                        c.LastSeenLatitude.HasValue &&
                        c.LastSeenLongitude.HasValue)
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .Select(c => new
                    {
                        id = c.Id,
                        firstName = c.Runner.FirstName,
                        lastName = c.Runner.LastName,
                        name = c.Runner.Name,
                        status = c.Status,
                        latitude = Math.Round(c.LastSeenLatitude!.Value, 3),
                        longitude = Math.Round(c.LastSeenLongitude!.Value, 3),
                        photoUrl = c.Runner.ProfileImageUrl,
                        locationRaw = c.Location ?? c.LastSeenLocation,
                        updatedAt = c.UpdatedAt ?? c.CreatedAt
                    })
                    .ToListAsync();

                var items = raw.Select(c =>
                {
                    var (city, state) = PublicCaseHelpers.ParseCityState(c.locationRaw);
                    var displayName = c.firstName != null && c.lastName != null && c.lastName != string.Empty
                        ? c.firstName + " " + c.lastName.Substring(0, 1) + "."
                        : c.name;
                    return new
                    {
                        id = c.id,
                        publicDisplayName = displayName,
                        status = c.status,
                        latitude = c.latitude,
                        longitude = c.longitude,
                        photoUrl = c.photoUrl,
                        lastSeenCity = city,
                        lastSeenState = state,
                        updatedAt = c.updatedAt
                    };
                }).ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public missing cases for map");
                return InternalServerErrorResponse("Failed to get public missing cases for map");
            }
        }

        /// <summary>
        /// GET /api/public/map/runners
        /// Returns community runners who opted in (ShowOnMap) with coordinates. Privacy-safe: display name only, no contact info.
        /// </summary>
        [HttpGet("~/api/public/map/runners")]
        public async Task<IActionResult> GetCommunityRunnersForMap()
        {
            try
            {
                var raw = await _context.Runners
                    .Where(r => r.ShowOnMap && r.MapLatitude.HasValue && r.MapLongitude.HasValue && r.IsActive)
                    .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                    .Select(r => new
                    {
                        id = r.Id,
                        firstName = r.FirstName,
                        lastName = r.LastName,
                        name = r.Name,
                        status = r.Status,
                        latitude = Math.Round(r.MapLatitude!.Value, 3),
                        longitude = Math.Round(r.MapLongitude!.Value, 3),
                        photoUrl = r.ProfileImageUrl,
                        locationRaw = r.LastKnownLocation,
                        updatedAt = r.UpdatedAt ?? r.CreatedAt
                    })
                    .ToListAsync();

                var items = raw.Select(r =>
                {
                    var (city, state) = PublicCaseHelpers.ParseCityState(r.locationRaw);
                    var displayName = r.firstName != null && r.lastName != null && r.lastName != string.Empty
                        ? r.firstName + " " + r.lastName.Substring(0, 1) + "."
                        : r.name;
                    return new
                    {
                        id = r.id,
                        publicDisplayName = displayName,
                        status = r.status,
                        latitude = r.latitude,
                        longitude = r.longitude,
                        photoUrl = r.photoUrl,
                        lastSeenCity = city,
                        lastSeenState = state,
                        updatedAt = r.updatedAt,
                        type = "community"
                    };
                }).ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting community runners for map");
                return InternalServerErrorResponse("Failed to get community runners for map");
            }
        }
    }
}

