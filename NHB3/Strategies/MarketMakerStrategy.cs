using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHB3.Core.Interfaces;
using NHB3.Core.Models;
using NHB3.Core.Services;
using NHB3.NiceHash;

namespace NHB3.Strategies
{
    /// <summary>
    /// Market maker strategy - maintains competitive pricing in the order book
    /// </summary>
    public class MarketMakerStrategy : BaseStrategy
    {
        private readonly NiceHashService _niceHashService;

        public override StrategyType Type => StrategyType.MarketMaker;

        public MarketMakerStrategy(NiceHashService niceHashService) : base("MarketMaker")
        {
            _niceHashService = niceHashService ?? throw new ArgumentNullException(nameof(niceHashService));
        }

        public override async Task<StrategyExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var result = new StrategyExecutionResult { Success = true };

            if (!IsEnabled)
            {
                result.Success = false;
                result.Message = "Strategy is disabled";
                return result;
            }

            UpdateStatus(s => s.IsRunning = true);

            try
            {
                foreach (var algorithm in Config.Algorithms)
                {
                    foreach (var market in Config.Markets)
                    {
                        await ProcessAlgorithmAsync(algorithm, market, result, cancellationToken);
                    }
                }

                UpdateStatus(s => s.SuccessCount++);
                result.Message = $"Processed {Config.Algorithms.Count} algorithms";
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "MarketMaker strategy execution failed");
                result.Success = false;
                result.Message = ex.Message;
                UpdateStatus(s => s.FailureCount++);
            }
            finally
            {
                UpdateStatus(s =>
                {
                    s.IsRunning = false;
                    s.ExecutionCount++;
                });
            }

            return result;
        }

        private async Task ProcessAlgorithmAsync(string algorithm, string market, StrategyExecutionResult result, CancellationToken cancellationToken)
        {
            try
            {
                // Get market data
                var marketData = await _niceHashService.GetMarketDataAsync(algorithm, market, cancellationToken);

                if (marketData == null || !marketData.PriceTiers.Any())
                {
                    Logger.Instance.Warning($"No market data for {algorithm} on {market}");
                    return;
                }

                // Get my active orders
                var myOrders = await _niceHashService.GetMyOrdersAsync(cancellationToken);
                var myAlgoOrders = myOrders
                    .Where(o => o.Algorithm == algorithm && o.Market == market && o.Alive)
                    .ToList();

                if (!myAlgoOrders.Any())
                {
                    Logger.Instance.Info($"No active orders for {algorithm} on {market}");
                    return;
                }

                // Calculate target price based on market position
                var targetPosition = Config.TargetPosition ?? 3; // Default to 3rd position
                var priceOffset = Config.PriceOffset ?? 0.005m; // Default to 0.5% offset

                var targetPrice = CalculateTargetPrice(marketData, targetPosition, priceOffset);

                // Update orders if needed
                foreach (var order in myAlgoOrders)
                {
                    var currentPrice = decimal.Parse(order.Price);
                    var priceDifference = Math.Abs(currentPrice - targetPrice) / currentPrice;

                    if (priceDifference > 0.01m) // More than 1% difference
                    {
                        Logger.Instance.Info($"Updating order {order.Id} price from {currentPrice:F8} to {targetPrice:F8}");
                        await _niceHashService.UpdateOrderAsync(order.Id, price: targetPrice, cancellationToken: cancellationToken);
                        result.Actions.Add($"Updated {algorithm} order price to {targetPrice:F8}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to process {algorithm} on {market}");
            }
        }

        private decimal CalculateTargetPrice(MarketData marketData, int targetPosition, decimal priceOffset)
        {
            if (targetPosition <= 0 || targetPosition > marketData.PriceTiers.Count)
            {
                // Default to best price if position is invalid
                return marketData.BestBuyPrice * (1 - priceOffset);
            }

            // Get the price at the target position
            var targetTier = marketData.PriceTiers.OrderBy(t => t.Price).ElementAt(targetPosition - 1);

            // Apply offset to be slightly more competitive
            return targetTier.Price * (1 - priceOffset);
        }

        public override bool ValidateConfig(StrategyConfig config, out string[] errors)
        {
            if (!base.ValidateConfig(config, out errors))
            {
                return false;
            }

            var errorList = errors.ToList();

            if (config.Markets == null || config.Markets.Count == 0)
                errorList.Add("At least one market must be specified");

            if (config.TargetPosition.HasValue && (config.TargetPosition.Value < 1 || config.TargetPosition.Value > 10))
                errorList.Add("Target position must be between 1 and 10");

            errors = errorList.ToArray();
            return errorList.Count == 0;
        }
    }
}
