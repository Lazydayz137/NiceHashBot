using System;
using System.Threading;
using System.Threading.Tasks;
using NHB3.Core.Interfaces;
using NHB3.Core.Models;
using NHB3.Core.Services;

namespace NHB3.Strategies
{
    /// <summary>
    /// Base class for all trading strategies
    /// </summary>
    public abstract class BaseStrategy : IStrategy
    {
        protected StrategyConfig Config { get; private set; }
        protected StrategyStatus Status { get; private set; }

        public string Name { get; protected set; }
        public abstract StrategyType Type { get; }
        public bool IsEnabled { get; set; }

        protected BaseStrategy(string name)
        {
            Name = name;
            Status = new StrategyStatus
            {
                StrategyName = name,
                IsRunning = false
            };
        }

        public virtual Task InitializeAsync(StrategyConfig config, CancellationToken cancellationToken = default)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            IsEnabled = config.Enabled;
            Logger.Instance.Info($"Initialized strategy: {Name}");
            return Task.CompletedTask;
        }

        public abstract Task<StrategyExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default);

        public virtual bool ValidateConfig(StrategyConfig config, out string[] errors)
        {
            var errorList = new System.Collections.Generic.List<string>();

            if (string.IsNullOrEmpty(config.Name))
                errorList.Add("Strategy name is required");

            if (config.Algorithms == null || config.Algorithms.Count == 0)
                errorList.Add("At least one algorithm must be specified");

            errors = errorList.ToArray();
            return errorList.Count == 0;
        }

        public virtual StrategyStatus GetStatus()
        {
            return Status;
        }

        public virtual Task StopAsync(CancellationToken cancellationToken = default)
        {
            Status.IsRunning = false;
            Logger.Instance.Info($"Stopped strategy: {Name}");
            return Task.CompletedTask;
        }

        protected void UpdateStatus(Action<StrategyStatus> updateAction)
        {
            updateAction(Status);
            Status.LastExecuted = DateTime.UtcNow;
        }
    }
}
