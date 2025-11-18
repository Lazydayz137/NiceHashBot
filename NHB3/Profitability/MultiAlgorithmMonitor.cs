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
    /// Monitors multiple algorithms simultaneously
    /// Top 5 most profitable algorithms based on market data
    /// </summary>
    public class MultiAlgorithmMonitor
    {
        private readonly RealArbitrageCalculator _calculator;
        private readonly MrrService _mrrService;
        private readonly MiningDutchClient _mdClient;
        private readonly NiceHashService _nhService;

        // Top algorithms to monitor
        private readonly List<AlgorithmInfo> _algorithms = new List<AlgorithmInfo>
        {
            new AlgorithmInfo
            {
                Name = "Equihash",
                DisplayName = "Equihash (Zcash, BTG)",
                HashrateUnit = "Sol/s",
                TestHashrates = new[] { 500m, 1000m, 2000m, 5000m },
                MinHashrate = 100m,
                Emoji = "💎",
                Description = "GPU mining - High profitability potential"
            },
            new AlgorithmInfo
            {
                Name = "SHA256",
                DisplayName = "SHA-256 (Bitcoin)",
                HashrateUnit = "TH/s",
                TestHashrates = new[] { 10m, 50m, 100m, 500m },
                MinHashrate = 1m,
                Emoji = "₿",
                Description = "ASIC mining - Most liquid market"
            },
            new AlgorithmInfo
            {
                Name = "Scrypt",
                DisplayName = "Scrypt (Litecoin, Doge)",
                HashrateUnit = "MH/s",
                TestHashrates = new[] { 100m, 500m, 1000m, 5000m },
                MinHashrate = 10m,
                Emoji = "🐕",
                Description = "ASIC mining - Stable profitability"
            },
            new AlgorithmInfo
            {
                Name = "X11",
                DisplayName = "X11 (Dash)",
                HashrateUnit = "MH/s",
                TestHashrates = new[] { 50m, 100m, 500m, 1000m },
                MinHashrate = 10m,
                Emoji = "💳",
                Description = "ASIC mining - Lower competition"
            },
            new AlgorithmInfo
            {
                Name = "KawPow",
                DisplayName = "KawPow (Ravencoin)",
                HashrateUnit = "MH/s",
                TestHashrates = new[] { 10m, 50m, 100m, 250m },
                MinHashrate = 5m,
                Emoji = "🦅",
                Description = "GPU mining - Growing market"
            }
        };

        // Scan results
        private Dictionary<string, AlgorithmScanResult> _lastScanResults = new Dictionary<string, AlgorithmScanResult>();

        public MultiAlgorithmMonitor(
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
        /// Scan all algorithms and find the best opportunities
        /// </summary>
        public async Task ScanAllAlgorithmsAsync(CancellationToken cancellationToken = default)
        {
            Console.Clear();
            PrintHeader();

            var results = new List<AlgorithmScanResult>();

            foreach (var algo in _algorithms)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n{algo.Emoji} Scanning {algo.DisplayName}...");
                    Console.ResetColor();

                    var result = await ScanAlgorithmAsync(algo, cancellationToken);
                    results.Add(result);
                    _lastScanResults[algo.Name] = result;

                    // Brief summary
                    PrintAlgorithmSummary(algo, result);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"⚠ Error scanning {algo.DisplayName}: {ex.Message}");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }

            // Print consolidated results
            PrintConsolidatedResults(results);
        }

        /// <summary>
        /// Scan a single algorithm
        /// </summary>
        private async Task<AlgorithmScanResult> ScanAlgorithmAsync(
            AlgorithmInfo algo,
            CancellationToken cancellationToken)
        {
            var result = new AlgorithmScanResult
            {
                Algorithm = algo,
                ScanTime = DateTime.Now
            };

            // Get Mining-Dutch status
            try
            {
                result.MiningDutchStatus = await _mdClient.GetAlgorithmStatusAsync(algo.Name, cancellationToken);

                if (result.MiningDutchStatus != null)
                {
                    result.IsAvailableOnMiningDutch = true;
                }
            }
            catch
            {
                result.IsAvailableOnMiningDutch = false;
            }

            // Get cheapest MRR rig
            if (_mrrService != null)
            {
                try
                {
                    result.CheapestMrrRig = await _mrrService.GetCheapestRigAsync(
                        algo.Name,
                        algo.MinHashrate,
                        cancellationToken);

                    if (result.CheapestMrrRig != null && result.MiningDutchStatus != null)
                    {
                        // Calculate MRR arbitrage
                        result.BestMrrOpportunity = await _calculator.CalculateMrrToMiningDutchAsync(
                            algo.Name,
                            result.CheapestMrrRig.Hashrate,
                            ProfitabilityType.Actual24h,
                            24,
                            cancellationToken);
                    }
                }
                catch
                {
                    // MRR not available for this algorithm
                }
            }

            // Test NiceHash at mid-range hashrate
            if (result.MiningDutchStatus != null)
            {
                try
                {
                    var testHashrate = algo.TestHashrates[algo.TestHashrates.Length / 2];
                    result.NiceHashOpportunity = await _calculator.CalculateNiceHashToMiningDutchAsync(
                        algo.Name,
                        testHashrate,
                        "USA",
                        ProfitabilityType.Actual24h,
                        cancellationToken);
                }
                catch
                {
                    // NiceHash not available for this algorithm
                }
            }

            // Determine best opportunity
            result.DetermineBestOpportunity();

            return result;
        }

        /// <summary>
        /// Print brief summary for an algorithm
        /// </summary>
        private void PrintAlgorithmSummary(AlgorithmInfo algo, AlgorithmScanResult result)
        {
            if (!result.IsAvailableOnMiningDutch)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  ⊗ Not available on Mining-Dutch");
                Console.ResetColor();
                return;
            }

            Console.Write($"  Workers: {result.MiningDutchStatus.Workers,-6} ");
            Console.Write($"Hashrate: {result.MiningDutchStatus.Hashrate:F2} {algo.HashrateUnit,-6} ");
            Console.Write($"Profit: {result.MiningDutchStatus.ActualLast24h:F8} mBTC/MH/day");
            Console.WriteLine();

            if (result.BestOpportunity != null)
            {
                Console.Write($"  Best: ");
                if (result.BestOpportunity.ProfitMargin >= 15)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (result.BestOpportunity.ProfitMargin >= 10)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (result.BestOpportunity.ProfitMargin >= 5)
                    Console.ForegroundColor = ConsoleColor.White;
                else if (result.BestOpportunity.ProfitMargin > 0)
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                else
                    Console.ForegroundColor = ConsoleColor.Red;

                Console.Write($"{result.BestOpportunity.ProfitMargin:F2}% margin");
                Console.ResetColor();
                Console.Write($" ({result.BestOpportunity.Type}) ");
                Console.Write($"- {result.BestOpportunity.NetProfit:F8} BTC");
                Console.WriteLine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  ⊗ No profitable opportunities found");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Print consolidated results table
        /// </summary>
        private void PrintConsolidatedResults(List<AlgorithmScanResult> results)
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║          MULTI-ALGORITHM PROFITABILITY SUMMARY            ║");
            Console.ResetColor();
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");

            // Sort by best margin
            var sorted = results
                .Where(r => r.BestOpportunity != null)
                .OrderByDescending(r => r.BestOpportunity.ProfitMargin)
                .ToList();

            if (sorted.Count == 0)
            {
                Console.WriteLine("║ No profitable opportunities found across all algorithms  ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");
                return;
            }

            Console.WriteLine("║                                                           ║");

            int rank = 1;
            foreach (var result in sorted.Take(10))
            {
                var algo = result.Algorithm;
                var opp = result.BestOpportunity;

                Console.Write($"║ {rank}. ");

                // Color based on margin
                if (opp.ProfitMargin >= 15)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (opp.ProfitMargin >= 10)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (opp.ProfitMargin >= 5)
                    Console.ForegroundColor = ConsoleColor.White;
                else if (opp.ProfitMargin > 0)
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write($"{algo.Emoji} {algo.DisplayName,-25}");
                Console.ResetColor();

                Console.Write($" {opp.ProfitMargin,6:F2}%");

                Console.WriteLine(" ║");

                rank++;
            }

            Console.WriteLine("║                                                           ║");
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");

            // Show top recommendation
            var topResult = sorted.First();
            var topOpp = topResult.BestOpportunity;
            var topAlgo = topResult.Algorithm;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"║ 🏆 TOP RECOMMENDATION: {topAlgo.DisplayName,-32}║");
            Console.ResetColor();
            Console.WriteLine($"║    Margin: {topOpp.ProfitMargin:F2}%                                          ║");
            Console.WriteLine($"║    Profit: {topOpp.NetProfit:F8} BTC/day                        ║");
            Console.WriteLine($"║    Type: {topOpp.Type,-46}║");
            Console.WriteLine($"║    Hashrate: {topOpp.RecommendedHashrate:F2} {topAlgo.HashrateUnit,-36}║");

            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            // Show profitability color legend
            PrintLegend();
        }

        /// <summary>
        /// Print header
        /// </summary>
        private void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║     ⚡ MULTI-ALGORITHM ARBITRAGE SCANNER v2.0 ⚡         ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║    Monitoring 5 algorithms for profitable opportunities  ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        /// <summary>
        /// Print profitability legend
        /// </summary>
        private void PrintLegend()
        {
            Console.WriteLine("Profitability Legend:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  💰 Green");
            Console.ResetColor();
            Console.Write("  (≥15%)  - Excellent | ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("💵 Yellow");
            Console.ResetColor();
            Console.Write(" (≥10%)  - Good | ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("💲 White");
            Console.ResetColor();
            Console.WriteLine(" (≥5%)  - OK");
        }

        /// <summary>
        /// Get detailed report for specific algorithm
        /// </summary>
        public string GetAlgorithmReport(string algorithmName)
        {
            if (!_lastScanResults.ContainsKey(algorithmName))
                return $"No scan data available for {algorithmName}";

            var result = _lastScanResults[algorithmName];
            var sb = new StringBuilder();

            sb.AppendLine($"╔═══════════════════════════════════════════════════════════╗");
            sb.AppendLine($"║ {result.Algorithm.Emoji} {result.Algorithm.DisplayName,-54}║");
            sb.AppendLine($"╚═══════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            if (!result.IsAvailableOnMiningDutch)
            {
                sb.AppendLine("⊗ Algorithm not available on Mining-Dutch");
                return sb.ToString();
            }

            sb.AppendLine($"Mining-Dutch Status:");
            sb.AppendLine($"  Workers: {result.MiningDutchStatus.Workers}");
            sb.AppendLine($"  Hashrate: {result.MiningDutchStatus.Hashrate:F2} {result.Algorithm.HashrateUnit}");
            sb.AppendLine($"  Actual 24h: {result.MiningDutchStatus.ActualLast24h:F8} mBTC/MH/day");
            sb.AppendLine($"  Fees: {result.MiningDutchStatus.Fees}%");
            sb.AppendLine();

            if (result.BestMrrOpportunity != null)
            {
                sb.AppendLine($"Best MRR Opportunity:");
                sb.AppendLine($"  Margin: {result.BestMrrOpportunity.ProfitMargin:F2}%");
                sb.AppendLine($"  Profit: {result.BestMrrOpportunity.NetProfit:F8} BTC/day");
                sb.AppendLine($"  Rig: #{result.CheapestMrrRig.Id} - {result.CheapestMrrRig.Hashrate:F2} {result.Algorithm.HashrateUnit}");
                sb.AppendLine();
            }

            if (result.NiceHashOpportunity != null)
            {
                sb.AppendLine($"NiceHash Opportunity:");
                sb.AppendLine($"  Margin: {result.NiceHashOpportunity.ProfitMargin:F2}%");
                sb.AppendLine($"  Profit: {result.NiceHashOpportunity.NetProfit:F8} BTC/day");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Continuous monitoring of all algorithms
        /// </summary>
        public async Task MonitorContinuouslyAsync(
            int intervalMinutes = 5,
            CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Starting continuous monitoring (scan interval: {intervalMinutes} minutes)");
            Console.WriteLine("Press Ctrl+C to stop\n");

            int scanNumber = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    scanNumber++;
                    Console.WriteLine($"═══ SCAN #{scanNumber} - {DateTime.Now:HH:mm:ss} ═══");

                    await ScanAllAlgorithmsAsync(cancellationToken);

                    Console.WriteLine($"\nNext scan in {intervalMinutes} minutes...\n");
                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), cancellationToken);
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
    }

    /// <summary>
    /// Algorithm configuration
    /// </summary>
    public class AlgorithmInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string HashrateUnit { get; set; }
        public decimal[] TestHashrates { get; set; }
        public decimal MinHashrate { get; set; }
        public string Emoji { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Scan result for a single algorithm
    /// </summary>
    public class AlgorithmScanResult
    {
        public AlgorithmInfo Algorithm { get; set; }
        public DateTime ScanTime { get; set; }

        public bool IsAvailableOnMiningDutch { get; set; }
        public MiningDutchAlgorithmStatus MiningDutchStatus { get; set; }

        public RigListing CheapestMrrRig { get; set; }
        public ArbitrageOpportunity BestMrrOpportunity { get; set; }
        public ArbitrageOpportunity NiceHashOpportunity { get; set; }

        public ArbitrageOpportunity BestOpportunity { get; set; }

        public void DetermineBestOpportunity()
        {
            var opportunities = new List<ArbitrageOpportunity>();

            if (BestMrrOpportunity != null)
                opportunities.Add(BestMrrOpportunity);

            if (NiceHashOpportunity != null)
                opportunities.Add(NiceHashOpportunity);

            BestOpportunity = opportunities
                .OrderByDescending(o => o.ProfitMargin)
                .FirstOrDefault();
        }
    }
}
