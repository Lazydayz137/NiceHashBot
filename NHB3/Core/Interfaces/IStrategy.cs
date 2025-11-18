using System;
using System.Threading;
using System.Threading.Tasks;
using NHB3.Core.Models;

namespace NHB3.Core.Interfaces
{
    /// <summary>
    /// Interface for trading/mining strategies
    /// </summary>
    public interface IStrategy
    {
        /// <summary>
        /// Strategy name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Strategy type
        /// </summary>
        StrategyType Type { get; }

        /// <summary>
        /// Is this strategy currently enabled?
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Initialize the strategy with configuration
        /// </summary>
        Task InitializeAsync(StrategyConfig config, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute strategy logic (called on timer interval)
        /// </summary>
        Task<StrategyExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Validate strategy configuration
        /// </summary>
        bool ValidateConfig(StrategyConfig config, out string[] errors);

        /// <summary>
        /// Get current strategy status and statistics
        /// </summary>
        StrategyStatus GetStatus();

        /// <summary>
        /// Gracefully stop the strategy
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Strategy type enumeration
    /// </summary>
    public enum StrategyType
    {
        /// <summary>
        /// Market maker strategy - maintain competitive pricing
        /// </summary>
        MarketMaker,

        /// <summary>
        /// OneShot strategy - target low-difficulty coins
        /// </summary>
        OneShot,

        /// <summary>
        /// Arbitrage strategy - profit from price differences
        /// </summary>
        Arbitrage,

        /// <summary>
        /// Custom strategy
        /// </summary>
        Custom
    }
}
