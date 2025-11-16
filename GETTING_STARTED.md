# 🚀 Getting Started with NiceHashBot v2.0

## Quick Start Guide

This guide will help you set up and run the modernized NiceHashBot with full arbitrage capabilities.

---

## Prerequisites

### Required:
- ✅ Windows with .NET Framework 4.7.2+
- ✅ NiceHash account with API credentials
- ✅ Bitcoin address for receiving Mining-Dutch payouts
- ✅ Visual Studio 2017+ or MSBuild for compilation

### Optional (for MRR arbitrage):
- 🔸 MiningRigRentals account with API credentials

---

## Installation

### 1. Clone or Download the Repository

```bash
git clone https://github.com/Lazydayz137/NiceHashBot.git
cd NiceHashBot
```

### 2. Configure API Credentials

**Copy the template files:**
```bash
copy settings.json.template settings.json
copy bot.json.template bot.json
```

**Edit `settings.json`:**
```json
{
  "OrganizationID": "YOUR_ORGANIZATION_ID_HERE",
  "ApiID": "YOUR_API_KEY_HERE",
  "ApiSecret": "YOUR_API_SECRET_HERE",
  "Enviorment": 1,
  "MrrApiKey": "YOUR_MRR_API_KEY_HERE",
  "MrrApiSecret": "YOUR_MRR_API_SECRET_HERE"
}
```

**Where to get NiceHash credentials:**
1. Login to NiceHash
2. Go to Settings → API Keys
3. Create new API key with permissions:
   - Wallet: View
   - Orders: Manage
   - Pools: View & Manage

**Environment values:**
- `0` = Testnet (recommended for testing!)
- `1` = Production (real money!)

### 3. Configure Bot Settings

**Edit `bot.json`:**

```json
{
  "RiskManagement": {
    "MaxDailySpend": 0.1,
    "MaxOrderSize": 0.01,
    "StopLossEnabled": true,
    "StopLossThreshold": 0.2,
    "MinProfitMargin": 5.0,
    "AutoExecuteMarginThreshold": 15.0
  },
  "MiningDutch": {
    "BTCAddress": "YOUR_BTC_ADDRESS_HERE",
    "WorkerPrefix": "NHB3"
  },
  "Arbitrage": {
    "ScanIntervalMinutes": 5,
    "PreferredMetric": "Actual24h",
    "EnabledPaths": [
      "NiceHash→MiningDutch",
      "MRR→MiningDutch"
    ]
  }
}
```

**Critical settings:**
- `MiningDutch.BTCAddress` - **REQUIRED** for auto-execution
- `MaxDailySpend` - Maximum BTC to spend per day
- `MaxOrderSize` - Maximum BTC per single order/rental

### 4. Build the Project

**Using Visual Studio:**
1. Open `NiceHashBot.sln`
2. Build → Build Solution (Ctrl+Shift+B)

**Using MSBuild:**
```bash
msbuild NiceHashBot.sln /p:Configuration=Release
```

### 5. Run Health Check

Before trading, verify your configuration:

```csharp
// Add to Program.cs Main():
var healthCheck = await SystemHealthCheck.PerformHealthCheckAsync(nhClient, mrrClient, mdClient);
SystemHealthCheck.PrintHealthCheckReport(healthCheck);

if (!healthCheck.IsHealthy)
{
    Console.WriteLine("Please fix errors before continuing.");
    return;
}
```

---

## First Launch

### Option 1: Launch via Home Form (Recommended)

1. Run `NHB3.exe`
2. From the Home form, add a menu item or button:

```csharp
// In Home form:
private void btnArbitrage_Click(object sender, EventArgs e)
{
    LaunchArbitrageDashboard();
}
```

3. The dashboard will automatically:
   - Load API credentials from `settings.json`
   - Initialize NiceHash, Mining-Dutch, and MRR clients
   - Display the arbitrage dashboard

### Option 2: Direct Dashboard Launch

```csharp
// Standalone launch:
var apiSettings = ConfigManager.Instance.LoadApiSettings();
var botConfig = ConfigManager.Instance.LoadBotConfig();

var nhClient = new NiceHashClient(...);
var nhService = new NiceHashService(nhClient);
var mdClient = new MiningDutchClient();
var mrrService = new MrrService(new MrrClient(...));

var calculator = new RealArbitrageCalculator(nhService, mdClient, mrrService);
var executor = new AutoExecutionEngine(nhService, mrrService, botConfig);
var tracker = new ArbitrageTracker();

var dashboard = new ArbitrageDashboard(calculator, executor, tracker);
dashboard.Show();
```

---

## Using the Arbitrage Dashboard

### Tab 1: Live Opportunities

#### Scan for Opportunities:

1. **Set minimum margin**: Start with 5% for conservative approach
2. **Select metric**:
   - **Actual 24h** (Recommended) - Real miner earnings from past 24h
   - Estimate 24h - Pool's 24h estimate
   - Current - Real-time snapshot
3. **Click "Scan All Algorithms"**
4. Wait 30-60 seconds for results

#### Interpret Results:

**Color coding:**
- 🟢 **Green** (>15% margin) - Excellent! Auto-execute candidate
- 🟡 **Yellow** (>10% margin) - Good opportunity
- ⚪ **White** (>5% margin) - Acceptable, monitor closely

**Columns explained:**
- **Algorithm** - Mining algorithm (SHA256, Scrypt, etc.)
- **Type** - Arbitrage path (NH→MD, MRR→MD, MRR→NH)
- **Margin %** - Profit margin percentage
- **Profit BTC/day** - Expected daily profit in BTC
- **Buy Cost** - Cost to buy/rent hashrate
- **Revenue** - Expected revenue from mining
- **Hashrate** - Recommended hashrate amount
- **Notes** - Additional info (pool stats, workers, etc.)

#### Execute Trade:

**Manual execution:**
1. Click on opportunity row to select
2. Click "Execute Selected"
3. Review confirmation dialog
4. Click "Yes" to create order/rental

**Auto-execute mode:**
1. Check "Auto-Execute Mode" checkbox
2. Bot will scan every 5 minutes
3. Automatically executes >15% margin opportunities
4. Notifications shown for each execution

### Tab 2: History & Performance

#### View Statistics:

- **Total executions** - All trades attempted
- **Success rate** - Percentage of successful executions
- **Expected vs Actual profit** - Profitability accuracy
- **Average margins** - Average profit margins achieved
- **Best/Worst trades** - Performance extremes

#### Analyze Performance:

**Per-algorithm breakdown:**
- Which algorithms are most profitable
- Success rates by algorithm
- Total profit by algorithm

**Per-type breakdown:**
- NH→MD vs MRR→MD vs MRR→NH
- Which arbitrage path works best
- Adjust strategy accordingly

**Time-based stats:**
- Last 24 hours profit
- Last 7 days profit
- Last 30 days profit

#### Export Data:

Click "Export to CSV" to analyze in Excel/Google Sheets

---

## Safety & Risk Management

### Start Small!

**Recommended testing approach:**

1. **Day 1-2: Testnet** (`Environment: 0`)
   - Test all features with fake money
   - Verify dashboard functionality
   - Practice manual execution

2. **Day 3-4: Production, Manual Only**
   - `MaxOrderSize: 0.001` BTC
   - Manual execution only
   - Monitor results closely

3. **Week 2: Small Auto-Execute**
   - `MaxOrderSize: 0.005` BTC
   - Enable auto-execute for >15% margins
   - Review daily

4. **Week 3+: Scale Gradually**
   - Increase order sizes based on performance
   - Use historical data to optimize
   - Focus on best-performing algorithms

### Risk Management Settings

**Conservative (Recommended for beginners):**
```json
{
  "RiskManagement": {
    "MaxDailySpend": 0.05,
    "MaxOrderSize": 0.005,
    "StopLossEnabled": true,
    "StopLossThreshold": 0.15,
    "MinProfitMargin": 8.0,
    "AutoExecuteMarginThreshold": 15.0
  }
}
```

**Moderate (After 2-4 weeks experience):**
```json
{
  "RiskManagement": {
    "MaxDailySpend": 0.1,
    "MaxOrderSize": 0.01,
    "MinProfitMargin": 5.0,
    "AutoExecuteMarginThreshold": 12.0
  }
}
```

**Aggressive (Only with proven track record):**
```json
{
  "RiskManagement": {
    "MaxDailySpend": 0.5,
    "MaxOrderSize": 0.05,
    "MinProfitMargin": 3.0,
    "AutoExecuteMarginThreshold": 8.0
  }
}
```

---

## Best Practices

### 1. Always Use Actual24h Metric

- Most reliable profitability data
- Based on real miner earnings
- Accounts for pool luck and variance
- Lower chance of negative surprises

### 2. Monitor Actively (First 2 Weeks)

- Check dashboard every 4-6 hours
- Review execution history daily
- Compare expected vs actual profit
- Adjust settings based on results

### 3. Focus on High-Margin Opportunities

- Only auto-execute >15% margin
- Manually review 10-15% opportunities
- Skip <10% unless very confident

### 4. Track Everything

- Export history weekly
- Calculate actual ROI
- Identify best-performing algorithms
- Adjust strategy based on data

### 5. Manage Your Bankroll

- Never invest more than you can afford to lose
- Keep reserve for refills
- Withdraw profits regularly
- Don't get greedy on single trades

---

## Troubleshooting

### "Failed to create NiceHash order"

**Possible causes:**
- Insufficient balance
- Invalid pool configuration
- API permission issues
- Order price not competitive

**Solutions:**
1. Check NiceHash balance
2. Verify pool exists and is active
3. Check API key permissions
4. Try lower hashrate amount

### "No opportunities found"

**This is normal!**
- Arbitrage opportunities are periodic
- Market conditions vary
- Try different times of day
- Lower minimum margin threshold

**When to scan:**
- During difficulty adjustments
- When BTC price is volatile
- During low network hashrate periods
- Different times of day (try 6am, 2pm, 10pm UTC)

### "Mining-Dutch BTC address not configured"

**Fix:**
1. Open `bot.json`
2. Set `MiningDutch.BTCAddress` to your BTC address
3. Restart application

### "Actual profit < Expected profit"

**This is normal variance!**
- Pool luck affects results
- Difficulty changes during order
- Network hashrate fluctuations
- Mining-Dutch Actual24h is most accurate but still estimates

**Improving accuracy:**
- Use longer order durations (24h minimum)
- Track accuracy metrics over time
- Focus on algorithms with consistent results
- Adjust expectations based on historical data

---

## Performance Expectations

### Realistic Results

Based on Mining-Dutch actual profitability data:

**Conservative (5% minimum margin):**
- Opportunities: 2-5 per day
- Average margin: 6-8%
- Daily profit: ~0.1-0.3% of capital

**Moderate (8% minimum margin):**
- Opportunities: 1-3 per day
- Average margin: 9-12%
- Daily profit: ~0.2-0.5% of capital

**Aggressive (15% minimum margin):**
- Opportunities: 0-2 per week
- Average margin: 16-20%
- Daily profit: ~0.3-0.8% of capital (when found)

**Important:** Results vary based on:
- Market conditions
- Pool performance
- Network difficulty changes
- Price volatility
- Your reaction time (manual) or auto-execute threshold

---

## Next Steps

### After First Week:

1. Review your execution history
2. Calculate your actual ROI
3. Identify best-performing algorithms
4. Adjust risk management settings
5. Consider enabling more arbitrage paths

### Optimization Tips:

- Export history to CSV weekly
- Track accuracy metrics (expected vs actual)
- Focus capital on best-performing algos
- Adjust scan intervals based on market conditions
- Join mining pools with good performance

### Advanced Features:

- Custom strategies (extend BaseStrategy)
- Webhook notifications (Discord, Telegram)
- Multiple pool integrations
- Backtesting framework

---

## Support & Resources

### Documentation:
- `README_MODERNIZATION.md` - Architecture overview
- `REAL_PROFITABILITY_GUIDE.md` - Profitability calculations explained
- `ARBITRAGE_FEATURES_GUIDE.md` - Feature usage guide
- `COMPLETE_FEATURES_SUMMARY.md` - Complete feature list

### Logs:
- `logs/nhb_YYYY-MM-DD.log` - Daily application logs
- `arbitrage_history.json` - Execution history

### Configuration:
- `settings.json` - API credentials
- `bot.json` - Bot and risk management settings

---

## Final Checklist

Before going live with real money:

- [ ] Tested on testnet successfully
- [ ] Configured `settings.json` with correct API credentials
- [ ] Set `MiningDutch.BTCAddress` in `bot.json`
- [ ] Set conservative `MaxOrderSize` and `MaxDailySpend`
- [ ] Ran health check - all components pass
- [ ] Tested manual execution successfully
- [ ] Understand color coding and metrics
- [ ] Have monitoring plan (check every 4-6 hours)
- [ ] Know how to check NiceHash orders
- [ ] Know how to check Mining-Dutch payouts
- [ ] Have exit strategy (when to stop/adjust)

---

## 🎯 You're Ready!

**Start small, monitor closely, scale gradually.**

The bot is a tool - your judgment and monitoring are critical to success.

Good luck and happy arbitraging! 🚀💰
