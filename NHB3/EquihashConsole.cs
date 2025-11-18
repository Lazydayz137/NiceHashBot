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
    /// Advanced console launcher for Equihash monitoring
    /// Provides interactive menu with configuration options
    /// </summary>
    public class EquihashConsole
    {
        public static async Task RunAsync()
        {
            try
            {
                Console.Clear();
                PrintBanner();

                // Load configuration
                var config = ConfigManager.LoadConfig();

                // Initialize clients with validation
                Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("│ Initializing API Clients...                            │");
                Console.ResetColor();
                Console.WriteLine("└─────────────────────────────────────────────────────────┘\n");

                NiceHashClient nhClient = null;
                NiceHashService nhService = null;
                MrrService mrrService = null;
                MiningDutchClient mdClient = null;

                try
                {
                    nhClient = new NiceHashClient(config.OrganizationID, config.ApiID, config.ApiSecret);
                    nhService = new NiceHashService(nhClient);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ NiceHash configured");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ NiceHash initialization failed: {ex.Message}");
                    Console.ResetColor();
                }

                try
                {
                    mdClient = new MiningDutchClient();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Mining-Dutch configured");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ Mining-Dutch initialization failed: {ex.Message}");
                    Console.ResetColor();
                }

                if (!string.IsNullOrEmpty(config.MrrApiKey) && !string.IsNullOrEmpty(config.MrrApiSecret))
                {
                    try
                    {
                        var mrrClient = new MrrClient(config.MrrApiKey, config.MrrApiSecret);
                        mrrService = new MrrService(mrrClient);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✓ MRR configured");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠ MRR initialization failed: {ex.Message}");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ MRR not configured - MRR arbitrage paths unavailable");
                    Console.WriteLine("  (Configure MrrApiKey and MrrApiSecret in settings.json)");
                    Console.ResetColor();
                }

                Console.WriteLine();

                if (nhService == null || mdClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("FATAL: Required services not initialized. Please check configuration.");
                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                    return;
                }

                // Create calculator and monitor
                var calculator = new RealArbitrageCalculator(nhService, mrrService, mdClient);
                var monitor = new EquihashMonitor(calculator, mrrService, mdClient, nhService);

                // Main menu loop
                bool running = true;
                while (running)
                {
                    ShowMainMenu();
                    var choice = Console.ReadLine()?.Trim().ToLower();
                    Console.WriteLine();

                    var cts = new CancellationTokenSource();
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\n\n⚠ Shutdown requested...");
                        Console.ResetColor();
                    };

                    switch (choice)
                    {
                        case "1":
                            await RunSingleScan(monitor, cts.Token);
                            break;

                        case "2":
                            await RunContinuousMonitoring(monitor, cts.Token);
                            break;

                        case "3":
                            await ShowQuickSummary(monitor, cts.Token);
                            break;

                        case "4":
                            ConfigureMonitor(monitor);
                            break;

                        case "5":
                            await ExportSessionResults(monitor);
                            break;

                        case "6":
                            ShowHelp();
                            break;

                        case "q":
                        case "quit":
                        case "exit":
                            running = false;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Thank you for using Equihash Monitor! 💎");
                            Console.ResetColor();
                            break;

                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("⚠ Invalid choice. Please try again.\n");
                            Console.ResetColor();
                            break;
                    }

                    if (running && choice != "2") // Don't wait after continuous monitoring
                    {
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        PrintBanner();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    FATAL ERROR                            ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        private static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║      ⚡ EQUIHASH ARBITRAGE MONITOR v2.0 ⚡              ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║    Real-time profitability tracking for Equihash mining  ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ShowMainMenu()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("║                      MAIN MENU                            ║");
            Console.ResetColor();
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  1 - Single Scan           Quick one-time opportunity    ║");
            Console.WriteLine("║                             scan                          ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  2 - Continuous Monitor     Auto-refresh with alerts     ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  3 - Quick Summary         Fast status check             ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  4 - Configure Settings    Adjust scan interval, alerts  ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  5 - Export Results        Save session data to file     ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  6 - Help                  Show detailed help            ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  Q - Quit                  Exit monitor                  ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.Write("\n→ Your choice: ");
        }

        private static async Task RunSingleScan(EquihashMonitor monitor, CancellationToken cancellationToken)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║              SINGLE SCAN MODE                             ║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            await monitor.ScanEquihashOpportunitiesAsync(cancellationToken);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✓ Scan complete!");
            Console.ResetColor();
        }

        private static async Task RunContinuousMonitoring(EquihashMonitor monitor, CancellationToken cancellationToken)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║            CONTINUOUS MONITORING MODE                     ║");
            Console.ResetColor();
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Scan Interval: {monitor.ScanIntervalMinutes} minutes                                   ║");
            Console.WriteLine($"║ Alert Threshold: {monitor.AlertThresholdPercent}%                                       ║");
            Console.WriteLine($"║ Alerts: {(monitor.EnableAlerts ? "ENABLED " : "DISABLED")}                                           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.WriteLine("\nPress Ctrl+C to stop monitoring...\n");

            await monitor.MonitorEquihashAsync(cancellationToken);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n✓ Monitoring stopped.");
            Console.ResetColor();
        }

        private static async Task ShowQuickSummary(EquihashMonitor monitor, CancellationToken cancellationToken)
        {
            Console.WriteLine("Loading quick summary...\n");

            var summary = await monitor.GetEquihashSummaryAsync(cancellationToken);
            Console.WriteLine(summary);
        }

        private static void ConfigureMonitor(EquihashMonitor monitor)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("║              CONFIGURATION MENU                           ║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            Console.WriteLine($"Current Settings:");
            Console.WriteLine($"  1. Scan Interval: {monitor.ScanIntervalMinutes} minutes");
            Console.WriteLine($"  2. Alert Threshold: {monitor.AlertThresholdPercent}%");
            Console.WriteLine($"  3. Alerts: {(monitor.EnableAlerts ? "ENABLED" : "DISABLED")}");
            Console.WriteLine($"  4. Detailed Stats: {(monitor.ShowDetailedStats ? "ENABLED" : "DISABLED")}");
            Console.WriteLine($"  5. Top Rigs to Show: {monitor.TopRigsToShow}");
            Console.WriteLine($"  0. Back to main menu\n");

            Console.Write("→ Select setting to change: ");
            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    Console.Write($"\nEnter scan interval in minutes (current: {monitor.ScanIntervalMinutes}): ");
                    if (int.TryParse(Console.ReadLine(), out int interval) && interval >= 1 && interval <= 60)
                    {
                        monitor.ScanIntervalMinutes = interval;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✓ Scan interval set to {interval} minutes");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("⚠ Invalid value. Must be between 1 and 60 minutes.");
                        Console.ResetColor();
                    }
                    break;

                case "2":
                    Console.Write($"\nEnter alert threshold % (current: {monitor.AlertThresholdPercent}): ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal threshold) && threshold >= 0 && threshold <= 100)
                    {
                        monitor.AlertThresholdPercent = threshold;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✓ Alert threshold set to {threshold}%");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("⚠ Invalid value. Must be between 0 and 100.");
                        Console.ResetColor();
                    }
                    break;

                case "3":
                    monitor.EnableAlerts = !monitor.EnableAlerts;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Alerts {(monitor.EnableAlerts ? "ENABLED" : "DISABLED")}");
                    Console.ResetColor();
                    break;

                case "4":
                    monitor.ShowDetailedStats = !monitor.ShowDetailedStats;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Detailed stats {(monitor.ShowDetailedStats ? "ENABLED" : "DISABLED")}");
                    Console.ResetColor();
                    break;

                case "5":
                    Console.Write($"\nEnter number of top rigs to show (current: {monitor.TopRigsToShow}): ");
                    if (int.TryParse(Console.ReadLine(), out int topRigs) && topRigs >= 1 && topRigs <= 20)
                    {
                        monitor.TopRigsToShow = topRigs;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✓ Top rigs set to {topRigs}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("⚠ Invalid value. Must be between 1 and 20.");
                        Console.ResetColor();
                    }
                    break;

                case "0":
                    Console.WriteLine("\nReturning to main menu...");
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n⚠ Invalid choice.");
                    Console.ResetColor();
                    break;
            }

            Console.WriteLine();
        }

        private static async Task ExportSessionResults(EquihashMonitor monitor)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("║              EXPORT SESSION RESULTS                       ║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            var defaultFileName = $"equihash_session_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            Console.WriteLine($"Default filename: {defaultFileName}");
            Console.Write("Press Enter to use default, or enter custom filename: ");

            var input = Console.ReadLine()?.Trim();
            var fileName = string.IsNullOrEmpty(input) ? defaultFileName : input;

            if (!fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".txt";
            }

            monitor.ExportResults(fileName);
        }

        private static void ShowHelp()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║                      HELP GUIDE                           ║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("ABOUT:");
            Console.WriteLine("  Equihash Arbitrage Monitor scans MRR and NiceHash for");
            Console.WriteLine("  profitable Equihash rental opportunities on Mining-Dutch.\n");

            Console.WriteLine("MODES:");
            Console.WriteLine("  • Single Scan - One-time scan of current opportunities");
            Console.WriteLine("  • Continuous Monitor - Automatic scanning with alerts");
            Console.WriteLine("  • Quick Summary - Fast overview of market status\n");

            Console.WriteLine("PROFITABILITY THRESHOLDS:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  💰 Green  (≥15%) - Excellent opportunities");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  💵 Yellow (≥10%) - Good opportunities");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  💲 White  (≥5%)  - Acceptable opportunities");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ⚖️  Gray   (>0%)  - Marginal opportunities");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ❌ Red    (<0%)  - Unprofitable\n");
            Console.ResetColor();

            Console.WriteLine("FEATURES:");
            Console.WriteLine("  ✓ Real-time MRR rig scanning (top 5 cheapest)");
            Console.WriteLine("  ✓ NiceHash profitability analysis (4 hashrate tiers)");
            Console.WriteLine("  ✓ MRR rental duration constraints (MinHours/MaxHours)");
            Console.WriteLine("  ✓ Earnings projections (daily/weekly/monthly)");
            Console.WriteLine("  ✓ Audio + visual alerts for high-profit opportunities");
            Console.WriteLine("  ✓ Session statistics tracking");
            Console.WriteLine("  ✓ Export results to file\n");

            Console.WriteLine("TIPS:");
            Console.WriteLine("  • Use Mining-Dutch 'Actual24h' metric (most reliable)");
            Console.WriteLine("  • Set alert threshold to 10%+ for serious opportunities");
            Console.WriteLine("  • Monitor during peak mining hours (8am-6pm UTC)");
            Console.WriteLine("  • Check MRR rig ratings (4.5+ stars recommended)");
            Console.WriteLine("  • Account for MRR 3% rental fees in calculations\n");

            Console.WriteLine("REQUIRED CONFIGURATION:");
            Console.WriteLine("  • settings.json - NiceHash API credentials");
            Console.WriteLine("  • settings.json - MRR API credentials (for MRR arbitrage)");
            Console.WriteLine("  • bot.json - Mining-Dutch BTC address\n");

            Console.WriteLine("═══════════════════════════════════════════════════════════");
        }
    }
}
