using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using _241RunnersAPI.Services;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "VALIDATION_FAILED",
                            message = "Email and password are required",
                            details = new { email = "Required", password = "Required" }
                        }
                    });
                }

                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "USER_EXISTS",
                            message = "User with this email already exists"
                        }
                    });
                }

                // Create new user
                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = request.Role ?? "User",
                    IsActive = true,
                    IsEmailVerified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user registered: {Email} with role {Role}", user.Email, user.Role);

                return CreatedAtAction(nameof(Register), new { id = user.Id }, new
                {
                    id = $"u_{user.Id}",
                    email = user.Email,
                    role = user.Role,
                    emailVerified = user.IsEmailVerified
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred during registration"
                    }
                });
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "VALIDATION_FAILED",
                            message = "Email and password are required"
                        }
                    });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Unauthorized(new
                    {
                        error = new
                        {
                            code = "INVALID_CREDENTIALS",
                            message = "Invalid email or password"
                        }
                    });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new
                    {
                        error = new
                        {
                            code = "ACCOUNT_DISABLED",
                            message = "Account is disabled"
                        }
                    });
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(user);

                _logger.LogInformation("User logged in: {Email}", user.Email);

                return Ok(new
                {
                    accessToken = token,
                    expiresIn = 3600,
                    user = new
                    {
                        id = $"u_{user.Id}",
                        email = user.Email,
                        role = user.Role,
                        name = $"{user.FirstName} {user.LastName}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred during login"
                    }
                });
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // In a stateless JWT system, logout is handled client-side
            // This endpoint exists for consistency and future token blacklisting
            return Ok(new { ok = true });
        }

        /// <summary>
        /// Verify email address
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            try
            {
                // For now, just mark as verified (in production, validate the token)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Token);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "INVALID_TOKEN",
                            message = "Invalid verification token"
                        }
                    });
                }

                user.IsEmailVerified = true;
                user.EmailVerifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { emailVerified = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                return StatusCode(500, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred during email verification"
                    }
                });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners";
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"] ?? "241RunnersAwareness";
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"] ?? "241RunnersAwareness";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyEmailRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}