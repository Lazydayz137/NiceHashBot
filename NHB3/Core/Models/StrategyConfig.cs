using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NHB3.Core.Models
{
    /// <summary>
    /// Configuration for a trading strategy
    /// </summary>
    public class StrategyConfig
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Core.Interfaces.StrategyType Type { get; set; }

        public bool Enabled { get; set; }

        public List<string> Algorithms { get; set; }
        public List<string> Markets { get; set; } // EU, USA, ASIA, etc.

        // General Settings
        public int UpdateIntervalSeconds { get; set; } = 60;
        public decimal MaxDailySpend { get; set; } = 0.1m;
        public decimal MaxOrderSize { get; set; } = 0.01m;

        // Market Maker Settings
        public int? TargetPosition { get; set; } // Target position in order book (1-10)
        public decimal? PriceOffset { get; set; } // Offset from market price (0.005 = 0.5%)
        public decimal? MinSpread { get; set; } // Minimum spread to maintain

        // OneShot Settings
        public long? MaxDifficulty { get; set; } // Maximum network difficulty
        public int? OrderDuration { get; set; } // Order duration in seconds
        public decimal? MaxPrice { get; set; } // Maximum price to pay

        // Arbitrage Settings
        public decimal? MinProfitMargin { get; set; } // Minimum profit margin (0.05 = 5%)
        public string TargetPool { get; set; } // Pool to redirect hash to
        public bool AutoExecute { get; set; } = false; // Automatically execute arbitrage

        // Risk Management
        public bool StopLossEnabled { get; set; } = true;
        public decimal? StopLossThreshold { get; set; } = 0.2m; // Stop if loss > 20%
        public decimal? TakeProfitThreshold { get; set; } // Auto-close if profit > threshold

        // Additional Configuration (custom key-value pairs)
        public Dictionary<string, object> CustomConfig { get; set; }

        public StrategyConfig()
        {
            Algorithms = new List<string>();
            Markets = new List<string>();
            CustomConfig = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Result of a strategy execution
    /// </summary>
    public class StrategyExecutionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Actions { get; set; } // List of actions taken
        public decimal ProfitOrLoss { get; set; }
        public Dictionary<string, object> Metrics { get; set; }

        public StrategyExecutionResult()
        {
            Actions = new List<string>();
            Metrics = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Current status of a strategy
    /// </summary>
    public class StrategyStatus
    {
        public string StrategyName { get; set; }
        public bool IsRunning { get; set; }
        public DateTime LastExecuted { get; set; }
        public int ExecutionCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }

        // Performance Metrics
        public decimal TotalSpent { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }

        // Current State
        public int ActiveOrders { get; set; }
        public int ActiveRentals { get; set; }
        public string CurrentAction { get; set; }
        public Dictionary<string, object> AdditionalMetrics { get; set; }

        public StrategyStatus()
        {
            AdditionalMetrics = new Dictionary<string, object>();
        }
    }
}
