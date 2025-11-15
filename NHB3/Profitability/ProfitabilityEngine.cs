using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHB3.Core.Interfaces;
using NHB3.Core.Models;
using NHB3.Core.Services;
using NHB3.NiceHash;

namespace NHB3.Profitability
{
    /// <summary>
    /// Cross-platform profitability analysis engine
    /// </summary>
    public class ProfitabilityEngine : IProfitabilityCalculator
    {
        private readonly NiceHashService _niceHashService;
        private readonly WhatToMineClient _whatToMineClient;

        public ProfitabilityEngine(NiceHashService niceHashService, WhatToMineClient whatToMineClient = null)
        {
            _niceHashService = niceHashService ?? throw new ArgumentNullException(nameof(niceHashService));
            _whatToMineClient = whatToMineClient;
        }

        public async Task<ProfitabilityReport> CalculateProfitabilityAsync(
            string algorithm,
            decimal hashrate,
            CancellationToken cancellationToken = default)
        {
            var report = new ProfitabilityReport
            {
                Algorithm = algorithm,
                Hashrate = hashrate,
                GeneratedAt = DateTime.UtcNow
            };

            try
            {
                // Get NiceHash market data
                var marketData = await _niceHashService.GetMarketDataAsync(algorithm, cancellationToken: cancellationToken);

                if (marketData != null && marketData.PriceTiers.Any())
                {
                    report.NiceHashBuyPrice = marketData.PriceTiers.First().Price;
                    report.NiceHashSellPrice = marketData.BestSellPrice;

                    // Calculate expected revenue from selling hashrate on NiceHash
                    var algoInfo = await _niceHashService.GetAlgorithmAsync(algorithm, cancellationToken);
                    if (algoInfo != null)
                    {
                        var priceFactorDecimal = decimal.Parse(algoInfo.PriceFactor);
                        var marketFactorDecimal = decimal.Parse(algoInfo.MarketFactor);

                        // Revenue = hashrate * price * priceFactor * 24 hours
                        report.NiceHashExpectedRevenue = hashrate * marketData.BestSellPrice *
                                                          priceFactorDecimal * marketFactorDecimal * 24;
                    }
                }

                // TODO: Add WhatToMine pool profitability analysis
                if (_whatToMineClient != null)
                {
                    // This would require mapping algorithms to specific coins
                    // For now, we'll leave this as a placeholder
                }

                // Find arbitrage opportunities
                report.ArbitrageOpportunities = await FindArbitrageOpportunitiesAsync(
                    algorithm,
                    minProfitMargin: 0.05m,
                    cancellationToken);

                // Determine best action
                if (report.ArbitrageOpportunities.Any())
                {
                    var bestOpportunity = report.ArbitrageOpportunities
                        .OrderByDescending(o => o.ProfitMargin)
                        .First();

                    report.RecommendedAction = $"{bestOpportunity.Type}: {bestOpportunity.Notes}";
                    report.ExpectedProfit = bestOpportunity.NetProfit;
                    report.ProfitMargin = bestOpportunity.ProfitMargin;
                }
                else if (report.NiceHashExpectedRevenue > 0)
                {
                    report.RecommendedAction = "Sell hashrate on NiceHash";
                    report.ExpectedProfit = report.NiceHashExpectedRevenue;
                }
                else
                {
                    report.RecommendedAction = "No profitable opportunities found";
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to calculate profitability for {algorithm}");
            }

            return report;
        }

        public async Task<List<ArbitrageOpportunity>> FindArbitrageOpportunitiesAsync(
            string algorithm,
            decimal minProfitMargin = 0.05m,
            CancellationToken cancellationToken = default)
        {
            var opportunities = new List<ArbitrageOpportunity>();

            try
            {
                var marketData = await _niceHashService.GetMarketDataAsync(algorithm, cancellationToken: cancellationToken);

                if (marketData == null || !marketData.PriceTiers.Any())
                {
                    return opportunities;
                }

                // Simple arbitrage: Buy at best price, hope to mine blocks worth more
                // This is a simplified example - real arbitrage requires pool profitability data

                var buyPrice = marketData.BestBuyPrice;
                var sellPrice = marketData.BestSellPrice;

                if (sellPrice > buyPrice)
                {
                    var profitMargin = ((sellPrice - buyPrice) / buyPrice) * 100;

                    if (profitMargin >= minProfitMargin * 100)
                    {
                        opportunities.Add(new ArbitrageOpportunity
                        {
                            Type = "Market-Spread",
                            SourcePlatform = "NiceHash-Buy",
                            TargetPlatform = "NiceHash-Sell",
                            Algorithm = algorithm,
                            BuyCost = buyPrice,
                            SellRevenue = sellPrice,
                            Fees = buyPrice * 0.03m, // Assume 3% fees
                            NetProfit = (sellPrice - buyPrice) - (buyPrice * 0.03m),
                            ProfitMargin = profitMargin,
                            ValidUntil = DateTime.UtcNow.AddHours(1),
                            Notes = $"Spread arbitrage: Buy @ {buyPrice:F8}, Sell @ {sellPrice:F8}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to find arbitrage opportunities for {algorithm}");
            }

            return opportunities;
        }

        public async Task<MarketData> GetMarketDataAsync(string algorithm, CancellationToken cancellationToken = default)
        {
            return await _niceHashService.GetMarketDataAsync(algorithm, cancellationToken: cancellationToken);
        }

        public async Task<decimal> CalculateNiceHashToPoolProfitAsync(
            string algorithm,
            decimal hashrate,
            string targetPool,
            decimal niceHashPrice,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implement pool profitability calculation
            // This requires integrating with pool APIs or WhatToMine data
            Logger.Instance.Warning("NiceHashToPool profit calculation not yet implemented");
            return 0m;
        }

        public async Task<decimal> CalculateMrrToNiceHashProfitAsync(
            string algorithm,
            decimal hashrate,
            decimal mrrRentalPrice,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implement MRR to NiceHash arbitrage calculation
            Logger.Instance.Warning("MRR to NiceHash profit calculation not yet implemented");
            return 0m;
        }
    }
}
