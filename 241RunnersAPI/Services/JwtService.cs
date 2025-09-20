using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using _241RunnersAPI.Models;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for JWT token generation and validation
    /// </summary>
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;
        private readonly ApplicationDbContext _context;
        private static readonly HashSet<string> _revokedTokens = new(); // In production, use Redis or database
        private static readonly Dictionary<string, DateTime> _tokenBlacklist = new(); // Track suspicious tokens

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger, ApplicationDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Generate JWT token for a user
        /// </summary>
        public string GenerateToken(User user)
        {
            try
            {
                var jwtKey = _configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners";
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "241RunnersAwareness";
                var jwtAudience = _configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "241RunnersAwareness";

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var jti = Guid.NewGuid().ToString();
                var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("IsEmailVerified", user.IsEmailVerified.ToString()),
                    new Claim("IsPhoneVerified", user.IsPhoneVerified.ToString()),
                    new Claim("IsActive", user.IsActive.ToString()),
                    new Claim("CreatedAt", user.CreatedAt.ToString("O")),
                    new Claim(JwtRegisteredClaimNames.Jti, jti),
                    new Claim(JwtRegisteredClaimNames.Iat, iat.ToString(), ClaimValueTypes.Integer64),
                    new Claim("SessionId", Guid.NewGuid().ToString()), // Track individual sessions
                    new Claim("TokenVersion", "1.0"), // For token versioning
                    new Claim("IssuedFor", "241RunnersAwareness") // Additional validation
                };

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
                throw;
            }
        }

        /// <summary>
        /// Validate JWT token and return claims principal with enhanced security checks
        /// </summary>
        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                // Check if token is revoked
                if (IsTokenRevoked(token))
                {
                    _logger.LogWarning("Attempted to use revoked token");
                    return null;
                }

                // Check if token is blacklisted
                if (IsTokenBlacklisted(token))
                {
                    _logger.LogWarning("Attempted to use blacklisted token");
                    return null;
                }

                var jwtKey = _configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners";
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "241RunnersAwareness";
                var jwtAudience = _configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "241RunnersAwareness";

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Additional security validations
                if (!await ValidateTokenClaims(principal, token))
                {
                    _logger.LogWarning("Token claims validation failed");
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("JWT token has expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("JWT token has invalid signature");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed");
                return null;
            }
        }

        /// <summary>
        /// Validate JWT token and return claims principal (synchronous version for backward compatibility)
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            return ValidateTokenAsync(token).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get user ID from JWT token
        /// </summary>
        public int? GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null) return null;

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting user ID from token");
                return null;
            }
        }

        /// <summary>
        /// Get user role from JWT token
        /// </summary>
        public string? GetUserRoleFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                return principal?.FindFirst(ClaimTypes.Role)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting user role from token");
                return null;
            }
        }

        /// <summary>
        /// Check if user is admin from JWT token
        /// </summary>
        public bool IsAdminFromToken(string token)
        {
            var role = GetUserRoleFromToken(token);
            return role?.ToLower() == "admin";
        }

        /// <summary>
        /// Generate refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Get token expiration time
        /// </summary>
        public DateTime GetTokenExpiration()
        {
            return DateTime.UtcNow.AddHours(24);
        }

        /// <summary>
        /// Revoke a token (add to blacklist)
        /// </summary>
        public void RevokeToken(string token)
        {
            try
            {
                _revokedTokens.Add(token);
                _logger.LogInformation("Token revoked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
            }
        }

        /// <summary>
        /// Check if token is revoked
        /// </summary>
        public bool IsTokenRevoked(string token)
        {
            return _revokedTokens.Contains(token);
        }

        /// <summary>
        /// Add token to blacklist (for suspicious activity)
        /// </summary>
        public void BlacklistToken(string token, TimeSpan? duration = null)
        {
            try
            {
                var expiry = DateTime.UtcNow.Add(duration ?? TimeSpan.FromHours(24));
                _tokenBlacklist[token] = expiry;
                _logger.LogWarning("Token blacklisted until {Expiry}", expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blacklisting token");
            }
        }

        /// <summary>
        /// Check if token is blacklisted
        /// </summary>
        public bool IsTokenBlacklisted(string token)
        {
            if (_tokenBlacklist.TryGetValue(token, out var expiry))
            {
                if (DateTime.UtcNow > expiry)
                {
                    _tokenBlacklist.Remove(token);
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validate token claims against database state
        /// </summary>
        private async Task<bool> ValidateTokenClaims(ClaimsPrincipal principal, string token)
        {
            try
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Invalid user ID in token");
                    return false;
                }

                // Check if user still exists and is active
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for token validation", userId);
                    BlacklistToken(token);
                    return false;
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user {UserId} attempted to use token", userId);
                    BlacklistToken(token);
                    return false;
                }

                // Validate token version
                var tokenVersion = principal.FindFirst("TokenVersion")?.Value;
                if (tokenVersion != "1.0")
                {
                    _logger.LogWarning("Invalid token version: {Version}", tokenVersion);
                    return false;
                }

                // Validate issued for claim
                var issuedFor = principal.FindFirst("IssuedFor")?.Value;
                if (issuedFor != "241RunnersAwareness")
                {
                    _logger.LogWarning("Invalid issued for claim: {IssuedFor}", issuedFor);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token claims");
                return false;
            }
        }

        /// <summary>
        /// Generate a short-lived token for sensitive operations
        /// </summary>
        public string GenerateShortLivedToken(User user, TimeSpan duration)
        {
            try
            {
                var jwtKey = _configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners";
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "241RunnersAwareness";
                var jwtAudience = _configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "241RunnersAwareness";

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim("TokenType", "ShortLived"),
                    new Claim("Duration", duration.TotalMinutes.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(duration),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating short-lived token for user {UserId}", user.Id);
                throw;
            }
        }

        /// <summary>
        /// Clean up expired tokens from blacklist
        /// </summary>
        public void CleanupExpiredTokens()
        {
            try
            {
                var now = DateTime.UtcNow;
                var expiredTokens = _tokenBlacklist.Where(kvp => kvp.Value < now).Select(kvp => kvp.Key).ToList();
                
                foreach (var token in expiredTokens)
                {
                    _tokenBlacklist.Remove(token);
                }

                if (expiredTokens.Count > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired tokens from blacklist", expiredTokens.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired tokens");
            }
        }
    }
}
