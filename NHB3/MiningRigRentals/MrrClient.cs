using System;
using System.Collections.Generic;
using System.Linq;
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

namespace NHB3.MiningRigRentals
{
    /// <summary>
    /// MiningRigRentals API v2 client
    /// </summary>
    public class MrrClient : IApiClient, IDisposable
    {
        private const string BASE_URL = "https://www.miningrigrentals.com/api/v2";
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly HttpClient _httpClient;
        private readonly RateLimiter _rateLimiter;
        private readonly RetryPolicy _retryPolicy;

        public string BaseUrl => BASE_URL;
        public string ServiceName => "MiningRigRentals";

        public MrrClient(string apiKey, string apiSecret)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // MRR allows 100 requests per minute
            _rateLimiter = new RateLimiter(100, TimeSpan.FromMinutes(1));
            _retryPolicy = new RetryPolicy(maxRetries: 3, initialDelayMs: 2000);
        }

        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await GetAsync<JObject>("/info/me", requiresAuth: true, cancellationToken);
                return response != null && response["success"]?.ToString() == "true";
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to test MRR connection");
                return false;
            }
        }

        public async Task<T> GetAsync<T>(string endpoint, bool requiresAuth = false, CancellationToken cancellationToken = default)
        {
            await _rateLimiter.WaitAsync(cancellationToken);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

                if (requiresAuth)
                {
                    AddAuthHeaders(request, endpoint);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                return await HandleResponseAsync<T>(response);
            }, cancellationToken, RetryPolicy.ShouldRetryHttpError);
        }

        public async Task<T> PostAsync<T>(string endpoint, object payload = null, bool requiresAuth = false, CancellationToken cancellationToken = default)
        {
            await _rateLimiter.WaitAsync(cancellationToken);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

                if (payload != null)
                {
                    var json = payload is string str ? str : JsonConvert.SerializeObject(payload);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                if (requiresAuth)
                {
                    AddAuthHeaders(request, endpoint);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                return await HandleResponseAsync<T>(response);
            }, cancellationToken, RetryPolicy.ShouldRetryHttpError);
        }

        public async Task<T> DeleteAsync<T>(string endpoint, bool requiresAuth = false, CancellationToken cancellationToken = default)
        {
            await _rateLimiter.WaitAsync(cancellationToken);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);

                if (requiresAuth)
                {
                    AddAuthHeaders(request, endpoint);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                return await HandleResponseAsync<T>(response);
            }, cancellationToken, RetryPolicy.ShouldRetryHttpError);
        }

        private void AddAuthHeaders(HttpRequestMessage request, string endpoint)
        {
            var nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var signature = CalculateSignature(endpoint, nonce);

            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("x-api-sign", signature);
            request.Headers.Add("x-api-nonce", nonce);
        }

        private string CalculateSignature(string endpoint, string nonce)
        {
            var message = $"{_apiKey}{nonce}{endpoint}";
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_apiSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Logger.Instance.Error($"MRR API error [{response.StatusCode}]: {content}");
                throw new Exception($"MRR API request failed with status {response.StatusCode}");
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)content;
            }
            else if (typeof(T) == typeof(JObject))
            {
                return (T)(object)JObject.Parse(content);
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
