using System;
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
    /// Console launcher for Equihash monitoring
    /// Quick utility to scan Equihash arbitrage opportunities
    /// </summary>
    public class EquihashConsole
    {
        public static async Task RunAsync()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════════════════════════");
                Console.WriteLine("         EQUIHASH ARBITRAGE MONITOR - INITIALIZING");
                Console.WriteLine("═══════════════════════════════════════════════════════════\n");

                // Load configuration
                var config = ConfigManager.LoadConfig();

                // Initialize clients
                Console.WriteLine("Initializing API clients...");

                var nhClient = new NiceHashClient(config.OrganizationID, config.ApiID, config.ApiSecret);
                var nhService = new NiceHashService(nhClient);

                var mdClient = new MiningDutchClient();

                MrrService mrrService = null;
                if (!string.IsNullOrEmpty(config.MrrApiKey) && !string.IsNullOrEmpty(config.MrrApiSecret))
                {
                    var mrrClient = new MrrClient(config.MrrApiKey, config.MrrApiSecret);
                    mrrService = new MrrService(mrrClient);
                    Console.WriteLine("✓ MRR configured");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ MRR not configured - MRR arbitrage paths unavailable");
                    Console.ResetColor();
                }

                Console.WriteLine("✓ NiceHash configured");
                Console.WriteLine("✓ Mining-Dutch configured");
                Console.WriteLine();

                // Create calculator and monitor
                var calculator = new RealArbitrageCalculator(nhService, mrrService, mdClient);
                var monitor = new EquihashMonitor(calculator, mrrService, mdClient, nhService);

                // Show menu
                Console.WriteLine("═══════════════════════════════════════════════════════════");
                Console.WriteLine("Select mode:");
                Console.WriteLine("  1 - Single scan (one-time)");
                Console.WriteLine("  2 - Continuous monitoring (every 2 minutes)");
                Console.WriteLine("  3 - Quick summary");
                Console.WriteLine("  Q - Quit");
                Console.WriteLine("═══════════════════════════════════════════════════════════\n");
                Console.Write("Choice: ");

                var choice = Console.ReadLine()?.Trim().ToLower();
                Console.WriteLine();

                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                    Console.WriteLine("\nShutdown requested...");
                };

                switch (choice)
                {
                    case "1":
                        await monitor.ScanEquihashOpportunitiesAsync(cts.Token);
                        Console.WriteLine("\nScan complete. Press any key to exit...");
                        Console.ReadKey();
                        break;

                    case "2":
                        await monitor.MonitorEquihashAsync(cts.Token);
                        break;

                    case "3":
                        var summary = await monitor.GetEquihashSummaryAsync(cts.Token);
                        Console.WriteLine(summary);
                        Console.WriteLine("\nPress any key to exit...");
                        Console.ReadKey();
                        break;

                    case "q":
                    case "quit":
                    case "exit":
                        Console.WriteLine("Exiting...");
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Exiting...");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nFATAL ERROR: {ex.Message}");
                Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
