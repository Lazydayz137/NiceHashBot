using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHB3.Core.Models;
using NHB3.Core.Services;
using NHB3.NiceHash;
using NHB3.MiningDutch;
using NHB3.MiningRigRentals;

namespace NHB3.Profitability
{
    /// <summary>
    /// REAL arbitrage calculator using actual pool data
    /// Supports: NH→MD, MRR→MD, MRR→NH arbitrage paths
    /// </summary>
    public partial class RealArbitrageCalculator
    {
        private readonly NiceHashService _niceHashService;
        private readonly MiningDutchClient _miningDutchClient;
        private readonly MrrService _mrrService;

        public RealArbitrageCalculator(
            NiceHashService niceHashService,
            MiningDutchClient miningDutchClient,
            MrrService mrrService = null)
        {
            _niceHashService = niceHashService ?? throw new ArgumentNullException(nameof(niceHashService));
            _miningDutchClient = miningDutchClient ?? throw new ArgumentNullException(nameof(miningDutchClient));
            _mrrService = mrrService; // Optional
        }

        /// <summary>
        /// Calculate REAL arbitrage opportunity: Buy hash on NiceHash, mine on Mining-Dutch
        /// </summary>
        public async Task<ArbitrageOpportunity> CalculateNiceHashToMiningDutchAsync(
            string niceHashAlgorithm,
            decimal niceHashHashrate, // In NiceHash units (e.g., TH/s for SHA256)
            string market = "USA",
            ProfitabilityType profitType = ProfitabilityType.Actual24h,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Get NiceHash pricing
                var nhMarketData = await _niceHashService.GetMarketDataAsync(niceHashAlgorithm, market, cancellationToken);

                if (nhMarketData == null || !nhMarketData.PriceTiers.Any())
                {
                    Logger.Instance.Warning($"No NiceHash market data for {niceHashAlgorithm}");
                    return CreateFailedOpportunity(niceHashAlgorithm, "No NiceHash market data available");
                }

                // Get best buy price (cheapest hash available)
                var nhBuyPriceBtcPerUnitPerDay = nhMarketData.BestBuyPrice;

                // Get algorithm info for proper unit conversion
                var nhAlgoInfo = await _niceHashService.GetAlgorithmAsync(niceHashAlgorithm, cancellationToken);
                if (nhAlgoInfo == null)
                {
                    return CreateFailedOpportunity(niceHashAlgorithm, "Algorithm not found on NiceHash");
                }

                // 2. Convert to Mining-Dutch algorithm name and hashrate units
                var mdAlgorithmName = AlgorithmMapper.ToMiningDutch(niceHashAlgorithm);
                var mdHashrate = AlgorithmMapper.ConvertHashrateToMiningDutch(niceHashAlgorithm, niceHashHashrate);

                Logger.Instance.Info($"Converted {niceHashHashrate} {niceHashAlgorithm} → {mdHashrate} MH for {mdAlgorithmName}");

                // 3. Get Mining-Dutch profitability
                var mdAlgoStatus = await _miningDutchClient.GetAlgorithmStatusAsync(mdAlgorithmName, cancellationToken);

                if (mdAlgoStatus == null)
                {
                    return CreateFailedOpportunity(niceHashAlgorithm, $"Algorithm '{mdAlgorithmName}' not found on Mining-Dutch");
                }

                // 4. Calculate costs and revenues

                // NiceHash cost calculation
                // NH returns price in BTC per speed unit per day, need to apply price factor
                var priceFactor = decimal.Parse(nhAlgoInfo.PriceFactor);
                var marketFactor = decimal.Parse(nhAlgoInfo.MarketFactor);

                // Cost = hashrate * price * priceFactor * marketFactor
                var nhCostPerDay = niceHashHashrate * nhBuyPriceBtcPerUnitPerDay * priceFactor * marketFactor;

                // Mining-Dutch revenue calculation
                var mdRevenuePerDay = _miningDutchClient.CalculateNetProfitability(mdAlgoStatus, mdHashrate, profitType);

                // 5. Calculate arbitrage
                var grossProfit = mdRevenuePerDay - nhCostPerDay;
                var profitMargin = nhCostPerDay > 0 ? (grossProfit / nhCostPerDay) * 100 : 0;

                // 6. Build opportunity
                var opportunity = new ArbitrageOpportunity
                {
                    Type = "NiceHash→MiningDutch",
                    SourcePlatform = $"NiceHash ({market})",
                    TargetPlatform = "Mining-Dutch",
                    Algorithm = niceHashAlgorithm,

                    BuyCost = nhCostPerDay,
                    SellRevenue = mdRevenuePerDay,
                    Fees = nhCostPerDay * 0.03m + mdRevenuePerDay * (mdAlgoStatus.Fees / 100m), // NH + MD fees
                    NetProfit = grossProfit,
                    ProfitMargin = profitMargin,

                    RecommendedHashrate = niceHashHashrate,
                    RecommendedDuration = 86400, // 24 hours

                    ValidUntil = DateTime.UtcNow.AddHours(1),
                    Notes = BuildOpportunityNotes(
                        niceHashAlgorithm,
                        niceHashHashrate,
                        nhBuyPriceBtcPerUnitPerDay,
                        nhCostPerDay,
                        mdAlgoStatus,
                        mdRevenuePerDay,
                        profitType,
                        profitMargin)
                };

                // Log the result
                if (profitMargin > 0)
                {
                    Logger.Instance.Info($"💰 PROFITABLE: {niceHashAlgorithm} - {profitMargin:F2}% margin ({grossProfit:F8} BTC/day)");
                }
                else
                {
                    Logger.Instance.Debug($"❌ Not profitable: {niceHashAlgorithm} - {profitMargin:F2}% margin");
                }

                return opportunity;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to calculate arbitrage for {niceHashAlgorithm}");
                return CreateFailedOpportunity(niceHashAlgorithm, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Scan all supported algorithms for arbitrage opportunities
        /// </summary>
        public async Task<List<ArbitrageOpportunity>> FindAllOpportunitiesAsync(
            decimal defaultHashrate = 1000m, // Default 1000 units
            decimal minProfitMargin = 5.0m,  // Minimum 5% profit
            string market = "USA",
            ProfitabilityType profitType = ProfitabilityType.Actual24h,
            CancellationToken cancellationToken = default)
        {
            var opportunities = new List<ArbitrageOpportunity>();

            Logger.Instance.Info("Scanning for arbitrage opportunities across all algorithms...");

            var supportedAlgos = AlgorithmMapper.GetSupportedAlgorithms();

            foreach (var algorithm in supportedAlgos)
            {
                try
                {
                    var opportunity = await CalculateNiceHashToMiningDutchAsync(
                        algorithm,
                        defaultHashrate,
                        market,
                        profitType,
                        cancellationToken);

                    if (opportunity != null && opportunity.ProfitMargin >= minProfitMargin)
                    {
                        opportunities.Add(opportunity);
                    }

                    // Small delay to avoid hammering APIs
                    await Task.Delay(500, cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Warning($"Skipping {algorithm}: {ex.Message}");
                }
            }

            // Sort by profit margin descending
            var sortedOpportunities = opportunities
                .OrderByDescending(o => o.ProfitMargin)
                .ToList();

            Logger.Instance.Info($"Found {sortedOpportunities.Count} profitable opportunities (>{minProfitMargin}% margin)");

            return sortedOpportunities;
        }

        /// <summary>
        /// Compare different profitability metrics for an algorithm
        /// </summary>
        public async Task<ProfitabilityComparison> CompareMetricsAsync(
            string niceHashAlgorithm,
            decimal hashrate,
            string market = "USA",
            CancellationToken cancellationToken = default)
        {
            var current = await CalculateNiceHashToMiningDutchAsync(
                niceHashAlgorithm, hashrate, market, ProfitabilityType.Current, cancellationToken);

            var estimate24h = await CalculateNiceHashToMiningDutchAsync(
                niceHashAlgorithm, hashrate, market, ProfitabilityType.Estimate24h, cancellationToken);

            var actual24h = await CalculateNiceHashToMiningDutchAsync(
                niceHashAlgorithm, hashrate, market, ProfitabilityType.Actual24h, cancellationToken);

            return new ProfitabilityComparison
            {
                Algorithm = niceHashAlgorithm,
                Hashrate = hashrate,
                CurrentProfit = current,
                Estimate24hProfit = estimate24h,
                Actual24hProfit = actual24h,
                RecommendedMetric = DetermineRecommendedMetric(current, estimate24h, actual24h)
            };
        }

        private string BuildOpportunityNotes(
            string algorithm,
            decimal hashrate,
            decimal nhPrice,
            decimal nhCost,
            AlgorithmStatus mdStatus,
            decimal mdRevenue,
            ProfitabilityType profitType,
            decimal profitMargin)
        {
            var profitTypeStr = profitType switch
            {
                ProfitabilityType.Current => "current estimate",
                ProfitabilityType.Estimate24h => "24h estimate",
                ProfitabilityType.Actual24h => "24h actual",
                _ => "unknown"
            };

            var verdict = profitMargin > 0 ? "PROFITABLE ✅" : "NOT PROFITABLE ❌";

            return $@"{verdict}
Buy {hashrate:F2} {algorithm} on NiceHash @ {nhPrice:F8} BTC/unit/day = {nhCost:F8} BTC/day
Mine on Mining-Dutch (pool fee: {mdStatus.Fees}%) using {profitTypeStr} = {mdRevenue:F8} BTC/day
Net profit: {(mdRevenue - nhCost):F8} BTC/day ({profitMargin:F2}% margin)

Mining-Dutch Stats:
- Workers: {mdStatus.Workers}
- Pool Hashrate: {mdStatus.Hashrate:F2} MH
- 24h Blocks: {mdStatus.Blocks24h}
- Current: {mdStatus.EstimateCurrent:F6} mBTC/MH/day
- Est 24h: {mdStatus.EstimateLast24h:F6} mBTC/MH/day
- Actual 24h: {mdStatus.ActualLast24h:F6} mBTC/MH/day (most reliable)";
        }

        private ArbitrageOpportunity CreateFailedOpportunity(string algorithm, string reason)
        {
            return new ArbitrageOpportunity
            {
                Type = "NiceHash→MiningDutch",
                Algorithm = algorithm,
                BuyCost = 0,
                SellRevenue = 0,
                NetProfit = 0,
                ProfitMargin = 0,
                Notes = $"Failed: {reason}"
            };
        }

        private ProfitabilityType DetermineRecommendedMetric(
            ArbitrageOpportunity current,
            ArbitrageOpportunity estimate24h,
            ArbitrageOpportunity actual24h)
        {
            // Actual24h is most reliable (real past performance)
            // But if current is significantly higher, might want to act fast
            // If actual24h is unavailable (= 0), fall back to estimate

            if (actual24h.SellRevenue > 0)
                return ProfitabilityType.Actual24h;

            if (estimate24h.SellRevenue > 0)
                return ProfitabilityType.Estimate24h;

            return ProfitabilityType.Current;
        }
    }

    /// <summary>
    /// Comparison of different profitability metrics
    /// </summary>
    public class ProfitabilityComparison
    {
        public string Algorithm { get; set; }
        public decimal Hashrate { get; set; }
        public ArbitrageOpportunity CurrentProfit { get; set; }
        public ArbitrageOpportunity Estimate24hProfit { get; set; }
        public ArbitrageOpportunity Actual24hProfit { get; set; }
        public ProfitabilityType RecommendedMetric { get; set; }
    }
}
