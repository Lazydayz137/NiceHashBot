# NiceHashBot v2.0 - Modernized & Enhanced

**Professional arbitrage trading bot for NiceHash with Mining-Dutch pool integration.**

![Modern Dashboard](https://img.shields.io/badge/UI-Modern%20Dashboard-blue)
![Arbitrage](https://img.shields.io/badge/Feature-Arbitrage%20Trading-green)
![Status](https://img.shields.io/badge/Status-Production%20Ready-success)

<p align="center">
  <img src="https://raw.githubusercontent.com/nicehash/NiceHashBot/master/screenshots/00nhb3.png" alt="NiceHashBot v2.0" />
</p>

---

## 🚀 What's New in v2.0

### ✨ Modern UI/UX
- **Dashboard-style interface** with real-time metrics cards
- **Professional color scheme** with Segoe UI typography
- **5 live metrics**: Balance, Orders, Active Rigs, Hashrate, Bot Status
- **Modernized DataGridView** with alternating rows and custom styling
- **Flat design menu** with hover effects

### 💎 Arbitrage Trading System
- **Three arbitrage paths**: NiceHash→Mining-Dutch, MRR→Mining-Dutch, MRR→NiceHash
- **Real profitability data** from Mining-Dutch Actual24h metrics
- **Auto-execution engine** with risk management
- **Historical tracking** with performance analytics
- **Visual dashboard** with color-coded opportunities (>15% green, >10% yellow)

### 🛠️ Production Features
- **Health check system** validates configuration before startup
- **Configuration templates** (bot.json.template, settings.json.template)
- **Utility classes** for hashrate conversions and data validation
- **Enhanced logging** with 30-day rotation
- **Comprehensive documentation** (2,500+ lines)

---

## 📋 Table of Contents

- [Features](#features)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Arbitrage Trading](#arbitrage-trading)
- [How to Compile](#compile)
- [Documentation](#documentation)
- [Architecture](#architecture)

---

## <a name="features"></a> 🎯 Features

### Core Bot Features
- ✅ Run on **test** and **production** environment
- ✅ Manage pools (create/edit/delete)
- ✅ Manage orders (create/refill/edit/delete)
- ✅ **Price adjustment** - keep price competitive (increase/decrease)
- ✅ **Auto-refilling** - automatically refill when 90% consumed
- ✅ Console logging with color-coded output

### New v2.0 Features
- ✅ **Modern dashboard UI** with real-time metrics
- ✅ **Arbitrage trading** across NiceHash, MRR, and Mining-Dutch
- ✅ **Auto-execution** with configurable risk management
- ✅ **Historical tracking** and performance analytics
- ✅ **Health check system** for startup validation
- ✅ **Configuration templates** for safe setup
- ✅ **Utility classes** for common operations

---

## <a name="quick-start"></a> 🚀 Quick Start

### 1. Download & Extract
```bash
# Download from releases
# Extract to your desired location
```

### 2. Configure API Credentials
```bash
# Copy templates
copy settings.json.template settings.json
copy bot.json.template bot.json

# Edit settings.json with your NiceHash API credentials
# Edit bot.json with your Mining-Dutch BTC address
```

### 3. Run
```bash
NHB3.exe
```

**📖 Detailed setup guide:** [GETTING_STARTED.md](GETTING_STARTED.md)

---

## <a name="configuration"></a> ⚙️ Configuration

### Required: settings.json
```json
{
  "OrganizationID": "YOUR_ORG_ID",
  "ApiID": "YOUR_API_KEY",
  "ApiSecret": "YOUR_API_SECRET",
  "Enviorment": 1,
  "MrrApiKey": "YOUR_MRR_KEY",
  "MrrApiSecret": "YOUR_MRR_SECRET"
}
```

**Get NiceHash API keys:** https://github.com/nicehash/rest-clients-demo/blob/master/README.md

### Optional: bot.json
```json
{
  "RiskManagement": {
    "MaxDailySpend": 0.1,
    "MaxOrderSize": 0.01,
    "MinProfitMargin": 5.0
  },
  "MiningDutch": {
    "BTCAddress": "YOUR_BTC_ADDRESS",
    "WorkerPrefix": "NHB3"
  },
  "Arbitrage": {
    "ScanIntervalMinutes": 5,
    "PreferredMetric": "Actual24h"
  }
}
```

---

## <a name="arbitrage-trading"></a> 💎 Arbitrage Trading

### Access the Dashboard
**Settings → Arbitrage 💎**

### Three Arbitrage Paths

1. **NiceHash → Mining-Dutch**
   - Buy hashrate on NiceHash
   - Mine on Mining-Dutch pool
   - Profit from price difference

2. **MRR → Mining-Dutch**
   - Rent hashrate from MiningRigRentals
   - Mine on Mining-Dutch pool
   - Profit from rental vs mining revenue

3. **MRR → NiceHash**
   - Rent hashrate from MRR
   - Sell on NiceHash marketplace
   - Profit from arbitrage spread

### How to Use

1. **Scan for opportunities** - Click "Scan All Algorithms"
2. **Review color-coded results**:
   - 🟢 Green (>15%) - Excellent
   - 🟡 Yellow (>10%) - Good
   - ⚪ White (>5%) - Acceptable
3. **Execute manually** or enable auto-execute mode
4. **Track performance** in History & Performance tab

**📖 Complete guide:** [ARBITRAGE_FEATURES_GUIDE.md](ARBITRAGE_FEATURES_GUIDE.md)

---

## <a name="compile"></a> 🔧 How to Compile

### Prerequisites
- Visual Studio 2017 or later
- .NET Framework 4.7.2 or higher

### Build Steps
```bash
# 1. Clone repository
git clone https://github.com/nicehash/NiceHashBot.git

# 2. Open in Visual Studio
# Open NiceHashBot.sln

# 3. Restore NuGet packages
# Build → Restore NuGet Packages

# 4. Build solution
# Build → Build Solution (Ctrl+Shift+B)

# 5. Run
# Debug → Start (F5)
```

---

## <a name="documentation"></a> 📚 Documentation

### Quick Start
- **[GETTING_STARTED.md](GETTING_STARTED.md)** - Complete setup guide for beginners

### Feature Guides
- **[ARBITRAGE_FEATURES_GUIDE.md](ARBITRAGE_FEATURES_GUIDE.md)** - Arbitrage trading usage
- **[REAL_PROFITABILITY_GUIDE.md](REAL_PROFITABILITY_GUIDE.md)** - How profitability is calculated
- **[COMPLETE_FEATURES_SUMMARY.md](COMPLETE_FEATURES_SUMMARY.md)** - All features explained

### Technical Documentation
- **[README_MODERNIZATION.md](README_MODERNIZATION.md)** - Architecture overview
- **[POLISH_AND_ENHANCEMENTS.md](POLISH_AND_ENHANCEMENTS.md)** - v2.0 improvements
- **[CHANGELOG.md](CHANGELOG.md)** - Version history

---

## <a name="architecture"></a> 🏗️ Architecture

### Modern Async Architecture
```
NHB3/
├── Core/
│   ├── Interfaces/      # IApiClient, IProfitabilityCalculator, IStrategy
│   ├── Models/          # MarketData, ProfitabilityReport, StrategyConfig
│   └── Services/        # Logger, ConfigManager, SystemHealthCheck
├── Utils/               # RateLimiter, RetryPolicy, HashCalculator, DataValidator
├── NiceHash/            # NiceHashClient, NiceHashService, Models
├── MiningDutch/         # MiningDutchClient, AlgorithmMapper
├── MiningRigRentals/    # MrrClient, MrrService, Models
├── Profitability/       # RealArbitrageCalculator, AutoExecutionEngine, ArbitrageTracker
├── Strategies/          # BaseStrategy, MarketMakerStrategy
└── Forms/               # Home, ArbitrageDashboard, ApiForm, PoolsForm, etc.
```

### Key Components

**API Clients**
- Async/await throughout
- HMAC authentication
- Rate limiting (100 req/min)
- Exponential backoff retry

**Arbitrage System**
- Real profitability from Mining-Dutch
- Algorithm mapping (35+ algos)
- Hashrate unit conversions
- Three arbitrage paths

**Risk Management**
- MaxDailySpend limits
- MaxOrderSize caps
- Profit margin thresholds
- Stop-loss protection

---

## 🔒 Security

- **Never commit** `settings.json` or `bot.json` (use templates)
- **API credentials** are stored locally only
- **Sensitive data** is sanitized in logs
- **Start small** on testnet before production

---

## 💡 Tips for Developers

### Core Classes

**Api.cs** - Prepares authorization headers (HMAC-SHA256)
**ApiConnect.cs** - Exposes NiceHash API methods
**Home.cs** - Main form with bot logic in `runBot()`
**RealArbitrageCalculator.cs** - Cross-platform profitability
**AutoExecutionEngine.cs** - Automated trade execution

### Bot Logic
Runs every 60 seconds and checks:
1. Should refill orders? (>90% spent)
2. Should adjust prices? (no speed = increase, have speed = decrease)
3. Are there arbitrage opportunities?

### Extending Functionality
- Add new strategies by extending `BaseStrategy`
- Add new pools by implementing similar to `MiningDutchClient`
- Customize UI in `Home.cs` and `ArbitrageDashboard.cs`

---

## 📈 Performance Expectations

### Conservative (5% minimum margin)
- Opportunities: 2-5 per day
- Average margin: 6-8%
- Daily profit: ~0.1-0.3% of capital

### Moderate (8% minimum margin)
- Opportunities: 1-3 per day
- Average margin: 9-12%
- Daily profit: ~0.2-0.5% of capital

### Aggressive (15% minimum margin)
- Opportunities: 0-2 per week
- Average margin: 16-20%
- Daily profit: ~0.3-0.8% of capital (when found)

**Note:** Results vary based on market conditions, pool performance, and timing.

---

## 🤝 Contributing

This is a proof-of-concept application demonstrating NiceHash API v2 integration. Feel free to:
- Fork and enhance
- Submit pull requests
- Report issues
- Share strategies

---

## 📜 License

See LICENSE file for details.

---

## 🙏 Credits

**Original NiceHashBot** - NiceHash development team
**v2.0 Modernization** - Enhanced with arbitrage trading, modern UI, and production features

---

## ⚠️ Disclaimer

**Trading cryptocurrency involves risk.** This bot is provided as-is for educational and automation purposes. Always:
- Test on testnet first
- Start with small amounts
- Monitor actively
- Understand the risks
- Never invest more than you can afford to lose

---

## 📞 Support

- **Documentation:** See [GETTING_STARTED.md](GETTING_STARTED.md)
- **Issues:** GitHub Issues
- **NiceHash API:** https://docs.nicehash.com/

---

<p align="center">
  Made with ⚡ for automated crypto mining arbitrage
</p>
