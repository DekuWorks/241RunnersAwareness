using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace _241RunnersAwareness.BackendAPI.Services
{
    public interface IImageService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file, string caseId);
        Task<ImageUploadResult> UploadImageFromBase64Async(string base64Data, string caseId);
        Task<byte[]> GetImageAsync(string imageId);
        Task<byte[]> GetThumbnailAsync(string imageId);
        Task<bool> DeleteImageAsync(string imageId);
        Task<ImageMetadata> GetImageMetadataAsync(string imageId);
        Task<byte[]> CompressImageAsync(byte[] imageData, int quality = 80);
        Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height);
        Task<string> GenerateImageIdAsync();
    }

    public class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _uploadPath;
        private readonly string _thumbnailPath;
        private readonly int _maxFileSize;
        private readonly string[] _allowedExtensions;

        public ImageService(ILogger<ImageService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _uploadPath = _configuration["ImageStorage:UploadPath"] ?? "wwwroot/uploads";
            _thumbnailPath = _configuration["ImageStorage:ThumbnailPath"] ?? "wwwroot/thumbnails";
            _maxFileSize = int.Parse(_configuration["ImageStorage:MaxFileSize"] ?? "10485760"); // 10MB
            _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            // Ensure directories exist
            Directory.CreateDirectory(_uploadPath);
            Directory.CreateDirectory(_thumbnailPath);
        }

        /// <summary>
        /// Uploads an image file with validation and processing
        /// </summary>
        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string caseId)
        {
            try
            {
                // Validate file
                var validationResult = await ValidateImageFileAsync(file);
                if (!validationResult.IsValid)
                {
                    return new ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = validationResult.ErrorMessage
                    };
                }

                // Generate unique image ID
                var imageId = await GenerateImageIdAsync();
                var fileName = $"{imageId}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(_uploadPath, fileName);
                var thumbnailPath = Path.Combine(_thumbnailPath, fileName);

                // Save original file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create thumbnail
                await CreateThumbnailAsync(filePath, thumbnailPath);

                // Get metadata
                var metadata = await GetImageMetadataFromFileAsync(filePath);

                _logger.LogInformation($"Image uploaded successfully: {imageId} for case {caseId}");

                return new ImageUploadResult
                {
                    Success = true,
                    ImageId = imageId,
                    FileName = fileName,
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    Metadata = metadata
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to upload image for case {caseId}");
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = "Failed to upload image"
                };
            }
        }

        /// <summary>
        /// Uploads an image from base64 data
        /// </summary>
        public async Task<ImageUploadResult> UploadImageFromBase64Async(string base64Data, string caseId)
        {
            try
            {
                // Remove data URL prefix if present
                var base64String = base64Data;
                if (base64Data.Contains(","))
                {
                    base64String = base64Data.Split(',')[1];
                }

                // Decode base64
                var imageBytes = Convert.FromBase64String(base64String);

                // Validate file size
                if (imageBytes.Length > _maxFileSize)
                {
                    return new ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = $"File size exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB"
                    };
                }

                // Generate unique image ID
                var imageId = await GenerateImageIdAsync();
                var fileName = $"{imageId}.jpg"; // Default to jpg for base64
                var filePath = Path.Combine(_uploadPath, fileName);
                var thumbnailPath = Path.Combine(_thumbnailPath, fileName);

                // Save file
                await File.WriteAllBytesAsync(filePath, imageBytes);

                // Create thumbnail
                await CreateThumbnailAsync(filePath, thumbnailPath);

                // Get metadata
                var metadata = await GetImageMetadataFromFileAsync(filePath);

                _logger.LogInformation($"Base64 image uploaded successfully: {imageId} for case {caseId}");

                return new ImageUploadResult
                {
                    Success = true,
                    ImageId = imageId,
                    FileName = fileName,
                    FileSize = imageBytes.Length,
                    ContentType = "image/jpeg",
                    Metadata = metadata
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to upload base64 image for case {caseId}");
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = "Failed to upload image"
                };
            }
        }

        /// <summary>
        /// Retrieves an image by ID
        /// </summary>
        public async Task<byte[]> GetImageAsync(string imageId)
        {
            try
            {
                var filePath = Path.Combine(_uploadPath, $"{imageId}.jpg");
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(_uploadPath, $"{imageId}.png");
                }

                if (File.Exists(filePath))
                {
                    return await File.ReadAllBytesAsync(filePath);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get image {imageId}");
                return null;
            }
        }

        /// <summary>
        /// Retrieves a thumbnail by ID
        /// </summary>
        public async Task<byte[]> GetThumbnailAsync(string imageId)
        {
            try
            {
                var filePath = Path.Combine(_thumbnailPath, $"{imageId}.jpg");
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(_thumbnailPath, $"{imageId}.png");
                }

                if (File.Exists(filePath))
                {
                    return await File.ReadAllBytesAsync(filePath);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get thumbnail {imageId}");
                return null;
            }
        }

        /// <summary>
        /// Deletes an image by ID
        /// </summary>
        public async Task<bool> DeleteImageAsync(string imageId)
        {
            try
            {
                var imagePath = Path.Combine(_uploadPath, $"{imageId}.jpg");
                var thumbnailPath = Path.Combine(_thumbnailPath, $"{imageId}.jpg");

                if (!File.Exists(imagePath))
                {
                    imagePath = Path.Combine(_uploadPath, $"{imageId}.png");
                    thumbnailPath = Path.Combine(_thumbnailPath, $"{imageId}.png");
                }

                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }

                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }

                _logger.LogInformation($"Image deleted successfully: {imageId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete image {imageId}");
                return false;
            }
        }

        /// <summary>
        /// Gets image metadata
        /// </summary>
        public async Task<ImageMetadata> GetImageMetadataAsync(string imageId)
        {
            try
            {
                var filePath = Path.Combine(_uploadPath, $"{imageId}.jpg");
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(_uploadPath, $"{imageId}.png");
                }

                if (File.Exists(filePath))
                {
                    return await GetImageMetadataFromFileAsync(filePath);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get metadata for image {imageId}");
                return null;
            }
        }

        /// <summary>
        /// Compresses an image
        /// </summary>
        public async Task<byte[]> CompressImageAsync(byte[] imageData, int quality = 80)
        {
            try
            {
                using var image = Image.Load(imageData);
                using var outputStream = new MemoryStream();
                
                var encoder = new JpegEncoder
                {
                    Quality = quality
                };

                await image.SaveAsync(outputStream, encoder);
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compress image");
                return imageData; // Return original if compression fails
            }
        }

        /// <summary>
        /// Resizes an image
        /// </summary>
        public async Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height)
        {
            try
            {
                using var image = Image.Load(imageData);
                using var outputStream = new MemoryStream();

                image.Mutate(x => x.Resize(width, height));
                await image.SaveAsync(outputStream, new JpegEncoder());

                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resize image");
                return imageData; // Return original if resize fails
            }
        }

        /// <summary>
        /// Generates a unique image ID
        /// </summary>
        public async Task<string> GenerateImageIdAsync()
        {
            return await Task.FromResult(Guid.NewGuid().ToString());
        }

        #region Private Methods

        private async Task<ImageValidationResult> ValidateImageFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new ImageValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "No file provided"
                };
            }

            if (file.Length > _maxFileSize)
            {
                return new ImageValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File size exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB"
                };
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Array.Exists(_allowedExtensions, ext => ext.Equals(extension)))
            {
                return new ImageValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}"
                };
            }

            // Validate image format
            try
            {
                using var stream = file.OpenReadStream();
                using var image = await Image.LoadAsync(stream);
                
                return new ImageValidationResult
                {
                    IsValid = true
                };
            }
            catch
            {
                return new ImageValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid image format"
                };
            }
        }

        private async Task CreateThumbnailAsync(string sourcePath, string thumbnailPath)
        {
            try
            {
                using var image = await Image.LoadAsync(sourcePath);
                using var outputStream = new FileStream(thumbnailPath, FileMode.Create);

                // Resize to thumbnail size (200x200)
                image.Mutate(x => x.Resize(200, 200));
                await image.SaveAsync(outputStream, new JpegEncoder { Quality = 80 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create thumbnail");
            }
        }

        private async Task<ImageMetadata> GetImageMetadataFromFileAsync(string filePath)
        {
            try
            {
                using var image = await Image.LoadAsync(filePath);
                var fileInfo = new FileInfo(filePath);

                return new ImageMetadata
                {
                    Width = image.Width,
                    Height = image.Height,
                    FileSize = fileInfo.Length,
                    Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown",
                    CreatedAt = fileInfo.CreationTimeUtc
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get image metadata");
                return null;
            }
        }

        #endregion
    }

    #region Data Models

    public class ImageUploadResult
    {
        public bool Success { get; set; }
        public string ImageId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public ImageMetadata Metadata { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ImageValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ImageMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public long FileSize { get; set; }
        public string Format { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion
} 