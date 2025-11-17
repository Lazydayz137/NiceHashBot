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
    /// Unified console for monitoring all algorithms
    /// Main entry point for multi-algorithm arbitrage monitoring
    /// </summary>
    public class UnifiedArbitrageConsole
    {
        public static async Task RunAsync()
        {
            try
            {
                Console.Clear();
                PrintBanner();

                // Load configuration
                var config = ConfigManager.LoadConfig();

                // Initialize services
                Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("│ Initializing Services...                               │");
                Console.ResetColor();
                Console.WriteLine("└─────────────────────────────────────────────────────────┘\n");

                var (nhService, mdClient, mrrService) = await InitializeServicesAsync(config);

                if (nhService == null || mdClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("FATAL: Required services not initialized.");
                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                    return;
                }

                // Create calculator and monitors
                var calculator = new RealArbitrageCalculator(nhService, mrrService, mdClient);
                var multiMonitor = new MultiAlgorithmMonitor(calculator, mrrService, mdClient, nhService);
                var equihashMonitor = new EquihashMonitor(calculator, mrrService, mdClient, nhService);

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
                            await multiMonitor.ScanAllAlgorithmsAsync(cts.Token);
                            break;

                        case "2":
                            await multiMonitor.MonitorContinuouslyAsync(5, cts.Token);
                            break;

                        case "3":
                            await RunEquihashMonitor(equihashMonitor, cts.Token);
                            break;

                        case "4":
                            await RunAlgorithmSelection(calculator, mrrService, mdClient, nhService, cts.Token);
                            break;

                        case "5":
                            ShowAlgorithmGuide();
                            break;

                        case "6":
                            ShowComprehensiveHelp();
                            break;

                        case "q":
                        case "quit":
                        case "exit":
                            running = false;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Thank you for using Unified Arbitrage Monitor! 💎⚡");
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

        private static async Task<(NiceHashService, MiningDutchClient, MrrService)> InitializeServicesAsync(ConfigManager config)
        {
            NiceHashService nhService = null;
            MiningDutchClient mdClient = null;
            MrrService mrrService = null;

            try
            {
                var nhClient = new NiceHashClient(config.OrganizationID, config.ApiID, config.ApiSecret);
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
                Console.WriteLine("⚠ MRR not configured - MRR arbitrage unavailable");
                Console.ResetColor();
            }

            Console.WriteLine();
            return (nhService, mdClient, mrrService);
        }

        private static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║    ⚡ UNIFIED ARBITRAGE MONITOR v2.0 ⚡                  ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║    💎 Equihash  |  ₿ SHA-256  |  🐕 Scrypt              ║");
            Console.WriteLine("║    💳 X11       |  🦅 KawPow   | + More                  ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║    Real-time multi-algorithm profitability monitoring    ║");
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
            Console.WriteLine("║  1 - Scan All Algorithms   Quick scan of 5 top algorithms║");
            Console.WriteLine("║                             Find best opportunities      ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  2 - Continuous Monitor     Auto-scan all algorithms     ║");
            Console.WriteLine("║                             (5 minute intervals)          ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  3 - Equihash Monitor       Specialized Equihash tracker ║");
            Console.WriteLine("║                             (Your active contracts)       ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  4 - Single Algorithm       Monitor specific algorithm   ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  5 - Algorithm Guide        Best practices per algorithm ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  6 - Help                   Comprehensive help guide     ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  Q - Quit                   Exit monitor                 ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.Write("\n→ Your choice: ");
        }

        private static async Task RunEquihashMonitor(EquihashMonitor monitor, CancellationToken cancellationToken)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║            EQUIHASH SPECIALIZED MONITOR                   ║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("Select mode:");
            Console.WriteLine("  1 - Single scan");
            Console.WriteLine("  2 - Continuous monitoring (2 min intervals)");
            Console.WriteLine("  3 - Quick summary");
            Console.Write("\n→ Choice: ");

            var choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await monitor.ScanEquihashOpportunitiesAsync(cancellationToken);
                    break;
                case "2":
                    await monitor.MonitorEquihashAsync(cancellationToken);
                    break;
                case "3":
                    var summary = await monitor.GetEquihashSummaryAsync(cancellationToken);
                    Console.WriteLine(summary);
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }

        private static async Task RunAlgorithmSelection(
            RealArbitrageCalculator calculator,
            MrrService mrrService,
            MiningDutchClient mdClient,
            NiceHashService nhService,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║            SELECT ALGORITHM TO MONITOR                    ║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("Available algorithms:");
            Console.WriteLine("  1 - 💎 Equihash (Zcash, Bitcoin Gold)");
            Console.WriteLine("  2 - ₿  SHA-256 (Bitcoin)");
            Console.WriteLine("  3 - 🐕 Scrypt (Litecoin, Dogecoin)");
            Console.WriteLine("  4 - 💳 X11 (Dash)");
            Console.WriteLine("  5 - 🦅 KawPow (Ravencoin)");
            Console.Write("\n→ Select algorithm (1-5): ");

            var choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            string algoName = choice switch
            {
                "1" => "Equihash",
                "2" => "SHA256",
                "3" => "Scrypt",
                "4" => "X11",
                "5" => "KawPow",
                _ => null
            };

            if (algoName == null)
            {
                Console.WriteLine("Invalid choice.");
                return;
            }

            // For now, just create an Equihash monitor as example
            // In production, you'd create algorithm-specific monitors
            var monitor = new EquihashMonitor(calculator, mrrService, mdClient, nhService);
            await monitor.ScanEquihashOpportunitiesAsync(cancellationToken);
        }

        private static void ShowAlgorithmGuide()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║                   ALGORITHM GUIDE                         ║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("💎 EQUIHASH (Zcash, Bitcoin Gold)");
            Console.ResetColor();
            Console.WriteLine("   Hardware: GPU (NVIDIA GTX 1070+, AMD RX 580+)");
            Console.WriteLine("   Profitability: HIGH - Great arbitrage potential");
            Console.WriteLine("   Best for: GPU owners, moderate investment");
            Console.WriteLine("   Rental duration: 3-168 hours typical");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("₿  SHA-256 (Bitcoin)");
            Console.ResetColor();
            Console.WriteLine("   Hardware: ASIC (Antminer S19, Whatsminer M30S+)");
            Console.WriteLine("   Profitability: STABLE - Most liquid market");
            Console.WriteLine("   Best for: Large investors, long-term rentals");
            Console.WriteLine("   Rental duration: 24+ hours recommended");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🐕 SCRYPT (Litecoin, Dogecoin)");
            Console.ResetColor();
            Console.WriteLine("   Hardware: ASIC (Antminer L7, Goldshell LT6)");
            Console.WriteLine("   Profitability: STABLE - Lower competition");
            Console.WriteLine("   Best for: Medium investors, steady returns");
            Console.WriteLine("   Rental duration: 12-48 hours optimal");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("💳 X11 (Dash)");
            Console.ResetColor();
            Console.WriteLine("   Hardware: ASIC (Antminer D7, iBeLink DM22G)");
            Console.WriteLine("   Profitability: MODERATE - Less saturated");
            Console.WriteLine("   Best for: Diversification, lower entry cost");
            Console.WriteLine("   Rental duration: 6-24 hours typical");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🦅 KAWPOW (Ravencoin)");
            Console.ResetColor();
            Console.WriteLine("   Hardware: GPU (NVIDIA RTX 3060+, AMD RX 6700+)");
            Console.WriteLine("   Profitability: VARIABLE - Growing market");
            Console.WriteLine("   Best for: GPU enthusiasts, short-term plays");
            Console.WriteLine("   Rental duration: 1-12 hours recommended");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ GENERAL TIPS:");
            Console.ResetColor();
            Console.WriteLine("  • Start with algorithms you understand");
            Console.WriteLine("  • Check MRR MinHours before renting");
            Console.WriteLine("  • Monitor Mining-Dutch 'Actual24h' (most reliable)");
            Console.WriteLine("  • Consider network hashrate trends");
            Console.WriteLine("  • Factor in MRR 3% rental fees");
            Console.WriteLine("  • Look for margins ≥10% for safer profits");
            Console.WriteLine();
        }

        private static void ShowComprehensiveHelp()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║                  COMPREHENSIVE HELP                       ║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("ABOUT:");
            Console.WriteLine("  Unified Arbitrage Monitor scans 5 top algorithms");
            Console.WriteLine("  simultaneously to find the most profitable rental");
            Console.WriteLine("  opportunities across MRR and NiceHash.\n");

            Console.WriteLine("MONITORED ALGORITHMS:");
            Console.WriteLine("  1. Equihash - GPU mining (Zcash, Bitcoin Gold)");
            Console.WriteLine("  2. SHA-256 - Bitcoin mining (most liquid)");
            Console.WriteLine("  3. Scrypt - Litecoin/Dogecoin (stable)");
            Console.WriteLine("  4. X11 - Dash mining (lower competition)");
            Console.WriteLine("  5. KawPow - Ravencoin (growing market)\n");

            Console.WriteLine("KEY FEATURES:");
            Console.WriteLine("  ✓ Multi-algorithm scanning");
            Console.WriteLine("  ✓ Predictive profitability metrics");
            Console.WriteLine("  ✓ Network hashrate trend analysis");
            Console.WriteLine("  ✓ Block time & reward forecasting");
            Console.WriteLine("  ✓ Risk assessment (Low/Medium/High)");
            Console.WriteLine("  ✓ Confidence scoring");
            Console.WriteLine("  ✓ MRR duration constraint handling");
            Console.WriteLine("  ✓ Earnings projections (1h-24h)");
            Console.WriteLine("  ✓ Volatility analysis");
            Console.WriteLine("  ✓ Automated alerts\n");

            Console.WriteLine("PREDICTIVE METRICS:");
            Console.WriteLine("  • Profitability Trend - 24h percentage change");
            Console.WriteLine("  • Volatility Rating - Low/Medium/High");
            Console.WriteLine("  • Confidence Score - Data reliability (0-100%)");
            Console.WriteLine("  • Risk Level - Overall risk assessment");
            Console.WriteLine("  • Recommended Action - Buy/Hold/Avoid");
            Console.WriteLine("  • Forecasts - 1h, 3h, 6h, 12h, 24h predictions\n");

            Console.WriteLine("PROFITABILITY THRESHOLDS:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  💰 ≥15% - Excellent (act immediately)");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  💵 ≥10% - Good (strong opportunity)");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  💲 ≥5%  - Acceptable (worth considering)");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ⚖️  >0%  - Marginal (risky)");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ❌ <0%  - Unprofitable (avoid)\n");
            Console.ResetColor();

            Console.WriteLine("USAGE RECOMMENDATIONS:");
            Console.WriteLine("  • Scan all algorithms first (Option 1)");
            Console.WriteLine("  • Review top recommendation");
            Console.WriteLine("  • Use specialized monitor for deep-dive");
            Console.WriteLine("  • Check predictive metrics before renting");
            Console.WriteLine("  • Avoid 'High Risk' opportunities");
            Console.WriteLine("  • Trust 'Actual24h' over current estimates");
            Console.WriteLine("  • Factor in rental duration constraints\n");

            Console.WriteLine("CONFIGURATION:");
            Console.WriteLine("  • settings.json - API credentials (NH, MRR)");
            Console.WriteLine("  • bot.json - Mining-Dutch BTC address");
            Console.WriteLine("  • Scan intervals configurable per algorithm");
            Console.WriteLine("  • Alert thresholds adjustable\n");

            Console.WriteLine("═══════════════════════════════════════════════════════════");
        }
    }
}
