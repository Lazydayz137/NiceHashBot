using System;
using System.Collections.Generic;
using System.Linq;

namespace NHB3.Profitability
{
    /// <summary>
    /// Predictive metrics for rental profitability forecasting
    /// Includes network stats, block times, reward trends
    /// </summary>
    public class PredictiveMetrics
    {
        // Network statistics
        public decimal NetworkHashrate { get; set; }
        public decimal NetworkHashrateTrend { get; set; } // % change over last 24h

        // Block statistics
        public decimal AverageBlockTime { get; set; } // seconds
        public decimal BlockTimeTrend { get; set; } // % change (negative = faster blocks)

        // Reward statistics
        public decimal CurrentBlockReward { get; set; }
        public decimal EstimatedRewardPer24h { get; set; }

        // Mining-Dutch specific trends
        public decimal ProfitabilityTrend { get; set; } // % change in last 24h
        public decimal EstimatedProfitability { get; set; } // mBTC/MH/day
        public decimal ActualProfitability { get; set; } // mBTC/MH/day

        // Confidence metrics
        public decimal ConfidenceScore { get; set; } // 0-100 (how reliable is prediction)
        public int SampleCount { get; set; } // Number of data points used

        // Volatility metrics
        public decimal PriceVolatility { get; set; } // Standard deviation of profitability
        public string VolatilityRating { get; set; } // Low/Medium/High

        // Predictions
        public decimal PredictedProfitIn1h { get; set; }
        public decimal PredictedProfitIn3h { get; set; }
        public decimal PredictedProfitIn6h { get; set; }
        public decimal PredictedProfitIn12h { get; set; }
        public decimal PredictedProfitIn24h { get; set; }

        // Risk assessment
        public string RiskLevel { get; set; } // Low/Medium/High
        public string RecommendedAction { get; set; } // Buy/Hold/Avoid
        public List<string> RiskFactors { get; set; } = new List<string>();

        // Market conditions
        public int ActiveWorkers { get; set; }
        public int WorkersTrend { get; set; } // Change in workers over 24h
        public decimal PoolHashrate { get; set; }
        public decimal PoolHashrateTrend { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Calculate confidence score based on data quality
        /// </summary>
        public void CalculateConfidence()
        {
            decimal confidence = 100m;

            // Reduce confidence if volatility is high
            if (VolatilityRating == "High")
                confidence -= 30m;
            else if (VolatilityRating == "Medium")
                confidence -= 15m;

            // Reduce confidence if sample count is low
            if (SampleCount < 10)
                confidence -= 20m;
            else if (SampleCount < 50)
                confidence -= 10m;

            // Reduce confidence if trends are unstable
            if (Math.Abs(ProfitabilityTrend) > 20m)
                confidence -= 15m;

            // Reduce confidence if worker count is low
            if (ActiveWorkers < 50)
                confidence -= 10m;

            ConfidenceScore = Math.Max(0, Math.Min(100, confidence));
        }

        /// <summary>
        /// Determine risk level based on metrics
        /// </summary>
        public void AssessRisk()
        {
            RiskFactors.Clear();
            int riskScore = 0;

            // High volatility = risk
            if (VolatilityRating == "High")
            {
                riskScore += 3;
                RiskFactors.Add("High price volatility");
            }
            else if (VolatilityRating == "Medium")
            {
                riskScore += 1;
            }

            // Declining profitability = risk
            if (ProfitabilityTrend < -10m)
            {
                riskScore += 2;
                RiskFactors.Add($"Profitability declining ({ProfitabilityTrend:F1}%)");
            }

            // Increasing network hashrate = risk (more competition)
            if (NetworkHashrateTrend > 20m)
            {
                riskScore += 2;
                RiskFactors.Add($"Network hashrate surging (+{NetworkHashrateTrend:F1}%)");
            }

            // Declining workers = risk (pool losing miners)
            if (WorkersTrend < -20)
            {
                riskScore += 1;
                RiskFactors.Add($"Pool workers declining ({WorkersTrend:+#;-#;0})");
            }

            // Low confidence = risk
            if (ConfidenceScore < 50m)
            {
                riskScore += 2;
                RiskFactors.Add($"Low confidence score ({ConfidenceScore:F0}%)");
            }

            // Determine risk level
            if (riskScore >= 5)
                RiskLevel = "High";
            else if (riskScore >= 3)
                RiskLevel = "Medium";
            else
                RiskLevel = "Low";

            // Recommend action
            if (RiskLevel == "High" || PredictedProfitIn6h < ActualProfitability * 0.8m)
                RecommendedAction = "Avoid";
            else if (RiskLevel == "Low" && PredictedProfitIn6h >= ActualProfitability * 1.1m)
                RecommendedAction = "Buy";
            else
                RecommendedAction = "Hold";
        }

        /// <summary>
        /// Simple linear prediction based on trend
        /// </summary>
        public void CalculatePredictions()
        {
            // Use trend to predict future profitability
            decimal hourlyChange = (ProfitabilityTrend / 24m); // % change per hour

            PredictedProfitIn1h = ActualProfitability * (1 + (hourlyChange / 100m));
            PredictedProfitIn3h = ActualProfitability * (1 + (hourlyChange * 3m / 100m));
            PredictedProfitIn6h = ActualProfitability * (1 + (hourlyChange * 6m / 100m));
            PredictedProfitIn12h = ActualProfitability * (1 + (hourlyChange * 12m / 100m));
            PredictedProfitIn24h = ActualProfitability * (1 + (hourlyChange * 24m / 100m));

            // Clamp predictions to reasonable values (don't go negative)
            PredictedProfitIn1h = Math.Max(0, PredictedProfitIn1h);
            PredictedProfitIn3h = Math.Max(0, PredictedProfitIn3h);
            PredictedProfitIn6h = Math.Max(0, PredictedProfitIn6h);
            PredictedProfitIn12h = Math.Max(0, PredictedProfitIn12h);
            PredictedProfitIn24h = Math.Max(0, PredictedProfitIn24h);
        }

        /// <summary>
        /// Get volatility rating from price volatility
        /// </summary>
        public void CalculateVolatility(List<decimal> profitabilityHistory)
        {
            if (profitabilityHistory == null || profitabilityHistory.Count < 2)
            {
                VolatilityRating = "Unknown";
                PriceVolatility = 0;
                return;
            }

            // Calculate standard deviation
            decimal mean = profitabilityHistory.Average();
            decimal sumSquaredDiff = profitabilityHistory.Sum(x => (x - mean) * (x - mean));
            PriceVolatility = (decimal)Math.Sqrt((double)(sumSquaredDiff / profitabilityHistory.Count));

            // Calculate coefficient of variation (CV)
            decimal cv = mean != 0 ? (PriceVolatility / mean) * 100m : 0;

            // Rate volatility
            if (cv < 5m)
                VolatilityRating = "Low";
            else if (cv < 15m)
                VolatilityRating = "Medium";
            else
                VolatilityRating = "High";

            SampleCount = profitabilityHistory.Count;
        }

        /// <summary>
        /// Full analysis: calculate all metrics
        /// </summary>
        public void PerformFullAnalysis(List<decimal> profitabilityHistory = null)
        {
            if (profitabilityHistory != null && profitabilityHistory.Count > 0)
            {
                CalculateVolatility(profitabilityHistory);
            }

            CalculatePredictions();
            CalculateConfidence();
            AssessRisk();
        }

        /// <summary>
        /// Get formatted summary
        /// </summary>
        public override string ToString()
        {
            return $"Confidence: {ConfidenceScore:F0}% | Risk: {RiskLevel} | Action: {RecommendedAction}";
        }
    }
}
