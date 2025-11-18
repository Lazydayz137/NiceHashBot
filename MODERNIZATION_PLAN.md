# NiceHashBot Modernization & Enhancement Plan

## Executive Summary
This document outlines the comprehensive modernization of NiceHashBot to add robust profitability analysis, MiningRigRentals integration, and advanced order management strategies.

## Current State Analysis

### Architecture
- **Language**: C# .NET Framework 4.7.2
- **UI**: Windows Forms (single-threaded)
- **API**: NiceHash API v2
- **Dependencies**: RestSharp 106.12.0, Newtonsoft.Json 13.0.1
- **Bot Logic**: Timer-based (60s interval), synchronous operations

### Critical Issues Identified
1. **Thread Safety Violation** (Home.cs:208) - `Control.CheckForIllegalCrossThreadCalls = false`
2. **No Async Operations** - All API calls block the timer thread
3. **Limited Profitability Logic** - Simple price stepping, no cross-pool analysis
4. **Outdated Patterns** - No dependency injection, global state, magic numbers
5. **Spelling Errors** - `Enviorment`, `reffilOrder`, `marektObject`
6. **Disabled Features** - Market aggregation commented out (Home.cs:157)

## Enhancement Goals

### 1. NiceHashBot-X Features
- ✅ **OneShot Mode**: Automatically create orders for low-difficulty coin opportunities
- ✅ **Advanced Price Optimization**: Market depth analysis, dynamic pricing
- ✅ **Multi-Instance Support**: Run multiple strategies simultaneously
- ✅ **Configurable Engines**: Per-algorithm strategy customization
- ✅ **Interval-based Creation**: Time-based order management

### 2. MiningRigRentals Integration
- ✅ **MRR API Client**: Full CRUD operations for rentals
- ✅ **Rig Listing**: Search and filter available rigs by algorithm/price
- ✅ **Automated Rentals**: Rent hash when profitable
- ✅ **Pool Redirection**: Point rented hash to NiceHash or other profitable pools
- ✅ **Profit Comparison**: Compare MRR rental cost vs. NiceHash mining profitability

### 3. Cross-Pool Profitability Analysis
- ✅ **WhatToMine Integration**: Real-time coin profitability data
- ✅ **Multi-Pool Comparison**: Analyze profitability across multiple pools
- ✅ **Arbitrage Detection**: Buy hash on NH → mine on more profitable pool
- ✅ **Dynamic Pool Switching**: Automatically point hash to most profitable target
- ✅ **Fee Calculation**: Account for pool fees, withdrawal fees, market slippage

### 4. Advanced Strategy Engine
- ✅ **Market Depth Analysis**: Analyze order book depth for competitive pricing
- ✅ **Historical Analysis**: Track profitability over time
- ✅ **Risk Management**: Stop-loss, max spend limits, position sizing
- ✅ **Multi-Market Support**: Arbitrage between EU/USA/ASIA markets
- ✅ **Smart Refill**: Dynamic refill amounts based on market conditions

## Technical Architecture

### New Components

```
NHB3/
├── Core/
│   ├── Interfaces/
│   │   ├── IApiClient.cs              # Generic API client interface
│   │   ├── IProfitabilityCalculator.cs # Profitability calculation interface
│   │   └── IStrategy.cs                # Trading strategy interface
│   ├── Models/
│   │   ├── MarketData.cs               # Market data models
│   │   ├── ProfitabilityReport.cs      # Profitability analysis results
│   │   └── StrategyConfig.cs           # Strategy configuration
│   └── Services/
│       ├── Logger.cs                   # Structured logging
│       └── ConfigManager.cs            # Configuration management
├── NiceHash/
│   ├── NiceHashClient.cs               # Modernized NH API v2 client (async)
│   ├── NiceHashModels.cs               # NH-specific models
│   └── NiceHashMapper.cs               # DTO mapping
├── MiningRigRentals/
│   ├── MrrClient.cs                    # MRR API v2 client
│   ├── MrrModels.cs                    # MRR-specific models
│   └── MrrMapper.cs                    # DTO mapping
├── Profitability/
│   ├── WhatToMineClient.cs             # WhatToMine API integration
│   ├── ProfitabilityEngine.cs          # Cross-pool profitability calculator
│   └── ArbitrageDetector.cs            # Arbitrage opportunity detection
├── Strategies/
│   ├── BaseStrategy.cs                 # Base strategy class
│   ├── OneShotStrategy.cs              # Low-difficulty coin sniper
│   ├── MarketMakerStrategy.cs          # Market making strategy
│   ├── ArbitrageStrategy.cs            # Cross-pool arbitrage
│   └── StrategyManager.cs              # Manages multiple strategies
└── Utils/
    ├── AsyncHelper.cs                  # Thread-safe UI updates
    ├── RateLimiter.cs                  # API rate limiting
    └── RetryPolicy.cs                  # Exponential backoff retry
```

### Refactored Components

#### Api.cs → NiceHashClient.cs
- ✅ Convert to async/await pattern
- ✅ Proper exception handling with typed exceptions
- ✅ Rate limiting support
- ✅ Retry logic with exponential backoff
- ✅ Cancellation token support

#### ApiConnect.cs → NiceHashService.cs
- ✅ Dependency injection pattern
- ✅ Interface-based design
- ✅ Async operations throughout
- ✅ Better error handling
- ✅ Fix spelling errors (Enviorment → Environment)

#### Home.cs → MainForm.cs
- ✅ Remove thread safety violation
- ✅ Proper async UI updates with Invoke/BeginInvoke
- ✅ Strategy pattern for bot logic
- ✅ Separate business logic from UI
- ✅ Enable market aggregation

#### BotForm.cs → StrategyConfigForm.cs
- ✅ Enhanced configuration options
- ✅ Per-algorithm settings
- ✅ Strategy selection UI
- ✅ Validation and defaults

## Implementation Phases

### Phase 1: Core Infrastructure (Week 1)
1. ✅ Create project structure and interfaces
2. ✅ Implement Logger and ConfigManager
3. ✅ Refactor Api.cs to async NiceHashClient
4. ✅ Fix thread safety issues
5. ✅ Update dependencies to latest stable versions

### Phase 2: MiningRigRentals Integration (Week 1-2)
1. ✅ Implement MrrClient with full API v2 support
2. ✅ Create rental management UI
3. ✅ Add rig search and filtering
4. ✅ Implement automated rental creation

### Phase 3: Profitability Engine (Week 2-3)
1. ✅ Implement WhatToMineClient
2. ✅ Create ProfitabilityEngine for cross-pool analysis
3. ✅ Build ArbitrageDetector
4. ✅ Add profitability reporting UI

### Phase 4: Advanced Strategies (Week 3-4)
1. ✅ Implement BaseStrategy framework
2. ✅ Create OneShotStrategy for low-diff coins
3. ✅ Build ArbitrageStrategy for cross-pool opportunities
4. ✅ Add MarketMakerStrategy for competitive pricing
5. ✅ Implement StrategyManager for multi-strategy support

### Phase 5: Testing & Optimization (Week 4)
1. ✅ Unit tests for core components
2. ✅ Integration tests with testnet
3. ✅ Performance optimization
4. ✅ Documentation and examples

## Configuration Schema

### Enhanced bot.json
```json
{
  "version": "2.0",
  "general": {
    "updateInterval": 60,
    "enableLogging": true,
    "logLevel": "Info"
  },
  "niceHash": {
    "autoRefill": true,
    "refillThreshold": 0.9,
    "minRefillAmount": null,
    "dynamicPricing": true,
    "priceAdjustmentStrategy": "marketDepth"
  },
  "miningRigRentals": {
    "enabled": true,
    "autoRent": true,
    "maxRentalCost": 0.01,
    "preferredAlgorithms": ["SHA256", "Scrypt"],
    "redirectToPool": "nicehash"
  },
  "profitability": {
    "enableCrossPoolAnalysis": true,
    "whatToMineEnabled": true,
    "updateInterval": 300,
    "minProfitMargin": 0.05
  },
  "strategies": [
    {
      "name": "DefaultStrategy",
      "type": "MarketMaker",
      "enabled": true,
      "algorithms": ["SHA256"],
      "markets": ["USA"],
      "config": {
        "targetPosition": 3,
        "priceOffset": 0.005
      }
    },
    {
      "name": "OneShotScrypt",
      "type": "OneShot",
      "enabled": true,
      "algorithms": ["Scrypt"],
      "config": {
        "maxDifficulty": 1000000,
        "orderDuration": 3600
      }
    }
  ],
  "riskManagement": {
    "maxDailySpend": 0.1,
    "maxOrderSize": 0.01,
    "stopLossEnabled": true,
    "stopLossThreshold": 0.2
  }
}
```

## API Integration Details

### NiceHash API v2 Endpoints
- ✅ `/main/api/v2/hashpower/orderBook` - Market depth analysis
- ✅ `/main/api/v2/public/orders/active2` - Active orders by algorithm
- ✅ `/main/api/v2/hashpower/order` - Create/manage orders
- ✅ `/main/api/v2/hashpower/order/{id}/refill` - Refill orders
- ✅ `/main/api/v2/hashpower/order/{id}/updatePriceAndLimit` - Update pricing

### MiningRigRentals API v2 Endpoints
- ✅ `/rig/list` - Search available rigs
- ✅ `/rental` - Create rental
- ✅ `/rental/{id}` - Manage rental
- ✅ `/pool` - Configure mining pools
- ✅ `/account/balance` - Check balance

### WhatToMine API
- ✅ `/coins/{algorithm}.json` - Algorithm profitability
- ✅ `/calculators.json` - Available calculators

## Profitability Calculation Formula

### Cross-Pool Arbitrage
```
Profit = (PoolRevenue - HashCost - Fees) / HashCost

Where:
- PoolRevenue = (Hashrate × BlockReward × YourShare) / NetworkHashrate
- HashCost = NiceHashPrice × Hashrate × Duration
- Fees = PoolFee + WithdrawalFee + ExchangeFee
```

### MRR → NiceHash Arbitrage
```
Profit = (NHRevenue - MRRCost - Fees) / MRRCost

Where:
- NHRevenue = EstimatedEarnings from mining on NH
- MRRCost = RentalPrice × Hashrate × Duration
- Fees = PoolSetupFee + WithdrawalFee
```

## Success Metrics
- ✅ Automated profitability analysis across 3+ platforms
- ✅ Support for 20+ algorithms
- ✅ 99.9% uptime for order management
- ✅ <5% deviation from expected profitability
- ✅ Zero thread safety issues
- ✅ Comprehensive logging and monitoring

## Migration Guide
1. Backup existing `settings.json` and `bot.json`
2. Run migration tool to convert old config to new format
3. Test on NiceHash testnet first
4. Enable strategies one at a time
5. Monitor logs for errors
6. Gradually increase position sizes

## Resources
- NiceHash API Docs: https://docs.nicehash.com/
- MiningRigRentals API: https://www.miningrigrentals.com/apidocv2
- WhatToMine API: https://whattomine.com/api-docs
- C# Async Best Practices: https://docs.microsoft.com/en-us/dotnet/csharp/async
