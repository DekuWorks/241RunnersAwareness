using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace _241RunnersAPI.Services
{
    public interface IBlobImageStorageService
    {
        Task<string?> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Uploads images to the shared Azure Blob "images" container (same as ImageUploadController).
    /// </summary>
    public class BlobImageStorageService : IBlobImageStorageService
    {
        private const string ContainerName = "images";
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BlobImageStorageService> _logger;

        public BlobImageStorageService(
            BlobServiceClient blobServiceClient,
            ILogger<BlobImageStorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task<string?> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file.Length == 0)
            {
                return null;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    $"Photo {file.FileName} has an unsupported format. Allowed formats: JPG, JPEG, PNG, GIF, WEBP.");
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = GetContentType(extension) },
                },
                cancellationToken);

            _logger.LogInformation("Uploaded image to blob storage: {BlobUrl}", blobClient.Uri);
            return blobClient.Uri.ToString();
        }

        private static string GetContentType(string extension) =>
            extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream",
            };
    }
}
