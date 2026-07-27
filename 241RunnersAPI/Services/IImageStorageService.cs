namespace _241RunnersAPI.Services
{
    public interface IBlobImageStorageService
    {
        Task<string?> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default);
    }

    public sealed class ImageUploadResult
    {
        public string FileName { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public long Size { get; init; }
        public string OriginalName { get; init; } = string.Empty;
    }

    public interface IImageStorageService
    {
        Task<ImageUploadResult?> UploadAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task<Stream?> OpenReadAsync(string fileName, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken = default);
        string GetServeUrl(string fileName);
        Task<string?> GetSignedUrlAsync(string fileName, TimeSpan expiry, CancellationToken cancellationToken = default);
    }
}
