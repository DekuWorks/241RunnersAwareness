using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using _241RunnersAPI.Services;

namespace _241RunnersAPI.Controllers
{
    /// <summary>
    /// Controller for device registration and management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DevicesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DevicesController> _logger;
        private readonly ITopicService _topicService;

        public DevicesController(
            ApplicationDbContext context,
            ILogger<DevicesController> logger,
            ITopicService topicService)
        {
            _context = context;
            _logger = logger;
            _topicService = topicService;
        }

        /// <summary>
        /// Register a device for push notifications
        /// </summary>
        /// <param name="request">Device registration request</param>
        /// <returns>Registration result</returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterDevice([FromBody] DeviceRegistrationDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserIdAsInt();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                // Check if device already exists for this user and platform
                var existingDevice = await _context.Devices
                    .FirstOrDefaultAsync(d => d.UserId == userId && d.Platform == request.Platform);

                if (existingDevice != null)
                {
                    // Update existing device
                    existingDevice.FcmToken = request.FcmToken;
                    existingDevice.AppVersion = request.AppVersion;
                    existingDevice.DeviceModel = request.DeviceModel;
                    existingDevice.OsVersion = request.OsVersion;
                    existingDevice.AppBuildNumber = request.AppBuildNumber;
                    existingDevice.LastSeenAt = DateTime.UtcNow;
                    existingDevice.UpdatedAt = DateTime.UtcNow;
                    existingDevice.IsActive = true;

                    _context.Devices.Update(existingDevice);
                }
                else
                {
                    // Create new device
                    var device = new Device
                    {
                        UserId = userId.Value,
                        Platform = request.Platform,
                        FcmToken = request.FcmToken,
                        AppVersion = request.AppVersion,
                        DeviceModel = request.DeviceModel,
                        OsVersion = request.OsVersion,
                        AppBuildNumber = request.AppBuildNumber,
                        LastSeenAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Devices.Add(device);
                }

                await _context.SaveChangesAsync();

                // Subscribe to default topics for the user
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    await _topicService.SubscribeToDefaultTopicsAsync(userId.Value, user.Role);
                }

                _logger.LogInformation("Device registered successfully for user {UserId} on platform {Platform}", 
                    userId, request.Platform);

                return Ok(new { 
                    success = true, 
                    message = "Device registered successfully",
                    platform = request.Platform,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Unregister a device (mark as inactive)
        /// </summary>
        /// <param name="platform">Platform to unregister (ios/android)</param>
        /// <returns>Unregistration result</returns>
        [HttpDelete("unregister")]
        public async Task<IActionResult> UnregisterDevice([FromQuery] string platform)
        {
            try
            {
                var userId = GetCurrentUserIdAsInt();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                if (string.IsNullOrEmpty(platform))
                {
                    return BadRequest("Platform is required");
                }

                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.UserId == userId && d.Platform == platform);

                if (device == null)
                {
                    return NotFound("Device not found");
                }

                device.IsActive = false;
                device.UpdatedAt = DateTime.UtcNow;

                _context.Devices.Update(device);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Device unregistered successfully for user {UserId} on platform {Platform}", 
                    userId, platform);

                return Ok(new { 
                    success = true, 
                    message = "Device unregistered successfully",
                    platform = platform,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering device for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Get user's registered devices
        /// </summary>
        /// <returns>List of user's devices</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserDevices()
        {
            try
            {
                var userId = GetCurrentUserIdAsInt();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var devices = await _context.Devices
                    .Where(d => d.UserId == userId && d.IsActive)
                    .Select(d => new DeviceResponseDto
                    {
                        Id = d.Id,
                        UserId = d.UserId,
                        Platform = d.Platform,
                        AppVersion = d.AppVersion,
                        LastSeenAt = d.LastSeenAt,
                        IsActive = d.IsActive,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt,
                        DeviceModel = d.DeviceModel,
                        OsVersion = d.OsVersion,
                        AppBuildNumber = d.AppBuildNumber,
                        Topics = new List<string>() // Will be populated below
                    })
                    .ToListAsync();

                // Populate topics after the query
                foreach (var device in devices)
                {
                    var deviceEntity = await _context.Devices.FindAsync(device.Id);
                    if (deviceEntity != null && !string.IsNullOrEmpty(deviceEntity.TopicsJson))
                    {
                        device.Topics = System.Text.Json.JsonSerializer.Deserialize<List<string>>(deviceEntity.TopicsJson) ?? new List<string>();
                    }
                }

                return Ok(new { 
                    success = true, 
                    devices = devices,
                    count = devices.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting devices for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Update device last seen timestamp
        /// </summary>
        /// <param name="platform">Platform to update</param>
        /// <returns>Update result</returns>
        [HttpPost("heartbeat")]
        public async Task<IActionResult> UpdateDeviceHeartbeat([FromQuery] string platform)
        {
            try
            {
                var userId = GetCurrentUserIdAsInt();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                if (string.IsNullOrEmpty(platform))
                {
                    return BadRequest("Platform is required");
                }

                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.UserId == userId && d.Platform == platform && d.IsActive);

                if (device == null)
                {
                    return NotFound("Active device not found");
                }

                device.LastSeenAt = DateTime.UtcNow;
                device.UpdatedAt = DateTime.UtcNow;

                _context.Devices.Update(device);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = "Device heartbeat updated",
                    lastSeenAt = device.LastSeenAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device heartbeat for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Get device statistics (admin only)
        /// </summary>
        /// <returns>Device statistics</returns>
        [HttpGet("stats")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetDeviceStats()
        {
            try
            {
                var totalDevices = await _context.Devices.CountAsync();
                var activeDevices = await _context.Devices.CountAsync(d => d.IsActive);
                var iosDevices = await _context.Devices.CountAsync(d => d.Platform == "ios" && d.IsActive);
                var androidDevices = await _context.Devices.CountAsync(d => d.Platform == "android" && d.IsActive);
                
                var recentDevices = await _context.Devices
                    .Where(d => d.LastSeenAt >= DateTime.UtcNow.AddDays(-7))
                    .CountAsync();

                return Ok(new { 
                    success = true, 
                    stats = new {
                        totalDevices = totalDevices,
                        activeDevices = activeDevices,
                        iosDevices = iosDevices,
                        androidDevices = androidDevices,
                        recentDevices = recentDevices,
                        lastUpdated = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device statistics");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }
    }
}
