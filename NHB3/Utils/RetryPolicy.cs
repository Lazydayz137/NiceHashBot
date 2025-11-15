using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NHB3.Core.Services;

namespace NHB3.Utils
{
    /// <summary>
    /// Retry policy with exponential backoff for failed API calls
    /// </summary>
    public class RetryPolicy
    {
        private readonly int _maxRetries;
        private readonly TimeSpan _initialDelay;
        private readonly TimeSpan _maxDelay;
        private readonly double _backoffMultiplier;

        public RetryPolicy(int maxRetries = 3, int initialDelayMs = 1000, int maxDelayMs = 30000, double backoffMultiplier = 2.0)
        {
            _maxRetries = maxRetries;
            _initialDelay = TimeSpan.FromMilliseconds(initialDelayMs);
            _maxDelay = TimeSpan.FromMilliseconds(maxDelayMs);
            _backoffMultiplier = backoffMultiplier;
        }

        /// <summary>
        /// Execute a function with retry logic
        /// </summary>
        public async Task<T> ExecuteAsync<T>(
            Func<Task<T>> action,
            CancellationToken cancellationToken = default,
            Func<Exception, bool> shouldRetry = null)
        {
            int attempt = 0;
            Exception lastException = null;

            while (attempt < _maxRetries)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;

                    // Check if we should retry this exception
                    if (shouldRetry != null && !shouldRetry(ex))
                    {
                        Logger.Instance.Warning($"Non-retryable exception on attempt {attempt}: {ex.Message}");
                        throw;
                    }

                    // Don't retry if we've exhausted attempts
                    if (attempt >= _maxRetries)
                    {
                        Logger.Instance.Error($"Max retries ({_maxRetries}) exceeded. Last error: {ex.Message}");
                        throw;
                    }

                    // Calculate delay with exponential backoff
                    var delay = CalculateDelay(attempt);
                    Logger.Instance.Warning($"Attempt {attempt}/{_maxRetries} failed: {ex.Message}. Retrying in {delay.TotalSeconds:F1}s...");

                    await Task.Delay(delay, cancellationToken);
                }
            }

            throw lastException ?? new Exception("Retry policy failed with no exception");
        }

        /// <summary>
        /// Execute an action with retry logic (no return value)
        /// </summary>
        public async Task ExecuteAsync(
            Func<Task> action,
            CancellationToken cancellationToken = default,
            Func<Exception, bool> shouldRetry = null)
        {
            await ExecuteAsync(async () =>
            {
                await action();
                return true; // Dummy return value
            }, cancellationToken, shouldRetry);
        }

        /// <summary>
        /// Calculate delay for the given attempt number
        /// </summary>
        private TimeSpan CalculateDelay(int attempt)
        {
            var delay = TimeSpan.FromMilliseconds(
                _initialDelay.TotalMilliseconds * Math.Pow(_backoffMultiplier, attempt - 1)
            );

            // Cap at max delay
            if (delay > _maxDelay)
            {
                delay = _maxDelay;
            }

            // Add jitter to prevent thundering herd
            var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, (int)(delay.TotalMilliseconds * 0.1)));
            return delay.Add(jitter);
        }

        /// <summary>
        /// Default retry predicate for HTTP errors
        /// </summary>
        public static bool ShouldRetryHttpError(Exception ex)
        {
            // Retry on network errors
            if (ex is System.Net.Http.HttpRequestException ||
                ex is System.Net.Sockets.SocketException ||
                ex is TaskCanceledException ||
                ex is TimeoutException)
            {
                return true;
            }

            // Retry on specific HTTP status codes
            if (ex is WebException webEx)
            {
                var response = webEx.Response as HttpWebResponse;
                if (response != null)
                {
                    var statusCode = (int)response.StatusCode;
                    // Retry on 429 (Too Many Requests), 502, 503, 504
                    return statusCode == 429 || statusCode >= 502;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Helper class to assist with async operations from synchronous contexts
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// Run async method synchronously (use sparingly - prefer async all the way)
        /// </summary>
        public static T RunSync<T>(Func<Task<T>> func)
        {
            return Task.Run(func).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Run async method synchronously (use sparingly - prefer async all the way)
        /// </summary>
        public static void RunSync(Func<Task> func)
        {
            Task.Run(func).GetAwaiter().GetResult();
        }
    }
}
