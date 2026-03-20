using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHB3.Core.Models;

namespace NHB3.Core.Services
{
    /// <summary>
    /// Configuration manager for loading/saving application settings
    /// </summary>
    public class ConfigManager
    {
        private static readonly Lazy<ConfigManager> _instance = new Lazy<ConfigManager>(() => new ConfigManager());
        private readonly string _configDirectory;

        public static ConfigManager Instance => _instance.Value;

        private ConfigManager()
        {
            _configDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Load configuration from file
        /// </summary>
        public T Load<T>(string filename) where T : class, new()
        {
            try
            {
                var filePath = Path.Combine(_configDirectory, filename);
                if (!File.Exists(filePath))
                {
                    Logger.Instance.Warning($"Config file not found: {filename}. Using defaults.");
                    return new T();
                }

                var json = File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<T>(json);
                Logger.Instance.Info($"Loaded configuration from {filename}");
                return config ?? new T();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to load config from {filename}");
                return new T();
            }
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        public bool Save<T>(T config, string filename) where T : class
        {
            try
            {
                var filePath = Path.Combine(_configDirectory, filename);
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(filePath, json);
                Logger.Instance.Info($"Saved configuration to {filename}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to save config to {filename}");
                return false;
            }
        }

        /// <summary>
        /// Load API settings (legacy support)
        /// </summary>
        public ApiSettings LoadApiSettings()
        {
            return Load<ApiSettings>("settings.json");
        }

        /// <summary>
        /// Save API settings (legacy support)
        /// </summary>
        public bool SaveApiSettings(ApiSettings settings)
        {
            return Save(settings, "settings.json");
        }

        /// <summary>
        /// Load enhanced bot configuration
        /// </summary>
        public BotConfiguration LoadBotConfig()
        {
            return Load<BotConfiguration>("bot.json");
        }

        /// <summary>
        /// Save enhanced bot configuration
        /// </summary>
        public bool SaveBotConfig(BotConfiguration config)
        {
            return Save(config, "bot.json");
        }
    }

    /// <summary>
    /// API credentials and settings
    /// </summary>
    public class ApiSettings
    {
        public string OrganizationID { get; set; }
        public string ApiID { get; set; }
        public string ApiSecret { get; set; }
        public int Environment { get; set; } // 0=Test, 1=Production, 99=Dev

        // MiningRigRentals Credentials
        public string MrrApiKey { get; set; }
        public string MrrApiSecret { get; set; }
    }

    /// <summary>
    /// Enhanced bot configuration
    /// </summary>
    public class BotConfiguration
    {
        public string Version { get; set; } = "2.0";

        public GeneralSettings General { get; set; } = new GeneralSettings();
        public NiceHashSettings NiceHash { get; set; } = new NiceHashSettings();
        public MrrSettings MiningRigRentals { get; set; } = new MrrSettings();
        public MiningDutchSettings MiningDutch { get; set; } = new MiningDutchSettings();
        public ArbitrageSettings Arbitrage { get; set; } = new ArbitrageSettings();
        public ProfitabilitySettings Profitability { get; set; } = new ProfitabilitySettings();
        public RiskManagementSettings RiskManagement { get; set; } = new RiskManagementSettings();
        public LoggingSettings Logging { get; set; } = new LoggingSettings();
        public PerformanceSettings Performance { get; set; } = new PerformanceSettings();
        public NotificationSettings Notifications { get; set; } = new NotificationSettings();
        public List<StrategyConfig> Strategies { get; set; } = new List<StrategyConfig>();

        // Legacy settings for backward compatibility
        public bool RefillOrder { get; set; } = true;
        public bool LowerPrice { get; set; } = true;
        public bool IncreasePrice { get; set; } = true;
    }

    public class GeneralSettings
    {
        public int UpdateInterval { get; set; } = 60;
        public bool EnableLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Info";
    }

    public class NiceHashSettings
    {
        public bool AutoRefill { get; set; } = true;
        public decimal RefillThreshold { get; set; } = 0.9m;
        public decimal? MinRefillAmount { get; set; }
        public bool DynamicPricing { get; set; } = true;
        public string PriceAdjustmentStrategy { get; set; } = "marketDepth";
    }

    public class MrrSettings
    {
        public bool Enabled { get; set; } = true;
        public bool AutoRent { get; set; } = false;
        public decimal MaxRentalCost { get; set; } = 0.01m;
        public List<string> PreferredAlgorithms { get; set; } = new List<string>();
        public string RedirectToPool { get; set; } = "mining-dutch";
        public int DefaultDuration { get; set; } = 24;
        public decimal MaxPricePerMH { get; set; } = 0.00001m;
    }

    public class MiningDutchSettings
    {
        public string BTCAddress { get; set; } = "";
        public string WorkerPrefix { get; set; } = "NHB3";
    }

    public class ArbitrageSettings
    {
        public int ScanIntervalMinutes { get; set; } = 5;
        public decimal DefaultHashrateMultiplier { get; set; } = 1000m;
        public int MinimumOpportunities { get; set; } = 1;
        public string PreferredMetric { get; set; } = "Actual24h";
        public List<string> EnabledPaths { get; set; } = new List<string>
        {
            "NiceHash→MiningDutch",
            "MRR→MiningDutch",
            "MRR→NiceHash"
        };
    }

    public class LoggingSettings
    {
        public string LogLevel { get; set; } = "Info";
        public bool LogToFile { get; set; } = true;
        public bool LogToConsole { get; set; } = true;
        public int RetentionDays { get; set; } = 30;
    }

    public class PerformanceSettings
    {
        public int CacheDurationMinutes { get; set; } = 2;
        public int RateLimitPerMinute { get; set; } = 100;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 2;
    }

    public class NotificationSettings
    {
        public bool Enabled { get; set; } = false;
        public string WebhookUrl { get; set; } = "";
        public bool NotifyOnExecution { get; set; } = true;
        public bool NotifyOnHighMargin { get; set; } = true;
        public decimal HighMarginThreshold { get; set; } = 20.0m;
    }

    public class ProfitabilitySettings
    {
        public bool EnableCrossPoolAnalysis { get; set; } = true;
        public bool WhatToMineEnabled { get; set; } = true;
        public int UpdateInterval { get; set; } = 300;
        /// <summary>Minimum profit margin as a percentage (e.g. 5.0 = 5%)</summary>
        public decimal MinProfitMargin { get; set; } = 5.0m;
    }

    public class RiskManagementSettings
    {
        public decimal MaxDailySpend { get; set; } = 0.1m;
        public decimal MaxOrderSize { get; set; } = 0.01m;
        public bool StopLossEnabled { get; set; } = true;
        public decimal StopLossThreshold { get; set; } = 0.2m;
    }
}
