namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Adapts unified image storage for runner photo uploads.
    /// </summary>
    public class ImageStorageBlobAdapter : IBlobImageStorageService
    {
        private readonly IImageStorageService _storage;

        public ImageStorageBlobAdapter(IImageStorageService storage)
        {
            _storage = storage;
        }

        public async Task<string?> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            var result = await _storage.UploadAsync(file, cancellationToken);
            return result?.Url;
        }
    }
}
