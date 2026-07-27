using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace _241RunnersAPI.Services
{
    public class AzureBlobImageStorageService : IImageStorageService
    {
        private const string ContainerName = "images";
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AzureBlobImageStorageService> _logger;
        private readonly string _publicApiBaseUrl;

        public AzureBlobImageStorageService(
            BlobServiceClient blobServiceClient,
            IConfiguration configuration,
            ILogger<AzureBlobImageStorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
            _publicApiBaseUrl = (Environment.GetEnvironmentVariable("PUBLIC_API_BASE_URL")
                ?? configuration["PublicApi:BaseUrl"]
                ?? string.Empty).TrimEnd('/');
        }

        public async Task<ImageUploadResult?> UploadAsync(
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            if (file.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = GetContentType(extension) },
                },
                cancellationToken);

            _logger.LogInformation("Uploaded image to Azure blob storage: {BlobUrl}", blobClient.Uri);
            return new ImageUploadResult
            {
                FileName = fileName,
                Url = blobClient.Uri.ToString(),
                Size = file.Length,
                OriginalName = file.FileName
            };
        }

        public async Task<Stream?> OpenReadAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                return null;
            }

            return await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
        }

        public async Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            return await containerClient.GetBlobClient(fileName).ExistsAsync(cancellationToken);
        }

        public string GetServeUrl(string fileName)
        {
            if (!string.IsNullOrEmpty(_publicApiBaseUrl))
            {
                return $"{_publicApiBaseUrl}/api/ImageUpload/{Uri.EscapeDataString(fileName)}";
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            return containerClient.GetBlobClient(fileName).Uri.ToString();
        }

        public Task<string?> GetSignedUrlAsync(
            string fileName,
            TimeSpan expiry,
            CancellationToken cancellationToken = default)
        {
            // SAS generation remains in ImageUploadController for Azure legacy path.
            return Task.FromResult<string?>(GetServeUrl(fileName));
        }

        private static string GetContentType(string extension) =>
            extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
    }

    /// <summary>
    /// Legacy name retained for runner controller injection.
    /// </summary>
    public class BlobImageStorageService : ImageStorageBlobAdapter
    {
        public BlobImageStorageService(IImageStorageService storage) : base(storage)
        {
        }
    }
}
