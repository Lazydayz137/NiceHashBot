using System;
using System.Collections.Generic;
using System.Linq;

namespace NHB3.MiningDutch
{
    /// <summary>
    /// Maps algorithm names between NiceHash and Mining-Dutch
    /// Also handles hashrate unit conversions
    /// </summary>
    public static class AlgorithmMapper
    {
        /// <summary>
        /// Mapping between NiceHash algorithm names and Mining-Dutch names
        /// </summary>
        private static readonly Dictionary<string, string> NiceHashToMiningDutch = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Common algorithms
            {"SHA256", "sha256"},
            {"SCRYPT", "scrypt"},
            {"X11", "x11"},
            {"X13", "x13"},
            {"X15", "x15"},
            {"QUBIT", "qubit"},
            {"QUARK", "quark"},
            {"NEOSCRYPT", "neoscrypt"},
            {"LYRA2REV2", "lyra2v2"},
            {"LYRA2Z", "lyra2z"},
            {"BLAKE2S", "blake2s"},
            {"BLAKE256R8", "blakecoin"},
            {"BLAKE256R14", "blake"},
            {"DECRED", "decred"},
            {"LBRY", "lbry"},
            {"PASCAL", "pascal"},
            {"KECCAK", "keccak"},
            {"SKEIN", "skein"},
            {"NIST5", "nist5"},
            {"X16R", "x16r"},
            {"X16S", "x16s"},
            {"EQUIHASH", "equihash"},
            {"ZHASH", "zhash"},
            {"BEAMHASH", "beamhash"},
            {"GRINCUCKATOO31", "grin31"},
            {"GRINCUCKATOO32", "grin32"},
            {"CUCKOOCYCLE", "cuckoo"},
            {"KAWPOW", "kawpow"},
            {"RANDOMXMONERO", "randomx"},
            {"ETHASH", "ethash"},
            {"ETCHASH", "etchash"},
            {"CRYPTONIGHTR", "cryptonight_r"},
            {"CRYPTONIGHTV7", "cryptonight_v7"},
            {"CRYPTONIGHTV8", "cryptonight_v8"},
            {"CRYPTONIGHTHEAVY", "cryptonight_heavy"}
        };

        /// <summary>
        /// Hashrate unit factors for conversion
        /// NiceHash uses different base units than Mining-Dutch
        /// </summary>
        private static readonly Dictionary<string, HashrateUnit> AlgorithmUnits = new Dictionary<string, HashrateUnit>(StringComparer.OrdinalIgnoreCase)
        {
            // SHA256 family - measured in TH/s on NH, but MD uses different base
            {"SHA256", new HashrateUnit { NiceHashBase = "TH", MiningDutchBase = "MH", ConversionFactor = 1000000m }}, // 1 TH = 1,000,000 MH

            // Scrypt - measured in MH/s on both
            {"SCRYPT", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},

            // X11 family - measured in MH/s
            {"X11", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},
            {"X13", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},
            {"X15", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},

            // Qubit/Quark - measured in MH/s
            {"QUBIT", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},
            {"QUARK", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},

            // NeoScrypt - measured in MH/s
            {"NEOSCRYPT", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},

            // Lyra2 variants - measured in MH/s
            {"LYRA2REV2", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},
            {"LYRA2Z", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},

            // Blake variants - measured in GH/s on NH, MH on MD
            {"BLAKE2S", new HashrateUnit { NiceHashBase = "GH", MiningDutchBase = "MH", ConversionFactor = 1000m }}, // 1 GH = 1000 MH

            // Equihash - measured in Sol/s (same as H/s) or KSol/s
            {"EQUIHASH", new HashrateUnit { NiceHashBase = "KSol", MiningDutchBase = "Sol", ConversionFactor = 1000m }},

            // KawPow - measured in MH/s
            {"KAWPOW", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},

            // Ethash - measured in MH/s
            {"ETHASH", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},
            {"ETCHASH", new HashrateUnit { NiceHashBase = "MH", MiningDutchBase = "MH", ConversionFactor = 1m }},

            // RandomX - measured in KH/s
            {"RANDOMXMONERO", new HashrateUnit { NiceHashBase = "KH", MiningDutchBase = "KH", ConversionFactor = 1m }}
        };

        /// <summary>
        /// Convert NiceHash algorithm name to Mining-Dutch name
        /// </summary>
        public static string ToMiningDutch(string niceHashAlgorithm)
        {
            if (string.IsNullOrEmpty(niceHashAlgorithm))
                return null;

            if (NiceHashToMiningDutch.TryGetValue(niceHashAlgorithm, out var mdName))
                return mdName;

            // If no mapping found, try lowercase
            return niceHashAlgorithm.ToLower();
        }

        /// <summary>
        /// Convert Mining-Dutch algorithm name to NiceHash name
        /// </summary>
        public static string ToNiceHash(string miningDutchAlgorithm)
        {
            if (string.IsNullOrEmpty(miningDutchAlgorithm))
                return null;

            var pair = NiceHashToMiningDutch.FirstOrDefault(kvp =>
                kvp.Value.Equals(miningDutchAlgorithm, StringComparison.OrdinalIgnoreCase));

            return pair.Key ?? miningDutchAlgorithm.ToUpper();
        }

        /// <summary>
        /// Convert NiceHash hashrate to Mining-Dutch base unit (MH)
        /// </summary>
        public static decimal ConvertHashrateToMiningDutch(string algorithm, decimal niceHashHashrate)
        {
            if (AlgorithmUnits.TryGetValue(algorithm, out var unit))
            {
                return niceHashHashrate * unit.ConversionFactor;
            }

            // Default: assume same unit
            return niceHashHashrate;
        }

        /// <summary>
        /// Convert Mining-Dutch hashrate to NiceHash base unit
        /// </summary>
        public static decimal ConvertHashrateToNiceHash(string algorithm, decimal miningDutchHashrate)
        {
            if (AlgorithmUnits.TryGetValue(algorithm, out var unit))
            {
                return miningDutchHashrate / unit.ConversionFactor;
            }

            // Default: assume same unit
            return miningDutchHashrate;
        }

        /// <summary>
        /// Get hashrate unit info for an algorithm
        /// </summary>
        public static HashrateUnit GetHashrateUnit(string algorithm)
        {
            if (AlgorithmUnits.TryGetValue(algorithm, out var unit))
                return unit;

            return new HashrateUnit { NiceHashBase = "H", MiningDutchBase = "MH", ConversionFactor = 1m };
        }

        /// <summary>
        /// Check if algorithm is supported on Mining-Dutch
        /// </summary>
        public static bool IsSupportedOnMiningDutch(string niceHashAlgorithm)
        {
            return NiceHashToMiningDutch.ContainsKey(niceHashAlgorithm);
        }

        /// <summary>
        /// Get all NiceHash algorithms supported on Mining-Dutch
        /// </summary>
        public static IEnumerable<string> GetSupportedAlgorithms()
        {
            return NiceHashToMiningDutch.Keys;
        }
    }

    /// <summary>
    /// Hashrate unit conversion information
    /// </summary>
    public class HashrateUnit
    {
        public string NiceHashBase { get; set; }      // e.g., "TH", "MH", "GH"
        public string MiningDutchBase { get; set; }   // e.g., "MH", "H"
        public decimal ConversionFactor { get; set; } // Factor to convert NH to MD
    }
}
