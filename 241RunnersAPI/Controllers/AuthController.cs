using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Models;
using _241RunnersAPI.Data;
using System.ComponentModel.DataAnnotations;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto request)
        {
            _logger.LogInformation("Register request received for email: {Email}", request.Email);

            // Validate the request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
                return Conflict(new { Message = "Email is already registered." });
            }

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                Email = request.Email,
                PasswordHash = hashedPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode,
                Organization = request.Organization,
                Title = request.Title,
                Credentials = request.Credentials,
                Specialization = request.Specialization,
                YearsOfExperience = request.YearsOfExperience,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                EmergencyContactRelationship = request.EmergencyContactRelationship,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailVerificationToken = Guid.NewGuid().ToString()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registration successful for {Email} with ID {UserId}", request.Email, user.Id);

            // Return user response without sensitive data
            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
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
                EmergencyContactName = user.EmergencyContactName,
                EmergencyContactPhone = user.EmergencyContactPhone,
                EmergencyContactRelationship = user.EmergencyContactRelationship,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified
            };

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful. Please verify your email.",
                User = userResponse
            });
        }

        /// <summary>
        /// User login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            _logger.LogInformation("Login request received for email: {Email}", request.Email);

            // Validate the request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Email} not found", request.Email);
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User {Email} is inactive", request.Email);
                return Unauthorized(new { Message = "Account is inactive. Please contact support." });
            }

            // Check if account is locked
            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                _logger.LogWarning("Login failed: User {Email} account is locked until {LockedUntil}", request.Email, user.LockedUntil);
                return Unauthorized(new { Message = "Account is temporarily locked due to multiple failed login attempts." });
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(30); // Lock for 30 minutes
                }
                await _context.SaveChangesAsync();

                _logger.LogWarning("Login failed: Invalid password for {Email}", request.Email);
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            // Reset failed login attempts on successful login
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user.Id, user.Email, user.Role, user.FirstName, user.LastName);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            _logger.LogInformation("User login successful for {Email} with role {Role}", request.Email, user.Role);

            // Return user response without sensitive data
            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
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
                EmergencyContactName = user.EmergencyContactName,
                EmergencyContactPhone = user.EmergencyContactPhone,
                EmergencyContactRelationship = user.EmergencyContactRelationship,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified
            };

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                User = userResponse,
                ExpiresAt = expiresAt
            });
        }

        /// <summary>
        /// Get current user information (requires authentication)
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user token." });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role,
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

            return Ok(userResponse);
        }

        /// <summary>
        /// Update user profile (requires authentication)
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user token." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Update user fields
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;
            user.City = request.City;
            user.State = request.State;
            user.ZipCode = request.ZipCode;
            user.Organization = request.Organization;
            user.Title = request.Title;
            user.Credentials = request.Credentials;
            user.Specialization = request.Specialization;
            user.YearsOfExperience = request.YearsOfExperience;
            user.EmergencyContactName = request.EmergencyContactName;
            user.EmergencyContactPhone = request.EmergencyContactPhone;
            user.EmergencyContactRelationship = request.EmergencyContactRelationship;
            user.Notes = request.Notes;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User profile updated for {Email}", user.Email);

            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role,
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
                EmergencyContactName = user.EmergencyContactName,
                EmergencyContactPhone = user.EmergencyContactPhone,
                EmergencyContactRelationship = user.EmergencyContactRelationship,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified
            };

            return Ok(new { Message = "Profile updated successfully.", User = userResponse });
        }

        /// <summary>
        /// Change user password (requires authentication)
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user token." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { Message = "Current password is incorrect." });
            }

            // Hash new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password changed for user {Email}", user.Email);

            return Ok(new { Message = "Password changed successfully." });
        }

        /// <summary>
        /// Logout user (client-side token removal)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            _logger.LogInformation("User logout requested");
            return Ok(new { Message = "Logout successful. Please remove the token from client storage." });
        }

        /// <summary>
        /// Seed admin users (development endpoint)
        /// </summary>
        [HttpPost("seed-admins")]
        public async Task<IActionResult> SeedAdmins()
        {
            try
            {
                var adminUsers = new List<User>
                {
                    new User
                    {
                        Email = "dekuworks1@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("marcus2025"),
                        FirstName = "Marcus",
                        LastName = "Brown",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "System Administrator",
                        PhoneNumber = "15551234567",
                        Address = "123 Admin Street",
                        City = "Administration City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "danielcarey9770@yahoo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Daniel2025!"),
                        FirstName = "Daniel",
                        LastName = "Carey",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "15551234568",
                        Address = "124 Admin Street",
                        City = "Administration City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "lthomas3350@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lisa2025!"),
                        FirstName = "Lisa",
                        LastName = "Thomas",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "15551234569",
                        Address = "125 Admin Street",
                        City = "Administration City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "tinaleggins@yahoo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tina2025!"),
                        FirstName = "Tina",
                        LastName = "Matthews",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "15551234570",
                        Address = "126 Admin Street",
                        City = "Administration City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "mmelasky@iplawconsulting.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mark2025!"),
                        FirstName = "Mark",
                        LastName = "Melasky",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "IP Law Consulting",
                        Title = "Legal Administrator",
                        PhoneNumber = "15551234571",
                        Address = "127 Legal Street",
                        City = "Legal City",
                        State = "Legal State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    },
                    new User
                    {
                        Email = "ralphfrank900@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Ralph2025!"),
                        FirstName = "Ralph",
                        LastName = "Frank",
                        Role = "admin",
                        IsActive = true,
                        IsEmailVerified = true,
                        IsPhoneVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailVerifiedAt = DateTime.UtcNow,
                        PhoneVerifiedAt = DateTime.UtcNow,
                        Organization = "241 Runners Awareness",
                        Title = "Administrator",
                        PhoneNumber = "15551234572",
                        Address = "128 Admin Street",
                        City = "Administration City",
                        State = "Admin State",
                        ZipCode = "12345",
                        EmergencyContactName = "Emergency Services",
                        EmergencyContactPhone = "1555911",
                        EmergencyContactRelationship = "Emergency Contact"
                    }
                };

                int addedCount = 0;
                int skippedCount = 0;

                foreach (var adminUser in adminUsers)
                {
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminUser.Email);
                    if (existingUser == null)
                    {
                        _context.Users.Add(adminUser);
                        addedCount++;
                        _logger.LogInformation("Added admin user: {Email} ({FirstName} {LastName})", 
                            adminUser.Email, adminUser.FirstName, adminUser.LastName);
                    }
                    else
                    {
                        skippedCount++;
                        _logger.LogInformation("Admin user already exists: {Email} ({FirstName} {LastName})", 
                            adminUser.Email, adminUser.FirstName, adminUser.LastName);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    success = true, 
                    message = "Admin users seeded successfully",
                    added = addedCount,
                    skipped = skippedCount,
                    total = adminUsers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding admin users");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all users (admin only)
        /// </summary>
        [HttpGet("admin/users")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = $"{u.FirstName} {u.LastName}",
                        Role = u.Role,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt,
                        UpdatedAt = u.UpdatedAt,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        City = u.City,
                        State = u.State,
                        ZipCode = u.ZipCode,
                        Organization = u.Organization,
                        Title = u.Title,
                        Credentials = u.Credentials,
                        Specialization = u.Specialization,
                        YearsOfExperience = u.YearsOfExperience,
                        ProfileImageUrl = u.ProfileImageUrl,
                        EmergencyContactName = u.EmergencyContactName,
                        EmergencyContactPhone = u.EmergencyContactPhone,
                        EmergencyContactRelationship = u.EmergencyContactRelationship,
                        IsEmailVerified = u.IsEmailVerified,
                        IsPhoneVerified = u.IsPhoneVerified,
                        EmailVerifiedAt = u.EmailVerifiedAt,
                        PhoneVerifiedAt = u.PhoneVerifiedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, users = users, count = users.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all admin users
        /// </summary>
        [HttpGet("admin/admins")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAdmins()
        {
            try
            {
                var admins = await _context.Users
                    .Where(u => u.Role == "admin")
                    .Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = $"{u.FirstName} {u.LastName}",
                        Role = u.Role,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt,
                        UpdatedAt = u.UpdatedAt,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        City = u.City,
                        State = u.State,
                        ZipCode = u.ZipCode,
                        Organization = u.Organization,
                        Title = u.Title,
                        Credentials = u.Credentials,
                        Specialization = u.Specialization,
                        YearsOfExperience = u.YearsOfExperience,
                        ProfileImageUrl = u.ProfileImageUrl,
                        EmergencyContactName = u.EmergencyContactName,
                        EmergencyContactPhone = u.EmergencyContactPhone,
                        EmergencyContactRelationship = u.EmergencyContactRelationship,
                        IsEmailVerified = u.IsEmailVerified,
                        IsPhoneVerified = u.IsPhoneVerified,
                        EmailVerifiedAt = u.EmailVerifiedAt,
                        PhoneVerifiedAt = u.PhoneVerifiedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, admins = admins, count = admins.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admins");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("admin/dashboard-stats")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalAdmins = await _context.Users.CountAsync(u => u.Role == "admin");
                var activeAdmins = await _context.Users.CountAsync(u => u.Role == "admin" && u.IsActive);
                var totalRunners = await _context.Users.CountAsync(u => u.Role == "runner");
                var totalPublicCases = 0; // Placeholder for future implementation

                return Ok(new
                {
                    success = true,
                    stats = new
                    {
                        totalUsers,
                        totalAdmins,
                        activeAdmins,
                        totalRunners,
                        totalPublicCases,
                        systemStatus = "healthy"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        private string GenerateJwtToken(int userId, string email, string role, string firstName, string lastName)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "241RunnersAwareness";
            var jwtAudience = _configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "241RunnersAwareness";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24), // Token valid for 24 hours
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}