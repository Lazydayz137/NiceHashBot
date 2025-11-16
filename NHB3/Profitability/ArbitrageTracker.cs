using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NHB3.Core.Services;

namespace NHB3.Profitability
{
    /// <summary>
    /// Tracks arbitrage execution history and performance analytics
    /// </summary>
    public class ArbitrageTracker
    {
        private readonly string _historyFile;
        private readonly object _lockObject = new object();
        private List<ArbitrageExecution> _executionHistory;

        public ArbitrageTracker(string historyFile = "arbitrage_history.json")
        {
            _historyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, historyFile);
            _executionHistory = new List<ArbitrageExecution>();
            LoadHistory();
        }

        /// <summary>
        /// Log a new arbitrage execution
        /// </summary>
        public void LogExecution(ArbitrageExecution execution)
        {
            if (execution == null) return;

            lock (_lockObject)
            {
                _executionHistory.Add(execution);
                SaveHistory();

                Logger.Instance.Info($"Logged arbitrage execution: {execution.Algorithm} - {execution.Type} - {execution.Status}");
            }
        }

        /// <summary>
        /// Update an existing execution with actual results
        /// </summary>
        public void UpdateExecution(string orderId, decimal actualProfit, decimal actualMargin, string status = "Completed")
        {
            lock (_lockObject)
            {
                var execution = _executionHistory.FirstOrDefault(e => e.OrderId == orderId || e.RentalId == orderId);

                if (execution != null)
                {
                    execution.ActualProfit = actualProfit;
                    execution.ActualMargin = actualMargin;
                    execution.Status = status;
                    execution.CompletedAt = DateTime.UtcNow;

                    SaveHistory();

                    Logger.Instance.Info($"Updated execution {orderId}: {actualMargin:F2}% margin ({actualProfit:F8} BTC profit)");
                }
            }
        }

        /// <summary>
        /// Get recent executions
        /// </summary>
        public List<ArbitrageExecution> GetRecentExecutions(int count = 50)
        {
            lock (_lockObject)
            {
                return _executionHistory
                    .OrderByDescending(e => e.Timestamp)
                    .Take(count)
                    .ToList();
            }
        }

        /// <summary>
        /// Get executions for a specific algorithm
        /// </summary>
        public List<ArbitrageExecution> GetExecutionsByAlgorithm(string algorithm)
        {
            lock (_lockObject)
            {
                return _executionHistory
                    .Where(e => e.Algorithm.Equals(algorithm, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(e => e.Timestamp)
                    .ToList();
            }
        }

        /// <summary>
        /// Get executions within a date range
        /// </summary>
        public List<ArbitrageExecution> GetExecutionsByDateRange(DateTime startDate, DateTime endDate)
        {
            lock (_lockObject)
            {
                return _executionHistory
                    .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
                    .OrderByDescending(e => e.Timestamp)
                    .ToList();
            }
        }

        /// <summary>
        /// Get comprehensive performance statistics
        /// </summary>
        public ArbitrageStatistics GetStatistics(DateTime? startDate = null, DateTime? endDate = null)
        {
            lock (_lockObject)
            {
                var executions = _executionHistory.AsEnumerable();

                if (startDate.HasValue)
                    executions = executions.Where(e => e.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    executions = executions.Where(e => e.Timestamp <= endDate.Value);

                var executionList = executions.ToList();
                var successfulExecutions = executionList.Where(e => e.Success).ToList();
                var completedExecutions = successfulExecutions.Where(e => e.ActualProfit.HasValue).ToList();

                return new ArbitrageStatistics
                {
                    TotalExecutions = executionList.Count,
                    SuccessfulExecutions = successfulExecutions.Count,
                    FailedExecutions = executionList.Count - successfulExecutions.Count,
                    SuccessRate = executionList.Count > 0 ? (successfulExecutions.Count / (decimal)executionList.Count) * 100 : 0,

                    TotalExpectedProfit = successfulExecutions.Sum(e => e.ExpectedProfit),
                    TotalActualProfit = completedExecutions.Sum(e => e.ActualProfit ?? 0),

                    AverageExpectedMargin = successfulExecutions.Any() ? successfulExecutions.Average(e => e.ExpectedMargin) : 0,
                    AverageActualMargin = completedExecutions.Any() ? completedExecutions.Average(e => e.ActualMargin ?? 0) : 0,

                    AverageMargin = completedExecutions.Any() ? completedExecutions.Average(e => e.ActualMargin ?? 0) : 0,

                    BestTrade = completedExecutions.OrderByDescending(e => e.ActualMargin).FirstOrDefault(),
                    WorstTrade = completedExecutions.OrderBy(e => e.ActualMargin).FirstOrDefault(),

                    // Per-algorithm breakdown
                    PerformanceByAlgorithm = executionList
                        .GroupBy(e => e.Algorithm)
                        .ToDictionary(
                            g => g.Key,
                            g => new AlgorithmPerformance
                            {
                                TotalExecutions = g.Count(),
                                SuccessfulExecutions = g.Count(e => e.Success),
                                AverageMargin = g.Where(e => e.ActualMargin.HasValue).Any()
                                    ? g.Where(e => e.ActualMargin.HasValue).Average(e => e.ActualMargin.Value)
                                    : 0,
                                TotalProfit = g.Where(e => e.ActualProfit.HasValue).Sum(e => e.ActualProfit.Value)
                            }
                        ),

                    // Per-type breakdown
                    PerformanceByType = executionList
                        .GroupBy(e => e.Type)
                        .ToDictionary(
                            g => g.Key,
                            g => new TypePerformance
                            {
                                TotalExecutions = g.Count(),
                                SuccessfulExecutions = g.Count(e => e.Success),
                                AverageMargin = g.Where(e => e.ActualMargin.HasValue).Any()
                                    ? g.Where(e => e.ActualMargin.HasValue).Average(e => e.ActualMargin.Value)
                                    : 0,
                                TotalProfit = g.Where(e => e.ActualProfit.HasValue).Sum(e => e.ActualProfit.Value)
                            }
                        ),

                    // Time-based statistics
                    LastDayProfit = executionList
                        .Where(e => e.Timestamp >= DateTime.UtcNow.AddDays(-1) && e.ActualProfit.HasValue)
                        .Sum(e => e.ActualProfit.Value),

                    LastWeekProfit = executionList
                        .Where(e => e.Timestamp >= DateTime.UtcNow.AddDays(-7) && e.ActualProfit.HasValue)
                        .Sum(e => e.ActualProfit.Value),

                    LastMonthProfit = executionList
                        .Where(e => e.Timestamp >= DateTime.UtcNow.AddDays(-30) && e.ActualProfit.HasValue)
                        .Sum(e => e.ActualProfit.Value),

                    // Accuracy metrics
                    ProfitAccuracy = completedExecutions.Any()
                        ? completedExecutions.Average(e =>
                            e.ExpectedProfit > 0
                                ? ((e.ActualProfit ?? 0) / e.ExpectedProfit) * 100
                                : 0)
                        : 0,

                    MarginAccuracy = completedExecutions.Any()
                        ? completedExecutions.Average(e =>
                            e.ExpectedMargin > 0
                                ? ((e.ActualMargin ?? 0) / e.ExpectedMargin) * 100
                                : 0)
                        : 0
                };
            }
        }

        /// <summary>
        /// Get daily profit summary
        /// </summary>
        public Dictionary<DateTime, decimal> GetDailyProfits(int days = 30)
        {
            lock (_lockObject)
            {
                var startDate = DateTime.UtcNow.Date.AddDays(-days);

                return _executionHistory
                    .Where(e => e.Timestamp >= startDate && e.ActualProfit.HasValue)
                    .GroupBy(e => e.Timestamp.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(e => e.ActualProfit.Value)
                    );
            }
        }

        /// <summary>
        /// Export history to CSV
        /// </summary>
        public void ExportToCsv(string filePath)
        {
            lock (_lockObject)
            {
                var lines = new List<string>
                {
                    "Timestamp,Algorithm,Type,Expected Margin,Actual Margin,Expected Profit,Actual Profit,Status,Order/Rental ID"
                };

                foreach (var exec in _executionHistory.OrderBy(e => e.Timestamp))
                {
                    lines.Add($"{exec.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                             $"{exec.Algorithm}," +
                             $"{exec.Type}," +
                             $"{exec.ExpectedMargin:F2}," +
                             $"{exec.ActualMargin?.ToString("F2") ?? "N/A"}," +
                             $"{exec.ExpectedProfit:F8}," +
                             $"{exec.ActualProfit?.ToString("F8") ?? "N/A"}," +
                             $"{exec.Status}," +
                             $"{exec.OrderId ?? exec.RentalId ?? "N/A"}");
                }

                File.WriteAllLines(filePath, lines);
                Logger.Instance.Info($"Exported {_executionHistory.Count} executions to {filePath}");
            }
        }

        /// <summary>
        /// Clear old history (keep last N days)
        /// </summary>
        public void CleanupOldHistory(int keepDays = 90)
        {
            lock (_lockObject)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-keepDays);
                var countBefore = _executionHistory.Count;

                _executionHistory = _executionHistory
                    .Where(e => e.Timestamp >= cutoffDate)
                    .ToList();

                var removed = countBefore - _executionHistory.Count;
                if (removed > 0)
                {
                    SaveHistory();
                    Logger.Instance.Info($"Cleaned up {removed} old executions (older than {keepDays} days)");
                }
            }
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(_historyFile))
                {
                    var json = File.ReadAllText(_historyFile);
                    _executionHistory = JsonConvert.DeserializeObject<List<ArbitrageExecution>>(json)
                        ?? new List<ArbitrageExecution>();

                    Logger.Instance.Info($"Loaded {_executionHistory.Count} arbitrage executions from history");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to load arbitrage history");
                _executionHistory = new List<ArbitrageExecution>();
            }
        }

        private void SaveHistory()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_executionHistory, Formatting.Indented);
                File.WriteAllText(_historyFile, json);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to save arbitrage history");
            }
        }
    }

    /// <summary>
    /// Comprehensive arbitrage performance statistics
    /// </summary>
    public class ArbitrageStatistics
    {
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public decimal SuccessRate { get; set; }

        public decimal TotalExpectedProfit { get; set; }
        public decimal TotalActualProfit { get; set; }

        public decimal AverageExpectedMargin { get; set; }
        public decimal AverageActualMargin { get; set; }
        public decimal AverageMargin { get; set; }

        public ArbitrageExecution BestTrade { get; set; }
        public ArbitrageExecution WorstTrade { get; set; }

        public Dictionary<string, AlgorithmPerformance> PerformanceByAlgorithm { get; set; }
        public Dictionary<string, TypePerformance> PerformanceByType { get; set; }

        public decimal LastDayProfit { get; set; }
        public decimal LastWeekProfit { get; set; }
        public decimal LastMonthProfit { get; set; }

        public decimal ProfitAccuracy { get; set; } // % of expected profit achieved
        public decimal MarginAccuracy { get; set; } // % of expected margin achieved
    }

    public class AlgorithmPerformance
    {
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public decimal AverageMargin { get; set; }
        public decimal TotalProfit { get; set; }
    }

    public class TypePerformance
    {
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public decimal AverageMargin { get; set; }
        public decimal TotalProfit { get; set; }
    }
}
