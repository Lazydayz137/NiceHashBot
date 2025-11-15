using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NHB3.NiceHash.Models
{
    /// <summary>
    /// Mining algorithm information
    /// </summary>
    public class Algorithm
    {
        [JsonProperty("algorithm")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("displayMiningFactor")]
        public string DisplayMiningFactor { get; set; }

        [JsonProperty("displayMarketFactor")]
        public string DisplayMarketFactor { get; set; }

        [JsonProperty("miningFactor")]
        public string MiningFactor { get; set; }

        [JsonProperty("marketFactor")]
        public string MarketFactor { get; set; }

        [JsonProperty("priceFactor")]
        public string PriceFactor { get; set; }

        [JsonProperty("displayPriceFactor")]
        public string DisplayPriceFactor { get; set; }

        [JsonProperty("minimalOrderAmount")]
        public string MinimalOrderAmount { get; set; }

        [JsonProperty("minSpeedLimit")]
        public string MinSpeedLimit { get; set; }

        [JsonProperty("maxSpeedLimit")]
        public string MaxSpeedLimit { get; set; }

        [JsonProperty("priceDownStep")]
        public string PriceDownStep { get; set; }

        [JsonProperty("ordersEnabled")]
        public bool OrdersEnabled { get; set; }
    }

    /// <summary>
    /// Account balance information
    /// </summary>
    public class AccountBalance
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("available")]
        public string Available { get; set; }

        [JsonProperty("totalBalance")]
        public string TotalBalance { get; set; }

        [JsonProperty("pending")]
        public string Pending { get; set; }

        [JsonProperty("debt")]
        public string Debt { get; set; }
    }

    /// <summary>
    /// Mining pool configuration
    /// </summary>
    public class Pool
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("algorithm")]
        public string Algorithm { get; set; }

        [JsonProperty("stratumHostname")]
        public string StratumHostname { get; set; }

        [JsonProperty("stratumPort")]
        public int StratumPort { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("updatedTs")]
        public string UpdatedTs { get; set; }

        [JsonProperty("inMoratorium")]
        public bool InMoratorium { get; set; }
    }

    /// <summary>
    /// Hashpower order
    /// </summary>
    public class Order
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } // STANDARD, FIXED

        [JsonProperty("market")]
        public string Market { get; set; } // EU, USA, EU_N, USA_E, ASIA, SA

        [JsonProperty("algorithm")]
        public string Algorithm { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("limit")]
        public string Limit { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("availableAmount")]
        public string AvailableAmount { get; set; }

        [JsonProperty("payedAmount")]
        public string PayedAmount { get; set; }

        [JsonProperty("alive")]
        public bool Alive { get; set; }

        [JsonProperty("createdTs")]
        public string CreatedTs { get; set; }

        [JsonProperty("updatedTs")]
        public string UpdatedTs { get; set; }

        [JsonProperty("pool")]
        public OrderPool Pool { get; set; }

        [JsonProperty("rigsCount")]
        public int RigsCount { get; set; }

        [JsonProperty("acceptedCurrentSpeed")]
        public string AcceptedCurrentSpeed { get; set; }

        [JsonProperty("miningStatus")]
        public string MiningStatus { get; set; }

        [JsonProperty("displayMarketFactor")]
        public string DisplayMarketFactor { get; set; }

        [JsonProperty("marketFactor")]
        public string MarketFactor { get; set; }

        [JsonProperty("priceFactor")]
        public string PriceFactor { get; set; }

        [JsonProperty("estimateDurationInSeconds")]
        public long? EstimateDurationInSeconds { get; set; }
    }

    /// <summary>
    /// Pool information within an order
    /// </summary>
    public class OrderPool
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("algorithm")]
        public string Algorithm { get; set; }

        [JsonProperty("stratumHostname")]
        public string StratumHostname { get; set; }

        [JsonProperty("stratumPort")]
        public int StratumPort { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    /// <summary>
    /// Order book (market depth)
    /// </summary>
    public class OrderBook
    {
        [JsonProperty("stats")]
        public OrderBookStats Stats { get; set; }

        [JsonProperty("asksFromBuyers")]
        public List<OrderBookEntry> AsksFromBuyers { get; set; }

        [JsonProperty("offersFromMiners")]
        public List<OrderBookEntry> OffersFromMiners { get; set; }
    }

    public class OrderBookStats
    {
        [JsonProperty("totalSpeed")]
        public string TotalSpeed { get; set; }

        [JsonProperty("totalOrders")]
        public int TotalOrders { get; set; }
    }

    public class OrderBookEntry
    {
        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("speed")]
        public string Speed { get; set; }

        [JsonProperty("orders")]
        public int Orders { get; set; }
    }

    /// <summary>
    /// Request to create a new order
    /// </summary>
    public class CreateOrderRequest
    {
        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("algorithm")]
        public string Algorithm { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("limit")]
        public string Limit { get; set; }

        [JsonProperty("poolId")]
        public string PoolId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "STANDARD";

        [JsonProperty("marketFactor")]
        public string MarketFactor { get; set; }

        [JsonProperty("displayMarketFactor")]
        public string DisplayMarketFactor { get; set; }
    }
}
