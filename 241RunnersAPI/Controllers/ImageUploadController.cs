using Microsoft.AspNetCore.Mvc;
using _241RunnersAPI.Services;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageUploadController : ControllerBase
    {
        private readonly ILogger<ImageUploadController> _logger;
        private readonly IImageStorageService _imageStorage;

        public ImageUploadController(
            ILogger<ImageUploadController> logger,
            IImageStorageService imageStorage)
        {
            _logger = logger;
            _imageStorage = imageStorage;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(50 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        public async Task<IActionResult> UploadImages(IFormFileCollection files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { success = false, message = "No files provided" });
                }

                if (files.Count > 10)
                {
                    return BadRequest(new { success = false, message = "Maximum 10 files allowed per request." });
                }

                var uploadedFiles = new List<object>();

                foreach (var file in files)
                {
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { success = false, message = $"File {file.FileName} is too large. Maximum size is 5MB." });
                    }

                    var sanitizedFileName = SanitizeFileName(file.FileName);
                    if (string.IsNullOrEmpty(sanitizedFileName))
                    {
                        return BadRequest(new { success = false, message = $"File {file.FileName} has an invalid name." });
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(sanitizedFileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { success = false, message = $"File {file.FileName} has an unsupported format. Allowed formats: JPG, PNG, GIF, WebP." });
                    }

                    if (!ValidateFileContent(file))
                    {
                        return BadRequest(new { success = false, message = $"File {file.FileName} appears to be corrupted or invalid." });
                    }

                    var result = await _imageStorage.UploadAsync(file);
                    if (result == null)
                    {
                        continue;
                    }

                    uploadedFiles.Add(new
                    {
                        originalName = result.OriginalName,
                        fileName = result.FileName,
                        url = result.Url,
                        size = result.Size
                    });
                }

                _logger.LogInformation("Successfully uploaded {Count} images", uploadedFiles.Count);
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

        [HttpGet("sas-token/{fileName}")]
        public async Task<IActionResult> GetImageSasToken(string fileName)
        {
            try
            {
                if (!await _imageStorage.ExistsAsync(fileName))
                {
                    return NotFound(new { success = false, message = "Image not found" });
                }

                var signedUrl = await _imageStorage.GetSignedUrlAsync(fileName, TimeSpan.FromHours(1));
                if (string.IsNullOrEmpty(signedUrl))
                {
                    signedUrl = _imageStorage.GetServeUrl(fileName);
                }

                return Ok(new
                {
                    success = true,
                    sasUrl = signedUrl,
                    expiresAt = DateTimeOffset.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating signed URL for image {FileName}", fileName);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetImage(string fileName)
        {
            try
            {
                var stream = await _imageStorage.OpenReadAsync(fileName);
                if (stream == null)
                {
                    return NotFound(new { success = false, message = "Image not found" });
                }

                return File(stream, GetContentType(fileName), fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image {FileName}", fileName);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        private static string GetContentType(string fileName)
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

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return string.Empty;
            }

            fileName = fileName.Replace("..", "").Replace("/", "").Replace("\\", "");
            var invalidChars = Path.GetInvalidFileNameChars();
            fileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            if (fileName.Length > 255)
            {
                fileName = fileName[..255];
            }

            return string.IsNullOrWhiteSpace(fileName) ? "sanitized_file" : fileName;
        }

        private bool ValidateFileContent(IFormFile file)
        {
            try
            {
                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    return false;
                }

                using var stream = file.OpenReadStream();
                var buffer = new byte[12];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead < 4)
                {
                    return false;
                }

                var magicBytes = Convert.ToHexString(buffer, 0, Math.Min(bytesRead, 8));
                return magicBytes.StartsWith("FFD8FF")
                    || magicBytes.StartsWith("89504E470D0A1A0A")
                    || magicBytes.StartsWith("47494638")
                    || magicBytes.StartsWith("52494646");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file content for {FileName}", file.FileName);
                return false;
            }
        }
    }
}
