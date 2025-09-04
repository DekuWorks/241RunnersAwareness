using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using _241RunnersAwarenessAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace _241RunnersAwarenessAPI.Services
{
    public interface IImageUploadService
    {
        Task<ImageUploadResponse> UploadImageAsync(ImageUploadRequest request, string uploadedBy);
        Task<MultipleImageUploadResponse> UploadMultipleImagesAsync(MultipleImageUploadRequest request, string uploadedBy);
        Task<bool> DeleteImageAsync(string imageUrl, string deletedBy);
        Task<bool> ValidateImageAsync(IFormFile image);
        Task<ImageMetadata> ExtractImageMetadataAsync(IFormFile image, string uploadedBy, string category, int? relatedId = null);
        Task<ImageSearchResponse> SearchImagesAsync(ImageSearchRequest request, string userId);
        Task<bool> IsImageAccessibleAsync(string imageUrl, string userId);
    }

    public class ImageUploadService : IImageUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImageUploadService> _logger;
        private readonly string _uploadPath;
        private readonly string _baseUrl;
        private readonly int _maxFileSize;
        private readonly string[] _allowedExtensions;
        private readonly string[] _allowedMimeTypes;
        private readonly int _maxImageDimension;
        private readonly bool _enableImageResizing;
        private readonly bool _enableWatermarking;

        public ImageUploadService(IConfiguration configuration, ILogger<ImageUploadService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _uploadPath = _configuration["ImageUpload:Path"] ?? "wwwroot/uploads";
            _baseUrl = _configuration["ImageUpload:BaseUrl"] ?? "/uploads";
            _maxFileSize = _configuration.GetValue<int>("ImageUpload:MaxFileSize", 10 * 1024 * 1024); // 10MB default
            _allowedExtensions = _configuration.GetSection("ImageUpload:AllowedExtensions").Get<string[]>() ?? 
                new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            _allowedMimeTypes = _configuration.GetSection("ImageUpload:AllowedMimeTypes").Get<string[]>() ?? 
                new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" };
            _maxImageDimension = _configuration.GetValue<int>("ImageUpload:MaxImageDimension", 4096); // 4K max
            _enableImageResizing = _configuration.GetValue<bool>("ImageUpload:EnableImageResizing", false); // Disabled for cross-platform
            _enableWatermarking = _configuration.GetValue<bool>("ImageUpload:EnableWatermarking", false); // Disabled for cross-platform
            
            // Ensure upload directory exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<ImageUploadResponse> UploadImageAsync(ImageUploadRequest request, string uploadedBy)
        {
            try
            {
                // Validate request
                if (!ValidateImageAsync(request.Image).Result)
                {
                    return new ImageUploadResponse
                    {
                        Success = false,
                        Error = "Invalid image file. Please ensure it's a valid image under the size limit."
                    };
                }

                // Extract metadata
                var metadata = await ExtractImageMetadataAsync(request.Image, uploadedBy, request.Category, request.RelatedId);
                
                // Create category directory
                var categoryPath = Path.Combine(_uploadPath, request.Category);
                if (!Directory.Exists(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                }

                // Generate unique filename and ID
                var imageId = GenerateImageId();
                var fileName = GenerateUniqueFileName(request.Image.FileName, imageId);
                var filePath = Path.Combine(categoryPath, fileName);

                // Save file directly (no image processing for cross-platform compatibility)
                await SaveFileAsync(request.Image, filePath);

                // Generate URL
                var imageUrl = $"{_baseUrl}/{request.Category}/{fileName}";
                
                // Update metadata with final URL
                metadata.ImageId = imageId;
                metadata.ImageUrl = imageUrl;
                
                _logger.LogInformation("Image uploaded successfully: {ImageUrl} by {UploadedBy}", imageUrl, uploadedBy);
                
                return new ImageUploadResponse
                {
                    Success = true,
                    ImageUrl = imageUrl,
                    ImageId = imageId,
                    Message = "Image uploaded successfully",
                    Metadata = metadata
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image: {FileName}", request.Image.FileName);
                return new ImageUploadResponse
                {
                    Success = false,
                    Error = "An error occurred while uploading the image"
                };
            }
        }

        public async Task<MultipleImageUploadResponse> UploadMultipleImagesAsync(MultipleImageUploadRequest request, string uploadedBy)
        {
            var response = new MultipleImageUploadResponse();
            var uploadedUrls = new List<string>();
            var uploadedIds = new List<string>();
            var metadataList = new List<ImageMetadata>();
            var failedCount = 0;
            
            foreach (var image in request.Images)
            {
                try
                {
                    var singleRequest = new ImageUploadRequest
                    {
                        Image = image,
                        Category = request.Category,
                        RelatedId = request.RelatedId,
                        Description = request.Description,
                        Tags = request.Tags,
                        IsPublic = request.IsPublic
                    };
                    
                    var result = await UploadImageAsync(singleRequest, uploadedBy);
                    
                    if (result.Success)
                    {
                        uploadedUrls.Add(result.ImageUrl!);
                        uploadedIds.Add(result.ImageId!);
                        if (result.Metadata != null)
                            metadataList.Add(result.Metadata);
                    }
                    else
                    {
                        failedCount++;
                        _logger.LogWarning("Failed to upload image {FileName}: {Error}", image.FileName, result.Error);
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    _logger.LogError(ex, "Error uploading image: {FileName}", image.FileName);
                }
            }

            response.Success = uploadedUrls.Count > 0;
            response.ImageUrls = uploadedUrls;
            response.ImageIds = uploadedIds;
            response.Metadata = metadataList;
            response.TotalUploaded = uploadedUrls.Count;
            response.TotalFailed = failedCount;
            response.Message = $"Successfully uploaded {uploadedUrls.Count} images. {failedCount} failed.";
            
            return response;
        }

        public Task<bool> DeleteImageAsync(string imageUrl, string deletedBy)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return Task.FromResult(false);

                // Extract file path from URL
                var relativePath = imageUrl.Replace(_baseUrl, "").TrimStart('/');
                var fullPath = Path.Combine(_uploadPath, relativePath);

                if (File.Exists(fullPath))
                {
                    // Log deletion for audit
                    _logger.LogInformation("Image deleted by {DeletedBy}: {ImageUrl}", deletedBy, imageUrl);
                    
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
                return Task.FromResult(false);
            }
        }

        public Task<bool> ValidateImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return Task.FromResult(false);

            // Check file size
            if (image.Length > _maxFileSize)
            {
                _logger.LogWarning("Image file size exceeds limit: {FileName} ({Size} bytes)", 
                    image.FileName, image.Length);
                return Task.FromResult(false);
            }

            // Check file extension
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid file extension: {FileName} ({Extension})", 
                    image.FileName, extension);
                return Task.FromResult(false);
            }

            // Check MIME type
            if (!_allowedMimeTypes.Contains(image.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("Invalid MIME type: {FileName} ({MimeType})", 
                    image.FileName, image.ContentType);
                return Task.FromResult(false);
            }

            // Additional security checks
            if (image.FileName.Contains("..") || image.FileName.Contains("/") || image.FileName.Contains("\\"))
            {
                _logger.LogWarning("Suspicious filename detected: {FileName}", image.FileName);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public async Task<ImageMetadata> ExtractImageMetadataAsync(IFormFile image, string uploadedBy, string category, int? relatedId = null)
        {
            var metadata = new ImageMetadata
            {
                OriginalFileName = image.FileName,
                FileExtension = Path.GetExtension(image.FileName).ToLowerInvariant(),
                MimeType = image.ContentType,
                FileSizeBytes = image.Length,
                Category = category,
                UploadedBy = uploadedBy,
                UploadedAt = DateTime.UtcNow,
                Checksum = await CalculateFileChecksumAsync(image)
            };

            // For cross-platform compatibility, we'll set default dimensions
            // In a production environment, you might want to use a cross-platform image library like ImageSharp
            metadata.Width = 0; // Will be extracted if image processing is enabled
            metadata.Height = 0; // Will be extracted if image processing is enabled

            return metadata;
        }

        public Task<ImageSearchResponse> SearchImagesAsync(ImageSearchRequest request, string userId)
        {
            try
            {
                var response = new ImageSearchResponse();
                
                // This would typically query a database for image metadata
                // For now, we'll return a basic response
                response.Success = true;
                response.Message = "Image search completed";
                
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching images for user: {UserId}", userId);
                return Task.FromResult(new ImageSearchResponse
                {
                    Success = false,
                    Error = "An error occurred while searching images"
                });
            }
        }

        public Task<bool> IsImageAccessibleAsync(string imageUrl, string userId)
        {
            try
            {
                // This would typically check database permissions
                // For now, we'll assume all images are accessible to authenticated users
                return Task.FromResult(!string.IsNullOrEmpty(userId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking image accessibility: {ImageUrl} for user: {UserId}", imageUrl, userId);
                return Task.FromResult(false);
            }
        }

        private async Task SaveFileAsync(IFormFile file, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        private string GenerateImageId()
        {
            return $"IMG_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
        }

        private string GenerateUniqueFileName(string originalFileName, string imageId)
        {
            var extension = Path.GetExtension(originalFileName);
            var baseName = Path.GetFileNameWithoutExtension(originalFileName);
            
            // Sanitize base name
            var sanitizedName = SanitizeFileName(baseName);
            
            return $"{sanitizedName}_{imageId}{extension}";
        }

        private string SanitizeFileName(string fileName)
        {
            // Remove or replace invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = fileName;
            
            foreach (var invalidChar in invalidChars)
            {
                sanitized = sanitized.Replace(invalidChar, '_');
            }
            
            // Limit length
            if (sanitized.Length > 50)
            {
                sanitized = sanitized[..50];
            }
            
            return sanitized;
        }

        private async Task<string> CalculateFileChecksumAsync(IFormFile file)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = file.OpenReadStream())
            {
                var hash = await sha256.ComputeHashAsync(stream);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }
    }
} 