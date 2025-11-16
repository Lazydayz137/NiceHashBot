using System;
using System.Text.RegularExpressions;

namespace NHB3.Utils
{
    /// <summary>
    /// Data validation utilities
    /// </summary>
    public static class DataValidator
    {
        private static readonly Regex BtcAddressRegex = new Regex(
            @"^(bc1|[13])[a-zA-HJ-NP-Z0-9]{25,62}$",
            RegexOptions.Compiled
        );

        private static readonly Regex UuidRegex = new Regex(
            @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Validate Bitcoin address format
        /// </summary>
        public static bool IsValidBtcAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;

            return BtcAddressRegex.IsMatch(address);
        }

        /// <summary>
        /// Validate UUID/GUID format
        /// </summary>
        public static bool IsValidUuid(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return false;

            return UuidRegex.IsMatch(uuid);
        }

        /// <summary>
        /// Validate decimal is positive
        /// </summary>
        public static bool IsPositive(decimal value)
        {
            return value > 0;
        }

        /// <summary>
        /// Validate decimal is within range
        /// </summary>
        public static bool IsInRange(decimal value, decimal min, decimal max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Validate percentage (0-100)
        /// </summary>
        public static bool IsValidPercentage(decimal percentage)
        {
            return IsInRange(percentage, 0, 100);
        }

        /// <summary>
        /// Validate algorithm name
        /// </summary>
        public static bool IsValidAlgorithm(string algorithm)
        {
            if (string.IsNullOrWhiteSpace(algorithm))
                return false;

            // Algorithm names should be alphanumeric with optional hyphens/underscores
            return Regex.IsMatch(algorithm, @"^[a-zA-Z0-9_-]+$");
        }

        /// <summary>
        /// Sanitize string for logging (remove sensitive data)
        /// </summary>
        public static string SanitizeForLog(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Mask API keys/secrets
            if (input.Length > 8)
            {
                return input.Substring(0, 4) + "****" + input.Substring(input.Length - 4);
            }

            return "****";
        }

        /// <summary>
        /// Validate and clamp decimal value to range
        /// </summary>
        public static decimal ClampToRange(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Validate configuration value with default fallback
        /// </summary>
        public static T ValidateOrDefault<T>(T value, T defaultValue, Func<T, bool> validator)
        {
            if (validator(value))
                return value;

            return defaultValue;
        }
    }
}
