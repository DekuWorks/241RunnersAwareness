using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageUploadController : ControllerBase
    {
        private readonly ILogger<ImageUploadController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ImageUploadController(ILogger<ImageUploadController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Upload images for missing person cases
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImages(IFormFileCollection files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { success = false, message = "No files provided" });
                }

                var uploadedFiles = new List<object>();
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "images");

                // Create uploads directory if it doesn't exist
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                foreach (var file in files)
                {
                    if (file.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        return BadRequest(new { success = false, message = $"File {file.FileName} is too large. Maximum size is 5MB." });
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { success = false, message = $"File {file.FileName} has an unsupported format. Allowed formats: JPG, PNG, GIF, WebP." });
                    }

                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    uploadedFiles.Add(new
                    {
                        originalName = file.FileName,
                        fileName = fileName,
                        url = $"/uploads/images/{fileName}",
                        size = file.Length
                    });
                }

                _logger.LogInformation($"Successfully uploaded {uploadedFiles.Count} images");

                return Ok(new
                {
                    success = true,
                    message = $"Successfully uploaded {uploadedFiles.Count} image(s)",
                    files = uploadedFiles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading images");
                return StatusCode(500, new { success = false, message = "Internal server error during upload" });
            }
        }

        /// <summary>
        /// Get uploaded image by filename
        /// </summary>
        [HttpGet("{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "images");
                var filePath = Path.Combine(uploadsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { success = false, message = "Image not found" });
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(fileName);

                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image {FileName}", fileName);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
