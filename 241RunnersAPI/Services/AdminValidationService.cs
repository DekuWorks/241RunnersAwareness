using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Validation service for admin operations
    /// </summary>
    public class AdminValidationService
    {
        /// <summary>
        /// Validates runner search parameters
        /// </summary>
        public static ValidationResult ValidateRunnerSearch(string? search, string? status, int page, int pageSize)
        {
            var errors = new List<string>();

            // Validate search term
            if (!string.IsNullOrEmpty(search))
            {
                if (search.Length > 100)
                    errors.Add("Search term cannot exceed 100 characters");
                
                if (ContainsInvalidCharacters(search))
                    errors.Add("Search term contains invalid characters");
            }

            // Validate status
            if (!string.IsNullOrEmpty(status))
            {
                var validStatuses = new[] { "Active", "Inactive", "Missing", "Found", "Resolved" };
                if (!validStatuses.Contains(status))
                    errors.Add($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
            }

            // Validate pagination
            if (page < 1)
                errors.Add("Page must be greater than 0");
            
            if (pageSize < 1 || pageSize > 100)
                errors.Add("Page size must be between 1 and 100");

            if (errors.Any())
            {
                var result = new ValidationResult();
                foreach (var error in errors)
                {
                    result.AddError(error, "VALIDATION_ERROR");
                }
                return result;
            }
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Validates case search parameters
        /// </summary>
        public static ValidationResult ValidateCaseSearch(string? search, string? status, int page, int pageSize)
        {
            var errors = new List<string>();

            // Validate search term
            if (!string.IsNullOrEmpty(search))
            {
                if (search.Length > 100)
                    errors.Add("Search term cannot exceed 100 characters");
                
                if (ContainsInvalidCharacters(search))
                    errors.Add("Search term contains invalid characters");
            }

            // Validate status
            if (!string.IsNullOrEmpty(status))
            {
                var validStatuses = new[] { "Open", "In Progress", "Resolved", "Closed", "Active", "Missing" };
                if (!validStatuses.Contains(status))
                    errors.Add($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
            }

            // Validate pagination
            if (page < 1)
                errors.Add("Page must be greater than 0");
            
            if (pageSize < 1 || pageSize > 100)
                errors.Add("Page size must be between 1 and 100");

            if (errors.Any())
            {
                var result = new ValidationResult();
                foreach (var error in errors)
                {
                    result.AddError(error, "VALIDATION_ERROR");
                }
                return result;
            }
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Validates analytics time range
        /// </summary>
        public static ValidationResult ValidateAnalyticsTimeRange(int days)
        {
            if (days < 1)
            {
                var result = new ValidationResult();
                result.AddError("Days must be greater than 0", "INVALID_DAYS");
                return result;
            }
            
            if (days > 365)
            {
                var result = new ValidationResult();
                result.AddError("Days cannot exceed 365", "INVALID_DAYS");
                return result;
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Validates runner ID for admin operations
        /// </summary>
        public static ValidationResult ValidateRunnerId(int runnerId)
        {
            if (runnerId <= 0)
            {
                var result = new ValidationResult();
                result.AddError("Invalid runner ID", "INVALID_RUNNER_ID");
                return result;
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Validates case ID for admin operations
        /// </summary>
        public static ValidationResult ValidateCaseId(int caseId)
        {
            if (caseId <= 0)
            {
                var result = new ValidationResult();
                result.AddError("Invalid case ID", "INVALID_CASE_ID");
                return result;
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Validates export parameters
        /// </summary>
        public static ValidationResult ValidateExportParameters(string format, string? entityType)
        {
            var errors = new List<string>();

            // Validate format
            var validFormats = new[] { "csv", "json", "xlsx" };
            if (!validFormats.Contains(format.ToLower()))
                errors.Add($"Invalid format. Must be one of: {string.Join(", ", validFormats)}");

            // Validate entity type
            if (!string.IsNullOrEmpty(entityType))
            {
                var validEntityTypes = new[] { "runners", "cases", "users", "analytics" };
                if (!validEntityTypes.Contains(entityType.ToLower()))
                    errors.Add($"Invalid entity type. Must be one of: {string.Join(", ", validEntityTypes)}");
            }

            if (errors.Any())
            {
                var result = new ValidationResult();
                foreach (var error in errors)
                {
                    result.AddError(error, "VALIDATION_ERROR");
                }
                return result;
            }
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Validates bulk operation parameters
        /// </summary>
        public static ValidationResult ValidateBulkOperation(string operation, List<int> ids)
        {
            var errors = new List<string>();

            // Validate operation
            var validOperations = new[] { "activate", "deactivate", "delete", "export" };
            if (!validOperations.Contains(operation.ToLower()))
                errors.Add($"Invalid operation. Must be one of: {string.Join(", ", validOperations)}");

            // Validate IDs
            if (ids == null || !ids.Any())
                errors.Add("At least one ID must be provided");

            if (ids.Count > 100)
                errors.Add("Cannot process more than 100 items at once");

            if (ids.Any(id => id <= 0))
                errors.Add("All IDs must be greater than 0");

            if (errors.Any())
            {
                var result = new ValidationResult();
                foreach (var error in errors)
                {
                    result.AddError(error, "VALIDATION_ERROR");
                }
                return result;
            }
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Validates admin action parameters
        /// </summary>
        public static ValidationResult ValidateAdminAction(string action, Dictionary<string, object> parameters)
        {
            var errors = new List<string>();

            // Validate action
            var validActions = new[] { "verify", "unverify", "activate", "deactivate", "approve", "reject" };
            if (!validActions.Contains(action.ToLower()))
                errors.Add($"Invalid action. Must be one of: {string.Join(", ", validActions)}");

            // Validate parameters based on action
            switch (action.ToLower())
            {
                case "verify":
                case "unverify":
                    if (!parameters.ContainsKey("reason") || string.IsNullOrEmpty(parameters["reason"]?.ToString()))
                        errors.Add("Reason is required for verification actions");
                    break;
                
                case "approve":
                case "reject":
                    if (!parameters.ContainsKey("notes") || string.IsNullOrEmpty(parameters["notes"]?.ToString()))
                        errors.Add("Notes are required for approval/rejection actions");
                    break;
            }

            if (errors.Any())
            {
                var result = new ValidationResult();
                foreach (var error in errors)
                {
                    result.AddError(error, "VALIDATION_ERROR");
                }
                return result;
            }
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Checks if string contains invalid characters
        /// </summary>
        private static bool ContainsInvalidCharacters(string input)
        {
            // Allow letters, numbers, spaces, hyphens, apostrophes, periods, and common punctuation
            var validPattern = @"^[a-zA-Z0-9\s\-'.,!?()&]+$";
            return !Regex.IsMatch(input, validPattern);
        }

        /// <summary>
        /// Sanitizes input for admin operations
        /// </summary>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove potentially dangerous characters
            var sanitized = Regex.Replace(input, @"[<>""'&]", "");
            
            // Trim and limit length
            return sanitized.Trim().Substring(0, Math.Min(sanitized.Length, 1000));
        }

        /// <summary>
        /// Validates date range for analytics
        /// </summary>
        public static ValidationResult ValidateDateRange(DateTime startDate, DateTime endDate)
        {
            var errors = new List<string>();

            if (startDate >= endDate)
                errors.Add("Start date must be before end date");

            if (startDate < DateTime.UtcNow.AddYears(-1))
                errors.Add("Start date cannot be more than 1 year ago");

            if (endDate > DateTime.UtcNow)
                errors.Add("End date cannot be in the future");

            if (errors.Any())
            {
                var result = new ValidationResult();
                foreach (var error in errors)
                {
                    result.AddError(error, "VALIDATION_ERROR");
                }
                return result;
            }
            return new ValidationResult { IsValid = true };
        }
    }
}
