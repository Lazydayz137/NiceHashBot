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
    /// PROPERLY accounts for MinHours and MaxHours rental constraints
    /// </summary>
    public partial class RealArbitrageCalculator
    {
        /// <summary>
        /// Calculate MRR → Mining-Dutch arbitrage
        /// Rent hash from MRR, point to Mining-Dutch pool
        /// IMPORTANT: Accounts for minimum rental duration constraints
        /// </summary>
        public async Task<ArbitrageOpportunity> CalculateMrrToMiningDutchAsync(
            string algorithm,
            decimal targetHashrateMh, // Target hashrate in MH
            ProfitabilityType profitType = ProfitabilityType.Actual24h,
            int desiredDurationHours = 24, // Desired rental duration (default 24h)
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

                // 2. Determine actual rental duration based on constraints
                decimal actualRentalHours = desiredDurationHours;

                // Must rent for at least MinHours
                if (actualRentalHours < cheapestRig.MinHours)
                {
                    actualRentalHours = cheapestRig.MinHours;
                }

                // Cannot exceed MaxHours
                if (cheapestRig.MaxHours > 0 && actualRentalHours > cheapestRig.MaxHours)
                {
                    // If desired duration exceeds max, use max allowed
                    actualRentalHours = cheapestRig.MaxHours;
                }

                // 3. Calculate ACTUAL rental cost for the rental period
                // MRR price is in BTC/MH/day, so convert to actual rental period
                decimal mrrCostPerDay = cheapestRig.Price * cheapestRig.Hashrate;
                decimal mrrActualCost = mrrCostPerDay * (actualRentalHours / 24m);

                // 4. Get Mining-Dutch revenue for the same period
                var mdAlgorithmName = AlgorithmMapper.ToMiningDutch(algorithm);
                var mdAlgoStatus = await _miningDutchClient.GetAlgorithmStatusAsync(mdAlgorithmName, cancellationToken);

                if (mdAlgoStatus == null)
                {
                    return CreateFailedOpportunity(algorithm, $"Algorithm '{mdAlgorithmName}' not on Mining-Dutch");
                }

                decimal mdRevenuePerDay = _miningDutchClient.CalculateNetProfitability(
                    mdAlgoStatus,
                    cheapestRig.Hashrate, // Already in MH
                    profitType);

                // Revenue for the actual rental period
                decimal mdActualRevenue = mdRevenuePerDay * (actualRentalHours / 24m);

                // 5. Calculate MRR fees (3%)
                decimal mrrFees = mrrActualCost * 0.03m;

                // Mining-Dutch fees
                decimal mdFees = mdActualRevenue * (mdAlgoStatus.Fees / 100m);

                // 6. Calculate arbitrage for the rental period
                decimal totalCost = mrrActualCost + mrrFees + mdFees;
                decimal grossProfit = mdActualRevenue - totalCost;
                decimal profitMargin = totalCost > 0 ? (grossProfit / totalCost) * 100 : 0;

                // 7. Normalize to daily rates for comparison
                decimal normalizedDailyCost = totalCost * (24m / actualRentalHours);
                decimal normalizedDailyRevenue = mdActualRevenue * (24m / actualRentalHours);
                decimal normalizedDailyProfit = grossProfit * (24m / actualRentalHours);

                var opportunity = new ArbitrageOpportunity
                {
                    Type = "MRR→MiningDutch",
                    SourcePlatform = "MiningRigRentals",
                    TargetPlatform = "Mining-Dutch",
                    Algorithm = algorithm,

                    // Actual costs/revenue for the rental period
                    BuyCost = mrrActualCost,
                    SellRevenue = mdActualRevenue,
                    Fees = mrrFees + mdFees,
                    NetProfit = grossProfit,
                    ProfitMargin = profitMargin,

                    RecommendedHashrate = cheapestRig.Hashrate,
                    RecommendedDuration = (int)(actualRentalHours * 3600), // Convert to seconds

                    ValidUntil = DateTime.UtcNow.AddHours(1),
                    Notes = $@"{(profitMargin > 0 ? "PROFITABLE ✅" : "NOT PROFITABLE ❌")}

RENTAL PERIOD: {actualRentalHours} hours (Min: {cheapestRig.MinHours}h, Max: {cheapestRig.MaxHours}h)

Rig #{cheapestRig.Id}: {cheapestRig.Hashrate:F2} MH @ {cheapestRig.Price:F8} BTC/MH/day
Rental Cost:  {mrrActualCost:F8} BTC for {actualRentalHours}h ({mrrCostPerDay:F8} BTC/day rate)
MRR Fees:     {mrrFees:F8} BTC (3%)

Mining Revenue: {mdActualRevenue:F8} BTC for {actualRentalHours}h ({mdRevenuePerDay:F8} BTC/day rate using {profitType})
MD Fees:        {mdFees:F8} BTC ({mdAlgoStatus.Fees}%)

NET PROFIT: {grossProfit:F8} BTC for {actualRentalHours}h rental ({profitMargin:F2}% margin)
Daily equiv: {normalizedDailyProfit:F8} BTC/day if sustained

Rig rating: {cheapestRig.Rating:F1}/5
{(cheapestRig.MinHours != desiredDurationHours ? $"⚠️ Forced to rent {actualRentalHours}h (min constraint)" : "")}
{(cheapestRig.MaxHours > 0 && cheapestRig.MaxHours < desiredDurationHours ? $"⚠️ Limited to {actualRentalHours}h (max constraint)" : "")}"
                };

                if (profitMargin > 0)
                {
                    Logger.Instance.Info($"💰 MRR→MD PROFITABLE: {algorithm} - {profitMargin:F2}% margin ({actualRentalHours}h rental)");
                }
                else
                {
                    Logger.Instance.Debug($"❌ MRR→MD NOT PROFITABLE: {algorithm} - {profitMargin:F2}% margin ({actualRentalHours}h rental)");
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
        /// IMPORTANT: Accounts for minimum rental duration constraints
        /// </summary>
        public async Task<ArbitrageOpportunity> CalculateMrrToNiceHashAsync(
            string algorithm,
            decimal targetHashrateMh,
            string market = "USA",
            int desiredDurationHours = 24,
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

                // 2. Determine actual rental duration
                decimal actualRentalHours = desiredDurationHours;

                if (actualRentalHours < cheapestRig.MinHours)
                {
                    actualRentalHours = cheapestRig.MinHours;
                }

                if (cheapestRig.MaxHours > 0 && actualRentalHours > cheapestRig.MaxHours)
                {
                    actualRentalHours = cheapestRig.MaxHours;
                }

                // 3. Calculate rental cost
                decimal mrrCostPerDay = cheapestRig.Price * cheapestRig.Hashrate;
                decimal mrrActualCost = mrrCostPerDay * (actualRentalHours / 24m);
                decimal mrrFees = mrrActualCost * 0.03m;

                // 4. Get NiceHash selling price (what you'd earn mining/selling hash on NH)
                var nhMarketData = await _niceHashService.GetMarketDataAsync(algorithm, market, cancellationToken);

                if (nhMarketData == null || !nhMarketData.PriceTiers.Any())
                {
                    return CreateFailedOpportunity(algorithm, "No NH market data");
                }

                var nhSellPrice = nhMarketData.BestSellPrice;

                var nhAlgoInfo = await _niceHashService.GetAlgorithmAsync(algorithm, cancellationToken);
                if (nhAlgoInfo == null)
                {
                    return CreateFailedOpportunity(algorithm, "Algorithm not on NiceHash");
                }

                // Convert MRR hashrate to NH units
                var nhHashrate = AlgorithmMapper.ConvertHashrateToNiceHash(algorithm, cheapestRig.Hashrate);

                // Calculate NH revenue for the rental period
                var priceFactor = decimal.Parse(nhAlgoInfo.PriceFactor);
                var marketFactor = decimal.Parse(nhAlgoInfo.MarketFactor);
                decimal nhRevenuePerDay = nhHashrate * nhSellPrice * priceFactor * marketFactor;
                decimal nhActualRevenue = nhRevenuePerDay * (actualRentalHours / 24m);

                // 5. Calculate arbitrage
                decimal totalCost = mrrActualCost + mrrFees;
                decimal grossProfit = nhActualRevenue - totalCost;
                decimal profitMargin = totalCost > 0 ? (grossProfit / totalCost) * 100 : 0;

                return new ArbitrageOpportunity
                {
                    Type = "MRR→NiceHash",
                    SourcePlatform = "MiningRigRentals",
                    TargetPlatform = $"NiceHash Pool ({market})",
                    Algorithm = algorithm,

                    BuyCost = mrrActualCost,
                    SellRevenue = nhActualRevenue,
                    Fees = mrrFees,
                    NetProfit = grossProfit,
                    ProfitMargin = profitMargin,

                    RecommendedHashrate = nhHashrate,
                    RecommendedDuration = (int)(actualRentalHours * 3600),

                    ValidUntil = DateTime.UtcNow.AddHours(1),
                    Notes = $@"{(profitMargin > 0 ? "PROFITABLE ✅" : "NOT PROFITABLE ❌")}

RENTAL PERIOD: {actualRentalHours} hours (Min: {cheapestRig.MinHours}h, Max: {cheapestRig.MaxHours}h)

Rig #{cheapestRig.Id}: {cheapestRig.Hashrate:F2} MH @ {cheapestRig.Price:F8} BTC/MH/day
Rental Cost: {mrrActualCost:F8} BTC for {actualRentalHours}h
MRR Fees:    {mrrFees:F8} BTC (3%)

NiceHash Revenue: {nhActualRevenue:F8} BTC for {actualRentalHours}h ({nhRevenuePerDay:F8} BTC/day rate)
({nhHashrate:F2} {algorithm} @ {nhSellPrice:F8} BTC/unit/day)

NET PROFIT: {grossProfit:F8} BTC for {actualRentalHours}h rental ({profitMargin:F2}% margin)

⚠️ Note: MRR→NH usually less profitable than NH→Pool or MRR→Pool arbitrage
Rig rating: {cheapestRig.Rating:F1}/5"
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
            int desiredDurationHours = 24,
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
                    niceHashAlgorithm, mdHashrate, profitType, desiredDurationHours, cancellationToken);

                // Path 3: MRR → NiceHash
                comparison.MrrToNh = await CalculateMrrToNiceHashAsync(
                    niceHashAlgorithm, mdHashrate, market, desiredDurationHours, cancellationToken);
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
