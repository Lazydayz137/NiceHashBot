using System;
using System.Threading;
using System.Threading.Tasks;

namespace NHB3.Core.Interfaces
{
    /// <summary>
    /// Generic API client interface for external services
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Execute a GET request
        /// </summary>
        Task<T> GetAsync<T>(string endpoint, bool requiresAuth = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute a POST request
        /// </summary>
        Task<T> PostAsync<T>(string endpoint, object payload = null, bool requiresAuth = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute a DELETE request
        /// </summary>
        Task<T> DeleteAsync<T>(string endpoint, bool requiresAuth = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Test API connectivity and authentication
        /// </summary>
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the base URL for this API client
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Get the service name for this API client
        /// </summary>
        string ServiceName { get; }
    }
}
