using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using _241RunnersAPI.Services;
using Microsoft.Extensions.Logging;

namespace _241RunnersAPI.Controllers
{
    /// <summary>
    /// Security controller for security-related operations
    /// Provides endpoints for security monitoring and management
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize] // Require authentication for all security endpoints
    public class SecurityController : ControllerBase
    {
        private readonly SecurityAuditService _securityAuditService;
        private readonly SecureTokenService _secureTokenService;
        private readonly ILogger<SecurityController> _logger;

        public SecurityController(
            SecurityAuditService securityAuditService,
            SecureTokenService secureTokenService,
            ILogger<SecurityController> logger)
        {
            _securityAuditService = securityAuditService;
            _secureTokenService = secureTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Get security audit statistics
        /// </summary>
        /// <param name="startDate">Start date for statistics (optional)</param>
        /// <param name="endDate">End date for statistics (optional)</param>
        /// <returns>Security audit statistics</returns>
        [HttpGet("audit/statistics")]
        [Authorize(Roles = "admin")] // Only admins can view security statistics
        public async Task<IActionResult> GetSecurityStatistics(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
                var end = endDate ?? DateTimeOffset.UtcNow;

                _logger.LogInformation("Security statistics requested by {User} for period {StartDate} to {EndDate}", 
                    User.Identity?.Name, start, end);

                var statistics = await _securityAuditService.GetSecurityStatisticsAsync(start, end);

                return Ok(new
                {
                    success = true,
                    data = statistics,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get security statistics");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get security statistics",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Get token statistics
        /// </summary>
        /// <returns>Token statistics</returns>
        [HttpGet("tokens/statistics")]
        [Authorize(Roles = "admin")] // Only admins can view token statistics
        public async Task<IActionResult> GetTokenStatistics()
        {
            try
            {
                _logger.LogInformation("Token statistics requested by {User}", User.Identity?.Name);

                var statistics = await _secureTokenService.GetTokenStatisticsAsync();

                return Ok(new
                {
                    success = true,
                    data = statistics,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get token statistics");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get token statistics",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Revoke all tokens for the current user
        /// </summary>
        /// <returns>Revocation result</returns>
        [HttpPost("tokens/revoke-all")]
        public async Task<IActionResult> RevokeAllUserTokens()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "User ID not found in token",
                        timestamp = DateTimeOffset.UtcNow
                    });
                }

                _logger.LogInformation("Revoking all tokens for user {UserId}", userId);

                var revokedCount = await _secureTokenService.RevokeAllUserTokensAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = $"Revoked {revokedCount} tokens",
                    revokedCount = revokedCount,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke user tokens");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to revoke user tokens",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Revoke a specific token
        /// </summary>
        /// <param name="token">Token to revoke</param>
        /// <returns>Revocation result</returns>
        [HttpPost("tokens/revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Token is required",
                        timestamp = DateTimeOffset.UtcNow
                    });
                }

                _logger.LogInformation("Revoking token for user {User}", User.Identity?.Name);

                var success = await _secureTokenService.RevokeSecureTokenAsync(request.Token);

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Token revoked successfully",
                        timestamp = DateTimeOffset.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Token not found or already revoked",
                        timestamp = DateTimeOffset.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke token");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to revoke token",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Generate a new secure token for the current user
        /// </summary>
        /// <param name="request">Token generation request</param>
        /// <returns>New secure token</returns>
        [HttpPost("tokens/generate")]
        public async Task<IActionResult> GenerateSecureToken([FromBody] GenerateTokenRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "User ID not found in token",
                        timestamp = DateTimeOffset.UtcNow
                    });
                }

                _logger.LogInformation("Generating secure token for user {UserId}", userId);

                var claims = request.Claims ?? new Dictionary<string, object>();
                var token = await _secureTokenService.GenerateSecureTokenAsync(userId, claims);

                return Ok(new
                {
                    success = true,
                    data = new { token = token },
                    message = "Secure token generated successfully",
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate secure token");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to generate secure token",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Validate a secure token
        /// </summary>
        /// <param name="request">Token validation request</param>
        /// <returns>Token validation result</returns>
        [HttpPost("tokens/validate")]
        public async Task<IActionResult> ValidateSecureToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Token is required",
                        timestamp = DateTimeOffset.UtcNow
                    });
                }

                _logger.LogInformation("Validating secure token for user {User}", User.Identity?.Name);

                var tokenData = await _secureTokenService.ValidateSecureTokenAsync(request.Token);

                if (tokenData != null)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            userId = tokenData.UserId,
                            tokenId = tokenData.TokenId,
                            issuedAt = tokenData.IssuedAt,
                            expiresAt = tokenData.ExpiresAt,
                            lastAccessedAt = tokenData.LastAccessedAt,
                            claims = tokenData.Claims
                        },
                        message = "Token is valid",
                        timestamp = DateTimeOffset.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Token is invalid or expired",
                        timestamp = DateTimeOffset.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate secure token");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to validate secure token",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Get security health status
        /// </summary>
        /// <returns>Security health status</returns>
        [HttpGet("health")]
        public async Task<IActionResult> GetSecurityHealth()
        {
            try
            {
                _logger.LogInformation("Security health check requested by {User}", User.Identity?.Name);

                var health = new
                {
                    status = "healthy",
                    timestamp = DateTimeOffset.UtcNow,
                    features = new
                    {
                        csrfProtection = true,
                        httpsEnforcement = true,
                        secureTokens = true,
                        securityAudit = true,
                        rateLimiting = true
                    }
                };

                return Ok(new
                {
                    success = true,
                    data = health,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get security health status");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get security health status",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }
    }

    public class RevokeTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class GenerateTokenRequest
    {
        public Dictionary<string, object>? Claims { get; set; }
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
