# 🚀 Arbitrage Features - Complete Guide

## Overview

The NiceHashBot now includes **three powerful new features** for automated arbitrage trading:

1. **Arbitrage Dashboard** - Visual interface for monitoring opportunities
2. **Auto-Execution Engine** - Automatically execute profitable trades
3. **Historical Tracking** - Track performance and analyze results

---

## 1. Arbitrage Dashboard

### Features:
- **Real-time opportunity scanning** across all algorithms
- **Live profitability display** with color-coded margins
- **Multiple profitability metrics** (Current, Est24h, Actual24h)
- **Auto-refresh** every 5 minutes
- **One-click execution** of selected opportunities
- **Historical performance** tab with statistics

### How to Launch:

```csharp
// From your Home form or any form with ApiConnect
public void ShowArbitrageDashboard()
{
    LaunchArbitrageDashboard(); // Call the integration method
}
```

Or add a menu item:
```csharp
// In Home.Designer.cs, add to menu strip:
var arbitrageMenuItem = new ToolStripMenuItem("Arbitrage Dashboard");
arbitrageMenuItem.Click += (s, e) => LaunchArbitrageDashboard();
toolsMenu.DropDownItems.Add(arbitrageMenuItem);
```

### Dashboard Tabs:

#### Tab 1: Live Opportunities
- **Scan All Algorithms** button - Searches across all supported algos
- **Min Margin %** setting - Filter opportunities by minimum profit margin
- **Metric** dropdown - Choose Current/Estimate24h/Actual24h
- **Auto-Execute Mode** checkbox - Automatically execute >15% margin opportunities
- **Execute Selected** button - Manually execute chosen opportunity

**Color Coding:**
- 🟢 **Green** - Margin > 15% (excellent!)
- 🟡 **Yellow** - Margin > 10% (good)
- ⚪ **White** - Margin > 5% (acceptable)

#### Tab 2: History & Performance
- **Execution history** - All past trades with expected vs actual results
- **Performance stats** - Success rate, total profit, averages
- **Per-algorithm breakdown** - Which algos perform best
- **Per-type breakdown** - NH→MD vs MRR→MD vs MRR→NH

---

## 2. Auto-Execution Engine

### What It Does:
- Automatically creates NiceHash orders or MRR rentals
- Configures Mining-Dutch pool connections
- Validates opportunities before execution
- Applies risk management limits
- Tracks order/rental IDs for monitoring

### Execution Paths:

#### NiceHash → Mining-Dutch
1. Gets or creates Mining-Dutch pool for algorithm
2. Calculates competitive order price (0.5% below market)
3. Creates STANDARD order on NiceHash
4. Points hash to Mining-Dutch pool
5. Returns order ID for tracking

#### MRR → Mining-Dutch
1. Finds cheapest available rig for algorithm
2. Creates Mining-Dutch pool configuration
3. Creates rental on MRR
4. Configures rental to point to Mining-Dutch
5. Returns rental ID for tracking

#### MRR → NiceHash
1. Finds cheapest MRR rig
2. Creates NiceHash pool configuration
3. Creates rental pointing to NH pool
4. Earns by selling hashrate on NH marketplace

### Risk Management Checks:
- ✅ Opportunity not expired (ValidUntil)
- ✅ Margin > 5% minimum
- ✅ Order size within limits (MaxOrderSize)
- ✅ Daily spend limits (MaxDailySpend)

### Configuration:

In `bot.json`:
```json
{
  "RiskManagement": {
    "MaxDailySpend": 0.1,        // Max 0.1 BTC per day
    "MaxOrderSize": 0.01,        // Max 0.01 BTC per order
    "StopLossEnabled": true,
    "StopLossThreshold": 0.2     // Stop if loss > 20%
  }
}
```

### Usage Example:

```csharp
// Manual execution
var autoExecutor = new AutoExecutionEngine(nhService, mrrService, botConfig);
var execution = await autoExecutor.ExecuteAsync(opportunity);

if (execution.Success)
{
    Console.WriteLine($"Created order {execution.OrderId}");
    Console.WriteLine($"Expected profit: {execution.ExpectedProfit:F8} BTC/day");
}
else
{
    Console.WriteLine($"Execution failed: {execution.ErrorMessage}");
}
```

### Auto-Execute Mode:
When enabled in the dashboard:
- Scans for opportunities every 5 minutes
- **Automatically executes** any opportunity with margin > 15%
- Displays notification when executed
- Logs to history for tracking

---

## 3. Historical Tracking & Analytics

### What It Tracks:
- ✅ All execution attempts (success/failure)
- ✅ Expected vs actual profitability
- ✅ Expected vs actual profit margin
- ✅ Order/rental IDs
- ✅ Timestamp and duration
- ✅ Current status (Active/Completed/Failed)

### Statistics Provided:

#### Overall Performance:
- Total executions
- Success rate (%)
- Total expected vs actual profit
- Average margin (expected vs actual)
- Best and worst trades

#### Per-Algorithm Performance:
```
SHA256:
  - Total executions: 15
  - Success rate: 93%
  - Average margin: 8.5%
  - Total profit: 0.00235000 BTC

Scrypt:
  - Total executions: 8
  - Success rate: 100%
  - Average margin: 6.2%
  - Total profit: 0.00089000 BTC
```

#### Per-Type Performance:
```
NiceHash→MiningDutch:
  - Total executions: 18
  - Average margin: 7.8%
  - Total profit: 0.00280000 BTC

MRR→MiningDutch:
  - Total executions: 5
  - Average margin: 9.1%
  - Total profit: 0.00044000 BTC
```

#### Time-Based Stats:
- Last 24 hours profit
- Last 7 days profit
- Last 30 days profit
- Daily profit chart

#### Accuracy Metrics:
- **Profit Accuracy**: How close actual profit is to expected (%)
- **Margin Accuracy**: How close actual margin is to expected (%)

### Usage Examples:

```csharp
var tracker = new ArbitrageTracker();

// Log a new execution
tracker.LogExecution(execution);

// Update with actual results (after order completes)
tracker.UpdateExecution(
    orderId: execution.OrderId,
    actualProfit: 0.00038000m,
    actualMargin: 8.5m,
    status: "Completed"
);

// Get statistics
var stats = tracker.GetStatistics();
Console.WriteLine($"Success rate: {stats.SuccessRate:F1}%");
Console.WriteLine($"Total profit: {stats.TotalActualProfit:F8} BTC");
Console.WriteLine($"Average margin: {stats.AverageMargin:F2}%");
Console.WriteLine($"Profit accuracy: {stats.ProfitAccuracy:F1}%");

// Get recent executions
var recent = tracker.GetRecentExecutions(50);
foreach (var exec in recent)
{
    Console.WriteLine($"{exec.Timestamp}: {exec.Algorithm} - {exec.ActualMargin:F2}% margin");
}

// Export to CSV for analysis
tracker.ExportToCsv("arbitrage_history.csv");

// Cleanup old data
tracker.CleanupOldHistory(keepDays: 90);
```

### Data Persistence:
- Stored in `arbitrage_history.json`
- Auto-saves after each execution
- Auto-loads on startup
- Can export to CSV for external analysis

---

## Complete Workflow Example

### 1. Initial Setup
```csharp
// Configure API credentials in settings.json
{
  "OrganizationID": "your-org-id",
  "ApiID": "your-api-key",
  "ApiSecret": "your-api-secret",
  "Environment": 1,  // 1 = Production
  "MrrApiKey": "your-mrr-key",      // Optional
  "MrrApiSecret": "your-mrr-secret"  // Optional
}
```

### 2. Configure Bot Settings
```csharp
// Configure bot.json
{
  "RiskManagement": {
    "MaxDailySpend": 0.05,   // Start conservative
    "MaxOrderSize": 0.005,   // Small orders first
    "StopLossEnabled": true,
    "StopLossThreshold": 0.15
  }
}
```

### 3. Launch Dashboard
- From Home form, click "Tools → Arbitrage Dashboard"
- Or call `LaunchArbitrageDashboard()` programmatically

### 4. Scan for Opportunities
- Set **Min Margin**: 5% (start conservative)
- Set **Metric**: Actual 24h (most reliable)
- Click **Scan All Algorithms**
- Wait for results (30-60 seconds)

### 5. Review Opportunities
- Look for **green rows** (>15% margin)
- Click on opportunity to see full details
- Review Mining-Dutch stats (workers, pool hashrate, actual profitability)

### 6. Execute Trade
**Manual:**
- Select opportunity
- Click "Execute Selected"
- Confirm in dialog
- Monitor order in NiceHash or MRR

**Auto (Advanced):**
- Enable "Auto-Execute Mode"
- Bot will automatically execute >15% margin opportunities
- Check back in 5-10 minutes for results

### 7. Monitor Performance
- Switch to "History & Performance" tab
- Review execution history
- Check expected vs actual results
- Analyze which algorithms/types perform best

### 8. Optimize
Based on statistics:
- Focus on best-performing algorithms
- Adjust risk management limits
- Fine-tune profit margin thresholds
- Scale up position sizes gradually

---

## Tips for Success

### 1. Start Small
- Use **testnet first** (Environment: 0)
- Test with small amounts (0.001-0.005 BTC)
- Monitor closely for first 24-48 hours

### 2. Use Actual24h Metric
- Most reliable profitability data
- Based on real miner performance
- Accounts for pool luck

### 3. Be Patient
- Not all scans find opportunities
- Profitability varies by time of day
- Best opportunities during difficulty adjustments

### 4. Monitor Actively
- Check dashboard every few hours
- Update actual results in tracker
- Compare expected vs actual performance

### 5. Risk Management
- Never exceed daily spend limits
- Start with 5% minimum margin
- Only auto-execute >15% margin
- Keep stop-loss enabled

### 6. Track Everything
- Log every execution
- Export data monthly for analysis
- Identify patterns and trends
- Adjust strategy based on data

---

## Troubleshooting

### "Failed to create NiceHash order"
- Check API credentials
- Verify sufficient balance
- Check algorithm is enabled on NH
- Try lower hashrate/amount

### "No MRR rigs available"
- Algorithm may have low availability
- Try different time of day
- Check if algorithm name is correct
- Lower minimum hashrate requirement

### "Opportunity expired"
- Opportunities last ~1 hour
- Prices change rapidly
- Scan again for fresh opportunities

### Auto-execute not working
- Check "Auto-Execute Mode" is enabled
- Verify margin > 15%
- Check risk management limits aren't blocking
- Review logs for errors

### Actual profit < Expected profit
- Normal variance (pool luck, difficulty changes)
- Mining-Dutch Actual24h is most accurate but still estimates
- Longer orders (24h+) have better accuracy
- Track accuracy metrics over time

---

## Advanced Features

### Custom Strategies
You can extend the auto-executor with custom logic:

```csharp
public class CustomExecutor : AutoExecutionEngine
{
    protected override bool ValidateExecution(ArbitrageOpportunity opp, out string error)
    {
        // Add custom validation
        if (opp.Algorithm == "SHA256" && opp.ProfitMargin < 10.0m)
        {
            error = "SHA256 requires >10% margin";
            return false;
        }

        return base.ValidateExecution(opp, out error);
    }
}
```

### Webhook Notifications
Log executions to external services:

```csharp
tracker.OnExecutionLogged += async (execution) =>
{
    if (execution.ExpectedMargin > 15.0m)
    {
        await SendWebhookNotification(
            $"High-margin arbitrage: {execution.Algorithm} " +
            $"({execution.ExpectedMargin:F2}% margin)"
        );
    }
};
```

### Performance Alerts
```csharp
// Alert if accuracy drops below threshold
var stats = tracker.GetStatistics();
if (stats.ProfitAccuracy < 80.0m)
{
    Logger.Instance.Warning($"Profit accuracy low: {stats.ProfitAccuracy:F1}%");
    // Maybe pause auto-execute
    // Maybe adjust parameters
}
```

---

## Summary

You now have a complete arbitrage trading system:

✅ **Real-time opportunity scanning** with live pool data
✅ **Visual dashboard** with color-coded profitability
✅ **Automated execution** with risk management
✅ **Historical tracking** with performance analytics
✅ **Multiple arbitrage paths** (NH→MD, MRR→MD, MRR→NH)
✅ **Comprehensive statistics** for optimization

**Start small, monitor closely, scale gradually!**

Happy arbitraging! 🚀💰
