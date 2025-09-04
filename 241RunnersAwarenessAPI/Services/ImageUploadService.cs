using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic; // Added missing import for List

namespace _241RunnersAwarenessAPI.Services
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(IFormFile image, string category, int? relatedId = null);
        Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> images, string category, int? relatedId = null);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<bool> ValidateImageAsync(IFormFile image);
    }

    public class ImageUploadService : IImageUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImageUploadService> _logger;
        private readonly string _uploadPath;
        private readonly string _baseUrl;

        public ImageUploadService(IConfiguration configuration, ILogger<ImageUploadService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _uploadPath = _configuration["ImageUpload:Path"] ?? "wwwroot/uploads";
            _baseUrl = _configuration["ImageUpload:BaseUrl"] ?? "/uploads";
            
            // Ensure upload directory exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> UploadImageAsync(IFormFile image, string category, int? relatedId = null)
        {
            try
            {
                if (!await ValidateImageAsync(image))
                {
                    throw new InvalidOperationException("Invalid image file");
                }

                // Create category directory
                var categoryPath = Path.Combine(_uploadPath, category);
                if (!Directory.Exists(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                }

                // Generate unique filename
                var fileName = GenerateUniqueFileName(image.FileName);
                var filePath = Path.Combine(categoryPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Generate URL
                var imageUrl = $"{_baseUrl}/{category}/{fileName}";
                
                _logger.LogInformation("Image uploaded successfully: {ImageUrl}", imageUrl);
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image: {FileName}", image.FileName);
                throw;
            }
        }

        public async Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> images, string category, int? relatedId = null)
        {
            var uploadedUrls = new List<string>();
            
            foreach (var image in images)
            {
                try
                {
                    var url = await UploadImageAsync(image, category, relatedId);
                    uploadedUrls.Add(url);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image: {FileName}", image.FileName);
                    // Continue with other images
                }
            }

            return uploadedUrls;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                // Extract file path from URL
                var relativePath = imageUrl.Replace(_baseUrl, "").TrimStart('/');
                var fullPath = Path.Combine(_uploadPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Image deleted successfully: {ImageUrl}", imageUrl);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
                return false;
            }
        }

        public async Task<bool> ValidateImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return false;

            // Check file size (max 10MB)
            if (image.Length > 10 * 1024 * 1024)
                return false;

            // Check file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
                return false;

            // Check MIME type
            var allowedMimeTypes = new[] { 
                "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" 
            };
            
            if (!allowedMimeTypes.Contains(image.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var baseName = Path.GetFileNameWithoutExtension(originalFileName);
            
            // Generate hash for uniqueness
            var hash = GenerateHash($"{baseName}{DateTime.UtcNow:yyyyMMddHHmmss}");
            var uniqueName = $"{baseName}_{hash}{extension}";
            
            return uniqueName;
        }

        private string GenerateHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hash).Substring(0, 8).ToLowerInvariant();
            }
        }
    }
} 