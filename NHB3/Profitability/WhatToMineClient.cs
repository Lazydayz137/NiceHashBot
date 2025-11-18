using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHB3.Core.Services;
using NHB3.Utils;

namespace NHB3.Profitability
{
    /// <summary>
    /// WhatToMine API client for profitability data
    /// </summary>
    public class WhatToMineClient : IDisposable
    {
        private const string BASE_URL = "https://whattomine.com/";
        private readonly HttpClient _httpClient;
        private readonly RateLimiter _rateLimiter;
        private readonly Dictionary<string, CoinData> _cache;
        private readonly object _cacheLock = new object();

        public WhatToMineClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Conservative rate limit for WhatToMine (no official limit, be respectful)
            _rateLimiter = new RateLimiter(10, TimeSpan.FromMinutes(1));
            _cache = new Dictionary<string, CoinData>();
        }

        /// <summary>
        /// Get profitability data for a specific coin
        /// </summary>
        public async Task<CoinData> GetCoinDataAsync(string coinSymbol, CancellationToken cancellationToken = default)
        {
            await _rateLimiter.WaitAsync(cancellationToken);

            try
            {
                var response = await _httpClient.GetStringAsync($"coins/{coinSymbol}.json", cancellationToken);
                var data = JsonConvert.DeserializeObject<CoinData>(response);

                lock (_cacheLock)
                {
                    _cache[coinSymbol] = data;
                }

                return data;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to get WhatToMine data for {coinSymbol}");
                return null;
            }
        }

        /// <summary>
        /// Get list of available calculators (coins)
        /// </summary>
        public async Task<JObject> GetCalculatorsAsync(CancellationToken cancellationToken = default)
        {
            await _rateLimiter.WaitAsync(cancellationToken);

            try
            {
                var response = await _httpClient.GetStringAsync("calculators.json", cancellationToken);
                return JObject.Parse(response);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to get WhatToMine calculators");
                return new JObject();
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Coin profitability data from WhatToMine
    /// </summary>
    public class CoinData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("algorithm")]
        public string Algorithm { get; set; }

        [JsonProperty("block_time")]
        public string BlockTime { get; set; }

        [JsonProperty("block_reward")]
        public decimal BlockReward { get; set; }

        [JsonProperty("block_reward24")]
        public decimal BlockReward24 { get; set; }

        [JsonProperty("difficulty")]
        public decimal Difficulty { get; set; }

        [JsonProperty("difficulty24")]
        public decimal Difficulty24 { get; set; }

        [JsonProperty("nethash")]
        public decimal Nethash { get; set; }

        [JsonProperty("exchange_rate")]
        public decimal ExchangeRate { get; set; }

        [JsonProperty("exchange_rate24")]
        public decimal ExchangeRate24 { get; set; }

        [JsonProperty("exchange_rate_vol")]
        public decimal ExchangeRateVol { get; set; }

        [JsonProperty("exchange_rate_curr")]
        public string ExchangeRateCurr { get; set; }

        [JsonProperty("market_cap")]
        public string MarketCap { get; set; }

        [JsonProperty("estimated_rewards")]
        public string EstimatedRewards { get; set; }

        [JsonProperty("btc_revenue")]
        public string BtcRevenue { get; set; }

        [JsonProperty("profitability")]
        public decimal Profitability { get; set; }
    }
}
