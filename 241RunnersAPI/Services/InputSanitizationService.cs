using System.Text.RegularExpressions;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Comprehensive input sanitization service for security validation
    /// </summary>
    public class InputSanitizationService
    {
        private readonly ILogger<InputSanitizationService> _logger;
        private static readonly string[] DangerousPatterns = {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", // Script tags
            @"javascript:", // JavaScript URLs
            @"vbscript:", // VBScript URLs
            @"onload\s*=", // Event handlers
            @"onerror\s*=",
            @"onclick\s*=",
            @"onmouseover\s*=",
            @"expression\s*\(", // CSS expressions
            @"url\s*\(", // CSS URLs
            @"@import", // CSS imports
            @"eval\s*\(", // JavaScript eval
            @"setTimeout\s*\(",
            @"setInterval\s*\(",
            @"document\.", // Document object access
            @"window\.", // Window object access
            @"alert\s*\(",
            @"confirm\s*\(",
            @"prompt\s*\("
        };

        private static readonly string[] SqlInjectionPatterns = {
            @"('|(\\')|(;)|(--)|(\|)|(\*)|(%)|(\+)|(\<)|(\>)|(\[)|(\])|(\{)|(\})|(\()|(\))|(\=)|(\!)|(\&)|(\|)|(\^)|(\~)|(\`)|(\?)",
            @"(union|select|insert|delete|update|drop|create|alter|exec|execute|sp_|xp_)",
            @"(\bor\b|\band\b)",
            @"(\bnull\b|\btrue\b|\bfalse\b)",
            @"(0x[0-9a-fA-F]+)",
            @"(\bwaitfor\b|\bdelay\b)",
            @"(\bchar\b|\bnchar\b|\bvarchar\b|\bnvarchar\b)"
        };

        public InputSanitizationService(ILogger<InputSanitizationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sanitizes input to prevent XSS attacks
        /// </summary>
        public string SanitizeHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            try
            {
                // HTML encode the input
                var sanitized = HttpUtility.HtmlEncode(input);
                
                // Additional XSS pattern removal
                foreach (var pattern in DangerousPatterns)
                {
                    sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }

                _logger.LogDebug("HTML input sanitized successfully");
                return sanitized.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing HTML input");
                return string.Empty;
            }
        }

        /// <summary>
        /// Sanitizes rich text content while preserving safe HTML tags
        /// </summary>
        public string SanitizeRichText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            try
            {
                // Allowed HTML tags for rich text
                var allowedTags = new[] { "p", "br", "strong", "em", "ul", "ol", "li", "h1", "h2", "h3", "h4", "h5", "h6", "blockquote" };
                
                // Remove all script tags and dangerous patterns
                foreach (var pattern in DangerousPatterns)
                {
                    input = Regex.Replace(input, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }

                // Remove all HTML tags except allowed ones
                var pattern = $@"<(?!\/?(?:{string.Join("|", allowedTags)})\b)[^>]*>";
                input = Regex.Replace(input, pattern, "", RegexOptions.IgnoreCase);

                // Remove dangerous attributes from allowed tags
                input = Regex.Replace(input, @"\s*on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
                input = Regex.Replace(input, @"\s*style\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
                input = Regex.Replace(input, @"\s*class\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);

                _logger.LogDebug("Rich text input sanitized successfully");
                return input.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing rich text input");
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates and sanitizes text input for SQL injection prevention
        /// </summary>
        public string SanitizeForSql(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            try
            {
                var sanitized = input;

                // Check for SQL injection patterns
                foreach (var pattern in SqlInjectionPatterns)
                {
                    if (Regex.IsMatch(sanitized, pattern, RegexOptions.IgnoreCase))
                    {
                        _logger.LogWarning("Potential SQL injection pattern detected in input: {Pattern}", pattern);
                        sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase);
                    }
                }

                // Remove or escape dangerous characters
                sanitized = sanitized.Replace("'", "''"); // Escape single quotes for SQL
                sanitized = sanitized.Replace(";", ""); // Remove semicolons
                sanitized = sanitized.Replace("--", ""); // Remove SQL comments
                sanitized = sanitized.Replace("/*", ""); // Remove block comment starts
                sanitized = sanitized.Replace("*/", ""); // Remove block comment ends

                _logger.LogDebug("SQL input sanitized successfully");
                return sanitized.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing SQL input");
                return string.Empty;
            }
        }

        /// <summary>
        /// Sanitizes file names to prevent path traversal attacks
        /// </summary>
        public string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "sanitized_file";

            try
            {
                // Remove path traversal attempts
                fileName = fileName.Replace("..", "").Replace("/", "").Replace("\\", "");
                
                // Remove dangerous characters
                var invalidChars = Path.GetInvalidFileNameChars();
                fileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
                
                // Limit length
                if (fileName.Length > 255)
                    fileName = fileName.Substring(0, 255);
                
                // Ensure it's not empty after sanitization
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = "sanitized_file";

                _logger.LogDebug("File name sanitized successfully");
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing file name");
                return "sanitized_file";
            }
        }

        /// <summary>
        /// Validates and sanitizes URL inputs
        /// </summary>
        public string SanitizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            try
            {
                // Check if it's a valid URL format
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    _logger.LogWarning("Invalid URL format: {Url}", url);
                    return string.Empty;
                }

                // Only allow HTTP and HTTPS protocols
                if (uri.Scheme != "http" && uri.Scheme != "https")
                {
                    _logger.LogWarning("Invalid URL scheme: {Scheme}", uri.Scheme);
                    return string.Empty;
                }

                // Remove dangerous characters and patterns
                var sanitized = uri.ToString();
                foreach (var pattern in DangerousPatterns)
                {
                    sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase);
                }

                _logger.LogDebug("URL sanitized successfully");
                return sanitized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing URL");
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates and sanitizes email addresses
        /// </summary>
        public string SanitizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            try
            {
                // Basic email format validation
                var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                if (!Regex.IsMatch(email, emailPattern))
                {
                    _logger.LogWarning("Invalid email format: {Email}", email);
                    return string.Empty;
                }

                // Remove any potential XSS or SQL injection
                var sanitized = SanitizeHtml(email);
                sanitized = SanitizeForSql(sanitized);

                _logger.LogDebug("Email sanitized successfully");
                return sanitized.ToLowerInvariant().Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing email");
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates and sanitizes phone numbers
        /// </summary>
        public string SanitizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            try
            {
                // Remove all non-digit characters except + at the beginning
                var sanitized = Regex.Replace(phoneNumber, @"[^\d+]", "");
                
                // Ensure + is only at the beginning
                if (sanitized.StartsWith("+"))
                {
                    sanitized = "+" + Regex.Replace(sanitized.Substring(1), @"[^\d]", "");
                }
                else
                {
                    sanitized = Regex.Replace(sanitized, @"[^\d]", "");
                }

                // Validate length (7-15 digits is typical for phone numbers)
                if (sanitized.Length < 7 || sanitized.Length > 15)
                {
                    _logger.LogWarning("Invalid phone number length: {Length}", sanitized.Length);
                    return string.Empty;
                }

                _logger.LogDebug("Phone number sanitized successfully");
                return sanitized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing phone number");
                return string.Empty;
            }
        }

        /// <summary>
        /// Detects potential malicious content in input
        /// </summary>
        public bool IsMaliciousContent(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            try
            {
                // Check for XSS patterns
                foreach (var pattern in DangerousPatterns)
                {
                    if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    {
                        _logger.LogWarning("Malicious XSS pattern detected: {Pattern}", pattern);
                        return true;
                    }
                }

                // Check for SQL injection patterns
                foreach (var pattern in SqlInjectionPatterns)
                {
                    if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    {
                        _logger.LogWarning("Potential SQL injection pattern detected: {Pattern}", pattern);
                        return true;
                    }
                }

                // Check for excessive special characters (potential obfuscation)
                var specialCharCount = input.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
                if (specialCharCount > input.Length * 0.3) // More than 30% special characters
                {
                    _logger.LogWarning("Excessive special characters detected in input");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting malicious content");
                return true; // Err on the side of caution
            }
        }

        /// <summary>
        /// Calculates a hash for input to detect duplicates or suspicious patterns
        /// </summary>
        public string CalculateInputHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            try
            {
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input.ToLowerInvariant()));
                return Convert.ToHexString(hashBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating input hash");
                return string.Empty;
            }
        }
    }
}
