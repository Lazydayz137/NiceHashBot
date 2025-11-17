using System;
using System.Linq;
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
    /// Specialized Equihash profitability monitor
    /// Real-time calculations for Equihash arbitrage opportunities
    /// </summary>
    public class EquihashMonitor
    {
        private readonly RealArbitrageCalculator _calculator;
        private readonly MrrService _mrrService;
        private readonly MiningDutchClient _mdClient;
        private readonly NiceHashService _nhService;

        public EquihashMonitor(
            RealArbitrageCalculator calculator,
            MrrService mrrService,
            MiningDutchClient mdClient,
            NiceHashService nhService)
        {
            _calculator = calculator;
            _mrrService = mrrService;
            _mdClient = mdClient;
            _nhService = nhService;
        }

        /// <summary>
        /// Run continuous monitoring for Equihash opportunities
        /// </summary>
        public async Task MonitorEquihashAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("         EQUIHASH PROFITABILITY MONITOR");
            Console.WriteLine("═══════════════════════════════════════════════════════════\n");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ScanEquihashOpportunitiesAsync(cancellationToken);

                    Console.WriteLine("\n───────────────────────────────────────────────────────────");
                    Console.WriteLine($"Next scan in 2 minutes... (Press Ctrl+C to stop)");
                    Console.WriteLine("───────────────────────────────────────────────────────────\n");

                    await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
            }

            Console.WriteLine("\nMonitoring stopped.");
        }

        /// <summary>
        /// Scan all Equihash opportunities across platforms
        /// </summary>
        public async Task ScanEquihashOpportunitiesAsync(CancellationToken cancellationToken = default)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"[{timestamp}] Scanning Equihash opportunities...\n");

            // Get Mining-Dutch status for Equihash
            var mdStatus = await _mdClient.GetAlgorithmStatusAsync("equihash", cancellationToken);

            if (mdStatus != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Mining-Dutch Equihash Status:");
                Console.ResetColor();
                Console.WriteLine($"  Workers: {mdStatus.Workers}");
                Console.WriteLine($"  Hashrate: {mdStatus.Hashrate:F2} Sol/s");
                Console.WriteLine($"  Current: {mdStatus.EstimateCurrent:F8} mBTC/MH/day");
                Console.WriteLine($"  24h Est: {mdStatus.EstimateLast24h:F8} mBTC/MH/day");
                Console.WriteLine($"  24h Actual: {mdStatus.ActualLast24h:F8} mBTC/MH/day ⭐ (Most Reliable)");
                Console.WriteLine($"  Pool Fees: {mdStatus.Fees}%");
                Console.WriteLine($"  Port: {mdStatus.Port}");
                Console.WriteLine();
            }

            // Scan MRR → Mining-Dutch for Equihash
            await ScanMrrToMiningDutchAsync(cancellationToken);

            // Scan NiceHash → Mining-Dutch for Equihash
            await ScanNhToMiningDutchAsync(cancellationToken);
        }

        /// <summary>
        /// Scan MRR → Mining-Dutch Equihash opportunities
        /// </summary>
        private async Task ScanMrrToMiningDutchAsync(CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("═══ MRR → Mining-Dutch (Equihash) ═══");
            Console.ResetColor();

            try
            {
                // Get available Equihash rigs on MRR
                var rigs = await _mrrService.SearchRigsAsync("Equihash", minHashrate: 100, cancellationToken);

                if (rigs == null || rigs.Count == 0)
                {
                    Console.WriteLine("No Equihash rigs available on MRR\n");
                    return;
                }

                Console.WriteLine($"Found {rigs.Count} Equihash rigs on MRR\n");

                // Show top 5 rigs
                var topRigs = rigs.OrderBy(r => r.Price).Take(5).ToList();

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
                        PrintOpportunity(opportunity, rig);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"MRR scan error: {ex.Message}\n");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Scan NiceHash → Mining-Dutch Equihash opportunities
        /// </summary>
        private async Task ScanNhToMiningDutchAsync(CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("═══ NiceHash → Mining-Dutch (Equihash) ═══");
            Console.ResetColor();

            try
            {
                // Calculate for standard hashrate (1000 Sol/s)
                var opportunity = await _calculator.CalculateNiceHashToMiningDutchAsync(
                    "Equihash",
                    1000, // 1000 Sol/s
                    "USA",
                    ProfitabilityType.Actual24h,
                    cancellationToken);

                if (opportunity != null)
                {
                    PrintOpportunity(opportunity, null);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"NiceHash scan error: {ex.Message}\n");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Print opportunity in formatted way
        /// </summary>
        private void PrintOpportunity(ArbitrageOpportunity opp, RigListing rig)
        {
            // Color based on margin
            if (opp.ProfitMargin > 15)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (opp.ProfitMargin > 10)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (opp.ProfitMargin > 5)
                Console.ForegroundColor = ConsoleColor.White;
            else
                Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.WriteLine($"┌─ {opp.Type} ─────────────────");

            if (rig != null)
            {
                Console.WriteLine($"│ Rig #{rig.Id}: {rig.Hashrate:F2} Sol/s @ {rig.Price:F8} BTC/MH/day");
                Console.WriteLine($"│ Rating: {rig.Rating:F1}/5 | Min: {rig.MinHours}h | Max: {rig.MaxHours}h");
            }
            else
            {
                Console.WriteLine($"│ Hashrate: {opp.RecommendedHashrate:F2} Sol/s");
            }

            Console.WriteLine($"│ Cost:    {opp.BuyCost:F8} BTC");
            Console.WriteLine($"│ Revenue: {opp.SellRevenue:F8} BTC");
            Console.WriteLine($"│ Fees:    {opp.Fees:F8} BTC");
            Console.WriteLine($"│");

            if (opp.ProfitMargin > 0)
            {
                Console.WriteLine($"│ ✅ PROFIT:  {opp.NetProfit:F8} BTC ({opp.ProfitMargin:F2}% margin)");
            }
            else
            {
                Console.WriteLine($"│ ❌ LOSS:    {opp.NetProfit:F8} BTC ({opp.ProfitMargin:F2}% margin)");
            }

            Console.WriteLine($"└────────────────────────────────────");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Get quick Equihash summary
        /// </summary>
        public async Task<string> GetEquihashSummaryAsync(CancellationToken cancellationToken = default)
        {
            var summary = new System.Text.StringBuilder();
            summary.AppendLine("EQUIHASH QUICK SUMMARY");
            summary.AppendLine("══════════════════════");

            try
            {
                // Mining-Dutch status
                var mdStatus = await _mdClient.GetAlgorithmStatusAsync("equihash", cancellationToken);
                if (mdStatus != null)
                {
                    summary.AppendLine($"Mining-Dutch Actual24h: {mdStatus.ActualLast24h:F8} mBTC/MH/day");
                    summary.AppendLine($"Workers: {mdStatus.Workers} | Hashrate: {mdStatus.Hashrate:F2} Sol/s");
                }

                // Check cheapest MRR rig
                var cheapestRig = await _mrrService.GetCheapestRigAsync("Equihash", 100, cancellationToken);
                if (cheapestRig != null)
                {
                    summary.AppendLine($"Cheapest MRR Rig: {cheapestRig.Hashrate:F2} Sol/s @ {cheapestRig.Price:F8} BTC/MH/day");

                    // Quick calc
                    var opp = await _calculator.CalculateMrrToMiningDutchAsync(
                        "Equihash",
                        cheapestRig.Hashrate,
                        ProfitabilityType.Actual24h,
                        24,
                        cancellationToken);

                    if (opp != null)
                    {
                        summary.AppendLine($"MRR→MD Margin: {opp.ProfitMargin:F2}% {(opp.ProfitMargin > 5 ? "✅" : "❌")}");
                    }
                }
            }
            catch (Exception ex)
            {
                summary.AppendLine($"Error: {ex.Message}");
            }

            return summary.ToString();
        }
    }
}
