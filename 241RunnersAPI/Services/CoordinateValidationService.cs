using System.Text.RegularExpressions;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for validating geographic coordinates and location data
    /// </summary>
    public class CoordinateValidationService
    {
        private readonly ILogger<CoordinateValidationService> _logger;

        // Coordinate bounds for validation
        private const decimal MinLatitude = -90.0m;
        private const decimal MaxLatitude = 90.0m;
        private const decimal MinLongitude = -180.0m;
        private const decimal MaxLongitude = 180.0m;
        private const int MaxDecimalPlaces = 6; // Maximum precision for coordinates

        public CoordinateValidationService(ILogger<CoordinateValidationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates latitude and longitude coordinates
        /// </summary>
        public bool ValidateCoordinates(decimal? latitude, decimal? longitude)
        {
            try
            {
                // Both coordinates must be provided or both must be null
                if ((latitude.HasValue && !longitude.HasValue) || (!latitude.HasValue && longitude.HasValue))
                {
                    _logger.LogWarning("Invalid coordinates: latitude and longitude must both be provided or both be null");
                    return false;
                }

                // If both are null, that's valid (optional coordinates)
                if (!latitude.HasValue && !longitude.HasValue)
                {
                    return true;
                }

                // Validate latitude range
                if (latitude < MinLatitude || latitude > MaxLatitude)
                {
                    _logger.LogWarning("Invalid latitude: {Latitude}. Must be between {Min} and {Max}", 
                        latitude, MinLatitude, MaxLongitude);
                    return false;
                }

                // Validate longitude range
                if (longitude < MinLongitude || longitude > MaxLongitude)
                {
                    _logger.LogWarning("Invalid longitude: {Longitude}. Must be between {Min} and {Max}", 
                        longitude, MinLongitude, MaxLongitude);
                    return false;
                }

                // Check precision (decimal places)
                if (!ValidateCoordinatePrecision(latitude.Value) || !ValidateCoordinatePrecision(longitude.Value))
                {
                    _logger.LogWarning("Invalid coordinate precision. Maximum {MaxPlaces} decimal places allowed", MaxDecimalPlaces);
                    return false;
                }

                _logger.LogDebug("Coordinates validated successfully: {Latitude}, {Longitude}", latitude, longitude);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating coordinates");
                return false;
            }
        }

        /// <summary>
        /// Validates coordinate precision (number of decimal places)
        /// </summary>
        private bool ValidateCoordinatePrecision(decimal coordinate)
        {
            var coordinateString = coordinate.ToString("F10"); // Use high precision for calculation
            var decimalIndex = coordinateString.IndexOf('.');
            
            if (decimalIndex == -1)
                return true; // No decimal places is valid
            
            var decimalPlaces = coordinateString.Length - decimalIndex - 1;
            return decimalPlaces <= MaxDecimalPlaces;
        }

        /// <summary>
        /// Validates latitude specifically
        /// </summary>
        public bool ValidateLatitude(decimal? latitude)
        {
            if (!latitude.HasValue)
                return true; // Optional field

            try
            {
                if (latitude < MinLatitude || latitude > MaxLatitude)
                {
                    _logger.LogWarning("Invalid latitude: {Latitude}. Must be between {Min} and {Max}", 
                        latitude, MinLatitude, MaxLatitude);
                    return false;
                }

                if (!ValidateCoordinatePrecision(latitude.Value))
                {
                    _logger.LogWarning("Invalid latitude precision: {Latitude}", latitude);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating latitude");
                return false;
            }
        }

        /// <summary>
        /// Validates longitude specifically
        /// </summary>
        public bool ValidateLongitude(decimal? longitude)
        {
            if (!longitude.HasValue)
                return true; // Optional field

            try
            {
                if (longitude < MinLongitude || longitude > MaxLongitude)
                {
                    _logger.LogWarning("Invalid longitude: {Longitude}. Must be between {Min} and {Max}", 
                        longitude, MinLongitude, MaxLongitude);
                    return false;
                }

                if (!ValidateCoordinatePrecision(longitude.Value))
                {
                    _logger.LogWarning("Invalid longitude precision: {Longitude}", longitude);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating longitude");
                return false;
            }
        }

        /// <summary>
        /// Validates coordinate string format (e.g., "29.7604, -95.3698")
        /// </summary>
        public bool ValidateCoordinateString(string coordinateString)
        {
            if (string.IsNullOrWhiteSpace(coordinateString))
                return true; // Optional field

            try
            {
                // Expected format: "latitude, longitude" or "lat,lng"
                var coordinatePattern = @"^(-?\d{1,3}(?:\.\d{1,6})?),\s*(-?\d{1,3}(?:\.\d{1,6})?)$";
                
                if (!Regex.IsMatch(coordinateString, coordinatePattern))
                {
                    _logger.LogWarning("Invalid coordinate string format: {CoordinateString}", coordinateString);
                    return false;
                }

                // Extract latitude and longitude
                var match = Regex.Match(coordinateString, coordinatePattern);
                if (match.Success && match.Groups.Count == 3)
                {
                    if (decimal.TryParse(match.Groups[1].Value, out var latitude) &&
                        decimal.TryParse(match.Groups[2].Value, out var longitude))
                    {
                        return ValidateCoordinates(latitude, longitude);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating coordinate string");
                return false;
            }
        }

        /// <summary>
        /// Validates location string format and content
        /// </summary>
        public bool ValidateLocationString(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return false; // Location is required

            try
            {
                // Check length
                if (location.Length > 500)
                {
                    _logger.LogWarning("Location string too long: {Length} characters", location.Length);
                    return false;
                }

                // Check for suspicious patterns
                var suspiciousPatterns = new[]
                {
                    @"<script", // Script tags
                    @"javascript:", // JavaScript URLs
                    @"eval\s*\(", // JavaScript eval
                    @"document\.", // Document object access
                    @"window\.", // Window object access
                    @"\bunion\b", // SQL injection
                    @"\bselect\b",
                    @"\binsert\b",
                    @"\bdelete\b"
                };

                foreach (var pattern in suspiciousPatterns)
                {
                    if (Regex.IsMatch(location, pattern, RegexOptions.IgnoreCase))
                    {
                        _logger.LogWarning("Suspicious pattern detected in location: {Pattern}", pattern);
                        return false;
                    }
                }

                // Check for reasonable location format (basic validation)
                // Should contain some letters and possibly numbers, not just special characters
                var letterCount = location.Count(char.IsLetter);
                var digitCount = location.Count(char.IsDigit);
                var specialCharCount = location.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

                if (letterCount == 0)
                {
                    _logger.LogWarning("Location string contains no letters: {Location}", location);
                    return false;
                }

                // Too many special characters might indicate malicious input
                if (specialCharCount > location.Length * 0.3)
                {
                    _logger.LogWarning("Location string contains too many special characters: {Location}", location);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating location string");
                return false;
            }
        }

        /// <summary>
        /// Calculates distance between two coordinates (in kilometers)
        /// </summary>
        public double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            try
            {
                const double earthRadius = 6371; // Earth's radius in kilometers

                var lat1Rad = (double)lat1 * Math.PI / 180;
                var lon1Rad = (double)lon1 * Math.PI / 180;
                var lat2Rad = (double)lat2 * Math.PI / 180;
                var lon2Rad = (double)lon2 * Math.PI / 180;

                var dLat = lat2Rad - lat1Rad;
                var dLon = lon2Rad - lon1Rad;

                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var distance = earthRadius * c;

                return distance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance between coordinates");
                return -1; // Invalid distance
            }
        }

        /// <summary>
        /// Validates if coordinates are within a reasonable distance from each other
        /// </summary>
        public bool ValidateCoordinateProximity(decimal lat1, decimal lon1, decimal lat2, decimal lon2, double maxDistanceKm = 1000)
        {
            try
            {
                var distance = CalculateDistance(lat1, lon1, lat2, lon2);
                
                if (distance < 0)
                {
                    _logger.LogWarning("Invalid distance calculation");
                    return false;
                }

                if (distance > maxDistanceKm)
                {
                    _logger.LogWarning("Coordinates are too far apart: {Distance}km (max: {MaxDistance}km)", distance, maxDistanceKm);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating coordinate proximity");
                return false;
            }
        }

        /// <summary>
        /// Sanitizes coordinate values to remove excessive precision
        /// </summary>
        public (decimal?, decimal?) SanitizeCoordinates(decimal? latitude, decimal? longitude)
        {
            if (!latitude.HasValue || !longitude.HasValue)
                return (latitude, longitude);

            try
            {
                // Round to maximum allowed precision
                var sanitizedLat = Math.Round(latitude.Value, MaxDecimalPlaces, MidpointRounding.AwayFromZero);
                var sanitizedLng = Math.Round(longitude.Value, MaxDecimalPlaces, MidpointRounding.AwayFromZero);

                _logger.LogDebug("Coordinates sanitized: ({Latitude}, {Longitude}) -> ({SanitizedLat}, {SanitizedLng})", 
                    latitude, longitude, sanitizedLat, sanitizedLng);

                return (sanitizedLat, sanitizedLng);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing coordinates");
                return (latitude, longitude); // Return original values on error
            }
        }
    }
}
