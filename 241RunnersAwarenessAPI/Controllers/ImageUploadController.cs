using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _241RunnersAwarenessAPI.Models;
using _241RunnersAwarenessAPI.Services;
using _241RunnersAwarenessAPI.Data;
using Microsoft.EntityFrameworkCore;

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
                if (request.Image == null)
                {
                    return BadRequest(new ImageUploadResponse 
                    { 
                        Success = false, 
                        Error = "No image file provided" 
                    });
                }

                // Validate the image
                if (!await _imageUploadService.ValidateImageAsync(request.Image))
                {
                    return BadRequest(new ImageUploadResponse 
                    { 
                        Success = false, 
                        Error = "Invalid image file. Please ensure it's a valid image under 10MB." 
                    });
                }

                // Upload the image
                var imageUrl = await _imageUploadService.UploadImageAsync(
                    request.Image, 
                    request.Category ?? "general", 
                    request.RelatedId
                );

                // Update the related entity with the image URL
                if (request.RelatedId.HasValue)
                {
                    await UpdateEntityWithImageUrl(request.Category, request.RelatedId.Value, imageUrl);
                }

                return Ok(new ImageUploadResponse
                {
                    Success = true,
                    ImageUrl = imageUrl,
                    Message = "Image uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
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
                if (request.Images == null || !request.Images.Any())
                {
                    return BadRequest(new MultipleImageUploadResponse
                    {
                        Success = false,
                        Error = "No images provided"
                    });
                }

                // Validate all images
                foreach (var image in request.Images)
                {
                    if (!await _imageUploadService.ValidateImageAsync(image))
                    {
                        return BadRequest(new MultipleImageUploadResponse
                        {
                            Success = false,
                            Error = $"Invalid image file: {image.FileName}. Please ensure it's a valid image under 10MB."
                        });
                    }
                }

                // Upload all images
                var imageUrls = await _imageUploadService.UploadMultipleImagesAsync(
                    request.Images,
                    request.Category ?? "general",
                    request.RelatedId
                );

                // Update the related entity with the image URLs
                if (request.RelatedId.HasValue && imageUrls.Any())
                {
                    await UpdateEntityWithMultipleImageUrls(request.Category, request.RelatedId.Value, imageUrls);
                }

                return Ok(new MultipleImageUploadResponse
                {
                    Success = true,
                    ImageUrls = imageUrls,
                    Message = $"Successfully uploaded {imageUrls.Count} images"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading multiple images");
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
                if (string.IsNullOrEmpty(request.ImageUrl))
                {
                    return BadRequest(new ImageDeleteResponse
                    {
                        Success = false,
                        Error = "Image URL is required"
                    });
                }

                // Delete the image file
                var deleted = await _imageUploadService.DeleteImageAsync(request.ImageUrl);
                
                if (!deleted)
                {
                    return NotFound(new ImageDeleteResponse
                    {
                        Success = false,
                        Error = "Image not found"
                    });
                }

                // Update the related entity to remove the image URL
                if (request.RelatedId.HasValue)
                {
                    await RemoveImageUrlFromEntity(request.ImageUrl, request.RelatedId.Value);
                }

                return Ok(new ImageDeleteResponse
                {
                    Success = true,
                    Message = "Image deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
                return StatusCode(500, new ImageDeleteResponse
                {
                    Success = false,
                    Error = "An error occurred while deleting the image"
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
                _logger.LogError(ex, "Error retrieving entity images");
                return StatusCode(500, "An error occurred while retrieving the images");
            }
        }

        private async Task UpdateEntityWithImageUrl(string? category, int entityId, string imageUrl)
        {
            if (string.IsNullOrEmpty(category))
                return;

            switch (category.ToLower())
            {
                case "profile":
                    var user = await _context.Users.FindAsync(entityId);
                    if (user != null)
                    {
                        user.ProfileImageUrl = imageUrl;
                        user.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "runner":
                    var runner = await _context.Runners.FindAsync(entityId);
                    if (runner != null)
                    {
                        runner.ProfileImageUrl = imageUrl;
                        runner.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    break;
            }
        }

        private async Task UpdateEntityWithMultipleImageUrls(string? category, int entityId, List<string> imageUrls)
        {
            if (string.IsNullOrEmpty(category) || !imageUrls.Any())
                return;

            var imageUrlsJson = System.Text.Json.JsonSerializer.Serialize(imageUrls);

            switch (category.ToLower())
            {
                case "user":
                    var user = await _context.Users.FindAsync(entityId);
                    if (user != null)
                    {
                        user.AdditionalImageUrls = imageUrlsJson;
                        user.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "runner":
                    var runner = await _context.Runners.FindAsync(entityId);
                    if (runner != null)
                    {
                        runner.AdditionalImageUrls = imageUrlsJson;
                        runner.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    break;
            }
        }

        private async Task RemoveImageUrlFromEntity(string imageUrl, int entityId)
        {
            // Try to find and update user
            var user = await _context.Users.FindAsync(entityId);
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
            var runner = await _context.Runners.FindAsync(entityId);
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