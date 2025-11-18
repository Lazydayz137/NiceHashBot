using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHB3.MiningDutch;
using NHB3.MiningRigRentals;
using NHB3.NiceHash;

namespace NHB3.Core.Services
{
    /// <summary>
    /// System health check and validation
    /// Validates configuration and API connectivity before starting operations
    /// </summary>
    public class SystemHealthCheck
    {
        public class HealthCheckResult
        {
            public bool IsHealthy { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();
            public Dictionary<string, bool> ComponentStatus { get; set; } = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Perform comprehensive system health check
        /// </summary>
        public static async Task<HealthCheckResult> PerformHealthCheckAsync(
            NiceHashClient nhClient = null,
            MrrClient mrrClient = null,
            MiningDutchClient mdClient = null,
            CancellationToken cancellationToken = default)
        {
            var result = new HealthCheckResult { IsHealthy = true };

            Logger.Instance.Info("Starting system health check...");

            // 1. Check configuration files
            CheckConfigurationFiles(result);

            // 2. Check bot configuration
            CheckBotConfiguration(result);

            // 3. Check API credentials
            CheckApiCredentials(result);

            // 4. Test API connections
            await TestApiConnections(result, nhClient, mrrClient, mdClient, cancellationToken);

            // 5. Check write permissions
            CheckWritePermissions(result);

            // 6. Check required directories
            CheckRequiredDirectories(result);

            // Final health status
            result.IsHealthy = result.Errors.Count == 0;

            if (result.IsHealthy)
            {
                Logger.Instance.Info("✅ System health check PASSED");
            }
            else
            {
                Logger.Instance.Warning($"⚠️ System health check FAILED with {result.Errors.Count} errors and {result.Warnings.Count} warnings");
            }

            return result;
        }

        private static void CheckConfigurationFiles(HealthCheckResult result)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Check settings.json
            var settingsPath = Path.Combine(baseDir, "settings.json");
            if (!File.Exists(settingsPath))
            {
                result.Errors.Add("settings.json not found. Copy settings.json.template to settings.json and configure your API credentials.");
                result.ComponentStatus["Configuration:Settings"] = false;
            }
            else
            {
                result.ComponentStatus["Configuration:Settings"] = true;
            }

            // Check bot.json (optional, will use defaults)
            var botConfigPath = Path.Combine(baseDir, "bot.json");
            if (!File.Exists(botConfigPath))
            {
                result.Warnings.Add("bot.json not found. Using default configuration. Copy bot.json.template to bot.json to customize settings.");
                result.ComponentStatus["Configuration:Bot"] = false;
            }
            else
            {
                result.ComponentStatus["Configuration:Bot"] = true;
            }
        }

        private static void CheckBotConfiguration(HealthCheckResult result)
        {
            try
            {
                var config = ConfigManager.Instance.LoadBotConfig();

                // Check critical settings
                if (config.RiskManagement.MaxDailySpend <= 0)
                {
                    result.Warnings.Add("MaxDailySpend is 0. Risk management disabled.");
                }

                if (config.RiskManagement.MaxOrderSize <= 0)
                {
                    result.Warnings.Add("MaxOrderSize is 0. Orders will fail validation.");
                }

                if (string.IsNullOrEmpty(config.MiningDutch.BTCAddress))
                {
                    result.Errors.Add("Mining-Dutch BTC address not configured in bot.json. Auto-execution will fail.");
                    result.ComponentStatus["Configuration:MiningDutch"] = false;
                }
                else
                {
                    result.ComponentStatus["Configuration:MiningDutch"] = true;
                }

                result.ComponentStatus["Configuration:RiskManagement"] = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to load bot configuration: {ex.Message}");
                result.ComponentStatus["Configuration:Bot"] = false;
            }
        }

        private static void CheckApiCredentials(HealthCheckResult result)
        {
            try
            {
                var settings = ConfigManager.Instance.LoadApiSettings();

                // NiceHash credentials
                if (string.IsNullOrEmpty(settings.OrganizationID))
                {
                    result.Errors.Add("NiceHash OrganizationID not configured");
                    result.ComponentStatus["Credentials:NiceHash"] = false;
                }
                else if (string.IsNullOrEmpty(settings.ApiID) || string.IsNullOrEmpty(settings.ApiSecret))
                {
                    result.Errors.Add("NiceHash API credentials incomplete");
                    result.ComponentStatus["Credentials:NiceHash"] = false;
                }
                else
                {
                    result.ComponentStatus["Credentials:NiceHash"] = true;
                }

                // MRR credentials (optional)
                if (string.IsNullOrEmpty(settings.MrrApiKey) || string.IsNullOrEmpty(settings.MrrApiSecret))
                {
                    result.Warnings.Add("MRR API credentials not configured. MRR arbitrage paths will be unavailable.");
                    result.ComponentStatus["Credentials:MRR"] = false;
                }
                else
                {
                    result.ComponentStatus["Credentials:MRR"] = true;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to load API credentials: {ex.Message}");
            }
        }

        private static async Task TestApiConnections(
            HealthCheckResult result,
            NiceHashClient nhClient,
            MrrClient mrrClient,
            MiningDutchClient mdClient,
            CancellationToken cancellationToken)
        {
            // Test NiceHash connection
            if (nhClient != null)
            {
                try
                {
                    var connected = await nhClient.TestConnectionAsync(cancellationToken);
                    result.ComponentStatus["API:NiceHash"] = connected;
                    if (!connected)
                    {
                        result.Errors.Add("NiceHash API connection test failed");
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"NiceHash API test error: {ex.Message}");
                    result.ComponentStatus["API:NiceHash"] = false;
                }
            }

            // Test MRR connection
            if (mrrClient != null)
            {
                try
                {
                    var connected = await mrrClient.TestConnectionAsync(cancellationToken);
                    result.ComponentStatus["API:MRR"] = connected;
                    if (!connected)
                    {
                        result.Warnings.Add("MRR API connection test failed. MRR features unavailable.");
                    }
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"MRR API test error: {ex.Message}");
                    result.ComponentStatus["API:MRR"] = false;
                }
            }

            // Test Mining-Dutch connection
            if (mdClient != null)
            {
                try
                {
                    var status = await mdClient.GetStatusAsync(cancellationToken);
                    result.ComponentStatus["API:MiningDutch"] = status != null && status.Count > 0;
                    if (status == null || status.Count == 0)
                    {
                        result.Errors.Add("Mining-Dutch API connection test failed");
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Mining-Dutch API test error: {ex.Message}");
                    result.ComponentStatus["API:MiningDutch"] = false;
                }
            }
        }

        private static void CheckWritePermissions(HealthCheckResult result)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                // Test write to base directory
                var testFile = Path.Combine(baseDir, ".write_test");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                result.ComponentStatus["Permissions:Write"] = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"No write permission to application directory: {ex.Message}");
                result.ComponentStatus["Permissions:Write"] = false;
            }
        }

        private static void CheckRequiredDirectories(HealthCheckResult result)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var logsDir = Path.Combine(baseDir, "logs");

            try
            {
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                    Logger.Instance.Info("Created logs directory");
                }
                result.ComponentStatus["Directories:Logs"] = true;
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Failed to create logs directory: {ex.Message}");
                result.ComponentStatus["Directories:Logs"] = false;
            }
        }

        /// <summary>
        /// Print health check results to console
        /// </summary>
        public static void PrintHealthCheckReport(HealthCheckResult result)
        {
            Console.WriteLine("\n═══════════════════════════════════════════════════════");
            Console.WriteLine("          SYSTEM HEALTH CHECK REPORT");
            Console.WriteLine("═══════════════════════════════════════════════════════\n");

            // Overall status
            if (result.IsHealthy)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Overall Status: HEALTHY\n");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Overall Status: UNHEALTHY ({result.Errors.Count} errors)\n");
            }
            Console.ResetColor();

            // Component status
            Console.WriteLine("Component Status:");
            Console.WriteLine("─────────────────────────────────────────────────────");
            foreach (var component in result.ComponentStatus.OrderBy(c => c.Key))
            {
                var status = component.Value ? "✅ OK" : "❌ FAIL";
                var color = component.Value ? ConsoleColor.Green : ConsoleColor.Red;
                Console.ForegroundColor = color;
                Console.WriteLine($"  {component.Key.PadRight(35)} {status}");
                Console.ResetColor();
            }
            Console.WriteLine();

            // Errors
            if (result.Errors.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Errors ({result.Errors.Count}):");
                Console.WriteLine("─────────────────────────────────────────────────────");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"  ❌ {error}");
                }
                Console.WriteLine();
                Console.ResetColor();
            }

            // Warnings
            if (result.Warnings.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warnings ({result.Warnings.Count}):");
                Console.WriteLine("─────────────────────────────────────────────────────");
                foreach (var warning in result.Warnings)
                {
                    Console.WriteLine($"  ⚠️  {warning}");
                }
                Console.WriteLine();
                Console.ResetColor();
            }

            Console.WriteLine("═══════════════════════════════════════════════════════\n");
        }
    }
}
