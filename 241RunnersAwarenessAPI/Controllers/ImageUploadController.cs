using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _241RunnersAwarenessAPI.Models;
using _241RunnersAwarenessAPI.Services;
using _241RunnersAwarenessAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImageUploadController : ControllerBase
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageUploadController> _logger;

        public ImageUploadController(
            IImageUploadService imageUploadService,
            ApplicationDbContext context,
            ILogger<ImageUploadController> logger)
        {
            _imageUploadService = imageUploadService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Upload a single image for a user profile or runner case
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<ImageUploadResponse>> UploadImage([FromForm] ImageUploadRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ImageUploadResponse 
                    { 
                        Success = false, 
                        Error = "User not authenticated" 
                    });
                }

                // Validate request model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new ImageUploadResponse 
                    { 
                        Success = false, 
                        Error = $"Validation failed: {string.Join(", ", errors)}" 
                    });
                }

                // Check if user has permission to upload for this category
                if (!await HasUploadPermissionAsync(userId, request.Category, request.RelatedId))
                {
                    return Forbid();
                }

                // Upload the image
                var response = await _imageUploadService.UploadImageAsync(request, userId);
                
                if (response.Success)
                {
                    // Update the related entity with the image URL
                    await UpdateEntityWithImageUrl(request.Category, request.RelatedId, response.ImageUrl, response.ImageId);
                    
                    // Log successful upload
                    _logger.LogInformation("Image uploaded successfully by user {UserId} for {Category} {RelatedId}", 
                        userId, request.Category, request.RelatedId);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new ImageUploadResponse
                {
                    Success = false,
                    Error = "An error occurred while uploading the image"
                });
            }
        }

        /// <summary>
        /// Upload multiple images for a user profile or runner case
        /// </summary>
        [HttpPost("upload-multiple")]
        public async Task<ActionResult<MultipleImageUploadResponse>> UploadMultipleImages([FromForm] MultipleImageUploadRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new MultipleImageUploadResponse
                    {
                        Success = false,
                        Error = "User not authenticated"
                    });
                }

                // Validate request model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new MultipleImageUploadResponse
                    {
                        Success = false,
                        Error = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                // Check if user has permission to upload for this category
                if (!await HasUploadPermissionAsync(userId, request.Category, request.RelatedId))
                {
                    return Forbid();
                }

                // Validate number of images
                if (request.Images.Count > 10)
                {
                    return BadRequest(new MultipleImageUploadResponse
                    {
                        Success = false,
                        Error = "Maximum 10 images can be uploaded at once"
                    });
                }

                // Upload all images
                var response = await _imageUploadService.UploadMultipleImagesAsync(request, userId);

                if (response.Success && response.ImageUrls.Any())
                {
                    // Update the related entity with the image URLs
                    await UpdateEntityWithMultipleImageUrls(request.Category, request.RelatedId, response.ImageUrls, response.ImageIds);
                    
                    // Log successful upload
                    _logger.LogInformation("Multiple images uploaded successfully by user {UserId} for {Category} {RelatedId}: {Count} images", 
                        userId, request.Category, request.RelatedId, response.TotalUploaded);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading multiple images for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new MultipleImageUploadResponse
                {
                    Success = false,
                    Error = "An error occurred while uploading the images"
                });
            }
        }

        /// <summary>
        /// Delete an image
        /// </summary>
        [HttpDelete("delete")]
        public async Task<ActionResult<ImageDeleteResponse>> DeleteImage([FromBody] ImageDeleteRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ImageDeleteResponse
                    {
                        Success = false,
                        Error = "User not authenticated"
                    });
                }

                // Validate request model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new ImageDeleteResponse
                    {
                        Success = false,
                        Error = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                // Check if user has permission to delete this image
                if (!await HasDeletePermissionAsync(userId, request.ImageUrl, request.RelatedId))
                {
                    return Forbid();
                }

                // Delete the image file
                var deleted = await _imageUploadService.DeleteImageAsync(request.ImageUrl, userId);
                
                if (!deleted)
                {
                    return NotFound(new ImageDeleteResponse
                    {
                        Success = false,
                        Error = "Image not found"
                    });
                }

                // Update the related entity to remove the image URL
                await RemoveImageUrlFromEntity(request.ImageUrl, request.RelatedId);

                // Log successful deletion
                _logger.LogInformation("Image deleted successfully by user {UserId}: {ImageUrl}", userId, request.ImageUrl);

                return Ok(new ImageDeleteResponse
                {
                    Success = true,
                    Message = "Image deleted successfully",
                    DeletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new ImageDeleteResponse
                {
                    Success = false,
                    Error = "An error occurred while deleting the image"
                });
            }
        }

        /// <summary>
        /// Search images for a specific entity or category
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<ImageSearchResponse>> SearchImages([FromBody] ImageSearchRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ImageSearchResponse
                    {
                        Success = false,
                        Error = "User not authenticated"
                    });
                }

                // Validate request model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new ImageSearchResponse
                    {
                        Success = false,
                        Error = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                // Search images
                var response = await _imageUploadService.SearchImagesAsync(request, userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching images for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new ImageSearchResponse
                {
                    Success = false,
                    Error = "An error occurred while searching images"
                });
            }
        }

        /// <summary>
        /// Get images for a specific entity (user or runner)
        /// </summary>
        [HttpGet("entity/{entityType}/{entityId}")]
        public async Task<ActionResult<object>> GetEntityImages(string entityType, int entityId)
        {
            try
            {
                // Get current user ID from JWT token
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                // Validate entity type
                if (!new[] { "user", "runner" }.Contains(entityType.ToLower()))
                {
                    return BadRequest("Invalid entity type. Use 'user' or 'runner'");
                }

                // Check if user has permission to view these images
                if (!await HasViewPermissionAsync(userId, entityType, entityId))
                {
                    return Forbid();
                }

                switch (entityType.ToLower())
                {
                    case "user":
                        var user = await _context.Users
                            .Where(u => u.Id == entityId)
                            .Select(u => new { u.ProfileImageUrl, u.AdditionalImageUrls, u.DocumentUrls })
                            .FirstOrDefaultAsync();
                        
                        if (user == null)
                            return NotFound("User not found");
                        
                        return Ok(user);

                    case "runner":
                        var runner = await _context.Runners
                            .Where(r => r.Id == entityId)
                            .Select(r => new { r.ProfileImageUrl, r.AdditionalImageUrls, r.DocumentUrls })
                            .FirstOrDefaultAsync();
                        
                        if (runner == null)
                            return NotFound("Runner case not found");
                        
                        return Ok(runner);

                    default:
                        return BadRequest("Invalid entity type. Use 'user' or 'runner'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity images for user {UserId}", GetCurrentUserId());
                return StatusCode(500, "An error occurred while retrieving the images");
            }
        }

        /// <summary>
        /// Check if user has permission to upload images for a category
        /// </summary>
        private async Task<bool> HasUploadPermissionAsync(string userId, string category, int? relatedId)
        {
            try
            {
                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null) return false;

                // Admins can upload anywhere
                if (user.Role == "admin") return true;

                // Users can upload to their own profile
                if (category == "profile" && relatedId.HasValue && int.Parse(userId) == relatedId.Value)
                    return true;

                // Users can upload to runner cases they reported
                if (category == "runner" && relatedId.HasValue)
                {
                    var runner = await _context.Runners.FindAsync(relatedId.Value);
                    return runner?.ReportedByUserId == int.Parse(userId);
                }

                // Users can upload to documents they own
                if (category == "document" && relatedId.HasValue && int.Parse(userId) == relatedId.Value)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if user has permission to delete an image
        /// </summary>
        private async Task<bool> HasDeletePermissionAsync(string userId, string imageUrl, int? relatedId)
        {
            try
            {
                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null) return false;

                // Admins can delete any image
                if (user.Role == "admin") return true;

                // Users can delete their own images
                if (relatedId.HasValue && int.Parse(userId) == relatedId.Value)
                    return true;

                // Check if image belongs to user
                var userImages = await GetUserImageUrls(int.Parse(userId));
                return userImages.Contains(imageUrl);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if user has permission to view entity images
        /// </summary>
        private async Task<bool> HasViewPermissionAsync(string userId, string entityType, int entityId)
        {
            try
            {
                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null) return false;

                // Admins can view any images
                if (user.Role == "admin") return true;

                // Users can view their own images
                if (entityType == "user" && int.Parse(userId) == entityId)
                    return true;

                // Users can view runner case images they reported
                if (entityType == "runner")
                {
                    var runner = await _context.Runners.FindAsync(entityId);
                    return runner?.ReportedByUserId == int.Parse(userId);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        private string? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Try alternative claim names
                userIdClaim = User.FindFirst("sub")?.Value ?? 
                             User.FindFirst("userId")?.Value ?? 
                             User.FindFirst("id")?.Value;
            }
            return userIdClaim;
        }

        /// <summary>
        /// Get all image URLs for a user
        /// </summary>
        private async Task<List<string>> GetUserImageUrls(int userId)
        {
            var urls = new List<string>();
            
            var user = await _context.Users.FindAsync(userId);
            if (user?.ProfileImageUrl != null)
                urls.Add(user.ProfileImageUrl);
            
            if (!string.IsNullOrEmpty(user?.AdditionalImageUrls))
            {
                try
                {
                    var additionalUrls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.AdditionalImageUrls);
                    if (additionalUrls != null)
                        urls.AddRange(additionalUrls);
                }
                catch { /* Ignore JSON parsing errors */ }
            }
            
            if (!string.IsNullOrEmpty(user?.DocumentUrls))
            {
                try
                {
                    var documentUrls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.DocumentUrls);
                    if (documentUrls != null)
                        urls.AddRange(documentUrls);
                }
                catch { /* Ignore JSON parsing errors */ }
            }
            
            return urls;
        }

        private async Task UpdateEntityWithImageUrl(string category, int? entityId, string? imageUrl, string? imageId)
        {
            if (string.IsNullOrEmpty(category) || !entityId.HasValue || string.IsNullOrEmpty(imageUrl))
                return;

            switch (category.ToLower())
            {
                case "profile":
                    var user = await _context.Users.FindAsync(entityId.Value);
                    if (user != null)
                    {
                        user.ProfileImageUrl = imageUrl;
                        user.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "runner":
                    var runner = await _context.Runners.FindAsync(entityId.Value);
                    if (runner != null)
                    {
                        runner.ProfileImageUrl = imageUrl;
                        runner.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    break;
            }
        }

        private async Task UpdateEntityWithMultipleImageUrls(string category, int? entityId, List<string> imageUrls, List<string> imageIds)
        {
            if (string.IsNullOrEmpty(category) || !entityId.HasValue || !imageUrls.Any())
                return;

            var imageUrlsJson = System.Text.Json.JsonSerializer.Serialize(imageUrls);

            switch (category.ToLower())
            {
                case "user":
                    var user = await _context.Users.FindAsync(entityId.Value);
                    if (user != null)
                    {
                        user.AdditionalImageUrls = imageUrlsJson;
                        user.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "runner":
                    var runner = await _context.Runners.FindAsync(entityId.Value);
                    if (runner != null)
                    {
                        runner.AdditionalImageUrls = imageUrlsJson;
                        runner.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    break;
            }
        }

        private async Task RemoveImageUrlFromEntity(string imageUrl, int? entityId)
        {
            if (!entityId.HasValue) return;

            // Try to find and update user
            var user = await _context.Users.FindAsync(entityId.Value);
            if (user != null)
            {
                if (user.ProfileImageUrl == imageUrl)
                    user.ProfileImageUrl = null;
                
                if (!string.IsNullOrEmpty(user.AdditionalImageUrls))
                {
                    try
                    {
                        var urls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.AdditionalImageUrls);
                        if (urls != null)
                        {
                            urls.Remove(imageUrl);
                            user.AdditionalImageUrls = System.Text.Json.JsonSerializer.Serialize(urls);
                        }
                    }
                    catch { /* Ignore JSON parsing errors */ }
                }
                
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return;
            }

            // Try to find and update runner
            var runner = await _context.Runners.FindAsync(entityId.Value);
            if (runner != null)
            {
                if (runner.ProfileImageUrl == imageUrl)
                    runner.ProfileImageUrl = null;
                
                if (!string.IsNullOrEmpty(runner.AdditionalImageUrls))
                {
                    try
                    {
                        var urls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(runner.AdditionalImageUrls);
                        if (urls != null)
                        {
                            urls.Remove(imageUrl);
                            runner.AdditionalImageUrls = System.Text.Json.JsonSerializer.Serialize(urls);
                        }
                    }
                    catch { /* Ignore JSON parsing errors */ }
                }
                
                runner.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
} 