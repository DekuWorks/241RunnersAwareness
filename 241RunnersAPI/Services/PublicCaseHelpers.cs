namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Helpers for building privacy-safe public case DTOs: city/state parsing and age-range bucketing.
    /// </summary>
    public static class PublicCaseHelpers
    {
        /// <summary>
        /// Parses a location string (e.g. "Houston, TX", "Houston, Texas", "Houston TX") into city and state.
        /// No street address is ever returned; input is treated as city-level only.
        /// </summary>
        public static (string? City, string? State) ParseCityState(string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return (null, null);

            var s = location.Trim();
            var comma = s.IndexOf(',');
            if (comma >= 0)
            {
                var city = s.Substring(0, comma).Trim();
                var state = s.Substring(comma + 1).Trim();
                if (string.IsNullOrWhiteSpace(city)) city = null;
                if (string.IsNullOrWhiteSpace(state)) state = null;
                return (city, NormalizeState(state));
            }

            // No comma: try "City ST" (e.g. "Houston TX") by splitting on last space
            var lastSpace = s.LastIndexOf(' ');
            if (lastSpace > 0 && lastSpace < s.Length - 1)
            {
                var possibleState = s.Substring(lastSpace + 1).Trim();
                if (possibleState.Length <= 3 || possibleState.Equals("Texas", StringComparison.OrdinalIgnoreCase))
                {
                    var city = s.Substring(0, lastSpace).Trim();
                    return (city, NormalizeState(possibleState));
                }
            }

            return (s, null);
        }

        private static string? NormalizeState(string? state)
        {
            if (string.IsNullOrWhiteSpace(state)) return null;
            if (state.Equals("Texas", StringComparison.OrdinalIgnoreCase)) return "TX";
            if (state.Length == 2) return state.ToUpperInvariant();
            return state;
        }

        /// <summary>
        /// Computes age from date of birth (UTC). Use when Runner.Age is not available in EF projection (NotMapped).
        /// </summary>
        public static int? ComputeAge(DateTime? dateOfBirth)
        {
            if (dateOfBirth == null) return null;
            var today = DateTime.UtcNow;
            var age = today.Year - dateOfBirth.Value.Year;
            if (today.DayOfYear < dateOfBirth.Value.DayOfYear) age--;
            return age < 0 ? null : age;
        }

        /// <summary>
        /// Returns an age-range bucket for privacy-friendly display: "0-12", "13-17", "18-25", "26-40", "41-65", "65+".
        /// Returns null if age is null or negative.
        /// </summary>
        public static string? GetAgeRange(int? age)
        {
            if (age == null || age < 0) return null;
            if (age <= 12) return "0-12";
            if (age <= 17) return "13-17";
            if (age <= 25) return "18-25";
            if (age <= 40) return "26-40";
            if (age <= 65) return "41-65";
            return "65+";
        }
    }
}
