using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using _241RunnersAPI.Services;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class OAuthController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OAuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly PerformanceMonitoringService _performanceService;
        private readonly InputSanitizationService _sanitizationService;

        public OAuthController(
            ApplicationDbContext context, 
            ILogger<OAuthController> logger, 
            IConfiguration configuration, 
            PerformanceMonitoringService performanceService, 
            InputSanitizationService sanitizationService)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _performanceService = performanceService;
            _sanitizationService = sanitizationService;
        }

        /// <summary>
        /// OAuth login endpoint for Google, Apple, and Microsoft
        /// </summary>
        [HttpPost("oauth/login")]
        public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginDto request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                // Validate provider
                if (!IsValidOAuthProvider(request.Provider))
                {
                    return BadRequest(new { message = "Invalid OAuth provider. Supported providers: google, apple, microsoft" });
                }

                // Validate access token
                if (string.IsNullOrWhiteSpace(request.AccessToken))
                {
                    return BadRequest(new { message = "Access token is required" });
                }

                // Verify OAuth token with provider
                var userInfo = await VerifyOAuthToken(request.Provider, request.AccessToken, request.IdToken);
                if (userInfo == null)
                {
                    return Unauthorized(new { message = "Invalid OAuth token" });
                }

                // Check if user exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == userInfo.Email && u.AuthProvider == request.Provider);

                if (existingUser != null)
                {
                    // Update existing user's OAuth tokens
                    existingUser.ProviderAccessToken = EncryptToken(request.AccessToken);
                    existingUser.ProviderRefreshToken = request.RefreshToken != null ? EncryptToken(request.RefreshToken) : null;
                    existingUser.ProviderTokenExpires = DateTime.UtcNow.AddHours(1); // Default 1 hour
                    existingUser.LastLoginAt = DateTime.UtcNow;
                    existingUser.UpdatedAt = DateTime.UtcNow;

                    // Update profile if new information is available
                    if (!string.IsNullOrWhiteSpace(userInfo.FirstName) && existingUser.FirstName != userInfo.FirstName)
                    {
                        existingUser.FirstName = userInfo.FirstName;
                    }
                    if (!string.IsNullOrWhiteSpace(userInfo.LastName) && existingUser.LastName != userInfo.LastName)
                    {
                        existingUser.LastName = userInfo.LastName;
                    }
                    if (!string.IsNullOrWhiteSpace(userInfo.ProfileImageUrl) && existingUser.ProfileImageUrl != userInfo.ProfileImageUrl)
                    {
                        existingUser.ProfileImageUrl = userInfo.ProfileImageUrl;
                    }

                    await _context.SaveChangesAsync();

                    // Generate JWT token
                    var token = await GenerateJwtToken(existingUser);
                    var userResponse = MapToUserResponse(existingUser);

                    _logger.LogInformation("OAuth login successful for user {Email} via {Provider}", existingUser.Email, request.Provider);
                    // _performanceService.RecordOperation("oauth_login", DateTime.UtcNow - startTime);

                    return Ok(new AuthResponseDto
                    {
                        Success = true,
                        Message = "Login successful",
                        Token = token,
                        User = userResponse,
                        ExpiresAt = DateTime.UtcNow.AddHours(24)
                    });
                }
                else
                {
                    // User doesn't exist - return registration required
                    return BadRequest(new { 
                        message = "User not found. Please register first.",
                        requiresRegistration = true,
                        userInfo = new {
                            email = userInfo.Email,
                            firstName = userInfo.FirstName,
                            lastName = userInfo.LastName,
                            profileImageUrl = userInfo.ProfileImageUrl
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OAuth login failed for provider {Provider}", request.Provider);
                // _performanceService.RecordOperation("oauth_login_error", DateTime.UtcNow - startTime);
                return StatusCode(500, new { message = "OAuth login failed. Please try again." });
            }
        }

        /// <summary>
        /// OAuth registration endpoint for Google, Apple, and Microsoft
        /// </summary>
        [HttpPost("oauth/register")]
        public async Task<IActionResult> OAuthRegister([FromBody] OAuthRegistrationDto request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                // Validate provider
                if (!IsValidOAuthProvider(request.Provider))
                {
                    return BadRequest(new { message = "Invalid OAuth provider. Supported providers: google, apple, microsoft" });
                }

                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    return BadRequest(new { message = "User with this email already exists" });
                }

                // Create new user
                var user = new User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = request.Role,
                    AuthProvider = request.Provider,
                    ProviderUserId = request.ProviderUserId,
                    ProviderAccessToken = request.AccessToken != null ? EncryptToken(request.AccessToken) : null,
                    ProviderRefreshToken = request.RefreshToken != null ? EncryptToken(request.RefreshToken) : null,
                    ProviderTokenExpires = request.TokenExpires,
                    ProfileImageUrl = request.ProfileImageUrl,
                    IsActive = true,
                    IsEmailVerified = true, // OAuth users are considered email verified
                    EmailVerifiedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = await GenerateJwtToken(user);
                var userResponse = MapToUserResponse(user);

                _logger.LogInformation("OAuth registration successful for user {Email} via {Provider}", user.Email, request.Provider);
                // _performanceService.RecordOperation("oauth_register", DateTime.UtcNow - startTime);

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Registration successful",
                    Token = token,
                    User = userResponse,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OAuth registration failed for provider {Provider}", request.Provider);
                // _performanceService.RecordOperation("oauth_register_error", DateTime.UtcNow - startTime);
                return StatusCode(500, new { message = "OAuth registration failed. Please try again." });
            }
        }

        /// <summary>
        /// Verify OAuth token with the provider
        /// </summary>
        private async Task<OAuthUserInfo?> VerifyOAuthToken(string provider, string accessToken, string? idToken)
        {
            try
            {
                switch (provider.ToLower())
                {
                    case "google":
                        return await VerifyGoogleToken(accessToken, idToken);
                    case "apple":
                        return await VerifyAppleToken(accessToken, idToken);
                    case "microsoft":
                        return await VerifyMicrosoftToken(accessToken, idToken);
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify OAuth token for provider {Provider}", provider);
                return null;
            }
        }

        /// <summary>
        /// Verify Google OAuth token
        /// </summary>
        private async Task<OAuthUserInfo?> VerifyGoogleToken(string accessToken, string? idToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                
                var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var userInfo = System.Text.Json.JsonSerializer.Deserialize<GoogleUserInfo>(json);
                    
                    return new OAuthUserInfo
                    {
                        Email = userInfo?.email,
                        FirstName = userInfo?.given_name,
                        LastName = userInfo?.family_name,
                        ProfileImageUrl = userInfo?.picture
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify Google token");
            }
            return null;
        }

        /// <summary>
        /// Verify Apple OAuth token
        /// </summary>
        private async Task<OAuthUserInfo?> VerifyAppleToken(string accessToken, string? idToken)
        {
            try
            {
                // Apple Sign-In verification would go here
                // For now, return basic info from the ID token
                if (!string.IsNullOrWhiteSpace(idToken))
                {
                    // In a real implementation, you would verify the Apple ID token
                    // and extract user information from it
                    return new OAuthUserInfo
                    {
                        Email = "user@example.com", // Extract from ID token
                        FirstName = "Apple", // Extract from ID token
                        LastName = "User" // Extract from ID token
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify Apple token");
            }
            return null;
        }

        /// <summary>
        /// Verify Microsoft OAuth token
        /// </summary>
        private async Task<OAuthUserInfo?> VerifyMicrosoftToken(string accessToken, string? idToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                
                var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var userInfo = System.Text.Json.JsonSerializer.Deserialize<MicrosoftUserInfo>(json);
                    
                    return new OAuthUserInfo
                    {
                        Email = userInfo?.mail ?? userInfo?.userPrincipalName,
                        FirstName = userInfo?.givenName,
                        LastName = userInfo?.surname,
                        ProfileImageUrl = null // Microsoft Graph doesn't provide profile image in basic endpoint
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify Microsoft token");
            }
            return null;
        }

        /// <summary>
        /// Check if OAuth provider is valid
        /// </summary>
        private bool IsValidOAuthProvider(string provider)
        {
            var validProviders = new[] { "google", "apple", "microsoft" };
            return validProviders.Contains(provider.ToLower());
        }

        /// <summary>
        /// Encrypt OAuth tokens for storage
        /// </summary>
        private string EncryptToken(string token)
        {
            // In a real implementation, you would encrypt the token
            // For now, we'll just return the token (not recommended for production)
            return token;
        }

        /// <summary>
        /// Decrypt OAuth tokens for use
        /// </summary>
        private string DecryptToken(string encryptedToken)
        {
            // In a real implementation, you would decrypt the token
            // For now, we'll just return the token (not recommended for production)
            return encryptedToken;
        }

        /// <summary>
        /// Generate JWT token for user
        /// </summary>
        private async Task<string> GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["JWT_KEY"] ?? throw new InvalidOperationException("JWT_KEY not configured");
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName),
                new System.Security.Claims.Claim("role", user.Role)
            };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: _configuration["JWT_ISSUER"] ?? "241RunnersAwareness",
                audience: _configuration["JWT_AUDIENCE"] ?? "241RunnersAwareness",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Map user to response DTO
        /// </summary>
        private UserResponseDto MapToUserResponse(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role,
                AllRoles = user.AllRoles,
                PrimaryUserRole = user.PrimaryUserRole,
                IsAdminUser = user.IsAdminUser,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                UpdatedAt = user.UpdatedAt,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                State = user.State,
                ZipCode = user.ZipCode,
                Organization = user.Organization,
                Title = user.Title,
                Credentials = user.Credentials,
                Specialization = user.Specialization,
                YearsOfExperience = user.YearsOfExperience,
                ProfileImageUrl = user.ProfileImageUrl,
                EmergencyContactName = user.EmergencyContactName,
                EmergencyContactPhone = user.EmergencyContactPhone,
                EmergencyContactRelationship = user.EmergencyContactRelationship,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified,
                EmailVerifiedAt = user.EmailVerifiedAt,
                PhoneVerifiedAt = user.PhoneVerifiedAt
            };
        }

        /// <summary>
        /// OAuth user information
        /// </summary>
        public class OAuthUserInfo
        {
            public string? Email { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? ProfileImageUrl { get; set; }
        }

        /// <summary>
        /// Google user information
        /// </summary>
        public class GoogleUserInfo
        {
            public string? email { get; set; }
            public string? given_name { get; set; }
            public string? family_name { get; set; }
            public string? picture { get; set; }
        }

        /// <summary>
        /// Microsoft user information
        /// </summary>
        public class MicrosoftUserInfo
        {
            public string? mail { get; set; }
            public string? userPrincipalName { get; set; }
            public string? givenName { get; set; }
            public string? surname { get; set; }
        }
    }
}
