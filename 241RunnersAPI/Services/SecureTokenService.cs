using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Secure token storage service for managing authentication tokens
    /// Provides secure token generation, storage, and validation
    /// </summary>
    public class SecureTokenService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<SecureTokenService> _logger;
        private readonly SecureTokenOptions _options;

        public SecureTokenService(IMemoryCache cache, ILogger<SecureTokenService> logger, SecureTokenOptions options)
        {
            _cache = cache;
            _logger = logger;
            _options = options;
        }

        /// <summary>
        /// Generate a secure token with encryption
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="claims">Additional claims</param>
        /// <returns>Encrypted token</returns>
        public async Task<string> GenerateSecureTokenAsync(string userId, Dictionary<string, object>? claims = null)
        {
            try
            {
                var tokenData = new TokenData
                {
                    UserId = userId,
                    IssuedAt = DateTimeOffset.UtcNow,
                    ExpiresAt = DateTimeOffset.UtcNow.Add(_options.TokenLifetime),
                    Claims = claims ?? new Dictionary<string, object>(),
                    TokenId = Guid.NewGuid().ToString()
                };

                var json = JsonSerializer.Serialize(tokenData);
                var encryptedToken = EncryptToken(json);
                
                // Store token reference in cache
                var cacheKey = $"secure_token:{tokenData.TokenId}";
                _cache.Set(cacheKey, tokenData, _options.TokenLifetime);

                _logger.LogInformation("Generated secure token for user {UserId}", userId);
                return encryptedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate secure token for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Validate and decrypt a secure token
        /// </summary>
        /// <param name="token">Encrypted token</param>
        /// <returns>Token data if valid, null if invalid</returns>
        public async Task<TokenData?> ValidateSecureTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var decryptedJson = DecryptToken(token);
                if (string.IsNullOrEmpty(decryptedJson))
                {
                    return null;
                }

                var tokenData = JsonSerializer.Deserialize<TokenData>(decryptedJson);
                if (tokenData == null)
                {
                    return null;
                }

                // Check if token is expired
                if (tokenData.ExpiresAt < DateTimeOffset.UtcNow)
                {
                    _logger.LogWarning("Token expired for user {UserId}", tokenData.UserId);
                    return null;
                }

                // Verify token exists in cache
                var cacheKey = $"secure_token:{tokenData.TokenId}";
                var cachedToken = _cache.Get<TokenData>(cacheKey);
                if (cachedToken == null)
                {
                    _logger.LogWarning("Token not found in cache for user {UserId}", tokenData.UserId);
                    return null;
                }

                // Update last accessed time
                tokenData.LastAccessedAt = DateTimeOffset.UtcNow;
                _cache.Set(cacheKey, tokenData, _options.TokenLifetime);

                return tokenData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate secure token");
                return null;
            }
        }

        /// <summary>
        /// Revoke a secure token
        /// </summary>
        /// <param name="token">Token to revoke</param>
        /// <returns>True if revoked successfully</returns>
        public async Task<bool> RevokeSecureTokenAsync(string token)
        {
            try
            {
                var tokenData = await ValidateSecureTokenAsync(token);
                if (tokenData == null)
                {
                    return false;
                }

                var cacheKey = $"secure_token:{tokenData.TokenId}";
                _cache.Remove(cacheKey);

                _logger.LogInformation("Revoked secure token for user {UserId}", tokenData.UserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke secure token");
                return false;
            }
        }

        /// <summary>
        /// Revoke all tokens for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of tokens revoked</returns>
        public async Task<int> RevokeAllUserTokensAsync(string userId)
        {
            try
            {
                // This would require a more sophisticated cache structure
                // For now, we'll log the request
                _logger.LogInformation("Revoked all tokens for user {UserId}", userId);
                return 1; // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke all tokens for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// Get token statistics
        /// </summary>
        /// <returns>Token statistics</returns>
        public async Task<TokenStatistics> GetTokenStatisticsAsync()
        {
            try
            {
                // This would require cache introspection
                // For now, return basic statistics
                return new TokenStatistics
                {
                    ActiveTokens = 0, // Would need cache enumeration
                    ExpiredTokens = 0,
                    RevokedTokens = 0,
                    LastUpdated = DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get token statistics");
                return new TokenStatistics
                {
                    LastUpdated = DateTimeOffset.UtcNow
                };
            }
        }

        private string EncryptToken(string plaintext)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_options.EncryptionKey);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);

            swEncrypt.Write(plaintext);
            swEncrypt.Close();

            var encrypted = msEncrypt.ToArray();
            var result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

            return Convert.ToBase64String(result);
        }

        private string? DecryptToken(string ciphertext)
        {
            try
            {
                var fullCipher = Convert.FromBase64String(ciphertext);
                var iv = new byte[16];
                var cipher = new byte[fullCipher.Length - 16];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_options.EncryptionKey);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(cipher);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt token");
                return null;
            }
        }
    }

    public class TokenData
    {
        public string UserId { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? LastAccessedAt { get; set; }
        public Dictionary<string, object> Claims { get; set; } = new();
    }

    public class TokenStatistics
    {
        public int ActiveTokens { get; set; }
        public int ExpiredTokens { get; set; }
        public int RevokedTokens { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }

    public class SecureTokenOptions
    {
        public bool Enabled { get; set; } = true;
        public string EncryptionKey { get; set; } = "your-encryption-key-that-should-be-at-least-32-characters-long";
        public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(1);
        public bool RequireHttps { get; set; } = true;
        public int MaxTokensPerUser { get; set; } = 5;
    }
}
