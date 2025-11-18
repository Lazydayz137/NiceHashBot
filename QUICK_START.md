# 🚀 QUICK START GUIDE - NiceHashBot v2.0

## ⚡ Getting Up and Running

This guide will get you from zero to monitoring in **5 minutes**!

---

## 📋 Prerequisites

Before you start:
- ✅ Windows (7/8/10/11)
- ✅ .NET Framework 4.7.2+ installed
- ✅ Visual Studio 2017+ (for building)
- ✅ NiceHash account with API credentials
- ✅ (Optional) MRR account with API credentials
- ✅ (Optional) Mining-Dutch BTC address

---

## 🔧 Step 1: Build the Project

1. Open `NiceHashBot.sln` in Visual Studio
2. Press **F7** (or Build → Build Solution)
3. Wait for build to complete
4. Executable will be at: `NHB3\bin\Debug\NHB3.exe`

---

## ⚙️ Step 2: Configuration

### Create settings.json (REQUIRED)

```bash
cd NHB3\bin\Debug
copy settings.json.template settings.json
notepad settings.json
```

**Edit settings.json:**
```json
{
  "OrganizationID": "YOUR_NICEHASH_ORG_ID",
  "ApiID": "YOUR_NICEHASH_API_KEY",
  "ApiSecret": "YOUR_NICEHASH_API_SECRET",
  "Enviorment": 1,
  "MrrApiKey": "YOUR_MRR_API_KEY",
  "MrrApiSecret": "YOUR_MRR_API_SECRET"
}
```

**Get NiceHash API Credentials:**
1. Go to https://www.nicehash.com/my/settings/keys
2. Create new API key with permissions:
   - View Account Information
   - Manage Orders
3. Copy Org ID, API Key, and API Secret

**Get MRR API Credentials (Optional):**
1. Go to https://www.miningrigrentals.com/account/apikey
2. Create new API key
3. Copy Key and Secret

### Create bot.json (for Arbitrage features)

```bash
copy bot.json.template bot.json
notepad bot.json
```

**Edit bot.json:**
```json
{
  "RiskManagement": {
    "MaxDailySpend": 0.1,
    "MaxOrderSize": 0.01,
    "MinProfitMargin": 5.0
  },
  "MiningDutch": {
    "BTCAddress": "YOUR_BITCOIN_ADDRESS_HERE",
    "WorkerPrefix": "NHB3"
  }
}
```

---

## 🎯 Step 3: Choose Your Launch Method

### Method 1: Quick Launchers (Easiest!)

**Launch GUI:**
```bash
Launch_GUI.bat
```

**Launch Multi-Algorithm Monitor:**
```bash
Launch_Multi_Algorithm_Monitor.bat
```

**Launch Equihash Monitor:**
```bash
Launch_Equihash_Monitor.bat
```

### Method 2: Command Line

**GUI Mode (default):**
```bash
NHB3.exe
```

**Multi-Algorithm Monitor:**
```bash
NHB3.exe multi
```

**Equihash Monitor:**
```bash
NHB3.exe equihash
```

**Help:**
```bash
NHB3.exe help
```

### Method 3: From GUI

1. Launch `NHB3.exe` (GUI mode)
2. Click **Arbitrage 💎** menu
3. Choose:
   - **📊 Dashboard** - Arbitrage dashboard (Windows Form)
   - **⚡ Multi-Algorithm Monitor** - Scan all 5 algorithms
   - **💎 Equihash Monitor** - Specialized Equihash tracking

---

## 🎨 What Each Mode Does

### 📊 Arbitrage Dashboard (GUI)
- Windows Forms dashboard
- Manual arbitrage calculations
- Historical tracking
- Auto-execution engine

### ⚡ Multi-Algorithm Monitor (Console)
**Scans 5 algorithms simultaneously:**
- 💎 Equihash (Zcash, Bitcoin Gold)
- ₿ SHA-256 (Bitcoin)
- 🐕 Scrypt (Litecoin, Dogecoin)
- 💳 X11 (Dash)
- 🦅 KawPow (Ravencoin)

**Features:**
- Ranked by profitability
- Predictive analytics
- Risk assessment
- Confidence scoring
- Top recommendation
- Continuous monitoring (5 min intervals)

### 💎 Equihash Monitor (Console)
**Specialized Equihash tracking:**
- Top 5 cheapest MRR rigs
- 4 NiceHash hashrate tiers (500-5000 Sol/s)
- Predictive profitability metrics
- Earnings projections (daily/weekly/monthly)
- Session statistics
- Real-time alerts

---

## 🔥 First-Time Workflow

### For Equihash Contracts (Your Use Case!)

1. **Launch Equihash Monitor:**
   ```bash
   Launch_Equihash_Monitor.bat
   ```

2. **Choose Mode:**
   ```
   1 - Single scan           → Quick one-time check
   2 - Continuous monitoring → Auto-refresh every 2 min
   3 - Quick summary         → Fast status overview
   ```

3. **Review Results:**
   - Green (≥15%) = Excellent opportunity
   - Yellow (≥10%) = Good opportunity
   - White (≥5%) = Acceptable
   - Gray (>0%) = Marginal
   - Red (<0%) = Avoid

4. **Check Predictive Metrics:**
   - Profitability Trend (24h % change)
   - Volatility Rating (Low/Medium/High)
   - Confidence Score (0-100%)
   - Risk Level (Low/Medium/High)
   - Recommended Action (Buy/Hold/Avoid)
   - Forecasts (1h, 3h, 6h, 12h, 24h)

5. **Act on Alerts:**
   - Audio beep + visual banner for high-profit opportunities
   - Default threshold: 10% margin
   - Configurable in settings menu

### For Multi-Algorithm Scanning

1. **Launch Multi-Algorithm Monitor:**
   ```bash
   Launch_Multi_Algorithm_Monitor.bat
   ```

2. **Choose Mode:**
   ```
   1 - Scan All Algorithms    → Quick market overview
   2 - Continuous Monitor     → Auto-scan every 5 min
   3 - Equihash Monitor       → Jump to Equihash
   4 - Single Algorithm       → Choose specific algorithm
   5 - Algorithm Guide        → Hardware & tips per algo
   6 - Help                   → Comprehensive help
   ```

3. **Review Rankings:**
   ```
   ╔═══════════════════════════════════════════════════════════╗
   ║          MULTI-ALGORITHM PROFITABILITY SUMMARY            ║
   ╠═══════════════════════════════════════════════════════════╣
   ║ 1. 💎 Equihash (Zcash, BTG)          18.45% ║
   ║ 2. 🦅 KawPow (Ravencoin)              12.30% ║
   ║ 3. 🐕 Scrypt (Litecoin, Doge)         8.75% ║
   ║ 4. 💳 X11 (Dash)                      6.20% ║
   ║ 5. ₿  SHA-256 (Bitcoin)               4.10% ║
   ╚═══════════════════════════════════════════════════════════╝
   ```

4. **Follow Top Recommendation:**
   - System highlights best opportunity
   - Shows margin, profit, hashrate
   - Includes predictive metrics

---

## 💡 Pro Tips

### Monitoring Your MRR Equihash Contracts

Since you mentioned you have active Equihash contracts:

1. **Use Continuous Monitoring (Mode 2)**
   - Auto-scans every 2 minutes
   - Catches new profitable rigs as they appear
   - Shows real-time profitability changes

2. **Pay Attention to Predictive Metrics**
   - **Profitability Trend** - Is it going up or down?
   - **Risk Level** - Avoid "High Risk" opportunities
   - **Forecasts** - Check 6h/12h predictions
   - **Confidence Score** - Higher = more reliable

3. **Watch for Alerts**
   - Default: 10% margin triggers alert
   - Audio beep + green banner
   - Configure threshold in menu (Option 4)

4. **Check MRR Constraints**
   - MinHours - Minimum rental period
   - MaxHours - Maximum rental period
   - System automatically factors these in!
   - Warns if forced to rent minimum duration

5. **Use Session Statistics**
   - Track total scans
   - Count profitable opportunities
   - See best margin found
   - Monitor session uptime

### Algorithm Selection Tips

**GPU Mining:**
- 💎 Equihash - High profit potential
- 🦅 KawPow - Growing market

**ASIC Mining:**
- ₿ SHA-256 - Most liquid, stable
- 🐕 Scrypt - Lower competition
- 💳 X11 - Moderate profits

**Start With:**
1. Scan all algorithms (Multi-Algorithm Monitor)
2. See which is most profitable now
3. Use specialized monitor for deep dive
4. Check algorithm guide for hardware requirements

---

## 🛠️ Configuration Options

### Equihash Monitor Settings (Menu Option 4)

```
1. Scan Interval: 2 minutes (range: 1-60)
2. Alert Threshold: 10% (range: 0-100)
3. Alerts: ENABLED (toggle on/off)
4. Detailed Stats: ENABLED (toggle on/off)
5. Top Rigs to Show: 5 (range: 1-20)
```

### Multi-Algorithm Monitor

- Scan Interval: 5 minutes (continuous mode)
- Scans all 5 algorithms automatically
- Ranks by profitability
- Shows top recommendation

---

## 📤 Exporting Results

### From Equihash Monitor

1. Main Menu → Option 5 (Export Results)
2. Enter filename or press Enter for default
3. Creates timestamped text file with:
   - Session statistics
   - Best opportunity found
   - Scan count and duration

### From Multi-Algorithm Monitor

- Results displayed in console
- Copy/paste to save
- Or screenshot the rankings

---

## 🔍 Troubleshooting

### "NHB3.exe not found"
→ Build the project first (F7 in Visual Studio)

### "settings.json not found"
→ Copy settings.json.template and configure

### "API credentials invalid"
→ Check your NiceHash API settings
→ Verify Organization ID, API Key, API Secret

### "MRR not configured"
→ Add MrrApiKey and MrrApiSecret to settings.json
→ Or continue without MRR (NiceHash-only mode)

### "Mining-Dutch address not configured"
→ Add BTCAddress to bot.json
→ Only needed for arbitrage auto-execution

### Console window closes immediately
→ Run from Command Prompt or use batch launchers
→ Check for error messages

---

## 🎯 Next Steps

1. ✅ Build the project
2. ✅ Configure settings.json
3. ✅ Configure bot.json (optional)
4. ✅ Launch Equihash Monitor
5. ✅ Review profitable opportunities
6. ✅ Check predictive metrics
7. ✅ Monitor continuously
8. 💰 **Profit!**

---

## 📚 Additional Resources

- **README.md** - Full feature list and overview
- **CHANGELOG.md** - Version history and changes
- **GETTING_STARTED.md** - Detailed setup guide
- **Algorithm Guide** - In Multi-Algorithm Monitor (Option 5)
- **Help** - In consoles (Option 6 or `NHB3.exe help`)

---

## 🚨 Important Notes

### Security
- **NEVER** commit settings.json or bot.json to git
- Keep API keys secure
- Use read-only API keys if possible
- Enable 2FA on NiceHash and MRR accounts

### Profitability
- "Actual24h" is most reliable metric
- Factor in MRR 3% rental fees
- Consider MinHours/MaxHours constraints
- Check network hashrate trends
- Verify Mining-Dutch pool status

### Best Practices
- Start with single scans to test
- Use continuous monitoring once configured
- Set realistic alert thresholds (10%+ recommended)
- Monitor during peak hours (8am-6pm UTC)
- Check MRR rig ratings (4.5+ stars preferred)
- Avoid "High Risk" opportunities

---

## ✅ You're Ready!

Everything is now set up for professional-grade multi-algorithm arbitrage monitoring!

**Quick Launch Options:**
```bash
Launch_GUI.bat                        # GUI dashboard
Launch_Multi_Algorithm_Monitor.bat    # Scan all 5 algorithms
Launch_Equihash_Monitor.bat          # Equihash specialist
```

**Command Line:**
```bash
NHB3.exe            # GUI
NHB3.exe multi      # Multi-algorithm
NHB3.exe equihash   # Equihash
```

---

**Happy Mining! 💎⚡**
