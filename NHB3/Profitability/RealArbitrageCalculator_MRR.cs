using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHB3.Core.Models;
using NHB3.Core.Services;
using NHB3.MiningDutch;
using NHB3.MiningRigRentals;

namespace NHB3.Profitability
{
    /// <summary>
    /// MRR arbitrage extensions for RealArbitrageCalculator
    /// </summary>
    public partial class RealArbitrageCalculator
    {
        /// <summary>
        /// Calculate MRR → Mining-Dutch arbitrage
        /// Rent hash from MRR, point to Mining-Dutch pool
        /// </summary>
        public async Task<ArbitrageOpportunity> CalculateMrrToMiningDutchAsync(
            string algorithm,
            decimal targetHashrateMh, // Target hashrate in MH
            ProfitabilityType profitType = ProfitabilityType.Actual24h,
            CancellationToken cancellationToken = default)
        {
            if (_mrrService == null)
            {
                return CreateFailedOpportunity(algorithm, "MRR service not configured");
            }

            try
            {
                // 1. Get cheapest MRR rig
                var cheapestRig = await _mrrService.GetCheapestRigAsync(algorithm, minHashrate: targetHashrateMh, cancellationToken);

                if (cheapestRig == null)
                {
                    return CreateFailedOpportunity(algorithm, "No MRR rigs available");
                }

                // 2. Calculate MRR rental cost (BTC per day)
                // MRR price is in BTC/MH/day
                var mrrCostPerDay = cheapestRig.Price * cheapestRig.Hashrate;

                // 3. Get Mining-Dutch revenue
                var mdAlgorithmName = AlgorithmMapper.ToMiningDutch(algorithm);
                var mdAlgoStatus = await _miningDutchClient.GetAlgorithmStatusAsync(mdAlgorithmName, cancellationToken);

                if (mdAlgoStatus == null)
                {
                    return CreateFailedOpportunity(algorithm, $"Algorithm '{mdAlgorithmName}' not on Mining-Dutch");
                }

                var mdRevenuePerDay = _miningDutchClient.CalculateNetProfitability(
                    mdAlgoStatus,
                    cheapestRig.Hashrate, // Already in MH
                    profitType);

                // 4. Calculate arbitrage
                var grossProfit = mdRevenuePerDay - mrrCostPerDay;
                var profitMargin = mrrCostPerDay > 0 ? (grossProfit / mrrCostPerDay) * 100 : 0;

                var opportunity = new ArbitrageOpportunity
                {
                    Type = "MRR→MiningDutch",
                    SourcePlatform = "MiningRigRentals",
                    TargetPlatform = "Mining-Dutch",
                    Algorithm = algorithm,

                    BuyCost = mrrCostPerDay,
                    SellRevenue = mdRevenuePerDay,
                    Fees = mrrCostPerDay * 0.03m + mdRevenuePerDay * (mdAlgoStatus.Fees / 100m),
                    NetProfit = grossProfit,
                    ProfitMargin = profitMargin,

                    RecommendedHashrate = cheapestRig.Hashrate,
                    RecommendedDuration = (int)(cheapestRig.MinHours * 3600), // Convert to seconds

                    ValidUntil = DateTime.UtcNow.AddHours(1),
                    Notes = $@"{(profitMargin > 0 ? "PROFITABLE ✅" : "NOT PROFITABLE ❌")}
Rent rig #{cheapestRig.Id} on MRR: {cheapestRig.Hashrate:F2} MH @ {cheapestRig.Price:F8} BTC/MH/day = {mrrCostPerDay:F8} BTC/day
Mine on Mining-Dutch: {mdRevenuePerDay:F8} BTC/day (using {profitType})
Net profit: {grossProfit:F8} BTC/day ({profitMargin:F2}% margin)
Rig rating: {cheapestRig.Rating:F1}/5, Min hours: {cheapestRig.MinHours}"
                };

                if (profitMargin > 0)
                {
                    Logger.Instance.Info($"💰 MRR→MD PROFITABLE: {algorithm} - {profitMargin:F2}% margin");
                }

                return opportunity;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to calculate MRR→MD arbitrage for {algorithm}");
                return CreateFailedOpportunity(algorithm, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate MRR → NiceHash Pool arbitrage
        /// Rent hash from MRR, mine on NiceHash pool (sell hashrate)
        /// </summary>
        public async Task<ArbitrageOpportunity> CalculateMrrToNiceHashAsync(
            string algorithm,
            decimal targetHashrateMh,
            string market = "USA",
            CancellationToken cancellationToken = default)
        {
            if (_mrrService == null)
            {
                return CreateFailedOpportunity(algorithm, "MRR service not configured");
            }

            try
            {
                // 1. Get MRR rig
                var cheapestRig = await _mrrService.GetCheapestRigAsync(algorithm, minHashrate: targetHashrateMh, cancellationToken);

                if (cheapestRig == null)
                {
                    return CreateFailedOpportunity(algorithm, "No MRR rigs available");
                }

                var mrrCostPerDay = cheapestRig.Price * cheapestRig.Hashrate;

                // 2. Get NiceHash selling price (what you'd earn mining/selling hash on NH)
                var nhMarketData = await _niceHashService.GetMarketDataAsync(algorithm, market, cancellationToken);

                if (nhMarketData == null || !nhMarketData.PriceTiers.Any())
                {
                    return CreateFailedOpportunity(algorithm, "No NH market data");
                }

                // Best sell price (what buyers pay - what you'd earn as a seller)
                var nhSellPrice = nhMarketData.BestSellPrice;

                // Get algorithm info
                var nhAlgoInfo = await _niceHashService.GetAlgorithmAsync(algorithm, cancellationToken);
                if (nhAlgoInfo == null)
                {
                    return CreateFailedOpportunity(algorithm, "Algorithm not on NiceHash");
                }

                // Convert MRR hashrate to NH units
                var nhHashrate = AlgorithmMapper.ConvertHashrateToNiceHash(algorithm, cheapestRig.Hashrate);

                // Calculate NH revenue
                var priceFactor = decimal.Parse(nhAlgoInfo.PriceFactor);
                var marketFactor = decimal.Parse(nhAlgoInfo.MarketFactor);
                var nhRevenuePerDay = nhHashrate * nhSellPrice * priceFactor * marketFactor;

                // 3. Calculate arbitrage
                var grossProfit = nhRevenuePerDay - mrrCostPerDay;
                var profitMargin = mrrCostPerDay > 0 ? (grossProfit / mrrCostPerDay) * 100 : 0;

                return new ArbitrageOpportunity
                {
                    Type = "MRR→NiceHash",
                    SourcePlatform = "MiningRigRentals",
                    TargetPlatform = $"NiceHash Pool ({market})",
                    Algorithm = algorithm,

                    BuyCost = mrrCostPerDay,
                    SellRevenue = nhRevenuePerDay,
                    Fees = mrrCostPerDay * 0.03m,
                    NetProfit = grossProfit,
                    ProfitMargin = profitMargin,

                    RecommendedHashrate = nhHashrate,
                    RecommendedDuration = (int)(cheapestRig.MinHours * 3600),

                    ValidUntil = DateTime.UtcNow.AddHours(1),
                    Notes = $@"{(profitMargin > 0 ? "PROFITABLE ✅" : "NOT PROFITABLE ❌")}
Rent rig #{cheapestRig.Id} on MRR: {cheapestRig.Hashrate:F2} MH @ {cheapestRig.Price:F8} BTC/MH/day = {mrrCostPerDay:F8} BTC/day
Mine/sell on NiceHash: {nhHashrate:F2} {algorithm} @ {nhSellPrice:F8} BTC/unit/day = {nhRevenuePerDay:F8} BTC/day
Net profit: {grossProfit:F8} BTC/day ({profitMargin:F2}% margin)
Note: NH pool mining usually less profitable than NH→Pool arbitrage"
                };
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to calculate MRR→NH arbitrage for {algorithm}");
                return CreateFailedOpportunity(algorithm, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Find BEST arbitrage path across all options
        /// Compares: NH→MD, MRR→MD, MRR→NH
        /// </summary>
        public async Task<ArbitrageComparison> FindBestArbitragePathAsync(
            string niceHashAlgorithm,
            decimal defaultHashrate = 1000m,
            string market = "USA",
            ProfitabilityType profitType = ProfitabilityType.Actual24h,
            CancellationToken cancellationToken = default)
        {
            var comparison = new ArbitrageComparison
            {
                Algorithm = niceHashAlgorithm,
                Timestamp = DateTime.UtcNow
            };

            // Path 1: NiceHash → Mining-Dutch
            comparison.NhToMd = await CalculateNiceHashToMiningDutchAsync(
                niceHashAlgorithm, defaultHashrate, market, profitType, cancellationToken);

            // Path 2: MRR → Mining-Dutch (if MRR available)
            if (_mrrService != null)
            {
                var mdHashrate = AlgorithmMapper.ConvertHashrateToMiningDutch(niceHashAlgorithm, defaultHashrate);
                comparison.MrrToMd = await CalculateMrrToMiningDutchAsync(
                    niceHashAlgorithm, mdHashrate, profitType, cancellationToken);

                // Path 3: MRR → NiceHash
                comparison.MrrToNh = await CalculateMrrToNiceHashAsync(
                    niceHashAlgorithm, mdHashrate, market, cancellationToken);
            }

            // Determine best path
            var allPaths = new[]
            {
                comparison.NhToMd,
                comparison.MrrToMd,
                comparison.MrrToNh
            }.Where(p => p != null).ToList();

            comparison.BestPath = allPaths
                .OrderByDescending(p => p.ProfitMargin)
                .FirstOrDefault();

            if (comparison.BestPath != null && comparison.BestPath.ProfitMargin > 0)
            {
                Logger.Instance.Info($"🏆 BEST PATH for {niceHashAlgorithm}: {comparison.BestPath.Type} ({comparison.BestPath.ProfitMargin:F2}% margin)");
            }

            return comparison;
        }
    }

    /// <summary>
    /// Comparison of all arbitrage paths
    /// </summary>
    public class ArbitrageComparison
    {
        public string Algorithm { get; set; }
        public DateTime Timestamp { get; set; }
        public ArbitrageOpportunity NhToMd { get; set; }
        public ArbitrageOpportunity MrrToMd { get; set; }
        public ArbitrageOpportunity MrrToNh { get; set; }
        public ArbitrageOpportunity BestPath { get; set; }
    }
}
