using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using System.Text.RegularExpressions;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Comprehensive validation service for all database operations
    /// </summary>
    public class ValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(ApplicationDbContext context, ILogger<ValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Validate user deletion with comprehensive checks
        /// </summary>
        public async Task<ValidationResult> ValidateUserDeletion(int userId, int currentAdminId)
        {
            var result = new ValidationResult();

            try
            {
                // Check if user exists
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    result.AddError("User not found", "USER_NOT_FOUND");
                    return result;
                }

                // Prevent self-deletion
                if (user.Id == currentAdminId)
                {
                    result.AddError("You cannot delete your own account", "SELF_DELETE_NOT_ALLOWED");
                    return result;
                }

                // Check if user is the last admin
                if (user.Role == "admin")
                {
                    var activeAdminCount = await _context.Users
                        .CountAsync(u => u.Role == "admin" && u.IsActive && u.Id != userId);
                    
                    if (activeAdminCount == 0)
                    {
                        result.AddError("Cannot delete the last active admin user", "LAST_ADMIN_DELETE_NOT_ALLOWED");
                        return result;
                    }
                }

                // Check for related data that would prevent deletion
                var hasCases = await _context.Cases.AnyAsync(c => c.ReportedByUserId == userId);
                var hasRunners = await _context.Runners.AnyAsync(r => r.UserId == userId);

                if (hasCases || hasRunners)
                {
                    result.AddWarning("User has related data (cases or runners). User will be deactivated instead of deleted.", "HAS_RELATED_DATA");
                    result.Data = new { hasCases, hasRunners, action = "deactivate" };
                }
                else
                {
                    result.Data = new { hasCases, hasRunners, action = "delete" };
                }

                result.IsValid = true;
                _logger.LogInformation("User deletion validation passed for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user deletion for user {UserId}", userId);
                result.AddError("Validation error occurred", "VALIDATION_ERROR");
            }

            return result;
        }

        /// <summary>
        /// Validate admin deletion with comprehensive checks
        /// </summary>
        public async Task<ValidationResult> ValidateAdminDeletion(int adminId, int currentAdminId)
        {
            var result = new ValidationResult();

            try
            {
                // Check if admin exists
                var admin = await _context.Users.FindAsync(adminId);
                if (admin == null)
                {
                    result.AddError("Admin not found", "ADMIN_NOT_FOUND");
                    return result;
                }

                // Verify the user is actually an admin
                if (admin.Role != "admin")
                {
                    result.AddError("User is not an admin", "NOT_AN_ADMIN");
                    return result;
                }

                // Prevent self-deletion
                if (admin.Id == currentAdminId)
                {
                    result.AddError("You cannot delete your own admin account", "SELF_DELETE_NOT_ALLOWED");
                    return result;
                }

                // Check if admin is the last admin
                var activeAdminCount = await _context.Users
                    .CountAsync(u => u.Role == "admin" && u.IsActive && u.Id != adminId);
                
                if (activeAdminCount == 0)
                {
                    result.AddError("Cannot delete the last active admin user", "LAST_ADMIN_DELETE_NOT_ALLOWED");
                    return result;
                }

                // Check for related data
                var hasCases = await _context.Cases.AnyAsync(c => c.ReportedByUserId == adminId);
                var hasRunners = await _context.Runners.AnyAsync(r => r.UserId == adminId);

                if (hasCases || hasRunners)
                {
                    result.AddWarning("Admin has related data (cases or runners). Admin will be deactivated instead of deleted.", "HAS_RELATED_DATA");
                    result.Data = new { hasCases, hasRunners, action = "deactivate" };
                }
                else
                {
                    result.Data = new { hasCases, hasRunners, action = "delete" };
                }

                result.IsValid = true;
                _logger.LogInformation("Admin deletion validation passed for admin {AdminId}", adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating admin deletion for admin {AdminId}", adminId);
                result.AddError("Validation error occurred", "VALIDATION_ERROR");
            }

            return result;
        }

        /// <summary>
        /// Validate user data before update
        /// </summary>
        public async Task<ValidationResult> ValidateUserUpdate(int userId, UserUpdateDto updateData)
        {
            var result = new ValidationResult();

            try
            {
                // Check if user exists
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    result.AddError("User not found", "USER_NOT_FOUND");
                    return result;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(updateData.FirstName))
                {
                    result.AddError("First name is required", "FIRST_NAME_REQUIRED");
                }
                else if (updateData.FirstName.Length > 100)
                {
                    result.AddError("First name cannot exceed 100 characters", "FIRST_NAME_TOO_LONG");
                }
                else if (!Regex.IsMatch(updateData.FirstName, @"^[a-zA-Z\s\-'\.]+$"))
                {
                    result.AddError("First name can only contain letters, spaces, hyphens, apostrophes, and periods", "FIRST_NAME_INVALID_FORMAT");
                }

                if (string.IsNullOrWhiteSpace(updateData.LastName))
                {
                    result.AddError("Last name is required", "LAST_NAME_REQUIRED");
                }
                else if (updateData.LastName.Length > 100)
                {
                    result.AddError("Last name cannot exceed 100 characters", "LAST_NAME_TOO_LONG");
                }
                else if (!Regex.IsMatch(updateData.LastName, @"^[a-zA-Z\s\-'\.]+$"))
                {
                    result.AddError("Last name can only contain letters, spaces, hyphens, apostrophes, and periods", "LAST_NAME_INVALID_FORMAT");
                }

                // Validate phone number if provided
                if (!string.IsNullOrWhiteSpace(updateData.PhoneNumber))
                {
                    if (!Regex.IsMatch(updateData.PhoneNumber, @"^[\+]?[1-9][\d]{0,15}$"))
                    {
                        result.AddError("Please enter a valid phone number", "PHONE_INVALID_FORMAT");
                    }
                    else if (updateData.PhoneNumber.Length > 20)
                    {
                        result.AddError("Phone number cannot exceed 20 characters", "PHONE_TOO_LONG");
                    }
                }

                // Validate address fields if provided
                if (!string.IsNullOrWhiteSpace(updateData.City) && !Regex.IsMatch(updateData.City, @"^[a-zA-Z\s\-'\.]+$"))
                {
                    result.AddError("City can only contain letters, spaces, hyphens, apostrophes, and periods", "CITY_INVALID_FORMAT");
                }

                if (!string.IsNullOrWhiteSpace(updateData.State) && !Regex.IsMatch(updateData.State, @"^[a-zA-Z\s\-'\.]+$"))
                {
                    result.AddError("State can only contain letters, spaces, hyphens, apostrophes, and periods", "STATE_INVALID_FORMAT");
                }

                if (!string.IsNullOrWhiteSpace(updateData.ZipCode) && !Regex.IsMatch(updateData.ZipCode, @"^[\d\-]+$"))
                {
                    result.AddError("Zip code can only contain numbers and hyphens", "ZIP_INVALID_FORMAT");
                }

                // Validate professional fields if provided
                if (!string.IsNullOrWhiteSpace(updateData.Organization) && !Regex.IsMatch(updateData.Organization, @"^[a-zA-Z0-9\s\-'\.&()]+$"))
                {
                    result.AddError("Organization can only contain letters, numbers, spaces, hyphens, apostrophes, periods, ampersands, and parentheses", "ORGANIZATION_INVALID_FORMAT");
                }

                if (!string.IsNullOrWhiteSpace(updateData.Title) && !Regex.IsMatch(updateData.Title, @"^[a-zA-Z\s\-'\.]+$"))
                {
                    result.AddError("Title can only contain letters, spaces, hyphens, apostrophes, and periods", "TITLE_INVALID_FORMAT");
                }

                // Validate emergency contact if provided
                if (!string.IsNullOrWhiteSpace(updateData.EmergencyContactPhone))
                {
                    if (!Regex.IsMatch(updateData.EmergencyContactPhone, @"^[\+]?[1-9][\d]{0,15}$"))
                    {
                        result.AddError("Please enter a valid emergency contact phone number", "EMERGENCY_PHONE_INVALID_FORMAT");
                    }
                }

                // Validate notes length
                if (!string.IsNullOrWhiteSpace(updateData.Notes) && updateData.Notes.Length > 2000)
                {
                    result.AddError("Notes cannot exceed 2000 characters", "NOTES_TOO_LONG");
                }

                result.IsValid = result.Errors.Count == 0;
                
                if (result.IsValid)
                {
                    _logger.LogInformation("User update validation passed for user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("User update validation failed for user {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Message)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user update for user {UserId}", userId);
                result.AddError("Validation error occurred", "VALIDATION_ERROR");
            }

            return result;
        }

        /// <summary>
        /// Validate case data before creation or update
        /// </summary>
        public async Task<ValidationResult> ValidateCaseData(CaseCreationDto caseData)
        {
            var result = new ValidationResult();

            try
            {
                // Validate required fields
                if (caseData.RunnerId <= 0)
                {
                    result.AddError("Valid runner ID is required", "RUNNER_ID_REQUIRED");
                }
                else
                {
                    // Check if runner exists
                    var runnerExists = await _context.Runners.AnyAsync(r => r.Id == caseData.RunnerId);
                    if (!runnerExists)
                    {
                        result.AddError("Runner not found", "RUNNER_NOT_FOUND");
                    }
                }

                if (string.IsNullOrWhiteSpace(caseData.Title))
                {
                    result.AddError("Case title is required", "TITLE_REQUIRED");
                }
                else if (caseData.Title.Length > 200)
                {
                    result.AddError("Case title cannot exceed 200 characters", "TITLE_TOO_LONG");
                }

                if (string.IsNullOrWhiteSpace(caseData.Description))
                {
                    result.AddError("Case description is required", "DESCRIPTION_REQUIRED");
                }
                else if (caseData.Description.Length > 2000)
                {
                    result.AddError("Case description cannot exceed 2000 characters", "DESCRIPTION_TOO_LONG");
                }

                if (string.IsNullOrWhiteSpace(caseData.LastSeenLocation))
                {
                    result.AddError("Last seen location is required", "LOCATION_REQUIRED");
                }
                else if (caseData.LastSeenLocation.Length > 500)
                {
                    result.AddError("Last seen location cannot exceed 500 characters", "LOCATION_TOO_LONG");
                }

                // Validate date
                if (caseData.LastSeenDate == default)
                {
                    result.AddError("Last seen date is required", "DATE_REQUIRED");
                }
                else if (caseData.LastSeenDate > DateTime.UtcNow)
                {
                    result.AddError("Last seen date cannot be in the future", "DATE_IN_FUTURE");
                }

                // Validate priority
                var validPriorities = new[] { "Low", "Medium", "High", "Critical" };
                if (!validPriorities.Contains(caseData.Priority))
                {
                    result.AddError("Priority must be one of: Low, Medium, High, Critical", "INVALID_PRIORITY");
                }

                // Validate optional fields
                if (!string.IsNullOrWhiteSpace(caseData.ContactPersonPhone) && 
                    !Regex.IsMatch(caseData.ContactPersonPhone, @"^[\+]?[1-9][\d]{0,15}$"))
                {
                    result.AddError("Please enter a valid contact phone number", "CONTACT_PHONE_INVALID");
                }

                if (!string.IsNullOrWhiteSpace(caseData.ContactPersonEmail) && 
                    !Regex.IsMatch(caseData.ContactPersonEmail, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                {
                    result.AddError("Please enter a valid contact email", "CONTACT_EMAIL_INVALID");
                }

                result.IsValid = result.Errors.Count == 0;
                
                if (result.IsValid)
                {
                    _logger.LogInformation("Case data validation passed");
                }
                else
                {
                    _logger.LogWarning("Case data validation failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Message)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating case data");
                result.AddError("Validation error occurred", "VALIDATION_ERROR");
            }

            return result;
        }

        /// <summary>
        /// Validate database constraints before operations
        /// </summary>
        public async Task<ValidationResult> ValidateDatabaseConstraints(string operation, int entityId, string entityType)
        {
            var result = new ValidationResult();

            try
            {
                switch (entityType.ToLower())
                {
                    case "user":
                        await ValidateUserConstraints(operation, entityId, result);
                        break;
                    case "case":
                        await ValidateCaseConstraints(operation, entityId, result);
                        break;
                    case "runner":
                        await ValidateRunnerConstraints(operation, entityId, result);
                        break;
                    default:
                        result.AddError($"Unknown entity type: {entityType}", "UNKNOWN_ENTITY_TYPE");
                        break;
                }

                result.IsValid = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating database constraints for {EntityType} {EntityId}", entityType, entityId);
                result.AddError("Constraint validation error occurred", "CONSTRAINT_VALIDATION_ERROR");
            }

            return result;
        }

        private async Task ValidateUserConstraints(string operation, int userId, ValidationResult result)
        {
            // Check if user has active cases
            var activeCases = await _context.Cases.CountAsync(c => c.ReportedByUserId == userId && c.Status == "Active");
            if (activeCases > 0)
            {
                result.AddWarning($"User has {activeCases} active cases", "HAS_ACTIVE_CASES");
            }

            // Check if user has runners
            var runnerCount = await _context.Runners.CountAsync(r => r.UserId == userId);
            if (runnerCount > 0)
            {
                result.AddWarning($"User has {runnerCount} associated runners", "HAS_RUNNERS");
            }
        }

        private async Task ValidateCaseConstraints(string operation, int caseId, ValidationResult result)
        {
            // Check if case has associated runner
            var caseExists = await _context.Cases
                .Include(c => c.Runner)
                .FirstOrDefaultAsync(c => c.Id == caseId);
            
            if (caseExists?.Runner == null)
            {
                result.AddError("Case must have an associated runner", "MISSING_RUNNER");
            }
        }

        private async Task ValidateRunnerConstraints(string operation, int runnerId, ValidationResult result)
        {
            // Check if runner has active cases
            var activeCases = await _context.Cases.CountAsync(c => c.RunnerId == runnerId && c.Status == "Active");
            if (activeCases > 0)
            {
                result.AddWarning($"Runner has {activeCases} active cases", "HAS_ACTIVE_CASES");
            }
        }
    }

    /// <summary>
    /// Validation result class
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<ValidationError> Errors { get; set; } = new();
        public List<ValidationWarning> Warnings { get; set; } = new();
        public object? Data { get; set; }

        public void AddError(string message, string code)
        {
            Errors.Add(new ValidationError { Message = message, Code = code });
            IsValid = false;
        }

        public void AddWarning(string message, string code)
        {
            Warnings.Add(new ValidationWarning { Message = message, Code = code });
        }
    }

    /// <summary>
    /// Validation error class
    /// </summary>
    public class ValidationError
    {
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validation warning class
    /// </summary>
    public class ValidationWarning
    {
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
