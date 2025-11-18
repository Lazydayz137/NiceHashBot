using System;
using System.Windows.Forms;
using NHB3.Core.Services;
using NHB3.MiningDutch;
using NHB3.MiningRigRentals;
using NHB3.NiceHash;
using NHB3.Profitability;

namespace NHB3
{
    /// <summary>
    /// Extension to Home form for arbitrage dashboard integration
    /// Add this method to Home.cs or call from a menu item
    /// </summary>
    public partial class Home : Form
    {
        private ArbitrageDashboard _arbitrageDashboard;

        /// <summary>
        /// Launch the arbitrage opportunity dashboard
        /// Call this from a menu item or button
        /// </summary>
        public void LaunchArbitrageDashboard()
        {
            try
            {
                // Check if already open
                if (_arbitrageDashboard != null && !_arbitrageDashboard.IsDisposed)
                {
                    _arbitrageDashboard.Focus();
                    return;
                }

                // Initialize clients (using existing API settings)
                var apiSettings = ac.readSettings();

                if (string.IsNullOrEmpty(apiSettings.OrganizationID))
                {
                    MessageBox.Show(
                        "Please configure NiceHash API credentials first (File → API Settings)",
                        "API Not Configured",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Create NiceHash client
                var baseUrl = apiSettings.Enviorment == 1
                    ? "https://api2.nicehash.com"
                    : "https://api-test.nicehash.com";

                var nhClient = new NiceHashClient(
                    baseUrl,
                    apiSettings.OrganizationID,
                    apiSettings.ApiID,
                    apiSettings.ApiSecret
                );

                var nhService = new NiceHashService(nhClient);

                // Create Mining-Dutch client
                var mdClient = new MiningDutchClient();

                // Create MRR client if configured
                MrrService mrrService = null;
                if (!string.IsNullOrEmpty(apiSettings.MrrApiKey))
                {
                    var mrrClient = new MrrClient(apiSettings.MrrApiKey, apiSettings.MrrApiSecret);
                    mrrService = new MrrService(mrrClient);
                }

                // Create arbitrage calculator
                var arbCalculator = new RealArbitrageCalculator(nhService, mdClient, mrrService);

                // Create auto-executor and tracker
                var botConfig = ConfigManager.Instance.LoadBotConfig();
                var autoExecutor = new AutoExecutionEngine(nhService, mrrService, botConfig);
                var tracker = new ArbitrageTracker();

                // Launch dashboard
                _arbitrageDashboard = new ArbitrageDashboard(arbCalculator, autoExecutor, tracker);
                _arbitrageDashboard.Show();

                Logger.Instance.Info("Arbitrage dashboard launched");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to launch arbitrage dashboard");
                MessageBox.Show(
                    $"Failed to launch arbitrage dashboard:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
