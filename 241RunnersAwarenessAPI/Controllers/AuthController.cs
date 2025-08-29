using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Models;
using _241RunnersAwarenessAPI.Services;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, JwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validate input
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (existingUser != null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "A user with this email already exists."
                    });
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Create new user
                var user = new User
                {
                    Email = request.Email.ToLower(),
                    PasswordHash = passwordHash,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Role = request.Role.ToLower(),
                    PhoneNumber = request.PhoneNumber?.Trim(),
                    Address = request.Address?.Trim(),
                    City = request.City?.Trim(),
                    State = request.State?.Trim(),
                    ZipCode = request.ZipCode?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate token
                var token = _jwtService.GenerateToken(user);

                // Return success response
                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "User registered successfully.",
                    Token = token,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        City = user.City,
                        State = user.State,
                        ZipCode = user.ZipCode
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during registration. Please try again."
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate input
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                // Find user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    });
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Account is deactivated. Please contact support."
                    });
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    });
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate token
                var token = _jwtService.GenerateToken(user);

                // Return success response
                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    Token = token,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        City = user.City,
                        State = user.State,
                        ZipCode = user.ZipCode
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again."
                });
            }
        }

        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok(new { message = "API is working!", timestamp = DateTime.UtcNow, status = "healthy" });
        }

        [HttpGet("health")]
        public ActionResult<string> Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
} 