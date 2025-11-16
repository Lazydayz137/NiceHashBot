using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHB3.Core.Services;
using NHB3.Utils;

namespace NHB3.MiningDutch
{
    /// <summary>
    /// Mining-Dutch pool API client for real profitability data
    /// </summary>
    public class MiningDutchClient : IDisposable
    {
        private const string BASE_URL = "https://www.mining-dutch.nl/api";
        private readonly HttpClient _httpClient;
        private readonly RateLimiter _rateLimiter;
        private Dictionary<string, AlgorithmStatus> _cachedStatus;
        private DateTime _cacheExpiry;

        public MiningDutchClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Conservative rate limit - be respectful
            _rateLimiter = new RateLimiter(20, TimeSpan.FromMinutes(1));
            _cachedStatus = new Dictionary<string, AlgorithmStatus>();
            _cacheExpiry = DateTime.MinValue;
        }

        /// <summary>
        /// Get status for all algorithms with profitability data
        /// </summary>
        public async Task<Dictionary<string, AlgorithmStatus>> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            // Cache for 2 minutes to avoid hammering the API
            if (_cachedStatus.Count > 0 && DateTime.UtcNow < _cacheExpiry)
            {
                return _cachedStatus;
            }

            await _rateLimiter.WaitAsync(cancellationToken);

            try
            {
                var response = await _httpClient.GetStringAsync("/status", cancellationToken);
                var data = JsonConvert.DeserializeObject<Dictionary<string, AlgorithmStatus>>(response);

                _cachedStatus = data ?? new Dictionary<string, AlgorithmStatus>();
                _cacheExpiry = DateTime.UtcNow.AddMinutes(2);

                Logger.Instance.Info($"Mining-Dutch: Loaded {_cachedStatus.Count} algorithms");
                return _cachedStatus;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to fetch Mining-Dutch status");
                return _cachedStatus; // Return cached data if available
            }
        }

        /// <summary>
        /// Get profitability for a specific algorithm
        /// </summary>
        public async Task<AlgorithmStatus> GetAlgorithmStatusAsync(string algorithm, CancellationToken cancellationToken = default)
        {
            var status = await GetStatusAsync(cancellationToken);

            // Try exact match first
            if (status.TryGetValue(algorithm, out var algoStatus))
            {
                return algoStatus;
            }

            // Try case-insensitive match
            foreach (var kvp in status)
            {
                if (kvp.Key.Equals(algorithm, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            Logger.Instance.Warning($"Algorithm '{algorithm}' not found on Mining-Dutch");
            return null;
        }

        /// <summary>
        /// Get average profitability for an algorithm
        /// </summary>
        public async Task<AverageProfitability> GetAverageProfitabilityAsync(string algorithm, CancellationToken cancellationToken = default)
        {
            await _rateLimiter.WaitAsync(cancellationToken);

            try
            {
                var endpoint = $"/v1/public/multiport/?method=avgprofitability&algorithm={algorithm}";
                var response = await _httpClient.GetStringAsync(endpoint, cancellationToken);
                var data = JsonConvert.DeserializeObject<AverageProfitability>(response);

                return data;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to fetch average profitability for {algorithm}");
                return null;
            }
        }

        /// <summary>
        /// Get pool statistics for an algorithm
        /// </summary>
        public async Task<PoolStats> GetPoolStatsAsync(string algorithm, CancellationToken cancellationToken = default)
        {
            await _rateLimiter.WaitAsync(cancellationToken);

            try
            {
                var endpoint = $"/v1/public/pooldata/?method=totalstats&algorithm={algorithm}";
                var response = await _httpClient.GetStringAsync(endpoint, cancellationToken);
                var data = JsonConvert.DeserializeObject<PoolStats>(response);

                return data;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to fetch pool stats for {algorithm}");
                return null;
            }
        }

        /// <summary>
        /// Calculate actual profitability in BTC per unit hashrate per day
        /// </summary>
        public decimal CalculateProfitability(AlgorithmStatus algo, decimal hashrate, ProfitabilityType type = ProfitabilityType.Current)
        {
            if (algo == null) return 0m;

            decimal profitabilityMbtcPerMh = type switch
            {
                ProfitabilityType.Current => algo.EstimateCurrent,
                ProfitabilityType.Estimate24h => algo.EstimateLast24h,
                ProfitabilityType.Actual24h => algo.ActualLast24h,
                _ => algo.EstimateCurrent
            };

            // Mining-Dutch returns mBTC/MH/day
            // Convert to BTC per unit hashrate per day
            decimal mbtcToBtc = 0.001m; // 1 mBTC = 0.001 BTC

            // Apply the factor (mbtc_mh_factor)
            decimal factor = string.IsNullOrEmpty(algo.MbtcMhFactor) ? 1m : decimal.Parse(algo.MbtcMhFactor);

            // Calculate: (profitability in mBTC/MH) * (hashrate in MH) * (mBTC to BTC conversion) * factor
            decimal dailyRevenueBtc = profitabilityMbtcPerMh * hashrate * mbtcToBtc * factor;

            return dailyRevenueBtc;
        }

        /// <summary>
        /// Calculate net profitability after pool fees
        /// </summary>
        public decimal CalculateNetProfitability(AlgorithmStatus algo, decimal hashrate, ProfitabilityType type = ProfitabilityType.Current)
        {
            var grossProfit = CalculateProfitability(algo, hashrate, type);
            var feePercent = algo.Fees / 100m; // Convert from percentage to decimal
            var netProfit = grossProfit * (1 - feePercent);

            return netProfit;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Algorithm status and profitability data from Mining-Dutch
    /// </summary>
    public class AlgorithmStatus
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("coins")]
        public int Coins { get; set; }

        [JsonProperty("fees")]
        public decimal Fees { get; set; }

        [JsonProperty("hashrate")]
        public decimal Hashrate { get; set; }

        [JsonProperty("hashrate_shared")]
        public decimal HashrateShared { get; set; }

        [JsonProperty("hashrate_solo")]
        public decimal HashrateSolo { get; set; }

        [JsonProperty("workers")]
        public int Workers { get; set; }

        // Profitability metrics (mBTC/MH/day)
        [JsonProperty("estimate_current")]
        public decimal EstimateCurrent { get; set; }

        [JsonProperty("estimate_last24h")]
        public decimal EstimateLast24h { get; set; }

        [JsonProperty("actual_last24h")]
        public decimal ActualLast24h { get; set; }

        [JsonProperty("mbtc_mh_factor")]
        public string MbtcMhFactor { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("timesincelast")]
        public int TimeSinceLast { get; set; }

        [JsonProperty("24h_blocks")]
        public int Blocks24h { get; set; }

        [JsonProperty("24h_btc")]
        public decimal Btc24h { get; set; }
    }

    /// <summary>
    /// Average profitability data
    /// </summary>
    public class AverageProfitability
    {
        [JsonProperty("average")]
        public decimal Average { get; set; }

        [JsonProperty("minimum")]
        public decimal Minimum { get; set; }

        [JsonProperty("last_updated")]
        public string LastUpdated { get; set; }
    }

    /// <summary>
    /// Pool statistics
    /// </summary>
    public class PoolStats
    {
        [JsonProperty("workers")]
        public int Workers { get; set; }

        [JsonProperty("hashrate")]
        public decimal Hashrate { get; set; }

        [JsonProperty("shares_this_round")]
        public long SharesThisRound { get; set; }
    }

    /// <summary>
    /// Type of profitability metric to use
    /// </summary>
    public enum ProfitabilityType
    {
        /// <summary>
        /// Current real-time estimate
        /// </summary>
        Current,

        /// <summary>
        /// Last 24 hours estimated profitability
        /// </summary>
        Estimate24h,

        /// <summary>
        /// Last 24 hours actual profitability (most accurate)
        /// </summary>
        Actual24h
    }
}
