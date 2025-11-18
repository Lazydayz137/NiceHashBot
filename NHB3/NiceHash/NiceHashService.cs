using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NHB3.Core.Services;
using NHB3.NiceHash.Models;

namespace NHB3.NiceHash
{
    /// <summary>
    /// High-level service for NiceHash operations
    /// </summary>
    public class NiceHashService
    {
        private readonly NiceHashClient _client;
        private List<Algorithm> _algorithmsCache;
        private DateTime _algorithmsCachedAt;

        public NiceHashService(NiceHashClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        #region Algorithms

        /// <summary>
        /// Get list of mining algorithms (cached for 1 hour)
        /// </summary>
        public async Task<List<Algorithm>> GetAlgorithmsAsync(CancellationToken cancellationToken = default)
        {
            // Cache algorithms for 1 hour
            if (_algorithmsCache != null && (DateTime.UtcNow - _algorithmsCachedAt).TotalHours < 1)
            {
                return _algorithmsCache;
            }

            var response = await _client.GetAsync<JObject>("/main/api/v2/mining/algorithms", requiresAuth: false, cancellationToken);
            var algorithms = response["miningAlgorithms"].ToObject<List<Algorithm>>();

            _algorithmsCache = algorithms;
            _algorithmsCachedAt = DateTime.UtcNow;

            Logger.Instance.Info($"Loaded {algorithms.Count} mining algorithms");
            return algorithms;
        }

        /// <summary>
        /// Get algorithm by name
        /// </summary>
        public async Task<Algorithm> GetAlgorithmAsync(string algorithmName, CancellationToken cancellationToken = default)
        {
            var algorithms = await GetAlgorithmsAsync(cancellationToken);
            return algorithms.FirstOrDefault(a => a.Name.Equals(algorithmName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Account & Balance

        /// <summary>
        /// Get account balance for a specific currency
        /// </summary>
        public async Task<AccountBalance> GetBalanceAsync(string currency = "BTC", CancellationToken cancellationToken = default)
        {
            var response = await _client.GetAsync<JObject>($"/main/api/v2/accounting/accounts2", requiresAuth: true, cancellationToken);
            var accounts = response["currencies"].ToObject<List<AccountBalance>>();
            return accounts.FirstOrDefault(a => a.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Pools

        /// <summary>
        /// Get all pools
        /// </summary>
        public async Task<List<Pool>> GetPoolsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _client.GetAsync<JObject>("/main/api/v2/pools", requiresAuth: true, cancellationToken);
            return response["list"].ToObject<List<Pool>>();
        }

        /// <summary>
        /// Create or update a pool
        /// </summary>
        public async Task<Pool> SavePoolAsync(Pool pool, CancellationToken cancellationToken = default)
        {
            var response = await _client.PostAsync<JObject>("/main/api/v2/pool", pool, requiresAuth: true, cancellationToken);
            return response.ToObject<Pool>();
        }

        /// <summary>
        /// Delete a pool
        /// </summary>
        public async Task<bool> DeletePoolAsync(string poolId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.DeleteAsync<JObject>($"/main/api/v2/pool/{poolId}", requiresAuth: true, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to delete pool {poolId}");
                return false;
            }
        }

        #endregion

        #region Orders

        /// <summary>
        /// Get user's active orders
        /// </summary>
        public async Task<List<Order>> GetMyOrdersAsync(CancellationToken cancellationToken = default)
        {
            var response = await _client.GetAsync<JObject>("/main/api/v2/hashpower/myOrders", requiresAuth: true, cancellationToken);
            return response["list"].ToObject<List<Order>>();
        }

        /// <summary>
        /// Get active market orders for a specific algorithm
        /// </summary>
        public async Task<List<Order>> GetMarketOrdersAsync(string algorithm, CancellationToken cancellationToken = default)
        {
            var endpoint = string.IsNullOrEmpty(algorithm)
                ? "/main/api/v2/public/orders/active2"
                : $"/main/api/v2/public/orders/active2?algorithm={algorithm}";

            var response = await _client.GetAsync<JObject>(endpoint, requiresAuth: false, cancellationToken);
            return response["list"].ToObject<List<Order>>();
        }

        /// <summary>
        /// Get order book (market depth)
        /// </summary>
        public async Task<OrderBook> GetOrderBookAsync(string algorithm, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetAsync<JObject>($"/main/api/v2/hashpower/orderBook?algorithm={algorithm}", requiresAuth: false, cancellationToken);
            return response.ToObject<OrderBook>();
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        public async Task<Order> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _client.PostAsync<JObject>("/main/api/v2/hashpower/order", request, requiresAuth: true, cancellationToken);
            return response.ToObject<Order>();
        }

        /// <summary>
        /// Refill an existing order
        /// </summary>
        public async Task<Order> RefillOrderAsync(string orderId, decimal amount, CancellationToken cancellationToken = default)
        {
            var payload = new { amount = amount.ToString("F8") };
            var response = await _client.PostAsync<JObject>($"/main/api/v2/hashpower/order/{orderId}/refill", payload, requiresAuth: true, cancellationToken);
            return response.ToObject<Order>();
        }

        /// <summary>
        /// Update order price and limit
        /// </summary>
        public async Task<Order> UpdateOrderAsync(string orderId, decimal? price = null, decimal? limit = null, decimal? displayMarketFactor = null, decimal? marketFactor = null, CancellationToken cancellationToken = default)
        {
            var payload = new JObject();

            if (price.HasValue)
                payload["price"] = price.Value.ToString("F8");

            if (limit.HasValue)
                payload["limit"] = limit.Value.ToString("F8");

            if (displayMarketFactor.HasValue)
                payload["displayMarketFactor"] = displayMarketFactor.Value.ToString();

            if (marketFactor.HasValue)
                payload["marketFactor"] = marketFactor.Value.ToString();

            var response = await _client.PostAsync<JObject>($"/main/api/v2/hashpower/order/{orderId}/updatePriceAndLimit", payload.ToString(), requiresAuth: true, cancellationToken);
            return response.ToObject<Order>();
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        public async Task<bool> CancelOrderAsync(string orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.DeleteAsync<JObject>($"/main/api/v2/hashpower/order/{orderId}", requiresAuth: true, cancellationToken);
                Logger.Instance.Info($"Cancelled order {orderId}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to cancel order {orderId}");
                return false;
            }
        }

        /// <summary>
        /// Get fixed price for an order
        /// </summary>
        public async Task<decimal> GetFixedPriceAsync(string market, string algorithm, CancellationToken cancellationToken = default)
        {
            var payload = new { market, algorithm };
            var response = await _client.PostAsync<JObject>("/main/api/v2/hashpower/orders/fixedPrice", payload, requiresAuth: false, cancellationToken);
            return decimal.Parse(response["fixedPrice"].ToString());
        }

        #endregion

        #region Market Analysis

        /// <summary>
        /// Get aggregated market data for an algorithm
        /// </summary>
        public async Task<Core.Models.MarketData> GetMarketDataAsync(string algorithm, string market = "USA", CancellationToken cancellationToken = default)
        {
            var orders = await GetMarketOrdersAsync(algorithm, cancellationToken);

            // Filter for specific market and active orders
            var marketOrders = orders
                .Where(o => o.Market == market && o.Type == "STANDARD" && o.AcceptedCurrentSpeed > 0)
                .OrderBy(o => decimal.Parse(o.Price))
                .ToList();

            var marketData = new Core.Models.MarketData
            {
                Algorithm = algorithm,
                Timestamp = DateTime.UtcNow,
                ActiveOrders = marketOrders.Count
            };

            if (marketOrders.Any())
            {
                marketData.BestBuyPrice = decimal.Parse(marketOrders.First().Price);
                marketData.BestSellPrice = decimal.Parse(marketOrders.Last().Price);
                marketData.AveragePrice = marketOrders.Average(o => decimal.Parse(o.Price));
                marketData.TotalHashrate = marketOrders.Sum(o => decimal.Parse(o.AcceptedCurrentSpeed));

                // Build price tiers
                var priceTiers = marketOrders
                    .GroupBy(o => decimal.Parse(o.Price))
                    .Select(g => new Core.Models.PriceTier
                    {
                        Price = g.Key,
                        TotalHashrate = g.Sum(o => decimal.Parse(o.AcceptedCurrentSpeed)),
                        OrderCount = g.Count()
                    })
                    .OrderBy(t => t.Price)
                    .ToList();

                marketData.PriceTiers = priceTiers;
            }

            return marketData;
        }

        #endregion
    }
}
