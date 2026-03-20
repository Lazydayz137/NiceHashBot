using System;
using System.Threading;
using System.Threading.Tasks;
using NHB3.Core.Models;
using NHB3.Core.Services;
using NHB3.MiningDutch;
using NHB3.MiningRigRentals;
using NHB3.NiceHash;
using NHB3.NiceHash.Models;

namespace NHB3.Profitability
{
    /// <summary>
    /// Automatically executes profitable arbitrage opportunities
    /// Creates orders on NiceHash or rentals on MRR based on opportunity type
    /// </summary>
    public class AutoExecutionEngine
    {
        private readonly NiceHashService _niceHashService;
        private readonly MrrService _mrrService;
        private readonly BotConfiguration _config;

        public AutoExecutionEngine(
            NiceHashService niceHashService,
            MrrService mrrService = null,
            BotConfiguration config = null)
        {
            _niceHashService = niceHashService ?? throw new ArgumentNullException(nameof(niceHashService));
            _mrrService = mrrService;
            _config = config ?? ConfigManager.Instance.LoadBotConfig();
        }

        /// <summary>
        /// Execute an arbitrage opportunity
        /// </summary>
        public async Task<ArbitrageExecution> ExecuteAsync(
            ArbitrageOpportunity opportunity,
            CancellationToken cancellationToken = default)
        {
            if (opportunity == null)
                throw new ArgumentNullException(nameof(opportunity));

            Logger.Instance.Info($"Executing arbitrage: {opportunity.Type} for {opportunity.Algorithm}");

            try
            {
                // Risk management checks
                if (!ValidateExecution(opportunity, out string validationError))
                {
                    return CreateFailedExecution(opportunity, validationError);
                }

                // Execute based on type
                return opportunity.Type switch
                {
                    "NiceHash→MiningDutch" => await ExecuteNiceHashToMiningDutchAsync(opportunity, cancellationToken),
                    "MRR→MiningDutch" => await ExecuteMrrToMiningDutchAsync(opportunity, cancellationToken),
                    "MRR→NiceHash" => await ExecuteMrrToNiceHashAsync(opportunity, cancellationToken),
                    _ => CreateFailedExecution(opportunity, $"Unknown arbitrage type: {opportunity.Type}")
                };
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to execute arbitrage for {opportunity.Algorithm}");
                return CreateFailedExecution(opportunity, ex.Message);
            }
        }

        private async Task<ArbitrageExecution> ExecuteNiceHashToMiningDutchAsync(
            ArbitrageOpportunity opportunity,
            CancellationToken cancellationToken)
        {
            Logger.Instance.Info($"Creating NiceHash order for {opportunity.Algorithm}");

            try
            {
                // 1. Get or create Mining-Dutch pool
                var pool = await GetOrCreateMiningDutchPoolAsync(opportunity.Algorithm, cancellationToken);

                if (pool == null)
                {
                    return CreateFailedExecution(opportunity, "Failed to create Mining-Dutch pool");
                }

                // 2. Get algorithm info
                var algoInfo = await _niceHashService.GetAlgorithmAsync(opportunity.Algorithm, cancellationToken);

                if (algoInfo == null)
                {
                    return CreateFailedExecution(opportunity, "Algorithm not found on NiceHash");
                }

                // 3. Get market data to determine competitive price
                var marketData = await _niceHashService.GetMarketDataAsync(
                    opportunity.Algorithm,
                    opportunity.SourcePlatform.Contains("USA") ? "USA" : "EU",
                    cancellationToken);

                // Use slightly lower than best price to ensure fill
                var orderPrice = marketData.BestBuyPrice * 0.995m; // 0.5% below best price

                // 4. Calculate order amount (limit to max order size from config)
                var maxOrderSize = _config.RiskManagement.MaxOrderSize;
                var orderAmount = Math.Min(opportunity.BuyCost, maxOrderSize);

                // 5. Create order request
                var orderRequest = new CreateOrderRequest
                {
                    Market = opportunity.SourcePlatform.Contains("USA") ? "USA" : "EU",
                    Algorithm = opportunity.Algorithm,
                    Amount = orderAmount.ToString("F8"),
                    Price = orderPrice.ToString("F8"),
                    Limit = opportunity.RecommendedHashrate.ToString("F8"),
                    PoolId = pool.Id,
                    Type = "STANDARD",
                    MarketFactor = algoInfo.MarketFactor,
                    DisplayMarketFactor = algoInfo.DisplayMarketFactor
                };

                // 6. Create the order
                var order = await _niceHashService.CreateOrderAsync(orderRequest, cancellationToken);

                if (order == null)
                {
                    return CreateFailedExecution(opportunity, "Failed to create NiceHash order");
                }

                Logger.Instance.Info($"✅ Created NiceHash order {order.Id} for {opportunity.Algorithm}");

                return new ArbitrageExecution
                {
                    Success = true,
                    Opportunity = opportunity,
                    Timestamp = DateTime.UtcNow,
                    Algorithm = opportunity.Algorithm,
                    Type = opportunity.Type,
                    ExpectedMargin = opportunity.ProfitMargin,
                    ExpectedProfit = opportunity.NetProfit,
                    Message = $"Created NiceHash order {order.Id}\nPrice: {orderPrice:F8} BTC/unit/day\nAmount: {orderAmount:F8} BTC\nPool: {pool.Name}",
                    OrderId = order.Id,
                    Status = "Active"
                };
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to execute NiceHash→MiningDutch arbitrage");
                return CreateFailedExecution(opportunity, ex.Message);
            }
        }

        private async Task<ArbitrageExecution> ExecuteMrrToMiningDutchAsync(
            ArbitrageOpportunity opportunity,
            CancellationToken cancellationToken)
        {
            if (_mrrService == null)
            {
                return CreateFailedExecution(opportunity, "MRR service not configured");
            }

            Logger.Instance.Info($"Creating MRR rental for {opportunity.Algorithm}");

            try
            {
                // 1. Get cheapest available rig
                var mdAlgorithm = AlgorithmMapper.ToMiningDutch(opportunity.Algorithm);
                var mdHashrate = AlgorithmMapper.ConvertHashrateToMiningDutch(opportunity.Algorithm, opportunity.RecommendedHashrate);

                var rig = await _mrrService.GetCheapestRigAsync(
                    opportunity.Algorithm,
                    minHashrate: mdHashrate * 0.8m, // Allow 20% variance
                    cancellationToken);

                if (rig == null)
                {
                    return CreateFailedExecution(opportunity, "No MRR rigs available");
                }

                // 2. Create Mining-Dutch pool configuration
                if (string.IsNullOrEmpty(_config.MiningDutch.BTCAddress))
                {
                    return CreateFailedExecution(opportunity, "Mining-Dutch BTC address not configured in bot.json");
                }

                var btcAddress = _config.MiningDutch.BTCAddress;
                var workerPrefix = _config.MiningDutch.WorkerPrefix ?? "NHB3";

                var pool = new MrrPool
                {
                    Host = $"{mdAlgorithm}.mining-dutch.nl",
                    Port = 3333, // Default port
                    User = $"{btcAddress}.{workerPrefix}_MRR_Arb",
                    Pass = "c=BTC", // Auto-convert to BTC
                    Notes = $"Arbitrage: MRR→MD ({opportunity.ProfitMargin:F2}% margin)"
                };

                // 3. Create rental (minimum duration from rig)
                var rental = await _mrrService.CreateRentalAsync(
                    rig.Id,
                    rig.MinHours,
                    pool,
                    cancellationToken);

                if (rental == null)
                {
                    return CreateFailedExecution(opportunity, "Failed to create MRR rental");
                }

                Logger.Instance.Info($"✅ Created MRR rental {rental.Id} for rig {rig.Id}");

                return new ArbitrageExecution
                {
                    Success = true,
                    Opportunity = opportunity,
                    Timestamp = DateTime.UtcNow,
                    Algorithm = opportunity.Algorithm,
                    Type = opportunity.Type,
                    ExpectedMargin = opportunity.ProfitMargin,
                    ExpectedProfit = opportunity.NetProfit,
                    Message = $"Created MRR rental {rental.Id}\nRig: {rig.Id} ({rig.Hashrate:F2} MH)\nPrice: {rig.Price:F8} BTC/MH/day\nDuration: {rig.MinHours}h\nPool: {pool.Host}",
                    RentalId = rental.Id.ToString(),
                    Status = "Active"
                };
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to execute MRR→MiningDutch arbitrage");
                return CreateFailedExecution(opportunity, ex.Message);
            }
        }

        private async Task<ArbitrageExecution> ExecuteMrrToNiceHashAsync(
            ArbitrageOpportunity opportunity,
            CancellationToken cancellationToken)
        {
            // Similar to ExecuteMrrToMiningDutchAsync but point to NiceHash pool
            // This is less common but possible

            return CreateFailedExecution(opportunity, "MRR→NiceHash execution not yet implemented");
        }

        private async Task<Pool> GetOrCreateMiningDutchPoolAsync(string algorithm, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Check if pool already exists
                var pools = await _niceHashService.GetPoolsAsync(cancellationToken);
                var mdAlgorithm = AlgorithmMapper.ToMiningDutch(algorithm);

                var existingPool = pools.Find(p =>
                    p.Algorithm == algorithm &&
                    p.StratumHostname.Contains("mining-dutch.nl"));

                if (existingPool != null)
                {
                    Logger.Instance.Info($"Using existing Mining-Dutch pool: {existingPool.Name}");
                    return existingPool;
                }

                // 2. Create new pool
                if (string.IsNullOrEmpty(_config.MiningDutch.BTCAddress))
                {
                    Logger.Instance.Error("Mining-Dutch BTC address not configured in bot.json");
                    return null;
                }

                var btcAddress = _config.MiningDutch.BTCAddress;
                var workerPrefix = _config.MiningDutch.WorkerPrefix ?? "NHB3";

                var newPool = new Pool
                {
                    Name = $"Mining-Dutch {algorithm}",
                    Algorithm = algorithm,
                    StratumHostname = $"{mdAlgorithm}.mining-dutch.nl",
                    StratumPort = 3333,
                    Username = $"{btcAddress}.{workerPrefix}_NH_Arb",
                    Password = "c=BTC" // Auto-convert to BTC
                };

                var createdPool = await _niceHashService.SavePoolAsync(newPool, cancellationToken);
                Logger.Instance.Info($"Created new Mining-Dutch pool: {createdPool.Name}");

                return createdPool;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to get/create Mining-Dutch pool");
                return null;
            }
        }

        private bool ValidateExecution(ArbitrageOpportunity opportunity, out string error)
        {
            error = null;

            // Check if opportunity is still valid
            if (opportunity.ValidUntil < DateTime.UtcNow)
            {
                error = "Opportunity has expired";
                return false;
            }

            // Check profit margin threshold (configured in bot.json → Profitability.MinProfitMargin)
            var minMargin = _config?.Profitability?.MinProfitMargin ?? 5.0m;
            if (opportunity.ProfitMargin < minMargin)
            {
                error = $"Profit margin too low: {opportunity.ProfitMargin:F2}% (min: {minMargin:F2}%)";
                return false;
            }

            // Check daily spend limit
            if (_config.RiskManagement.MaxDailySpend > 0)
            {
                // TODO: Track daily spend
                // For now, just check order size
                if (opportunity.BuyCost > _config.RiskManagement.MaxOrderSize)
                {
                    error = $"Order size exceeds max: {opportunity.BuyCost:F8} > {_config.RiskManagement.MaxOrderSize:F8} BTC";
                    return false;
                }
            }

            return true;
        }

        private ArbitrageExecution CreateFailedExecution(ArbitrageOpportunity opportunity, string errorMessage)
        {
            return new ArbitrageExecution
            {
                Success = false,
                Opportunity = opportunity,
                Timestamp = DateTime.UtcNow,
                Algorithm = opportunity.Algorithm,
                Type = opportunity.Type,
                ExpectedMargin = opportunity.ProfitMargin,
                ExpectedProfit = opportunity.NetProfit,
                ErrorMessage = errorMessage,
                Status = "Failed"
            };
        }
    }

    /// <summary>
    /// Result of an arbitrage execution
    /// </summary>
    public class ArbitrageExecution
    {
        public bool Success { get; set; }
        public ArbitrageOpportunity Opportunity { get; set; }
        public DateTime Timestamp { get; set; }
        public string Algorithm { get; set; }
        public string Type { get; set; }

        // Expected values (from opportunity)
        public decimal ExpectedMargin { get; set; }
        public decimal ExpectedProfit { get; set; }

        // Actual values (measured after execution)
        public decimal? ActualMargin { get; set; }
        public decimal? ActualProfit { get; set; }

        // Execution details
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public string OrderId { get; set; }
        public string RentalId { get; set; }
        public string Status { get; set; } // Active, Completed, Failed, Cancelled

        // Performance tracking
        public DateTime? CompletedAt { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? ActualRevenue { get; set; }
    }
}
