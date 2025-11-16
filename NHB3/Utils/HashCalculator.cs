using System;

namespace NHB3.Utils
{
    /// <summary>
    /// Utility class for hashrate conversions and calculations
    /// </summary>
    public static class HashCalculator
    {
        // Conversion factors
        public const decimal H_TO_KH = 1000m;
        public const decimal KH_TO_MH = 1000m;
        public const decimal MH_TO_GH = 1000m;
        public const decimal GH_TO_TH = 1000m;
        public const decimal TH_TO_PH = 1000m;

        /// <summary>
        /// Convert hashrate between units
        /// </summary>
        public static decimal ConvertHashrate(decimal hashrate, string fromUnit, string toUnit)
        {
            var normalized = NormalizeToHash(hashrate, fromUnit);
            return ConvertFromHash(normalized, toUnit);
        }

        /// <summary>
        /// Normalize any unit to H/s
        /// </summary>
        public static decimal NormalizeToHash(decimal hashrate, string unit)
        {
            return unit.ToUpper() switch
            {
                "H" or "H/S" => hashrate,
                "KH" or "KH/S" => hashrate * H_TO_KH,
                "MH" or "MH/S" => hashrate * H_TO_KH * KH_TO_MH,
                "GH" or "GH/S" => hashrate * H_TO_KH * KH_TO_MH * MH_TO_GH,
                "TH" or "TH/S" => hashrate * H_TO_KH * KH_TO_MH * MH_TO_GH * GH_TO_TH,
                "PH" or "PH/S" => hashrate * H_TO_KH * KH_TO_MH * MH_TO_GH * GH_TO_TH * TH_TO_PH,
                _ => throw new ArgumentException($"Unknown hashrate unit: {unit}")
            };
        }

        /// <summary>
        /// Convert from H/s to specific unit
        /// </summary>
        public static decimal ConvertFromHash(decimal hashInHashes, string toUnit)
        {
            return toUnit.ToUpper() switch
            {
                "H" or "H/S" => hashInHashes,
                "KH" or "KH/S" => hashInHashes / H_TO_KH,
                "MH" or "MH/S" => hashInHashes / (H_TO_KH * KH_TO_MH),
                "GH" or "GH/S" => hashInHashes / (H_TO_KH * KH_TO_MH * MH_TO_GH),
                "TH" or "TH/S" => hashInHashes / (H_TO_KH * KH_TO_MH * MH_TO_GH * GH_TO_TH),
                "PH" or "PH/S" => hashInHashes / (H_TO_KH * KH_TO_MH * MH_TO_GH * GH_TO_TH * TH_TO_PH),
                _ => throw new ArgumentException($"Unknown hashrate unit: {toUnit}")
            };
        }

        /// <summary>
        /// Format hashrate with appropriate unit
        /// </summary>
        public static string FormatHashrate(decimal hashrate, string currentUnit, int decimals = 2)
        {
            var hashInHashes = NormalizeToHash(hashrate, currentUnit);

            // Auto-select appropriate unit
            if (hashInHashes >= H_TO_KH * KH_TO_MH * MH_TO_GH * GH_TO_TH * TH_TO_PH)
            {
                return $"{ConvertFromHash(hashInHashes, "PH"):F{decimals}} PH/s";
            }
            if (hashInHashes >= H_TO_KH * KH_TO_MH * MH_TO_GH * GH_TO_TH)
            {
                return $"{ConvertFromHash(hashInHashes, "TH"):F{decimals}} TH/s";
            }
            if (hashInHashes >= H_TO_KH * KH_TO_MH * MH_TO_GH)
            {
                return $"{ConvertFromHash(hashInHashes, "GH"):F{decimals}} GH/s";
            }
            if (hashInHashes >= H_TO_KH * KH_TO_MH)
            {
                return $"{ConvertFromHash(hashInHashes, "MH"):F{decimals}} MH/s";
            }
            if (hashInHashes >= H_TO_KH)
            {
                return $"{ConvertFromHash(hashInHashes, "KH"):F{decimals}} KH/s";
            }
            return $"{hashInHashes:F{decimals}} H/s";
        }

        /// <summary>
        /// Calculate expected daily earnings
        /// </summary>
        public static decimal CalculateDailyEarnings(decimal hashrate, string unit, decimal profitabilityPerUnit)
        {
            // profitabilityPerUnit is in BTC per unit per day
            return hashrate * profitabilityPerUnit;
        }

        /// <summary>
        /// Calculate profit margin percentage
        /// </summary>
        public static decimal CalculateProfitMargin(decimal cost, decimal revenue)
        {
            if (cost <= 0) return 0;
            return ((revenue - cost) / cost) * 100m;
        }

        /// <summary>
        /// Calculate break-even price
        /// </summary>
        public static decimal CalculateBreakEvenPrice(decimal cost, decimal hashrate)
        {
            if (hashrate <= 0) return 0;
            return cost / hashrate;
        }
    }
}
