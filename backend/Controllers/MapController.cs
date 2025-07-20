using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using _241RunnersAwareness.BackendAPI.Services;
using System.Text.Json;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly RunnersDbContext _context;
        private readonly ILogger<MapController> _logger;
        private readonly IAnalyticsService _analyticsService;

        // Houston coordinates
        private const double HOUSTON_LAT = 29.7604;
        private const double HOUSTON_LNG = -95.3698;
        private const double HOUSTON_RADIUS_MILES = 50.0; // 50 mile radius around Houston

        public MapController(RunnersDbContext context, ILogger<MapController> logger, IAnalyticsService analyticsService)
        {
            _context = context;
            _logger = logger;
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get all individuals with location data for map display
        /// </summary>
        [HttpGet("individuals")]
        public async Task<IActionResult> GetIndividualsForMap()
        {
            try
            {
                var individuals = await _context.Individuals
                    .Where(i => i.Latitude.HasValue && i.Longitude.HasValue)
                    .Select(i => new
                    {
                        i.Id,
                        i.FirstName,
                        i.LastName,
                        i.MiddleName,
                        i.DateOfBirth,
                        i.Gender,
                        i.Address,
                        i.City,
                        i.State,
                        i.ZipCode,
                        i.Latitude,
                        i.Longitude,
                        i.CurrentStatus,
                        i.LastSeenDate,
                        i.LastSeenLocation,
                        i.CreatedAt,
                        FullName = $"{i.FirstName} {i.LastName}",
                        Age = DateTime.Now.Year - i.DateOfBirth.Year - (DateTime.Now.DayOfYear < i.DateOfBirth.DayOfYear ? 1 : 0)
                    })
                    .ToListAsync();

                await _analyticsService.TrackMapInteractionAsync(
                    User.Identity?.Name ?? "anonymous",
                    AnalyticsService.MapView,
                    "houston",
                    null
                );

                return Ok(individuals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get individuals for map");
                return StatusCode(500, new { Error = "Failed to retrieve map data" });
            }
        }

        /// <summary>
        /// Get individuals within Houston area (50 mile radius)
        /// </summary>
        [HttpGet("houston")]
        public async Task<IActionResult> GetHoustonIndividuals()
        {
            try
            {
                var individuals = await _context.Individuals
                    .Where(i => i.Latitude.HasValue && i.Longitude.HasValue)
                    .ToListAsync();

                var houstonIndividuals = individuals
                    .Where(i => CalculateDistance(HOUSTON_LAT, HOUSTON_LNG, i.Latitude.Value, i.Longitude.Value) <= HOUSTON_RADIUS_MILES)
                    .Select(i => new
                    {
                        i.Id,
                        i.FirstName,
                        i.LastName,
                        i.MiddleName,
                        i.DateOfBirth,
                        i.Gender,
                        i.Address,
                        i.City,
                        i.State,
                        i.ZipCode,
                        i.Latitude,
                        i.Longitude,
                        i.CurrentStatus,
                        i.LastSeenDate,
                        i.LastSeenLocation,
                        i.CreatedAt,
                        FullName = $"{i.FirstName} {i.LastName}",
                        Age = DateTime.Now.Year - i.DateOfBirth.Year - (DateTime.Now.DayOfYear < i.DateOfBirth.DayOfYear ? 1 : 0),
                        DistanceFromHouston = CalculateDistance(HOUSTON_LAT, HOUSTON_LNG, i.Latitude.Value, i.Longitude.Value)
                    })
                    .OrderBy(i => i.DistanceFromHouston)
                    .ToList();

                await _analyticsService.TrackMapInteractionAsync(
                    User.Identity?.Name ?? "anonymous",
                    AnalyticsService.MapSearch,
                    "houston_area",
                    null
                );

                return Ok(new
                {
                    Individuals = houstonIndividuals,
                    HoustonCenter = new { Latitude = HOUSTON_LAT, Longitude = HOUSTON_LNG },
                    RadiusMiles = HOUSTON_RADIUS_MILES,
                    TotalCount = houstonIndividuals.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Houston individuals");
                return StatusCode(500, new { Error = "Failed to retrieve Houston area data" });
            }
        }

        /// <summary>
        /// Get map statistics for Houston area
        /// </summary>
        [HttpGet("houston/stats")]
        public async Task<IActionResult> GetHoustonStats()
        {
            try
            {
                var individuals = await _context.Individuals
                    .Where(i => i.Latitude.HasValue && i.Longitude.HasValue)
                    .ToListAsync();

                var houstonIndividuals = individuals
                    .Where(i => CalculateDistance(HOUSTON_LAT, HOUSTON_LNG, i.Latitude.Value, i.Longitude.Value) <= HOUSTON_RADIUS_MILES)
                    .ToList();

                var stats = new
                {
                    TotalIndividuals = houstonIndividuals.Count,
                    StatusBreakdown = houstonIndividuals
                        .GroupBy(i => i.CurrentStatus ?? "Unknown")
                        .Select(g => new { Status = g.Key, Count = g.Count() })
                        .ToList(),
                    AgeGroups = houstonIndividuals
                        .GroupBy(i => GetAgeGroup(DateTime.Now.Year - i.DateOfBirth.Year))
                        .Select(g => new { AgeGroup = g.Key, Count = g.Count() })
                        .OrderBy(x => x.AgeGroup)
                        .ToList(),
                    GenderBreakdown = houstonIndividuals
                        .GroupBy(i => i.Gender ?? "Unknown")
                        .Select(g => new { Gender = g.Key, Count = g.Count() })
                        .ToList(),
                    RecentCases = houstonIndividuals
                        .Where(i => i.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                        .Count(),
                    MissingCases = houstonIndividuals
                        .Count(i => i.CurrentStatus == "Missing"),
                    FoundCases = houstonIndividuals
                        .Count(i => i.CurrentStatus == "Found"),
                    SafeCases = houstonIndividuals
                        .Count(i => i.CurrentStatus == "Safe")
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Houston stats");
                return StatusCode(500, new { Error = "Failed to retrieve Houston statistics" });
            }
        }

        /// <summary>
        /// Geocode an address to get coordinates
        /// </summary>
        [HttpPost("geocode")]
        public async Task<IActionResult> GeocodeAddress([FromBody] GeocodeRequest request)
        {
            try
            {
                // For now, return mock coordinates for Houston addresses
                // In production, you would integrate with a geocoding service like Google Maps API
                var coordinates = await GeocodeAddressMock(request.Address);
                
                return Ok(coordinates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to geocode address");
                return StatusCode(500, new { Error = "Failed to geocode address" });
            }
        }

        /// <summary>
        /// Update individual's location coordinates
        /// </summary>
        [HttpPut("individuals/{id}/location")]
        public async Task<IActionResult> UpdateIndividualLocation(int id, [FromBody] LocationUpdateRequest request)
        {
            try
            {
                var individual = await _context.Individuals.FindAsync(id);
                if (individual == null)
                {
                    return NotFound(new { Error = "Individual not found" });
                }

                individual.Latitude = request.Latitude;
                individual.Longitude = request.Longitude;
                individual.Address = request.Address;
                individual.City = request.City;
                individual.State = request.State;
                individual.ZipCode = request.ZipCode;
                individual.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _analyticsService.TrackMapInteractionAsync(
                    User.Identity?.Name ?? "anonymous",
                    AnalyticsService.MapClick,
                    $"{request.Latitude},{request.Longitude}",
                    id.ToString()
                );

                return Ok(new { Message = "Location updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update individual location");
                return StatusCode(500, new { Error = "Failed to update location" });
            }
        }

        #region Private Methods

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 3959; // Earth's radius in miles
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private string GetAgeGroup(int age)
        {
            return age switch
            {
                < 18 => "Under 18",
                < 25 => "18-24",
                < 35 => "25-34",
                < 50 => "35-49",
                < 65 => "50-64",
                _ => "65+"
            };
        }

        private async Task<GeocodeResult> GeocodeAddressMock(string address)
        {
            // Mock geocoding for Houston addresses
            // In production, use Google Maps API or similar service
            var houstonCoordinates = new Dictionary<string, (double lat, double lng)>
            {
                { "downtown", (29.7604, -95.3698) },
                { "galleria", (29.7404, -95.4628) },
                { "medical center", (29.7074, -95.3978) },
                { "heights", (29.8004, -95.4198) },
                { "montrose", (29.7404, -95.3698) },
                { "river oaks", (29.7404, -95.4198) },
                { "memorial", (29.7804, -95.5198) },
                { "spring", (30.0794, -95.4178) },
                { "katy", (29.7854, -95.8248) },
                { "sugar land", (29.6194, -95.6348) }
            };

            var lowerAddress = address.ToLower();
            foreach (var coord in houstonCoordinates)
            {
                if (lowerAddress.Contains(coord.Key))
                {
                    return new GeocodeResult
                    {
                        Latitude = coord.Value.lat + (new Random().NextDouble() - 0.5) * 0.01,
                        Longitude = coord.Value.lng + (new Random().NextDouble() - 0.5) * 0.01,
                        FormattedAddress = address
                    };
                }
            }

            // Default to Houston center with some random offset
            return new GeocodeResult
            {
                Latitude = HOUSTON_LAT + (new Random().NextDouble() - 0.5) * 0.1,
                Longitude = HOUSTON_LNG + (new Random().NextDouble() - 0.5) * 0.1,
                FormattedAddress = address
            };
        }

        #endregion
    }

    public class GeocodeRequest
    {
        public string Address { get; set; } = string.Empty;
    }

    public class GeocodeResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string FormattedAddress { get; set; } = string.Empty;
    }

    public class LocationUpdateRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
    }
} 