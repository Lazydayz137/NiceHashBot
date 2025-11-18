using System;
using System.Collections.Generic;

namespace NHB3.Core.Models
{
    /// <summary>
    /// Comprehensive profitability report for an algorithm
    /// </summary>
    public class ProfitabilityReport
    {
        public string Algorithm { get; set; }
        public decimal Hashrate { get; set; }
        public DateTime GeneratedAt { get; set; }

        // NiceHash Data
        public decimal NiceHashBuyPrice { get; set; } // Price to buy hash
        public decimal NiceHashSellPrice { get; set; } // Price you'd earn selling hash
        public decimal NiceHashExpectedRevenue { get; set; }

        // MiningRigRentals Data
        public decimal MrrBestRentalPrice { get; set; }
        public decimal MrrExpectedRevenue { get; set; }
        public bool MrrRigsAvailable { get; set; }

        // Pool Mining Data (from WhatToMine)
        public Dictionary<string, PoolProfitability> PoolProfitability { get; set; }

        // Arbitrage Opportunities
        public List<ArbitrageOpportunity> ArbitrageOpportunities { get; set; }

        // Best Action Recommendation
        public string RecommendedAction { get; set; }
        public decimal ExpectedProfit { get; set; }
        public decimal ProfitMargin { get; set; }

        public ProfitabilityReport()
        {
            PoolProfitability = new Dictionary<string, PoolProfitability>();
            ArbitrageOpportunities = new List<ArbitrageOpportunity>();
        }
    }

    /// <summary>
    /// Profitability data for mining on a specific pool
    /// </summary>
    public class PoolProfitability
    {
        public string PoolName { get; set; }
        public string CoinSymbol { get; set; }
        public decimal EstimatedRevenue24h { get; set; }
        public decimal NetworkDifficulty { get; set; }
        public decimal BlockReward { get; set; }
        public decimal PoolFee { get; set; }
        public decimal ExchangeRate { get; set; } // Coin to BTC
    }

    /// <summary>
    /// Arbitrage opportunity between platforms
    /// </summary>
    public class ArbitrageOpportunity
    {
        public string Type { get; set; } // "NH->Pool", "MRR->NH", "Market->Market"
        public string SourcePlatform { get; set; }
        public string TargetPlatform { get; set; }
        public string Algorithm { get; set; }

        public decimal BuyCost { get; set; }
        public decimal SellRevenue { get; set; }
        public decimal Fees { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; } // Percentage

        public decimal RecommendedHashrate { get; set; }
        public int RecommendedDuration { get; set; } // Seconds

        public DateTime ValidUntil { get; set; }
        public string Notes { get; set; }
    }
}
