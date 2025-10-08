using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/map")]
    public class MapController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MapController> _logger;

        public MapController(ApplicationDbContext context, ILogger<MapController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get map points (public for aggregated, admin for raw)
        /// </summary>
        [HttpGet("points")]
        [Authorize]
        public async Task<IActionResult> GetMapPoints([FromQuery] MapQuery query)
        {
            try
            {
                if (query.Cluster == true)
                {
                    // Return clustered points for public access
                    var clusters = new[]
                    {
                        new { lat = 29.76, lng = -95.36, count = 12 },
                        new { lat = 29.71, lng = -95.45, count = 7 }
                    };

                    return Ok(new { clusters });
                }
                else
                {
                    // Return individual points (requires admin access)
                    if (!IsAdmin())
                    {
                        return UnauthorizedResponse("Admin access required for raw points");
                    }

                    var points = await _context.Cases
                        .Include(c => c.Runner)
                        .Where(c => c.Status == query.Status || string.IsNullOrEmpty(query.Status))
                        .Select(c => new
                        {
                            id = $"ind_{c.RunnerId}",
                            lat = 29.76, // Mock coordinates
                            lng = -95.36,
                            status = c.Status
                        })
                        .ToListAsync();

                    return Ok(new { data = points });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving map points");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while retrieving map points"
                    }
                });
            }
        }

        /// <summary>
        /// Get raw map points (admin only)
        /// </summary>
        [HttpGet("points/raw")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetRawMapPoints()
        {
            try
            {
                var points = await _context.Cases
                    .Include(c => c.Runner)
                    .Select(c => new
                    {
                        id = $"ind_{c.RunnerId}",
                        lat = 29.76, // Mock coordinates
                        lng = -95.36,
                        status = c.Status
                    })
                    .ToListAsync();

                return Ok(new { data = points });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving raw map points");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while retrieving raw map points"
                    }
                });
            }
        }
    }

    public class MapQuery
    {
        public string? Bbox { get; set; }
        public string? Status { get; set; }
        public bool? Cluster { get; set; }
    }
}
