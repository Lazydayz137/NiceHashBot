using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NHB3
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Supports both GUI mode and Console monitoring modes
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Check for console mode arguments
            if (args.Length > 0)
            {
                RunConsoleMode(args[0]).GetAwaiter().GetResult();
                return;
            }

            // Default: Launch GUI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Home());
        }

        /// <summary>
        /// Run console-based monitoring modes
        /// </summary>
        private static async Task RunConsoleMode(string mode)
        {
            try
            {
                switch (mode.ToLower())
                {
                    case "multi":
                    case "unified":
                        Console.WriteLine("Launching Unified Multi-Algorithm Monitor...\n");
                        await UnifiedArbitrageConsole.RunAsync();
                        break;

                    case "equihash":
                    case "eq":
                        Console.WriteLine("Launching Equihash Specialized Monitor...\n");
                        await EquihashConsole.RunAsync();
                        break;

                    case "help":
                    case "?":
                    case "-h":
                    case "--help":
                        ShowConsoleHelp();
                        break;

                    default:
                        Console.WriteLine($"Unknown mode: {mode}");
                        Console.WriteLine("Run with 'help' argument for usage information.\n");
                        ShowConsoleHelp();
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

        /// <summary>
        /// Show console mode help
        /// </summary>
        private static void ShowConsoleHelp()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║        NICEHASHBOT v2.0 - CONSOLE MODE OPTIONS           ║");
            Console.ResetColor();
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("USAGE:");
            Console.WriteLine("  NHB3.exe [mode]\n");

            Console.WriteLine("AVAILABLE MODES:\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  multi, unified");
            Console.ResetColor();
            Console.WriteLine("    Launch Unified Multi-Algorithm Monitor");
            Console.WriteLine("    Scans 5 algorithms: Equihash, SHA-256, Scrypt, X11, KawPow");
            Console.WriteLine("    Shows ranked profitability with predictive metrics\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  equihash, eq");
            Console.ResetColor();
            Console.WriteLine("    Launch Equihash Specialized Monitor");
            Console.WriteLine("    Deep-dive monitoring for Equihash contracts");
            Console.WriteLine("    Top 5 MRR rigs + NiceHash profitability\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  (no arguments)");
            Console.ResetColor();
            Console.WriteLine("    Launch GUI mode (default)\n");

            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("  NHB3.exe                  → Launch GUI");
            Console.WriteLine("  NHB3.exe multi            → Launch multi-algorithm monitor");
            Console.WriteLine("  NHB3.exe equihash         → Launch Equihash monitor\n");

            Console.WriteLine("REQUIREMENTS:");
            Console.WriteLine("  • settings.json - API credentials (NH, MRR)");
            Console.WriteLine("  • bot.json - Mining-Dutch BTC address");
            Console.WriteLine("  • Internet connection\n");

            Console.WriteLine("═══════════════════════════════════════════════════════════\n");
        }
    }
}
