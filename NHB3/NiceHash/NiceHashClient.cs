using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHB3.Core.Interfaces;
using NHB3.Core.Services;
using NHB3.Utils;

namespace NHB3.NiceHash
{
    /// <summary>
    /// Modernized async NiceHash API v2 client with rate limiting and retry logic
    /// </summary>
    public class NiceHashClient : IApiClient, IDisposable
    {
        private readonly string _baseUrl;
        private readonly string _orgId;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly HttpClient _httpClient;
        private readonly RateLimiter _rateLimiter;
        private readonly RetryPolicy _retryPolicy;
        private string _cachedServerTime;
        private DateTime _serverTimeCachedAt;

        public string BaseUrl => _baseUrl;
        public string ServiceName => "NiceHash";

        public NiceHashClient(string baseUrl, string orgId, string apiKey, string apiSecret)
        {
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _orgId = orgId;
            _apiKey = apiKey;
            _apiSecret = apiSecret;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // NiceHash allows ~1000 requests per 10 minutes (conservative: 100/min)
            _rateLimiter = new RateLimiter(100, TimeSpan.FromMinutes(1));
            _retryPolicy = new RetryPolicy(maxRetries: 3, initialDelayMs: 2000, maxDelayMs: 30000);
        }

        /// <summary>
        /// Test API connectivity and authentication
        /// </summary>
        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await GetAsync<JObject>("/api/v2/time", requiresAuth: false, cancellationToken);
                return response != null && response["serverTime"] != null;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to test NiceHash connection");
                return false;
            }
        }

        /// <summary>
        /// Execute a GET request
        /// </summary>
        public async Task<T> GetAsync<T>(string endpoint, bool requiresAuth = false, CancellationToken cancellationToken = default)
        {
            await _rateLimiter.WaitAsync(cancellationToken);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

                if (requiresAuth)
                {
                    await AddAuthHeadersAsync(request, "GET", endpoint, null, cancellationToken);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                return await HandleResponseAsync<T>(response);
            }, cancellationToken, RetryPolicy.ShouldRetryHttpError);
        }

        /// <summary>
        /// Execute a POST request
        /// </summary>
        public async Task<T> PostAsync<T>(string endpoint, object payload = null, bool requiresAuth = false, CancellationToken cancellationToken = default)
        {
            await _rateLimiter.WaitAsync(cancellationToken);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                string bodyStr = null;

                if (payload != null)
                {
                    bodyStr = payload is string str ? str : JsonConvert.SerializeObject(payload);
                    request.Content = new StringContent(bodyStr, Encoding.UTF8, "application/json");
                }

                if (requiresAuth)
                {
                    await AddAuthHeadersAsync(request, "POST", endpoint, bodyStr, cancellationToken);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                return await HandleResponseAsync<T>(response);
            }, cancellationToken, RetryPolicy.ShouldRetryHttpError);
        }

        /// <summary>
        /// Execute a DELETE request
        /// </summary>
        public async Task<T> DeleteAsync<T>(string endpoint, bool requiresAuth = false, CancellationToken cancellationToken = default)
        {
            await _rateLimiter.WaitAsync(cancellationToken);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);

                if (requiresAuth)
                {
                    await AddAuthHeadersAsync(request, "DELETE", endpoint, null, cancellationToken);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                return await HandleResponseAsync<T>(response);
            }, cancellationToken, RetryPolicy.ShouldRetryHttpError);
        }

        /// <summary>
        /// Add NiceHash authentication headers to request
        /// </summary>
        private async Task AddAuthHeadersAsync(HttpRequestMessage request, string method, string url, string body, CancellationToken cancellationToken)
        {
            var time = await GetServerTimeAsync(cancellationToken);
            var nonce = Guid.NewGuid().ToString();
            var path = GetPath(url);
            var query = GetQuery(url);
            var digest = HashBySegments(_apiSecret, _apiKey, time, nonce, _orgId, method, path, query, body);

            request.Headers.Add("X-Time", time);
            request.Headers.Add("X-Nonce", nonce);
            request.Headers.Add("X-Auth", $"{_apiKey}:{digest}");
            request.Headers.Add("X-Organization-Id", _orgId);
            request.Headers.Add("X-Request-Id", Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Get server time (cached for 30 seconds to reduce API calls)
        /// </summary>
        private async Task<string> GetServerTimeAsync(CancellationToken cancellationToken)
        {
            // Cache server time for 30 seconds
            if (_cachedServerTime != null && (DateTime.UtcNow - _serverTimeCachedAt).TotalSeconds < 30)
            {
                return _cachedServerTime;
            }

            try
            {
                var response = await GetAsync<JObject>("/api/v2/time", requiresAuth: false, cancellationToken);
                if (response?["serverTime"] != null)
                {
                    _cachedServerTime = response["serverTime"].ToString();
                    _serverTimeCachedAt = DateTime.UtcNow;
                    return _cachedServerTime;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Warning($"Failed to get server time, using local time: {ex.Message}");
            }

            // Fallback to local time
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        }

        /// <summary>
        /// Handle HTTP response and deserialize
        /// </summary>
        private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            Logger.Instance.Debug($"[{response.StatusCode}] {response.RequestMessage.Method} {response.RequestMessage.RequestUri}");

            if (!response.IsSuccessStatusCode)
            {
                Logger.Instance.Error($"HTTP {response.StatusCode}: {content}");
                throw new NiceHashApiException($"API request failed with status {response.StatusCode}", (int)response.StatusCode, content);
            }

            // Check for API-level errors
            var json = JObject.Parse(content);
            if (json["error_id"] != null && json["error_id"].ToString() != "0")
            {
                var errorId = json["error_id"].ToString();
                var errorMessage = json["errors"]?.ToString() ?? "Unknown error";
                Logger.Instance.Error($"NiceHash API error {errorId}: {errorMessage}");
                throw new NiceHashApiException($"API error {errorId}: {errorMessage}", int.Parse(errorId), content);
            }

            // Deserialize response
            if (typeof(T) == typeof(string))
            {
                return (T)(object)content;
            }
            else if (typeof(T) == typeof(JObject) || typeof(T) == typeof(JToken))
            {
                return (T)(object)json;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
        }

        /// <summary>
        /// Hash request for NiceHash API authentication
        /// </summary>
        private static string HashBySegments(string key, string apiKey, string time, string nonce, string orgId, string method, string path, string query, string body)
        {
            var segments = new List<string>
            {
                apiKey,
                time,
                nonce,
                null,
                orgId,
                null,
                method,
                path,
                query
            };

            if (!string.IsNullOrEmpty(body))
            {
                segments.Add(body);
            }

            var input = JoinSegments(segments);
            return CalcHMACSHA256Hash(input, key);
        }

        /// <summary>
        /// Join segments with null delimiter
        /// </summary>
        private static string JoinSegments(List<string> segments)
        {
            var sb = new StringBuilder();
            bool first = true;

            foreach (var segment in segments)
            {
                if (!first)
                {
                    sb.Append("\x00");
                }
                else
                {
                    first = false;
                }

                if (segment != null)
                {
                    sb.Append(segment);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Calculate HMAC SHA256 hash
        /// </summary>
        private static string CalcHMACSHA256Hash(string plaintext, string salt)
        {
            var enc = Encoding.Default;
            var textBytes = enc.GetBytes(plaintext);
            var saltBytes = enc.GetBytes(salt);

            using (var hasher = new HMACSHA256(saltBytes))
            {
                var hashBytes = hasher.ComputeHash(textBytes);
                return string.Join("", hashBytes.Select(b => b.ToString("x2")));
            }
        }

        /// <summary>
        /// Extract path from URL
        /// </summary>
        private static string GetPath(string url)
        {
            var split = url.Split('?');
            return split[0];
        }

        /// <summary>
        /// Extract query string from URL
        /// </summary>
        private static string GetQuery(string url)
        {
            var split = url.Split('?');
            return split.Length > 1 ? split[1] : null;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Custom exception for NiceHash API errors
    /// </summary>
    public class NiceHashApiException : Exception
    {
        public int ErrorCode { get; }
        public string ResponseContent { get; }

        public NiceHashApiException(string message, int errorCode, string responseContent) : base(message)
        {
            ErrorCode = errorCode;
            ResponseContent = responseContent;
        }
    }
}
