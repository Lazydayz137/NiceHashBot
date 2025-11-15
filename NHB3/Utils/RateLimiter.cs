using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NHB3.Utils
{
    /// <summary>
    /// Token bucket rate limiter for API calls
    /// </summary>
    public class RateLimiter
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;
        private readonly ConcurrentQueue<DateTime> _requestTimes;
        private readonly object _lockObject = new object();

        public RateLimiter(int maxRequests, TimeSpan timeWindow)
        {
            _maxRequests = maxRequests;
            _timeWindow = timeWindow;
            _semaphore = new SemaphoreSlim(1, 1);
            _requestTimes = new ConcurrentQueue<DateTime>();
        }

        /// <summary>
        /// Wait until a request slot is available
        /// </summary>
        public async Task WaitAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                while (true)
                {
                    var now = DateTime.UtcNow;

                    // Remove old requests outside the time window
                    while (_requestTimes.TryPeek(out DateTime oldestRequest))
                    {
                        if (now - oldestRequest > _timeWindow)
                        {
                            _requestTimes.TryDequeue(out _);
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Check if we can make a new request
                    if (_requestTimes.Count < _maxRequests)
                    {
                        _requestTimes.Enqueue(now);
                        return;
                    }

                    // Calculate wait time until the oldest request expires
                    if (_requestTimes.TryPeek(out DateTime oldest))
                    {
                        var waitTime = (_timeWindow - (now - oldest)).Add(TimeSpan.FromMilliseconds(100));
                        if (waitTime > TimeSpan.Zero)
                        {
                            await Task.Delay(waitTime, cancellationToken);
                        }
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Get current request count in the time window
        /// </summary>
        public int GetCurrentRequestCount()
        {
            var now = DateTime.UtcNow;
            int count = 0;

            foreach (var time in _requestTimes)
            {
                if (now - time <= _timeWindow)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
