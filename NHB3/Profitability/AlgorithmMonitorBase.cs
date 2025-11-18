using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NHB3.Core.Services;
using NHB3.MiningDutch;
using NHB3.MiningRigRentals;
using NHB3.NiceHash;

namespace NHB3.Profitability
{
    /// <summary>
    /// Base class for algorithm-specific monitors
    /// Provides common functionality for monitoring any algorithm
    /// </summary>
    public abstract class AlgorithmMonitorBase
    {
        protected readonly RealArbitrageCalculator _calculator;
        protected readonly MrrService _mrrService;
        protected readonly MiningDutchClient _mdClient;
        protected readonly NiceHashService _nhService;

        // Algorithm info
        public abstract string AlgorithmName { get; }
        public abstract string AlgorithmDisplayName { get; }
        public abstract string HashrateUnit { get; }
        public abstract decimal[] TestHashrates { get; }

        // Session statistics
        protected int _totalScans = 0;
        protected int _profitableOpportunities = 0;
        protected decimal _bestMarginSeen = 0;
        protected ArbitrageOpportunity _bestOpportunity = null;
        protected DateTime _sessionStart = DateTime.Now;
        protected DateTime _lastScanTime = DateTime.MinValue;

        // Profitability history for trend analysis
        protected List<decimal> _profitabilityHistory = new List<decimal>();
        protected const int MAX_HISTORY_POINTS = 100;

        // Configuration
        public int ScanIntervalMinutes { get; set; } = 2;
        public decimal AlertThresholdPercent { get; set; } = 10.0m;
        public bool EnableAlerts { get; set; } = true;
        public bool ShowDetailedStats { get; set; } = true;
        public int TopRigsToShow { get; set; } = 5;
        public bool EnablePredictiveMetrics { get; set; } = true;

        protected AlgorithmMonitorBase(
            RealArbitrageCalculator calculator,
            MrrService mrrService,
            MiningDutchClient mdClient,
            NiceHashService nhService)
        {
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _mdClient = mdClient ?? throw new ArgumentNullException(nameof(mdClient));
            _nhService = nhService ?? throw new ArgumentNullException(nameof(nhService));
            _mrrService = mrrService; // Can be null
        }

        /// <summary>
        /// Run continuous monitoring
        /// </summary>
        public async Task MonitorAsync(CancellationToken cancellationToken = default)
        {
            PrintHeader($"{AlgorithmDisplayName} PROFITABILITY MONITOR - CONTINUOUS MODE");
            Console.WriteLine($"Scan Interval: {ScanIntervalMinutes} min | Alert: {AlertThresholdPercent}% | Predictions: {(EnablePredictiveMetrics ? "ON" : "OFF")}\n");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ScanOpportunitiesAsync(cancellationToken);

                    if (ShowDetailedStats)
                    {
                        PrintSessionStats();
                    }

                    Console.WriteLine("\n" + new string('─', 63));
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"Next scan in {ScanIntervalMinutes} minutes... (Ctrl+C to stop)");
                    Console.WriteLine($"Session uptime: {GetUptime()}");
                    Console.ResetColor();
                    Console.WriteLine(new string('─', 63) + "\n");

                    await Task.Delay(TimeSpan.FromMinutes(ScanIntervalMinutes), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"⚠ Scan Error: {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine("Retrying in 1 minute...\n");
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
            }

            PrintFinalStats();
        }

        /// <summary>
        /// Scan opportunities for this algorithm
        /// </summary>
        public async Task ScanOpportunitiesAsync(CancellationToken cancellationToken = default)
        {
            _totalScans++;
            _lastScanTime = DateTime.Now;

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"\n╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"║  SCAN #{_totalScans} - {timestamp} - {AlgorithmDisplayName}".PadRight(63) + "║");
            Console.ResetColor();
            Console.WriteLine($"╚═══════════════════════════════════════════════════════════╝\n");

            // Get Mining-Dutch status
            var mdStatus = await GetMiningDutchStatusAsync(cancellationToken);

            // Calculate predictive metrics if enabled
            PredictiveMetrics predictions = null;
            if (EnablePredictiveMetrics && mdStatus != null)
            {
                predictions = await CalculatePredictiveMetricsAsync(mdStatus, cancellationToken);
                PrintPredictiveMetrics(predictions);
            }

            // Scan MRR → Mining-Dutch
            if (_mrrService != null)
            {
                await ScanMrrToMiningDutchAsync(mdStatus, predictions, cancellationToken);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⊗ MRR scanning disabled (not configured)\n");
                Console.ResetColor();
            }

            // Scan NiceHash → Mining-Dutch
            await ScanNhToMiningDutchAsync(mdStatus, predictions, cancellationToken);
        }

        /// <summary>
        /// Get Mining-Dutch status for algorithm
        /// </summary>
        protected async Task<MiningDutchAlgorithmStatus> GetMiningDutchStatusAsync(CancellationToken cancellationToken)
        {
            try
            {
                var mdStatus = await _mdClient.GetAlgorithmStatusAsync(AlgorithmName, cancellationToken);

                if (mdStatus != null)
                {
                    // Add to history for trend analysis
                    _profitabilityHistory.Add(mdStatus.ActualLast24h);
                    if (_profitabilityHistory.Count > MAX_HISTORY_POINTS)
                        _profitabilityHistory.RemoveAt(0);

                    PrintMiningDutchStatus(mdStatus);
                    return mdStatus;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"⚠ Mining-Dutch API Error: {ex.Message}\n");
                Console.ResetColor();
            }

            return null;
        }

        /// <summary>
        /// Calculate predictive metrics based on historical data
        /// </summary>
        protected async Task<PredictiveMetrics> CalculatePredictiveMetricsAsync(
            MiningDutchAlgorithmStatus mdStatus,
            CancellationToken cancellationToken)
        {
            var metrics = new PredictiveMetrics
            {
                // Mining-Dutch data
                ActualProfitability = mdStatus.ActualLast24h,
                EstimatedProfitability = mdStatus.EstimateCurrent,
                ActiveWorkers = mdStatus.Workers,
                PoolHashrate = mdStatus.Hashrate,

                // Calculate trends
                ProfitabilityTrend = CalculateTrend(mdStatus.ActualLast24h, mdStatus.EstimateLast24h),
                PoolHashrateTrend = 0, // Would need historical data
                WorkersTrend = 0, // Would need historical data
            };

            // Perform full analysis
            metrics.PerformFullAnalysis(_profitabilityHistory);

            return metrics;
        }

        /// <summary>
        /// Calculate trend percentage
        /// </summary>
        protected decimal CalculateTrend(decimal current, decimal previous)
        {
            if (previous == 0) return 0;
            return ((current - previous) / previous) * 100m;
        }

        /// <summary>
        /// Scan MRR → Mining-Dutch opportunities
        /// </summary>
        protected async Task ScanMrrToMiningDutchAsync(
            MiningDutchAlgorithmStatus mdStatus,
            PredictiveMetrics predictions,
            CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"═══════════ MRR → Mining-Dutch ({AlgorithmDisplayName}) ═══════════");
            Console.ResetColor();

            try
            {
                var rigs = await _mrrService.SearchRigsAsync(AlgorithmName, minHashrate: TestHashrates[0] / 2, cancellationToken);

                if (rigs == null || rigs.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"⊗ No {AlgorithmDisplayName} rigs available on MRR\n");
                    Console.ResetColor();
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"✓ Found {rigs.Count} {AlgorithmDisplayName} rigs on MRR (showing top {TopRigsToShow})\n");
                Console.ResetColor();

                var topRigs = rigs.OrderBy(r => r.Price).Take(TopRigsToShow).ToList();

                int rigNumber = 1;
                foreach (var rig in topRigs)
                {
                    var opportunity = await _calculator.CalculateMrrToMiningDutchAsync(
                        AlgorithmName,
                        rig.Hashrate,
                        ProfitabilityType.Actual24h,
                        24,
                        cancellationToken);

                    if (opportunity != null)
                    {
                        PrintEnhancedOpportunity(opportunity, rig, rigNumber++, predictions);

                        if (opportunity.ProfitMargin > _bestMarginSeen)
                        {
                            _bestMarginSeen = opportunity.ProfitMargin;
                            _bestOpportunity = opportunity;
                        }

                        if (opportunity.ProfitMargin > 0)
                            _profitableOpportunities++;

                        if (EnableAlerts && opportunity.ProfitMargin >= AlertThresholdPercent)
                            PrintAlert(opportunity, rig);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"⚠ MRR scan error: {ex.Message}\n");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Scan NiceHash → Mining-Dutch opportunities
        /// </summary>
        protected async Task ScanNhToMiningDutchAsync(
            MiningDutchAlgorithmStatus mdStatus,
            PredictiveMetrics predictions,
            CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"═══════════ NiceHash → Mining-Dutch ({AlgorithmDisplayName}) ═══════════");
            Console.ResetColor();

            try
            {
                foreach (var hashrate in TestHashrates)
                {
                    var opportunity = await _calculator.CalculateNiceHashToMiningDutchAsync(
                        AlgorithmName,
                        hashrate,
                        "USA",
                        ProfitabilityType.Actual24h,
                        cancellationToken);

                    if (opportunity != null)
                    {
                        PrintEnhancedOpportunity(opportunity, null, 0, predictions);

                        if (opportunity.ProfitMargin > _bestMarginSeen)
                        {
                            _bestMarginSeen = opportunity.ProfitMargin;
                            _bestOpportunity = opportunity;
                        }

                        if (opportunity.ProfitMargin > 0)
                            _profitableOpportunities++;

                        if (EnableAlerts && opportunity.ProfitMargin >= AlertThresholdPercent)
                            PrintAlert(opportunity, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"⚠ NiceHash scan error: {ex.Message}\n");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Print Mining-Dutch status
        /// </summary>
        protected virtual void PrintMiningDutchStatus(MiningDutchAlgorithmStatus mdStatus)
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("│ ");
            Console.Write($"Mining-Dutch {AlgorithmDisplayName} Status");
            Console.WriteLine("".PadRight(61 - 18 - AlgorithmDisplayName.Length - 8) + "│");
            Console.ResetColor();
            Console.WriteLine("├─────────────────────────────────────────────────────────┤");

            Console.Write("│ Workers: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{mdStatus.Workers,-8}");
            Console.ResetColor();
            Console.Write(" │ Hashrate: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{mdStatus.Hashrate,12:F2} {HashrateUnit}");
            Console.ResetColor();
            Console.WriteLine("".PadRight(61 - 12 - 12 - 12 - HashrateUnit.Length - 12) + "│");

            Console.Write("│ Current:  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{mdStatus.EstimateCurrent:F8} mBTC/MH/day");
            Console.ResetColor();
            Console.WriteLine("          │");

            Console.Write("│ 24h Est:  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{mdStatus.EstimateLast24h:F8} mBTC/MH/day");
            Console.ResetColor();
            Console.WriteLine("          │");

            Console.Write("│ 24h Act:  ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{mdStatus.ActualLast24h:F8} mBTC/MH/day");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" ⭐ Reliable");
            Console.ResetColor();
            Console.WriteLine(" │");

            Console.Write("│ Pool Fees: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{mdStatus.Fees}%");
            Console.ResetColor();
            Console.Write("      │ Port: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{mdStatus.Port}");
            Console.ResetColor();
            Console.WriteLine("".PadRight(61 - 12 - 7 - 9 - mdStatus.Port.ToString().Length) + "│");

            Console.WriteLine("└─────────────────────────────────────────────────────────┘\n");
        }

        /// <summary>
        /// Print predictive metrics
        /// </summary>
        protected void PrintPredictiveMetrics(PredictiveMetrics predictions)
        {
            if (predictions == null) return;

            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("│ 📊 PREDICTIVE METRICS & FORECAST                        │");
            Console.ResetColor();
            Console.WriteLine("├─────────────────────────────────────────────────────────┤");

            // Trend
            Console.Write("│ Profitability Trend: ");
            if (predictions.ProfitabilityTrend > 0)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (predictions.ProfitabilityTrend < 0)
                Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{predictions.ProfitabilityTrend:+0.00;-0.00;0.00}%");
            Console.ResetColor();
            Console.WriteLine(" (24h change)               │");

            // Volatility
            Console.Write("│ Volatility: ");
            if (predictions.VolatilityRating == "Low")
                Console.ForegroundColor = ConsoleColor.Green;
            else if (predictions.VolatilityRating == "Medium")
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{predictions.VolatilityRating,-8}");
            Console.ResetColor();
            Console.WriteLine($" │ Confidence: {predictions.ConfidenceScore:F0}%            │");

            // Risk
            Console.Write("│ Risk Level: ");
            if (predictions.RiskLevel == "Low")
                Console.ForegroundColor = ConsoleColor.Green;
            else if (predictions.RiskLevel == "Medium")
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{predictions.RiskLevel,-10}");
            Console.ResetColor();
            Console.Write(" │ Action: ");
            if (predictions.RecommendedAction == "Buy")
                Console.ForegroundColor = ConsoleColor.Green;
            else if (predictions.RecommendedAction == "Hold")
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{predictions.RecommendedAction,-6}");
            Console.ResetColor();
            Console.WriteLine("                 │");

            // Predictions
            Console.WriteLine("│                                                         │");
            Console.WriteLine("│ Profitability Forecast:                                 │");
            Console.WriteLine($"│   1h:  {predictions.PredictedProfitIn1h:F8} mBTC/MH/day                 │");
            Console.WriteLine($"│   3h:  {predictions.PredictedProfitIn3h:F8} mBTC/MH/day                 │");
            Console.WriteLine($"│   6h:  {predictions.PredictedProfitIn6h:F8} mBTC/MH/day                 │");
            Console.WriteLine($"│   12h: {predictions.PredictedProfitIn12h:F8} mBTC/MH/day                 │");
            Console.WriteLine($"│   24h: {predictions.PredictedProfitIn24h:F8} mBTC/MH/day                 │");

            // Risk factors
            if (predictions.RiskFactors.Count > 0)
            {
                Console.WriteLine("│                                                         │");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("│ ⚠ Risk Factors:                                         │");
                Console.ResetColor();
                foreach (var factor in predictions.RiskFactors.Take(3))
                {
                    var truncated = factor.Length > 50 ? factor.Substring(0, 50) + "..." : factor;
                    Console.WriteLine($"│   • {truncated.PadRight(53)}│");
                }
            }

            Console.WriteLine("└─────────────────────────────────────────────────────────┘\n");
        }

        // Abstract methods for derived classes to implement specific display logic
        protected abstract void PrintEnhancedOpportunity(
            ArbitrageOpportunity opp,
            RigListing rig,
            int rigNumber,
            PredictiveMetrics predictions);

        protected abstract void PrintAlert(ArbitrageOpportunity opp, RigListing rig);

        // Common utility methods
        protected void PrintHeader(string title)
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"║  {title.PadRight(57)}║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");
        }

        protected void PrintSessionStats()
        {
            Console.WriteLine("\n┌───────────────── Session Statistics ─────────────────┐");
            Console.WriteLine($"│ Total Scans: {_totalScans,-10} │ Uptime: {GetUptime(),-18} │");
            Console.WriteLine($"│ Profitable: {_profitableOpportunities,-11} │ Best Margin: {_bestMarginSeen,-12:F2}%    │");

            if (_bestOpportunity != null)
            {
                Console.WriteLine($"│ Best Profit: {_bestOpportunity.NetProfit,-10:F8} BTC                       │");
            }

            Console.WriteLine($"│ Last Scan: {_lastScanTime:HH:mm:ss}                                       │");
            Console.WriteLine("└───────────────────────────────────────────────────────┘");
        }

        protected void PrintFinalStats()
        {
            Console.WriteLine("\n\n╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║              MONITORING SESSION COMPLETE                  ║");
            Console.ResetColor();
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Algorithm: {AlgorithmDisplayName,-46} ║");
            Console.WriteLine($"║ Session Duration: {GetUptime(),-39} ║");
            Console.WriteLine($"║ Total Scans: {_totalScans,-44} ║");
            Console.WriteLine($"║ Profitable Opportunities: {_profitableOpportunities,-34} ║");
            Console.WriteLine($"║ Best Margin Seen: {_bestMarginSeen,-37:F2}%  ║");

            if (_bestOpportunity != null)
            {
                Console.WriteLine($"║ Best Profit: {_bestOpportunity.NetProfit:F8} BTC                             ║");
            }

            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");
        }

        protected string GetUptime()
        {
            var uptime = DateTime.Now - _sessionStart;
            if (uptime.TotalHours >= 1)
                return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
            else
                return $"{uptime.Minutes}m {uptime.Seconds}s";
        }
    }
}
