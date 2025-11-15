using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHB3.Core.Models;

namespace NHB3.Core.Interfaces
{
    /// <summary>
    /// Interface for profitability calculation across multiple platforms
    /// </summary>
    public interface IProfitabilityCalculator
    {
        /// <summary>
        /// Calculate profitability for a specific algorithm across all platforms
        /// </summary>
        Task<ProfitabilityReport> CalculateProfitabilityAsync(
            string algorithm,
            decimal hashrate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Find arbitrage opportunities between platforms
        /// </summary>
        Task<List<ArbitrageOpportunity>> FindArbitrageOpportunitiesAsync(
            string algorithm,
            decimal minProfitMargin = 0.05m,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get real-time market data for an algorithm
        /// </summary>
        Task<MarketData> GetMarketDataAsync(string algorithm, CancellationToken cancellationToken = default);

        /// <summary>
        /// Calculate expected profit for buying hash on NiceHash and mining on another pool
        /// </summary>
        Task<decimal> CalculateNiceHashToPoolProfitAsync(
            string algorithm,
            decimal hashrate,
            string targetPool,
            decimal niceHashPrice,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Calculate expected profit for renting from MRR and pointing to NiceHash
        /// </summary>
        Task<decimal> CalculateMrrToNiceHashProfitAsync(
            string algorithm,
            decimal hashrate,
            decimal mrrRentalPrice,
            CancellationToken cancellationToken = default);
    }
}
