using System;
using System.Collections.Generic;

namespace NHB3.Core.Models
{
    /// <summary>
    /// Market data for a specific algorithm
    /// </summary>
    public class MarketData
    {
        public string Algorithm { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal BestBuyPrice { get; set; }
        public decimal BestSellPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal TotalHashrate { get; set; }
        public int ActiveOrders { get; set; }
        public List<PriceTier> PriceTiers { get; set; }
        public Dictionary<string, decimal> MarketPrices { get; set; } // EU, USA, ASIA

        public MarketData()
        {
            PriceTiers = new List<PriceTier>();
            MarketPrices = new Dictionary<string, decimal>();
        }
    }

    /// <summary>
    /// Price tier showing aggregated orders at a specific price point
    /// </summary>
    public class PriceTier
    {
        public decimal Price { get; set; }
        public decimal TotalHashrate { get; set; }
        public int OrderCount { get; set; }
    }
}
