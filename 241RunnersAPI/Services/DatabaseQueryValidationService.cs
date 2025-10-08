using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Reflection;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for database query validation and SQL injection prevention
    /// </summary>
    public class DatabaseQueryValidationService
    {
        private readonly ILogger<DatabaseQueryValidationService> _logger;
        
        // Dangerous SQL patterns
        private static readonly string[] DangerousSqlPatterns = {
            @"\b(union|select|insert|delete|update|drop|create|alter|exec|execute)\b",
            @"\b(sp_|xp_|fn_)\b", // SQL Server system procedures
            @"\b(openrowset|opendatasource|bulk\s+insert)\b",
            @"\b(waitfor|delay|shutdown|kill)\b",
            @"\b(char|nchar|varchar|nvarchar|sysname)\s*\(",
            @"\b(ascii|charindex|patindex|soundex|difference)\s*\(",
            @"\b(cast|convert|parse|try_parse)\s*\(",
            @"\b(quotename|char|nchar|space|replicate)\s*\(",
            @"\b(db_name|user_name|suser_name|is_srvrolemember)\s*\(",
            @"\b(host_name|app_name|connectionproperty)\s*\(",
            @"\b(serverproperty|databaseproperty|objectproperty)\s*\(",
            @"\b(col_length|col_name|index_col|indexproperty)\s*\(",
            @"\b(schema_name|object_name|object_id|type_name)\s*\(",
            @"\b(fn_listextendedproperty|fn_helpcollations)\s*\(",
            @"\b(xp_cmdshell|xp_regread|xp_regwrite|xp_dirtree)\b",
            @"\b(master\.dbo\.|msdb\.dbo\.|tempdb\.dbo\.)\b",
            @"\b(information_schema\.|sys\.)\b",
            @"--|\/\*|\*\/", // Comments
            @"0x[0-9a-fA-F]+", // Hex strings
            @"@@[a-zA-Z_][a-zA-Z0-9_]*", // Global variables
            @"sp_executesql", // Dynamic SQL execution
            @"exec\s*\(", // Dynamic execution
            @"execute\s*\(",
            @"\b(declare|set|if|else|while|begin|end)\b"
        };

        // Parameter validation patterns
        private static readonly Dictionary<string, string> ParameterPatterns = new()
        {
            { "email", @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$" },
            { "phone", @"^[\+]?[1-9][\d]{0,15}$" },
            { "name", @"^[a-zA-Z\s\-'\.]+$" },
            { "id", @"^\d+$" },
            { "guid", @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$" },
            { "url", @"^https?:\/\/[^\s]+$" },
            { "zipcode", @"^[\d\-]+$" },
            { "date", @"^\d{4}-\d{2}-\d{2}$" },
            { "datetime", @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}" }
        };

        public DatabaseQueryValidationService(ILogger<DatabaseQueryValidationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates query parameters for SQL injection attempts
        /// </summary>
        public QueryValidationResult ValidateQueryParameters(object parameters)
        {
            var result = new QueryValidationResult { IsValid = true };

            if (parameters == null)
            {
                result.IsValid = false;
                result.Errors.Add("Parameters cannot be null");
                return result;
            }

            try
            {
                var properties = parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    var value = property.GetValue(parameters);
                    if (value == null) continue;

                    var parameterName = property.Name;
                    var stringValue = value.ToString();

                    // Check for SQL injection patterns
                    if (ContainsSqlInjection(stringValue))
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Parameter '{parameterName}' contains potential SQL injection: {stringValue}");
                    }

                    // Validate parameter format based on name/type
                    var validationError = ValidateParameterFormat(parameterName, stringValue, property.PropertyType);
                    if (!string.IsNullOrEmpty(validationError))
                    {
                        result.IsValid = false;
                        result.Errors.Add(validationError);
                    }

                    // Check parameter length
                    if (stringValue.Length > GetMaxLengthForParameter(parameterName))
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Parameter '{parameterName}' exceeds maximum length");
                    }
                }

                _logger.LogDebug("Query parameter validation completed. Valid: {IsValid}", result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating query parameters");
                result.IsValid = false;
                result.Errors.Add("Parameter validation error occurred");
                return result;
            }
        }

        /// <summary>
        /// Validates dynamic SQL queries for security risks
        /// </summary>
        public QueryValidationResult ValidateDynamicQuery(string query, Dictionary<string, object> parameters = null)
        {
            var result = new QueryValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(query))
            {
                result.IsValid = false;
                result.Errors.Add("Query cannot be empty");
                return result;
            }

            try
            {
                var lowerQuery = query.ToLowerInvariant();

                // Check for dangerous SQL patterns
                foreach (var pattern in DangerousSqlPatterns)
                {
                    if (Regex.IsMatch(lowerQuery, pattern, RegexOptions.IgnoreCase))
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Query contains dangerous SQL pattern: {pattern}");
                    }
                }

                // Check for dynamic SQL execution
                if (lowerQuery.Contains("exec") || lowerQuery.Contains("execute") || lowerQuery.Contains("sp_executesql"))
                {
                    result.IsValid = false;
                    result.Errors.Add("Dynamic SQL execution is not allowed");
                }

                // Check for system database access
                if (lowerQuery.Contains("master.") || lowerQuery.Contains("msdb.") || lowerQuery.Contains("tempdb."))
                {
                    result.IsValid = false;
                    result.Errors.Add("System database access is not allowed");
                }

                // Check for information schema access
                if (lowerQuery.Contains("information_schema.") || lowerQuery.Contains("sys."))
                {
                    result.IsValid = false;
                    result.Errors.Add("System schema access is not allowed");
                }

                // Validate parameters if provided
                if (parameters != null)
                {
                    var paramResult = ValidateQueryParameters(parameters);
                    if (!paramResult.IsValid)
                    {
                        result.IsValid = false;
                        result.Errors.AddRange(paramResult.Errors);
                    }
                }

                // Check query complexity (basic heuristics)
                var complexityScore = CalculateQueryComplexity(query);
                if (complexityScore > 10) // Arbitrary threshold
                {
                    result.IsValid = false;
                    result.Errors.Add("Query complexity exceeds allowed limits");
                    result.ComplexityScore = complexityScore;
                }

                _logger.LogDebug("Dynamic query validation completed. Valid: {IsValid}, Complexity: {Complexity}", 
                    result.IsValid, complexityScore);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating dynamic query");
                result.IsValid = false;
                result.Errors.Add("Query validation error occurred");
                return result;
            }
        }

        /// <summary>
        /// Validates Entity Framework LINQ queries for security
        /// </summary>
        public QueryValidationResult ValidateLinqQuery(IQueryable query, string queryDescription = "")
        {
            var result = new QueryValidationResult { IsValid = true };

            try
            {
                // Convert IQueryable to string representation for analysis
                var queryString = query.ToString();
                
                // Basic validation of query string representation
                if (ContainsSqlInjection(queryString))
                {
                    result.IsValid = false;
                    result.Errors.Add("LINQ query contains potential SQL injection patterns");
                }

                // Check for dangerous method calls
                var dangerousMethods = new[]
                {
                    "ExecuteSqlRaw", "ExecuteSqlInterpolated", "FromSqlRaw", "FromSqlInterpolated",
                    "Database.SqlQuery", "Database.ExecuteSqlCommand"
                };

                foreach (var method in dangerousMethods)
                {
                    if (queryString.Contains(method))
                    {
                        result.IsValid = false;
                        result.Errors.Add($"LINQ query uses potentially dangerous method: {method}");
                    }
                }

                _logger.LogDebug("LINQ query validation completed for {Description}. Valid: {IsValid}", 
                    queryDescription, result.IsValid);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating LINQ query");
                result.IsValid = false;
                result.Errors.Add("LINQ query validation error occurred");
                return result;
            }
        }

        /// <summary>
        /// Checks if a string contains SQL injection patterns
        /// </summary>
        private bool ContainsSqlInjection(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                var lowerInput = input.ToLowerInvariant();

                // Check for comment patterns
                if (lowerInput.Contains("--") || lowerInput.Contains("/*") || lowerInput.Contains("*/"))
                {
                    return true;
                }

                // Check for quote manipulation
                var singleQuoteCount = input.Count(c => c == '\'');
                var doubleQuoteCount = input.Count(c => c == '"');
                if (singleQuoteCount % 2 != 0 || doubleQuoteCount % 2 != 0)
                {
                    return true;
                }

                // Check for dangerous SQL patterns
                foreach (var pattern in DangerousSqlPatterns)
                {
                    if (Regex.IsMatch(lowerInput, pattern, RegexOptions.IgnoreCase))
                    {
                        return true;
                    }
                }

                // Check for hex patterns (potential binary injection)
                if (Regex.IsMatch(input, @"0x[0-9a-fA-F]{4,}"))
                {
                    return true;
                }

                // Check for semicolon injection
                if (input.Contains(";"))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for SQL injection patterns");
                return true; // Err on the side of caution
            }
        }

        /// <summary>
        /// Validates parameter format based on parameter name and type
        /// </summary>
        private string ValidateParameterFormat(string parameterName, string value, Type parameterType)
        {
            try
            {
                // Get parameter type name for pattern matching
                var typeName = parameterType.Name.ToLowerInvariant();
                var paramName = parameterName.ToLowerInvariant();

                // Check specific parameter patterns
                foreach (var kvp in ParameterPatterns)
                {
                    if (paramName.Contains(kvp.Key) || typeName.Contains(kvp.Key))
                    {
                        if (!Regex.IsMatch(value, kvp.Value))
                        {
                            return $"Parameter '{parameterName}' has invalid format for {kvp.Key}";
                        }
                    }
                }

                // Type-specific validations
                if (parameterType == typeof(int) || parameterType == typeof(int?))
                {
                    if (!int.TryParse(value, out _))
                    {
                        return $"Parameter '{parameterName}' must be a valid integer";
                    }
                }
                else if (parameterType == typeof(decimal) || parameterType == typeof(decimal?))
                {
                    if (!decimal.TryParse(value, out _))
                    {
                        return $"Parameter '{parameterName}' must be a valid decimal number";
                    }
                }
                else if (parameterType == typeof(DateTime) || parameterType == typeof(DateTime?))
                {
                    if (!DateTime.TryParse(value, out _))
                    {
                        return $"Parameter '{parameterName}' must be a valid date/time";
                    }
                }
                else if (parameterType == typeof(bool) || parameterType == typeof(bool?))
                {
                    if (!bool.TryParse(value, out _))
                    {
                        return $"Parameter '{parameterName}' must be a valid boolean";
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating parameter format");
                return "Parameter format validation error";
            }
        }

        /// <summary>
        /// Gets maximum length for a parameter based on its name/type
        /// </summary>
        private int GetMaxLengthForParameter(string parameterName)
        {
            var paramName = parameterName.ToLowerInvariant();

            return paramName switch
            {
                var name when name.Contains("email") => 255,
                var name when name.Contains("phone") => 20,
                var name when name.Contains("name") => 100,
                var name when name.Contains("title") => 200,
                var name when name.Contains("description") => 2000,
                var name when name.Contains("url") => 500,
                var name when name.Contains("address") => 500,
                var name when name.Contains("city") => 100,
                var name when name.Contains("state") => 50,
                var name when name.Contains("zipcode") => 20,
                var name when name.Contains("notes") => 2000,
                _ => 1000 // Default maximum length
            };
        }

        /// <summary>
        /// Calculates query complexity score
        /// </summary>
        private int CalculateQueryComplexity(string query)
        {
            try
            {
                var complexity = 0;
                var lowerQuery = query.ToLowerInvariant();

                // Count joins
                complexity += Regex.Matches(lowerQuery, @"\bjoin\b").Count;

                // Count subqueries
                complexity += Regex.Matches(lowerQuery, @"\bselect\b").Count - 1; // Subtract 1 for main query

                // Count unions
                complexity += Regex.Matches(lowerQuery, @"\bunion\b").Count;

                // Count group by clauses
                complexity += Regex.Matches(lowerQuery, @"\bgroup\s+by\b").Count;

                // Count order by clauses
                complexity += Regex.Matches(lowerQuery, @"\border\s+by\b").Count;

                // Count having clauses
                complexity += Regex.Matches(lowerQuery, @"\bhaving\b").Count;

                // Count case statements
                complexity += Regex.Matches(lowerQuery, @"\bcase\b").Count;

                // Count functions
                complexity += Regex.Matches(lowerQuery, @"\b(count|sum|avg|min|max|distinct)\s*\(").Count;

                return complexity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating query complexity");
                return 0;
            }
        }

        /// <summary>
        /// Validates database connection string for security
        /// </summary>
        public bool ValidateConnectionString(string connectionString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _logger.LogWarning("Connection string is null or empty");
                    return false;
                }

                // Check for dangerous connection string parameters
                var dangerousParams = new[]
                {
                    "Integrated Security=false", // Should use integrated security when possible
                    "TrustServerCertificate=true", // Should validate certificates
                    "Encrypt=false" // Should use encryption
                };

                foreach (var param in dangerousParams)
                {
                    if (connectionString.ToLowerInvariant().Contains(param.ToLowerInvariant()))
                    {
                        _logger.LogWarning("Connection string contains potentially insecure parameter: {Param}", param);
                        // Don't return false, just log warning
                    }
                }

                // Check for hardcoded credentials (basic check)
                if (connectionString.Contains("Password=") || connectionString.Contains("Pwd="))
                {
                    _logger.LogInformation("Connection string contains password parameter");
                    // In production, you might want to validate that passwords are not hardcoded
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating connection string");
                return false;
            }
        }

        /// <summary>
        /// Logs database query security events
        /// </summary>
        public void LogQuerySecurityEvent(string eventType, string query, QueryValidationResult result, string userId = null)
        {
            try
            {
                var logMessage = $"Database Query Security Event: {eventType}";
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    logMessage += $" from user {userId}";
                }

                if (result.Errors.Count > 0)
                {
                    logMessage += $" - Errors: {string.Join(", ", result.Errors)}";
                }

                if (result.ComplexityScore > 0)
                {
                    logMessage += $" - Complexity Score: {result.ComplexityScore}";
                }

                _logger.LogWarning(logMessage);

                // In production, you might want to:
                // - Send alerts to security team
                // - Store in security database
                // - Trigger automated responses
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging query security event");
            }
        }

        /// <summary>
        /// Sanitizes query parameters by removing dangerous characters
        /// </summary>
        public Dictionary<string, object> SanitizeParameters(Dictionary<string, object> parameters)
        {
            var sanitized = new Dictionary<string, object>();

            try
            {
                foreach (var kvp in parameters)
                {
                    var key = kvp.Key;
                    var value = kvp.Value;

                    if (value is string stringValue)
                    {
                        // Remove dangerous characters
                        var sanitizedValue = stringValue
                            .Replace("'", "''") // Escape single quotes
                            .Replace(";", "") // Remove semicolons
                            .Replace("--", "") // Remove SQL comments
                            .Replace("/*", "") // Remove block comment starts
                            .Replace("*/", ""); // Remove block comment ends

                        sanitized[key] = sanitizedValue;
                    }
                    else
                    {
                        sanitized[key] = value;
                    }
                }

                return sanitized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing parameters");
                return parameters; // Return original on error
            }
        }
    }

    /// <summary>
    /// Result of database query validation
    /// </summary>
    public class QueryValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public int ComplexityScore { get; set; } = 0;
        public TimeSpan EstimatedExecutionTime { get; set; } = TimeSpan.Zero;
        public int EstimatedRowCount { get; set; } = 0;
    }
}
