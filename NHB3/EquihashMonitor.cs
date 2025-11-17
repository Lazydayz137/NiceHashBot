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
using NHB3.Profitability;

namespace NHB3
{
    /// <summary>
    /// Specialized Equihash profitability monitor with advanced features
    /// Real-time calculations, statistics tracking, and alert system
    /// </summary>
    public class EquihashMonitor
    {
        private readonly RealArbitrageCalculator _calculator;
        private readonly MrrService _mrrService;
        private readonly MiningDutchClient _mdClient;
        private readonly NiceHashService _nhService;

        // Session statistics
        private int _totalScans = 0;
        private int _profitableOpportunities = 0;
        private decimal _bestMarginSeen = 0;
        private ArbitrageOpportunity _bestOpportunity = null;
        private DateTime _sessionStart = DateTime.Now;
        private DateTime _lastScanTime = DateTime.MinValue;

        // Configuration
        public int ScanIntervalMinutes { get; set; } = 2;
        public decimal AlertThresholdPercent { get; set; } = 10.0m;
        public bool EnableAlerts { get; set; } = true;
        public bool ShowDetailedStats { get; set; } = true;
        public int TopRigsToShow { get; set; } = 5;

        public EquihashMonitor(
            RealArbitrageCalculator calculator,
            MrrService mrrService,
            MiningDutchClient mdClient,
            NiceHashService nhService)
        {
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _mdClient = mdClient ?? throw new ArgumentNullException(nameof(mdClient));
            _nhService = nhService ?? throw new ArgumentNullException(nameof(nhService));
            _mrrService = mrrService; // Can be null if MRR not configured
        }

        /// <summary>
        /// Run continuous monitoring for Equihash opportunities
        /// </summary>
        public async Task MonitorEquihashAsync(CancellationToken cancellationToken = default)
        {
            PrintHeader("EQUIHASH PROFITABILITY MONITOR - CONTINUOUS MODE");
            Console.WriteLine($"Scan Interval: {ScanIntervalMinutes} minutes | Alert Threshold: {AlertThresholdPercent}%\n");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ScanEquihashOpportunitiesAsync(cancellationToken);

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

                    // Wait 1 minute before retry on error
                    Console.WriteLine("Retrying in 1 minute...\n");
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
            }

            PrintFinalStats();
        }

        /// <summary>
        /// Scan all Equihash opportunities across platforms
        /// </summary>
        public async Task ScanEquihashOpportunitiesAsync(CancellationToken cancellationToken = default)
        {
            _totalScans++;
            _lastScanTime = DateTime.Now;

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"\n╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"║  SCAN #{_totalScans} - {timestamp}  ".PadRight(63) + "║");
            Console.ResetColor();
            Console.WriteLine($"╚═══════════════════════════════════════════════════════════╝\n");

            // Get Mining-Dutch status for Equihash
            var mdStatus = await GetMiningDutchStatusAsync(cancellationToken);

            // Scan MRR → Mining-Dutch for Equihash
            if (_mrrService != null)
            {
                await ScanMrrToMiningDutchAsync(mdStatus, cancellationToken);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⊗ MRR scanning disabled (not configured)\n");
                Console.ResetColor();
            }

            // Scan NiceHash → Mining-Dutch for Equihash
            await ScanNhToMiningDutchAsync(mdStatus, cancellationToken);
        }

        /// <summary>
        /// Get and display Mining-Dutch status with enhanced formatting
        /// </summary>
        private async Task<MiningDutchAlgorithmStatus> GetMiningDutchStatusAsync(CancellationToken cancellationToken)
        {
            try
            {
                var mdStatus = await _mdClient.GetAlgorithmStatusAsync("equihash", cancellationToken);

                if (mdStatus != null)
                {
                    Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("│ ");
                    Console.Write("Mining-Dutch Equihash Status");
                    Console.WriteLine("                          │");
                    Console.ResetColor();
                    Console.WriteLine("├─────────────────────────────────────────────────────────┤");

                    Console.Write("│ Workers: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{mdStatus.Workers,-8}");
                    Console.ResetColor();
                    Console.Write(" │ Hashrate: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{mdStatus.Hashrate,12:F2} Sol/s");
                    Console.ResetColor();
                    Console.WriteLine("       │");

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
                    Console.WriteLine("                              │");

                    Console.WriteLine("└─────────────────────────────────────────────────────────┘\n");

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
        /// Scan MRR → Mining-Dutch Equihash opportunities with enhanced display
        /// </summary>
        private async Task ScanMrrToMiningDutchAsync(MiningDutchAlgorithmStatus mdStatus, CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("═══════════ MRR → Mining-Dutch (Equihash) ═══════════");
            Console.ResetColor();

            try
            {
                // Get available Equihash rigs on MRR
                var rigs = await _mrrService.SearchRigsAsync("Equihash", minHashrate: 100, cancellationToken);

                if (rigs == null || rigs.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("⊗ No Equihash rigs available on MRR\n");
                    Console.ResetColor();
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"✓ Found {rigs.Count} Equihash rigs on MRR (showing top {TopRigsToShow})\n");
                Console.ResetColor();

                // Show top N rigs by price
                var topRigs = rigs.OrderBy(r => r.Price).Take(TopRigsToShow).ToList();

                int rigNumber = 1;
                foreach (var rig in topRigs)
                {
                    // Calculate profitability for 24h rental
                    var opportunity = await _calculator.CalculateMrrToMiningDutchAsync(
                        "Equihash",
                        rig.Hashrate,
                        ProfitabilityType.Actual24h,
                        24, // Desired 24h
                        cancellationToken);

                    if (opportunity != null)
                    {
                        PrintEnhancedOpportunity(opportunity, rig, rigNumber++);

                        // Track best opportunity
                        if (opportunity.ProfitMargin > _bestMarginSeen)
                        {
                            _bestMarginSeen = opportunity.ProfitMargin;
                            _bestOpportunity = opportunity;
                        }

                        // Count profitable opportunities
                        if (opportunity.ProfitMargin > 0)
                        {
                            _profitableOpportunities++;
                        }

                        // Alert if above threshold
                        if (EnableAlerts && opportunity.ProfitMargin >= AlertThresholdPercent)
                        {
                            PrintAlert(opportunity, rig);
                        }
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
        /// Scan NiceHash → Mining-Dutch Equihash opportunities
        /// </summary>
        private async Task ScanNhToMiningDutchAsync(MiningDutchAlgorithmStatus mdStatus, CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("═══════════ NiceHash → Mining-Dutch (Equihash) ═══════════");
            Console.ResetColor();

            try
            {
                // Test multiple hashrate tiers
                var hashrates = new[] { 500m, 1000m, 2000m, 5000m };

                foreach (var hashrate in hashrates)
                {
                    var opportunity = await _calculator.CalculateNiceHashToMiningDutchAsync(
                        "Equihash",
                        hashrate,
                        "USA",
                        ProfitabilityType.Actual24h,
                        cancellationToken);

                    if (opportunity != null)
                    {
                        PrintEnhancedOpportunity(opportunity, null, 0);

                        // Track best opportunity
                        if (opportunity.ProfitMargin > _bestMarginSeen)
                        {
                            _bestMarginSeen = opportunity.ProfitMargin;
                            _bestOpportunity = opportunity;
                        }

                        // Count profitable opportunities
                        if (opportunity.ProfitMargin > 0)
                        {
                            _profitableOpportunities++;
                        }

                        // Alert if above threshold
                        if (EnableAlerts && opportunity.ProfitMargin >= AlertThresholdPercent)
                        {
                            PrintAlert(opportunity, null);
                        }
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
        /// Print opportunity with enhanced formatting and earnings projections
        /// </summary>
        private void PrintEnhancedOpportunity(ArbitrageOpportunity opp, RigListing rig, int rigNumber)
        {
            // Color based on margin with better thresholds
            ConsoleColor boxColor;
            string emoji;
            if (opp.ProfitMargin >= 15)
            {
                boxColor = ConsoleColor.Green;
                emoji = "💰";
            }
            else if (opp.ProfitMargin >= 10)
            {
                boxColor = ConsoleColor.Yellow;
                emoji = "💵";
            }
            else if (opp.ProfitMargin >= 5)
            {
                boxColor = ConsoleColor.White;
                emoji = "💲";
            }
            else if (opp.ProfitMargin > 0)
            {
                boxColor = ConsoleColor.DarkGray;
                emoji = "⚖️";
            }
            else
            {
                boxColor = ConsoleColor.Red;
                emoji = "❌";
            }

            Console.ForegroundColor = boxColor;

            if (rigNumber > 0)
            {
                Console.WriteLine($"┌─ #{rigNumber} {opp.Type} {emoji} " + new string('─', 40 - opp.Type.Length));
            }
            else
            {
                Console.WriteLine($"┌─ {opp.Type} {emoji} " + new string('─', 44 - opp.Type.Length));
            }

            if (rig != null)
            {
                Console.WriteLine($"│ Rig ID: #{rig.Id} | {rig.Hashrate:F2} Sol/s @ {rig.Price:F8} BTC/MH/day");
                Console.WriteLine($"│ Rating: {rig.Rating:F1}/5 ⭐ | Min: {rig.MinHours}h | Max: {rig.MaxHours}h | Type: {rig.Type}");
            }
            else
            {
                Console.WriteLine($"│ Hashrate: {opp.RecommendedHashrate:F2} Sol/s");
            }

            Console.WriteLine($"│");
            Console.WriteLine($"│ Cost:     {opp.BuyCost:F8} BTC  (rental + fees)");
            Console.WriteLine($"│ Revenue:  {opp.SellRevenue:F8} BTC  (Mining-Dutch payout)");
            Console.WriteLine($"│ Fees:     {opp.Fees:F8} BTC  (platform fees)");
            Console.WriteLine($"│ ─────────────────────────────────────────────────────");

            if (opp.ProfitMargin > 0)
            {
                Console.WriteLine($"│ ✅ NET PROFIT:  {opp.NetProfit:F8} BTC ({opp.ProfitMargin:F2}%)");

                // Show earnings projections
                var dailyProfit = opp.NetProfit;
                var weeklyProfit = dailyProfit * 7;
                var monthlyProfit = dailyProfit * 30;

                Console.WriteLine($"│");
                Console.WriteLine($"│ 📊 Earnings Projection:");
                Console.WriteLine($"│    Daily:   {dailyProfit:F8} BTC");
                Console.WriteLine($"│    Weekly:  {weeklyProfit:F8} BTC");
                Console.WriteLine($"│    Monthly: {monthlyProfit:F8} BTC");
            }
            else
            {
                Console.WriteLine($"│ ❌ NET LOSS:    {Math.Abs(opp.NetProfit):F8} BTC ({opp.ProfitMargin:F2}%)");
            }

            Console.WriteLine($"└" + new string('─', 57));
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Print alert for high-profit opportunities
        /// </summary>
        private void PrintAlert(ArbitrageOpportunity opp, RigListing rig)
        {
            Console.Beep(); // Audio alert

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine("\n" + new string('█', 63));
            Console.WriteLine($"█ 🚨 ALERT: HIGH PROFIT OPPORTUNITY DETECTED! {opp.ProfitMargin:F2}% MARGIN █".PadRight(62) + "█");
            Console.WriteLine(new string('█', 63));
            Console.ResetColor();

            if (rig != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"MRR Rig #{rig.Id}: {rig.Hashrate:F2} Sol/s → Profit: {opp.NetProfit:F8} BTC");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Print session statistics
        /// </summary>
        private void PrintSessionStats()
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

        /// <summary>
        /// Print final session summary
        /// </summary>
        private void PrintFinalStats()
        {
            Console.WriteLine("\n\n╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║              MONITORING SESSION COMPLETE                  ║");
            Console.ResetColor();
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
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

        /// <summary>
        /// Get quick Equihash summary with enhanced formatting
        /// </summary>
        public async Task<string> GetEquihashSummaryAsync(CancellationToken cancellationToken = default)
        {
            var summary = new StringBuilder();

            summary.AppendLine("╔═══════════════════════════════════════════════════════════╗");
            summary.AppendLine("║          EQUIHASH QUICK SUMMARY                           ║");
            summary.AppendLine("╚═══════════════════════════════════════════════════════════╝");
            summary.AppendLine();

            try
            {
                // Mining-Dutch status
                var mdStatus = await _mdClient.GetAlgorithmStatusAsync("equihash", cancellationToken);
                if (mdStatus != null)
                {
                    summary.AppendLine($"Mining-Dutch Status:");
                    summary.AppendLine($"  Actual 24h:  {mdStatus.ActualLast24h:F8} mBTC/MH/day ⭐");
                    summary.AppendLine($"  Workers:     {mdStatus.Workers}");
                    summary.AppendLine($"  Hashrate:    {mdStatus.Hashrate:F2} Sol/s");
                    summary.AppendLine($"  Pool Fees:   {mdStatus.Fees}%");
                    summary.AppendLine();
                }

                // Check cheapest MRR rig
                if (_mrrService != null)
                {
                    var cheapestRig = await _mrrService.GetCheapestRigAsync("Equihash", 100, cancellationToken);
                    if (cheapestRig != null)
                    {
                        summary.AppendLine($"Cheapest MRR Rig:");
                        summary.AppendLine($"  Rig ID:      #{cheapestRig.Id}");
                        summary.AppendLine($"  Hashrate:    {cheapestRig.Hashrate:F2} Sol/s");
                        summary.AppendLine($"  Price:       {cheapestRig.Price:F8} BTC/MH/day");
                        summary.AppendLine($"  Rating:      {cheapestRig.Rating:F1}/5");
                        summary.AppendLine();

                        // Quick calc
                        var opp = await _calculator.CalculateMrrToMiningDutchAsync(
                            "Equihash",
                            cheapestRig.Hashrate,
                            ProfitabilityType.Actual24h,
                            24,
                            cancellationToken);

                        if (opp != null)
                        {
                            summary.AppendLine($"MRR → Mining-Dutch Arbitrage:");
                            summary.AppendLine($"  Margin:      {opp.ProfitMargin:F2}% {(opp.ProfitMargin > 5 ? "✅ PROFITABLE" : "❌ NOT PROFITABLE")}");
                            summary.AppendLine($"  Net Profit:  {opp.NetProfit:F8} BTC");

                            if (opp.ProfitMargin > 0)
                            {
                                var monthlyProfit = opp.NetProfit * 30;
                                summary.AppendLine($"  Est. Monthly: {monthlyProfit:F8} BTC");
                            }
                        }
                    }
                }
                else
                {
                    summary.AppendLine("MRR not configured");
                }
            }
            catch (Exception ex)
            {
                summary.AppendLine($"ERROR: {ex.Message}");
            }

            summary.AppendLine();
            summary.AppendLine("═══════════════════════════════════════════════════════════");

            return summary.ToString();
        }

        /// <summary>
        /// Export scan results to file
        /// </summary>
        public void ExportResults(string filePath)
        {
            try
            {
                var export = new StringBuilder();
                export.AppendLine($"Equihash Monitor Session Export");
                export.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                export.AppendLine($"Session Start: {_sessionStart:yyyy-MM-dd HH:mm:ss}");
                export.AppendLine($"Session Duration: {GetUptime()}");
                export.AppendLine($"Total Scans: {_totalScans}");
                export.AppendLine($"Profitable Opportunities: {_profitableOpportunities}");
                export.AppendLine($"Best Margin Seen: {_bestMarginSeen:F2}%");

                if (_bestOpportunity != null)
                {
                    export.AppendLine($"\nBest Opportunity:");
                    export.AppendLine($"  Type: {_bestOpportunity.Type}");
                    export.AppendLine($"  Margin: {_bestOpportunity.ProfitMargin:F2}%");
                    export.AppendLine($"  Profit: {_bestOpportunity.NetProfit:F8} BTC");
                    export.AppendLine($"  Hashrate: {_bestOpportunity.RecommendedHashrate:F2} Sol/s");
                }

                System.IO.File.WriteAllText(filePath, export.ToString());

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ Results exported to: {filePath}\n");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n⚠ Export failed: {ex.Message}\n");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Get session uptime
        /// </summary>
        private string GetUptime()
        {
            var uptime = DateTime.Now - _sessionStart;
            if (uptime.TotalHours >= 1)
                return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
            else
                return $"{uptime.Minutes}m {uptime.Seconds}s";
        }

        /// <summary>
        /// Print formatted header
        /// </summary>
        private void PrintHeader(string title)
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"║  {title.PadRight(57)}║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");
        }
    }
}
