# Changelog

All notable changes to NiceHashBot will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [2.0.0] - 2024-11-16

### 🎨 UI/UX Modernization

#### Added
- **Modern dashboard-style Home form** with card-based metrics
- **5 real-time metric cards**: Balance, Total Orders, Active Rigs, Total Speed, Bot Status
- **Professional color scheme**: Blue primary, green success, orange warning, red danger
- **Segoe UI typography** throughout the application
- **Modern DataGridView styling** with alternating rows and custom headers
- **Flat design menu** with custom renderer and hover effects
- **Color-coded status indicators** in status bar

#### Changed
- Transformed dated Windows Forms UI into modern, professional dashboard
- Enhanced visual feedback for all bot states (Stopped/Idle/Working)
- Improved readability with larger fonts and better spacing
- Added ShowDialog() to all form launches for better UX

### 💎 Arbitrage Trading System

#### Added
- **ArbitrageDashboard** - Full-featured arbitrage trading interface
- **Three arbitrage paths**:
  - NiceHash → Mining-Dutch
  - MiningRigRentals → Mining-Dutch
  - MiningRigRentals → NiceHash
- **RealArbitrageCalculator** - Real profitability calculations using Mining-Dutch API
- **AutoExecutionEngine** - Automated order/rental creation with risk management
- **ArbitrageTracker** - Historical performance tracking and analytics
- **AlgorithmMapper** - Converts between NH and MD algorithm names/units (35+ algorithms)
- **Color-coded opportunities**:
  - Green (>15% margin) - Excellent
  - Yellow (>10% margin) - Good
  - White (>5% margin) - Acceptable
- **Real profitability metrics**:
  - Current - Real-time snapshot
  - Estimate 24h - Pool's 24h estimate
  - **Actual 24h** - Real miner earnings (recommended)
- **Auto-execute mode** for opportunities >15% margin
- **Historical tracking** with CSV export
- **Performance analytics** with accuracy metrics

### 🛠️ Core Infrastructure

#### Added
- **SystemHealthCheck** - Pre-startup validation system
  - Configuration file validation
  - API credential checking
  - Live API connection testing
  - Write permission verification
  - Directory structure validation
  - Colored console health report
- **HashCalculator** - Hashrate conversion utilities
  - Universal unit conversions (H, KH, MH, GH, TH, PH)
  - Auto-formatting with unit selection
  - Profit margin calculations
  - Break-even analysis
- **DataValidator** - Input validation and sanitization
  - Bitcoin address validation
  - UUID/GUID validation
  - Range validation and clamping
  - Sensitive data sanitization for logs
- **Enhanced ConfigManager** with new settings:
  - MiningDutchSettings
  - ArbitrageSettings
  - LoggingSettings
  - PerformanceSettings
  - NotificationSettings

### 📁 Configuration

#### Added
- **bot.json.template** - Safe configuration template with defaults
- **settings.json.template** - API credentials template
- **Enhanced .gitignore** - Prevents credential commits
- **Default configuration** for all new settings
- **Backward compatibility** with existing configurations

### 🔧 API Clients

#### Added
- **MiningDutchClient** - Full Mining-Dutch pool API integration
  - Algorithm status with 3 profitability metrics
  - 2-minute caching to reduce API calls
  - Hashrate conversion support
  - Pool fee calculations
- **Enhanced NiceHashClient** with modern async patterns
- **Enhanced MrrClient** with rental automation
- **WhatToMineClient** for additional profitability data

### 📚 Documentation

#### Added
- **GETTING_STARTED.md** (500+ lines) - Complete beginner guide
  - Step-by-step setup instructions
  - Configuration examples
  - Dashboard usage tutorial
  - Safety and risk management
  - Troubleshooting guide
  - Performance expectations
  - Final pre-launch checklist
- **ARBITRAGE_FEATURES_GUIDE.md** (500+ lines) - Arbitrage feature usage
- **REAL_PROFITABILITY_GUIDE.md** (600+ lines) - Profitability calculations explained
- **POLISH_AND_ENHANCEMENTS.md** (300+ lines) - v2.0 improvements summary
- **COMPLETE_FEATURES_SUMMARY.md** (486 lines) - Complete feature inventory
- **Updated README.md** - Modern documentation with badges and sections

### 🔒 Security & Safety

#### Added
- **Configuration templates** prevent accidental credential exposure
- **Enhanced .gitignore** explicitly excludes sensitive files
- **API key sanitization** in logs
- **Pre-flight validation** before operations
- **Risk management** with configurable limits:
  - MaxDailySpend
  - MaxOrderSize
  - StopLossThreshold
  - MinProfitMargin

### 🏗️ Architecture

#### Changed
- **Full async/await** architecture throughout
- **Modern dependency injection** patterns
- **Centralized configuration** management
- **Modular component design** for extensibility
- **Partial classes** for code organization (RealArbitrageCalculator split)

### 🐛 Bug Fixes

#### Fixed
- Thread safety issues with proper async patterns
- Configuration loading errors with better validation
- Pool creation with proper BTC address handling
- MRR arbitrage path implementation completed

### 📊 Statistics

#### Code Metrics
- **~5,000+ lines** of new production code
- **~2,500+ lines** of documentation
- **30+ new files** created
- **10+ existing files** enhanced
- **8 configuration templates** added
- **3 arbitrage paths** implemented
- **35+ algorithms** supported with mapping

---

## [1.0.0] - Original Release

### Features
- Basic NiceHash order management
- Pool management (create/edit/delete)
- Auto-refilling when 90% consumed
- Price adjustment (increase/decrease)
- Console logging
- Test and production environment support

---

## Upgrade Guide: v1.0 → v2.0

### Required Actions
1. **Copy configuration templates**:
   ```bash
   copy settings.json.template settings.json
   copy bot.json.template bot.json
   ```

2. **Configure Mining-Dutch** in bot.json:
   ```json
   {
     "MiningDutch": {
       "BTCAddress": "YOUR_BTC_ADDRESS",
       "WorkerPrefix": "NHB3"
     }
   }
   ```

3. **Optional: Configure MRR** in settings.json for MRR arbitrage paths

### New Features Available
- Modern dashboard UI (automatic)
- Arbitrage trading (Settings → Arbitrage 💎)
- Health check system (automatic on startup)
- Enhanced logging and utilities (automatic)

### Breaking Changes
None - v2.0 is fully backward compatible with v1.0 configurations.

---

## Future Roadmap

### Planned Features
- [ ] Additional pool integrations (Prohashing, Zergpool)
- [ ] Webhook notifications (Discord, Telegram)
- [ ] Advanced strategy framework
- [ ] Backtesting system
- [ ] Portfolio management
- [ ] Multi-account support
- [ ] Mobile app companion

### Community Requests
Open an issue on GitHub to suggest features!

---

## Version Numbering

We follow [Semantic Versioning](https://semver.org/):
- **MAJOR** version for incompatible API changes
- **MINOR** version for backwards-compatible functionality
- **PATCH** version for backwards-compatible bug fixes

---

<p align="center">
  <strong>v2.0</strong> - November 2024<br>
  A complete transformation from basic bot to professional arbitrage platform
</p>
