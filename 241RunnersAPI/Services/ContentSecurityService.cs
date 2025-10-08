using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for content security validation, spam detection, and content filtering
    /// </summary>
    public class ContentSecurityService
    {
        private readonly ILogger<ContentSecurityService> _logger;
        
        // Spam patterns and keywords
        private static readonly string[] SpamKeywords = {
            "viagra", "cialis", "casino", "lottery", "winner", "congratulations", "click here",
            "free money", "make money", "work from home", "bitcoin", "cryptocurrency",
            "investment", "loan", "credit", "debt", "insurance", "mortgage", "refinance",
            "weight loss", "diet", "supplement", "pharmacy", "prescription", "generic",
            "seo", "marketing", "advertising", "promotion", "discount", "offer", "deal",
            "dating", "single", "marriage", "relationship", "love", "sex", "adult",
            "porn", "xxx", "escort", "prostitute", "massage", "spa"
        };

        // Inappropriate content patterns
        private static readonly string[] InappropriatePatterns = {
            @"\b(fuck|shit|damn|hell|bitch|asshole|bastard|cunt|whore|slut)\b",
            @"\b(kill|murder|suicide|bomb|terrorist|attack|violence|weapon)\b",
            @"\b(hate|racist|nazi|kkk|white supremacy|discrimination)\b",
            @"\b(drug|marijuana|cocaine|heroin|meth|addiction|overdose)\b",
            @"\b(scam|fraud|steal|robbery|theft|illegal|crime)\b"
        };

        // Suspicious link patterns
        private static readonly string[] SuspiciousLinkPatterns = {
            @"http[s]?://(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+",
            @"www\.(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+",
            @"[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}"
        };

        // Known spam domains (simplified list)
        private static readonly string[] KnownSpamDomains = {
            "spam.com", "malicious.org", "phishing.net", "scam.info", "fake.biz"
        };

        public ContentSecurityService(ILogger<ContentSecurityService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates content for spam, inappropriate material, and security threats
        /// </summary>
        public ContentValidationResult ValidateContent(string content, string contentType = "text")
        {
            var result = new ContentValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(content))
            {
                result.IsValid = false;
                result.Errors.Add("Content cannot be empty");
                return result;
            }

            try
            {
                // Check for spam keywords
                var spamScore = CalculateSpamScore(content);
                if (spamScore > 0.7) // 70% spam threshold
                {
                    result.IsValid = false;
                    result.Errors.Add("Content appears to be spam");
                    result.SpamScore = spamScore;
                }

                // Check for inappropriate content
                var inappropriateScore = CalculateInappropriateScore(content);
                if (inappropriateScore > 0.5) // 50% inappropriate threshold
                {
                    result.IsValid = false;
                    result.Errors.Add("Content contains inappropriate material");
                    result.InappropriateScore = inappropriateScore;
                }

                // Check for suspicious links
                var suspiciousLinks = DetectSuspiciousLinks(content);
                if (suspiciousLinks.Count > 0)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Content contains {suspiciousLinks.Count} suspicious links");
                    result.SuspiciousLinks = suspiciousLinks;
                }

                // Check for excessive repetition (potential bot/spam)
                if (HasExcessiveRepetition(content))
                {
                    result.IsValid = false;
                    result.Errors.Add("Content contains excessive repetition");
                }

                // Check for suspicious patterns
                if (HasSuspiciousPatterns(content))
                {
                    result.IsValid = false;
                    result.Errors.Add("Content contains suspicious patterns");
                }

                // Check content length and complexity
                if (!IsReasonableContent(content, contentType))
                {
                    result.IsValid = false;
                    result.Errors.Add("Content length or complexity is unreasonable");
                }

                // Calculate overall risk score
                result.RiskScore = CalculateOverallRiskScore(content);

                _logger.LogDebug("Content validation completed. Valid: {IsValid}, Risk Score: {RiskScore}", 
                    result.IsValid, result.RiskScore);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating content");
                result.IsValid = false;
                result.Errors.Add("Content validation error occurred");
                return result;
            }
        }

        /// <summary>
        /// Calculates spam score based on keyword density and patterns
        /// </summary>
        private double CalculateSpamScore(string content)
        {
            try
            {
                var lowerContent = content.ToLowerInvariant();
                var words = lowerContent.Split(new char[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':' }, 
                    StringSplitOptions.RemoveEmptyEntries);
                
                if (words.Length == 0) return 0;

                var spamWordCount = 0;
                var totalWords = words.Length;

                // Check for spam keywords
                foreach (var keyword in SpamKeywords)
                {
                    var keywordCount = Regex.Matches(lowerContent, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase).Count;
                    spamWordCount += keywordCount;
                }

                // Check for excessive capitalization (spam indicator)
                var capsRatio = content.Count(char.IsUpper) / (double)content.Length;
                if (capsRatio > 0.5) spamWordCount += 2;

                // Check for excessive punctuation (spam indicator)
                var punctRatio = content.Count(c => "!?.".Contains(c)) / (double)content.Length;
                if (punctRatio > 0.1) spamWordCount += 1;

                // Check for repeated characters (spam indicator)
                if (HasRepeatedCharacters(content)) spamWordCount += 2;

                return Math.Min(spamWordCount / (double)totalWords, 1.0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating spam score");
                return 0;
            }
        }

        /// <summary>
        /// Calculates inappropriate content score
        /// </summary>
        private double CalculateInappropriateScore(string content)
        {
            try
            {
                var lowerContent = content.ToLowerInvariant();
                var inappropriateCount = 0;
                var totalWords = content.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

                if (totalWords == 0) return 0;

                foreach (var pattern in InappropriatePatterns)
                {
                    var matches = Regex.Matches(lowerContent, pattern, RegexOptions.IgnoreCase);
                    inappropriateCount += matches.Count;
                }

                return Math.Min(inappropriateCount / (double)totalWords, 1.0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating inappropriate score");
                return 0;
            }
        }

        /// <summary>
        /// Detects suspicious links in content
        /// </summary>
        private List<string> DetectSuspiciousLinks(string content)
        {
            var suspiciousLinks = new List<string>();

            try
            {
                foreach (var pattern in SuspiciousLinkPatterns)
                {
                    var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        var link = match.Value.ToLowerInvariant();
                        
                        // Check against known spam domains
                        foreach (var spamDomain in KnownSpamDomains)
                        {
                            if (link.Contains(spamDomain))
                            {
                                suspiciousLinks.Add(match.Value);
                                break;
                            }
                        }

                        // Check for suspicious patterns in URLs
                        if (link.Contains("bit.ly") || link.Contains("tinyurl") || link.Contains("short.link"))
                        {
                            suspiciousLinks.Add(match.Value);
                        }

                        // Check for IP addresses in URLs (suspicious)
                        if (Regex.IsMatch(link, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b"))
                        {
                            suspiciousLinks.Add(match.Value);
                        }
                    }
                }

                return suspiciousLinks.Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting suspicious links");
                return suspiciousLinks;
            }
        }

        /// <summary>
        /// Checks for excessive repetition (bot/spam indicator)
        /// </summary>
        private bool HasExcessiveRepetition(string content)
        {
            try
            {
                var words = content.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length < 10) return false;

                var wordCounts = words.GroupBy(w => w.ToLowerInvariant())
                                    .ToDictionary(g => g.Key, g => g.Count());

                // Check if any word appears more than 30% of the time
                var maxRepetition = wordCounts.Values.Max() / (double)words.Length;
                return maxRepetition > 0.3;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for excessive repetition");
                return false;
            }
        }

        /// <summary>
        /// Checks for repeated characters (spam indicator)
        /// </summary>
        private bool HasRepeatedCharacters(string content)
        {
            try
            {
                // Check for 3 or more consecutive identical characters
                return Regex.IsMatch(content, @"(.)\1{2,}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for repeated characters");
                return false;
            }
        }

        /// <summary>
        /// Checks for suspicious patterns that might indicate automated content
        /// </summary>
        private bool HasSuspiciousPatterns(string content)
        {
            try
            {
                var suspiciousPatterns = new[]
                {
                    @"\b\w{1,3}\s+\w{1,3}\s+\w{1,3}\s+\w{1,3}\b", // Very short repeated words
                    @"\$\d+|\d+\$", // Dollar amounts (spam indicator)
                    @"\b\d{4,}\b", // Long numbers (phone/credit card patterns)
                    @"[A-Z]{5,}", // Excessive capitalization
                    @"[!]{2,}", // Multiple exclamation marks
                    @"[?]{2,}"  // Multiple question marks
                };

                foreach (var pattern in suspiciousPatterns)
                {
                    if (Regex.IsMatch(content, pattern))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for suspicious patterns");
                return false;
            }
        }

        /// <summary>
        /// Validates if content length and complexity are reasonable
        /// </summary>
        private bool IsReasonableContent(string content, string contentType)
        {
            try
            {
                var length = content.Length;
                var wordCount = content.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

                // Check minimum length
                if (length < 3) return false;

                // Check maximum length based on content type
                var maxLength = contentType.ToLowerInvariant() switch
                {
                    "title" => 200,
                    "description" => 2000,
                    "comment" => 1000,
                    "message" => 5000,
                    _ => 10000
                };

                if (length > maxLength) return false;

                // Check for reasonable word-to-character ratio
                if (wordCount > 0)
                {
                    var avgWordLength = length / (double)wordCount;
                    if (avgWordLength > 20 || avgWordLength < 2) return false;
                }

                // Check for reasonable punctuation ratio
                var punctCount = content.Count(c => ".,!?;:".Contains(c));
                var punctRatio = punctCount / (double)length;
                if (punctRatio > 0.3) return false; // More than 30% punctuation is suspicious

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating content reasonableness");
                return false;
            }
        }

        /// <summary>
        /// Calculates overall risk score for content
        /// </summary>
        private double CalculateOverallRiskScore(string content)
        {
            try
            {
                var spamScore = CalculateSpamScore(content);
                var inappropriateScore = CalculateInappropriateScore(content);
                var suspiciousLinks = DetectSuspiciousLinks(content);
                var hasRepetition = HasExcessiveRepetition(content);
                var hasSuspiciousPatterns = HasSuspiciousPatterns(content);

                var riskScore = 0.0;

                // Spam score weight: 40%
                riskScore += spamScore * 0.4;

                // Inappropriate score weight: 30%
                riskScore += inappropriateScore * 0.3;

                // Suspicious links weight: 20%
                riskScore += Math.Min(suspiciousLinks.Count * 0.1, 0.2);

                // Repetition weight: 5%
                riskScore += hasRepetition ? 0.05 : 0;

                // Suspicious patterns weight: 5%
                riskScore += hasSuspiciousPatterns ? 0.05 : 0;

                return Math.Min(riskScore, 1.0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating overall risk score");
                return 0.5; // Default to medium risk on error
            }
        }

        /// <summary>
        /// Detects duplicate content using hash comparison
        /// </summary>
        public string CalculateContentHash(string content)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var normalizedContent = NormalizeContent(content);
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedContent));
                return Convert.ToHexString(hashBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating content hash");
                return string.Empty;
            }
        }

        /// <summary>
        /// Normalizes content for hash comparison
        /// </summary>
        private string NormalizeContent(string content)
        {
            try
            {
                // Convert to lowercase
                var normalized = content.ToLowerInvariant();

                // Remove extra whitespace
                normalized = Regex.Replace(normalized, @"\s+", " ");

                // Remove punctuation
                normalized = Regex.Replace(normalized, @"[^\w\s]", "");

                return normalized.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error normalizing content");
                return content;
            }
        }

        /// <summary>
        /// Validates content against a whitelist of allowed patterns
        /// </summary>
        public bool IsAllowedContent(string content, string[] allowedPatterns)
        {
            try
            {
                if (allowedPatterns == null || allowedPatterns.Length == 0)
                    return true;

                var lowerContent = content.ToLowerInvariant();

                foreach (var pattern in allowedPatterns)
                {
                    if (Regex.IsMatch(lowerContent, pattern, RegexOptions.IgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking allowed content patterns");
                return false;
            }
        }

        /// <summary>
        /// Logs content security events for monitoring
        /// </summary>
        public void LogContentSecurityEvent(string eventType, string content, ContentValidationResult result, string userId = null)
        {
            try
            {
                var logMessage = $"Content Security Event: {eventType}";
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    logMessage += $" from user {userId}";
                }

                logMessage += $" - Risk Score: {result.RiskScore:F2}";
                if (result.Errors.Count > 0)
                {
                    logMessage += $" - Errors: {string.Join(", ", result.Errors)}";
                }

                _logger.LogWarning(logMessage);

                // In production, you might want to:
                // - Send alerts to moderation team
                // - Store in security database
                // - Trigger automated content review
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging content security event");
            }
        }
    }

    /// <summary>
    /// Result of content validation
    /// </summary>
    public class ContentValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public double SpamScore { get; set; } = 0;
        public double InappropriateScore { get; set; } = 0;
        public List<string> SuspiciousLinks { get; set; } = new();
        public double RiskScore { get; set; } = 0;
        public string ContentHash { get; set; } = string.Empty;
    }
}
