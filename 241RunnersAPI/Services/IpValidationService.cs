using System.Net;
using System.Text.RegularExpressions;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for IP-based validation and security checks
    /// </summary>
    public class IpValidationService
    {
        private readonly ILogger<IpValidationService> _logger;
        
        // Known VPN/Proxy IP ranges (simplified list - in production, use a comprehensive database)
        private static readonly string[] KnownVpnRanges = {
            "10.0.0.0/8",
            "172.16.0.0/12", 
            "192.168.0.0/16",
            "127.0.0.0/8"
        };

        // Suspicious IP patterns
        private static readonly string[] SuspiciousPatterns = {
            @"^0\.", // IPs starting with 0
            @"^255\.", // Broadcast IPs
            @"^224\.", // Multicast IPs
            @"^240\.", // Reserved IPs
        };

        public IpValidationService(ILogger<IpValidationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates IP address format and checks for suspicious patterns
        /// </summary>
        public bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            try
            {
                // Check if it's a valid IP address
                if (!IPAddress.TryParse(ipAddress, out var ip))
                {
                    _logger.LogWarning("Invalid IP address format: {IpAddress}", ipAddress);
                    return false;
                }

                // Check for suspicious patterns
                foreach (var pattern in SuspiciousPatterns)
                {
                    if (Regex.IsMatch(ipAddress, pattern))
                    {
                        _logger.LogWarning("Suspicious IP pattern detected: {IpAddress} matches {Pattern}", ipAddress, pattern);
                        return false;
                    }
                }

                // Check if it's a private/local IP (might indicate VPN/Proxy)
                if (IsPrivateIpAddress(ip))
                {
                    _logger.LogWarning("Private IP address detected: {IpAddress}", ipAddress);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating IP address: {IpAddress}", ipAddress);
                return false;
            }
        }

        /// <summary>
        /// Checks if an IP address is a private/local address
        /// </summary>
        private bool IsPrivateIpAddress(IPAddress ip)
        {
            var ipBytes = ip.GetAddressBytes();
            
            // Check for private IP ranges
            if (ipBytes.Length == 4) // IPv4
            {
                // 10.0.0.0/8
                if (ipBytes[0] == 10)
                    return true;
                
                // 172.16.0.0/12
                if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31)
                    return true;
                
                // 192.168.0.0/16
                if (ipBytes[0] == 192 && ipBytes[1] == 168)
                    return true;
                
                // 127.0.0.0/8 (localhost)
                if (ipBytes[0] == 127)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Extracts the real IP address from request headers
        /// </summary>
        public string GetRealIpAddress(HttpContext context)
        {
            try
            {
                // Check for forwarded headers (from proxy/load balancer)
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
                var cfConnectingIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault(); // Cloudflare

                // Priority order: CF-Connecting-IP, X-Real-IP, X-Forwarded-For, RemoteIpAddress
                var ipAddress = cfConnectingIp ?? realIp ?? forwardedFor;

                if (!string.IsNullOrWhiteSpace(ipAddress))
                {
                    // X-Forwarded-For can contain multiple IPs, take the first one
                    if (ipAddress.Contains(','))
                    {
                        ipAddress = ipAddress.Split(',')[0].Trim();
                    }

                    if (IsValidIpAddress(ipAddress))
                    {
                        _logger.LogDebug("Real IP address extracted: {IpAddress}", ipAddress);
                        return ipAddress;
                    }
                }

                // Fallback to remote IP address
                var remoteIp = context.Connection.RemoteIpAddress?.ToString();
                if (!string.IsNullOrWhiteSpace(remoteIp) && IsValidIpAddress(remoteIp))
                {
                    _logger.LogDebug("Using remote IP address: {IpAddress}", remoteIp);
                    return remoteIp;
                }

                _logger.LogWarning("Unable to determine real IP address");
                return "unknown";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting real IP address");
                return "unknown";
            }
        }

        /// <summary>
        /// Checks if an IP address is from a suspicious source (VPN, Proxy, etc.)
        /// </summary>
        public bool IsSuspiciousIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return true;

            try
            {
                if (!IPAddress.TryParse(ipAddress, out var ip))
                    return true;

                // Check if it's a private IP (potential VPN/Proxy)
                if (IsPrivateIpAddress(ip))
                {
                    _logger.LogWarning("Suspicious IP detected (private range): {IpAddress}", ipAddress);
                    return true;
                }

                // Check for known VPN/Proxy ranges (simplified check)
                foreach (var range in KnownVpnRanges)
                {
                    if (IsIpInRange(ip, range))
                    {
                        _logger.LogWarning("Suspicious IP detected (known VPN/Proxy range): {IpAddress}", ipAddress);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking suspicious IP: {IpAddress}", ipAddress);
                return true; // Err on the side of caution
            }
        }

        /// <summary>
        /// Checks if an IP address is in a given CIDR range
        /// </summary>
        private bool IsIpInRange(IPAddress ip, string cidrRange)
        {
            try
            {
                var parts = cidrRange.Split('/');
                if (parts.Length != 2)
                    return false;

                if (!IPAddress.TryParse(parts[0], out var networkIp))
                    return false;

                if (!int.TryParse(parts[1], out var prefixLength))
                    return false;

                var ipBytes = ip.GetAddressBytes();
                var networkBytes = networkIp.GetAddressBytes();

                if (ipBytes.Length != networkBytes.Length)
                    return false;

                var bytesToCheck = prefixLength / 8;
                var bitsToCheck = prefixLength % 8;

                // Check full bytes
                for (int i = 0; i < bytesToCheck; i++)
                {
                    if (ipBytes[i] != networkBytes[i])
                        return false;
                }

                // Check remaining bits
                if (bitsToCheck > 0 && bytesToCheck < ipBytes.Length)
                {
                    var mask = (byte)(0xFF << (8 - bitsToCheck));
                    if ((ipBytes[bytesToCheck] & mask) != (networkBytes[bytesToCheck] & mask))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking IP range: {IpAddress} in {Range}", ip, cidrRange);
                return false;
            }
        }

        /// <summary>
        /// Validates request origin for CORS and security
        /// </summary>
        public bool IsAllowedOrigin(string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
                return false;

            try
            {
                var allowedOrigins = new[]
                {
                    "https://241runnersawareness.org",
                    "https://www.241runnersawareness.org",
                    "http://localhost:5173",
                    "http://localhost:3000",
                    "http://localhost:8080"
                };

                var isAllowed = allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
                
                if (!isAllowed)
                {
                    _logger.LogWarning("Disallowed origin: {Origin}", origin);
                }

                return isAllowed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating origin: {Origin}", origin);
                return false;
            }
        }

        /// <summary>
        /// Logs IP-based security events
        /// </summary>
        public void LogSecurityEvent(string eventType, string ipAddress, string? details = null)
        {
            try
            {
                var logMessage = $"Security Event: {eventType} from IP {ipAddress}";
                if (!string.IsNullOrWhiteSpace(details))
                {
                    logMessage += $" - {details}";
                }

                _logger.LogWarning(logMessage);

                // In a production environment, you might want to:
                // - Send alerts to security team
                // - Store in security database
                // - Trigger automated responses
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging security event");
            }
        }

        /// <summary>
        /// Validates user agent string for suspicious patterns
        /// </summary>
        public bool IsSuspiciousUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return true; // Missing user agent is suspicious

            try
            {
                var suspiciousPatterns = new[]
                {
                    @"bot", @"crawler", @"spider", @"scraper",
                    @"curl", @"wget", @"python", @"java",
                    @"automation", @"test", @"headless"
                };

                var lowerUserAgent = userAgent.ToLowerInvariant();

                foreach (var pattern in suspiciousPatterns)
                {
                    if (Regex.IsMatch(lowerUserAgent, pattern))
                    {
                        _logger.LogWarning("Suspicious user agent detected: {UserAgent}", userAgent);
                        return true;
                    }
                }

                // Check for unusually short user agents
                if (userAgent.Length < 10)
                {
                    _logger.LogWarning("Unusually short user agent: {UserAgent}", userAgent);
                    return true;
                }

                // Check for missing common browser indicators
                var hasBrowserIndicator = lowerUserAgent.Contains("mozilla") || 
                                        lowerUserAgent.Contains("chrome") || 
                                        lowerUserAgent.Contains("safari") || 
                                        lowerUserAgent.Contains("firefox") || 
                                        lowerUserAgent.Contains("edge");

                if (!hasBrowserIndicator)
                {
                    _logger.LogWarning("User agent missing browser indicators: {UserAgent}", userAgent);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user agent: {UserAgent}", userAgent);
                return true; // Err on the side of caution
            }
        }

        /// <summary>
        /// Comprehensive request validation including IP, origin, and user agent
        /// </summary>
        public bool ValidateRequest(HttpContext context)
        {
            try
            {
                var ipAddress = GetRealIpAddress(context);
                var origin = context.Request.Headers["Origin"].FirstOrDefault();
                var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();

                // Validate IP address
                if (!IsValidIpAddress(ipAddress))
                {
                    LogSecurityEvent("Invalid IP Address", ipAddress);
                    return false;
                }

                // Check for suspicious IP
                if (IsSuspiciousIpAddress(ipAddress))
                {
                    LogSecurityEvent("Suspicious IP Address", ipAddress);
                    // Don't block immediately, but log for monitoring
                }

                // Validate origin for CORS requests
                if (!string.IsNullOrWhiteSpace(origin) && !IsAllowedOrigin(origin))
                {
                    LogSecurityEvent("Disallowed Origin", ipAddress, $"Origin: {origin}");
                    return false;
                }

                // Validate user agent
                if (IsSuspiciousUserAgent(userAgent))
                {
                    LogSecurityEvent("Suspicious User Agent", ipAddress, $"User-Agent: {userAgent}");
                    // Don't block immediately, but log for monitoring
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating request");
                return false;
            }
        }
    }
}
