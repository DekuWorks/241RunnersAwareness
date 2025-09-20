using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace _241RunnersAPI.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        protected int? GetCurrentUserIdAsInt()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            return null;
        }

        protected string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        protected string? GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value;
        }

        protected bool IsAdmin()
        {
            return GetCurrentUserRole()?.ToLower() == "admin";
        }

        protected bool IsStaff()
        {
            var role = GetCurrentUserRole()?.ToLower();
            return role == "admin" || role == "staff";
        }

        protected IActionResult ErrorResponse(string code, string message, object? details = null)
        {
            return BadRequest(new
            {
                error = new
                {
                    code,
                    message,
                    details
                }
            });
        }

        protected IActionResult ValidationErrorResponse(Dictionary<string, string[]> errors)
        {
            return BadRequest(new
            {
                error = new
                {
                    code = "VALIDATION_FAILED",
                    message = "Validation failed",
                    details = errors
                }
            });
        }

        protected IActionResult NotFoundResponse(string message = "Resource not found")
        {
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message
                }
            });
        }

        protected IActionResult UnauthorizedResponse(string message = "Unauthorized")
        {
            return Unauthorized(new
            {
                error = new
                {
                    code = "UNAUTHORIZED",
                    message
                }
            });
        }

        protected IActionResult BadRequestResponse(string message = "Bad request")
        {
            return BadRequest(new
            {
                error = new
                {
                    code = "BAD_REQUEST",
                    message
                }
            });
        }

        protected IActionResult InternalServerErrorResponse(string message = "Internal server error")
        {
            return StatusCode(500, new
            {
                error = new
                {
                    code = "INTERNAL_SERVER_ERROR",
                    message
                }
            });
        }
    }
}
