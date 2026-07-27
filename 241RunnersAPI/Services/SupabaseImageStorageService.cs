using System.Net.Http.Headers;
using System.Text.Json;

namespace _241RunnersAPI.Services
{
    public class SupabaseImageStorageService : IImageStorageService
    {
        private const string Bucket = "images";
        private readonly HttpClient _http;
        private readonly ILogger<SupabaseImageStorageService> _logger;
        private readonly string _supabaseUrl;
        private readonly string _serviceRoleKey;
        private readonly string _publicApiBaseUrl;

        public SupabaseImageStorageService(
            HttpClient http,
            IConfiguration configuration,
            ILogger<SupabaseImageStorageService> logger)
        {
            _http = http;
            _logger = logger;
            _supabaseUrl = (Environment.GetEnvironmentVariable("SUPABASE_URL")
                ?? configuration["Supabase:Url"]
                ?? string.Empty).TrimEnd('/');
            _serviceRoleKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY")
                ?? configuration["Supabase:ServiceRoleKey"]
                ?? string.Empty;
            _publicApiBaseUrl = (Environment.GetEnvironmentVariable("PUBLIC_API_BASE_URL")
                ?? configuration["PublicApi:BaseUrl"]
                ?? "http://localhost:5051").TrimEnd('/');

            if (string.IsNullOrWhiteSpace(_supabaseUrl) || string.IsNullOrWhiteSpace(_serviceRoleKey))
            {
                throw new InvalidOperationException(
                    "Supabase storage requires SUPABASE_URL and SUPABASE_SERVICE_ROLE_KEY.");
            }
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
            var contentType = GetContentType(extension);

            using var stream = file.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_supabaseUrl}/storage/v1/object/{Bucket}/{fileName}")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);
            request.Headers.Add("x-upsert", "false");

            var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Supabase upload failed ({Status}): {Body}", response.StatusCode, body);
                throw new InvalidOperationException($"Supabase upload failed: {response.StatusCode}");
            }

            _logger.LogInformation("Uploaded image to Supabase storage: {FileName}", fileName);
            return new ImageUploadResult
            {
                FileName = fileName,
                Url = GetServeUrl(fileName),
                Size = file.Length,
                OriginalName = file.FileName
            };
        }

        public async Task<Stream?> OpenReadAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_supabaseUrl}/storage/v1/object/{Bucket}/{Uri.EscapeDataString(fileName)}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);

            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var ms = new MemoryStream();
            await response.Content.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;
            return ms;
        }

        public async Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var stream = await OpenReadAsync(fileName, cancellationToken);
            if (stream == null)
            {
                return false;
            }

            await stream.DisposeAsync();
            return true;
        }

        public string GetServeUrl(string fileName) =>
            $"{_publicApiBaseUrl}/api/ImageUpload/{Uri.EscapeDataString(fileName)}";

        public async Task<string?> GetSignedUrlAsync(
            string fileName,
            TimeSpan expiry,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_supabaseUrl}/storage/v1/object/sign/{Bucket}/{Uri.EscapeDataString(fileName)}")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { expiresIn = (int)expiry.TotalSeconds }),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);

            var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            if (doc.RootElement.TryGetProperty("signedURL", out var signed))
            {
                var path = signed.GetString();
                return string.IsNullOrEmpty(path) ? null : $"{_supabaseUrl}{path}";
            }

            return null;
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
}
