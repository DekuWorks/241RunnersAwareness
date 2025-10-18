using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Azure.Storage;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageUploadController : ControllerBase
    {
        private readonly ILogger<ImageUploadController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;

        public ImageUploadController(ILogger<ImageUploadController> logger, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _logger = logger;
            _environment = environment;
            _configuration = configuration;
            
            // Initialize Blob Service Client
            var connectionString = _configuration.GetConnectionString("AzureStorageConnectionString") ?? 
                                 _configuration["AzureStorageConnectionString"] ?? string.Empty;
            
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Azure Storage connection string is not configured");
                throw new InvalidOperationException("Azure Storage connection string is not configured");
            }
            
            _logger.LogInformation("Azure Storage connection string found: {ConnectionString}", 
                connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "...");
            
            try
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
                _logger.LogInformation("BlobServiceClient initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize BlobServiceClient with connection string: {ConnectionString}", 
                    connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "...");
                throw;
            }
        }

        /// <summary>
        /// Upload images for missing person cases with enhanced security validation
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50MB total request size limit
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)] // 50MB multipart limit
        public async Task<IActionResult> UploadImages(IFormFileCollection files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { success = false, message = "No files provided" });
                }

                var uploadedFiles = new List<object>();
                
                // Get or create blob container
                var containerName = "images";
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                
                try
                {
                    await containerClient.CreateIfNotExistsAsync();
                    _logger.LogInformation("Ensured blob container exists: {ContainerName}", containerName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create blob container: {ContainerName}. Error: {ErrorMessage}", containerName, ex.Message);
                    return StatusCode(500, new { success = false, message = $"Failed to initialize storage: {ex.Message}" });
                }

                // Enhanced security validation
                if (files.Count > 10) // Maximum 10 files per request
                {
                    return BadRequest(new { success = false, message = "Maximum 10 files allowed per request." });
                }

                foreach (var file in files)
                {
                    // File size validation (5MB per file)
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { success = false, message = $"File {file.FileName} is too large. Maximum size is 5MB." });
                    }

                    // File name sanitization
                    var sanitizedFileName = SanitizeFileName(file.FileName);
                    if (string.IsNullOrEmpty(sanitizedFileName))
                    {
                        return BadRequest(new { success = false, message = $"File {file.FileName} has an invalid name." });
                    }

                    // File extension validation
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(sanitizedFileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { success = false, message = $"File {file.FileName} has an unsupported format. Allowed formats: JPG, PNG, GIF, WebP." });
                    }

                    // File content validation (MIME type and magic bytes)
                    if (!ValidateFileContent(file))
                    {
                        return BadRequest(new { success = false, message = $"File {file.FileName} appears to be corrupted or invalid." });
                    }

                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var blobClient = containerClient.GetBlobClient(fileName);

                    // Upload to Azure Blob Storage
                    using var stream = file.OpenReadStream();
                    var blobHttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = GetContentType(fileName)
                    };

                    await blobClient.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = blobHttpHeaders
                    });

                    uploadedFiles.Add(new
                    {
                        originalName = file.FileName,
                        fileName = fileName,
                        url = blobClient.Uri.ToString(),
                        size = file.Length
                    });
                }

                _logger.LogInformation($"Successfully uploaded {uploadedFiles.Count} images to Azure Blob Storage");

                return Ok(new
                {
                    success = true,
                    message = $"Successfully uploaded {uploadedFiles.Count} image(s)",
                    files = uploadedFiles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading images to Azure Blob Storage");
                return StatusCode(500, new { success = false, message = "Internal server error during upload" });
            }
        }

        /// <summary>
        /// Get a secure SAS token for accessing an image
        /// </summary>
        [HttpGet("sas-token/{fileName}")]
        public async Task<IActionResult> GetImageSasToken(string fileName)
        {
            try
            {
                var containerName = "images";
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                // Check if blob exists
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    return NotFound(new { success = false, message = "Image not found" });
                }

                // Generate SAS token with read permissions, valid for 1 hour
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = fileName,
                    Resource = "b", // blob
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5) // Allow 5 minutes clock skew
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Get the storage account key for signing
                var connectionString = _configuration.GetConnectionString("AzureStorageConnectionString") ?? 
                                     _configuration["AzureStorageConnectionString"] ?? string.Empty;
                
                _logger.LogInformation("Generating SAS token for blob: {BlobName}", fileName);
                
                // Parse connection string to get account name and key
                var connectionStringParts = connectionString.Split(';');
                var accountNamePart = connectionStringParts.First(p => p.StartsWith("AccountName="));
                var accountKeyPart = connectionStringParts.First(p => p.StartsWith("AccountKey="));
                
                var accountName = accountNamePart.Substring(accountNamePart.IndexOf('=') + 1);
                var accountKey = accountKeyPart.Substring(accountKeyPart.IndexOf('=') + 1);
                
                _logger.LogInformation("Using account name: {AccountName}", accountName);
                
                var credential = new StorageSharedKeyCredential(accountName, accountKey);
                var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();

                var sasUrl = $"{blobClient.Uri}?{sasToken}";

                return Ok(new
                {
                    success = true,
                    sasUrl = sasUrl,
                    expiresAt = DateTimeOffset.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SAS token for image {FileName}: {ErrorMessage}", fileName, ex.Message);
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get uploaded image by filename from Azure Blob Storage
        /// </summary>
        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetImage(string fileName)
        {
            try
            {
                var containerName = "images";
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                // Check if blob exists
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    return NotFound(new { success = false, message = "Image not found" });
                }

                // Get blob properties to determine content type
                var properties = await blobClient.GetPropertiesAsync();
                var contentType = properties.Value.ContentType ?? GetContentType(fileName);

                // Download blob content and return it directly
                var blobStream = await blobClient.OpenReadAsync();
                return File(blobStream, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image {FileName} from Azure Blob Storage", fileName);
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

        /// <summary>
        /// Sanitizes file name to prevent path traversal and malicious characters
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            // Remove path traversal attempts
            fileName = fileName.Replace("..", "").Replace("/", "").Replace("\\", "");
            
            // Remove dangerous characters
            var invalidChars = Path.GetInvalidFileNameChars();
            fileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            
            // Limit length
            if (fileName.Length > 255)
                fileName = fileName.Substring(0, 255);
            
            // Ensure it's not empty after sanitization
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "sanitized_file";
            
            return fileName;
        }

        /// <summary>
        /// Validates file content by checking MIME type and magic bytes
        /// </summary>
        private bool ValidateFileContent(IFormFile file)
        {
            try
            {
                // Check MIME type
                var allowedMimeTypes = new[]
                {
                    "image/jpeg",
                    "image/png", 
                    "image/gif",
                    "image/webp"
                };

                if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    _logger.LogWarning("Invalid MIME type for file {FileName}: {ContentType}", file.FileName, file.ContentType);
                    return false;
                }

                // Read first few bytes to check magic bytes
                using var stream = file.OpenReadStream();
                var buffer = new byte[12];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead < 4)
                {
                    _logger.LogWarning("File {FileName} is too small to be a valid image", file.FileName);
                    return false;
                }

                // Check magic bytes for common image formats
                var magicBytes = Convert.ToHexString(buffer, 0, Math.Min(bytesRead, 8));
                
                return magicBytes switch
                {
                    // JPEG: FF D8 FF
                    var mb when mb.StartsWith("FFD8FF") => true,
                    // PNG: 89 50 4E 47 0D 0A 1A 0A
                    var mb when mb.StartsWith("89504E470D0A1A0A") => true,
                    // GIF: 47 49 46 38 (GIF8)
                    var mb when mb.StartsWith("47494638") => true,
                    // WebP: 52 49 46 46 (RIFF)
                    var mb when mb.StartsWith("52494646") => true,
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file content for {FileName}", file.FileName);
                return false;
            }
        }

        /// <summary>
        /// Validates file size and calculates hash for duplicate detection
        /// </summary>
        private async Task<string> CalculateFileHash(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexString(hashBytes);
        }
    }
}
