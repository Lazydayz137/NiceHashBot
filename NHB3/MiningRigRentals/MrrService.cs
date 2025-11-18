using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NHB3.Core.Services;

namespace NHB3.MiningRigRentals
{
    /// <summary>
    /// High-level service for MiningRigRentals operations
    /// </summary>
    public class MrrService
    {
        private readonly MrrClient _client;

        public MrrService(MrrClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Search for available rigs by algorithm
        /// </summary>
        public async Task<List<RigListing>> SearchRigsAsync(
            string algorithm,
            decimal minHashrate = 0,
            decimal maxPrice = decimal.MaxValue,
            int limit = 100,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // MRR API endpoint: /rig/list
                var endpoint = $"/rig/list?type={algorithm}&minhashrate={minHashrate}&limit={limit}";
                var response = await _client.GetAsync<RigListResponse>(endpoint, requiresAuth: false, cancellationToken);

                if (!response.Success || response.Data?.Records == null)
                {
                    Logger.Instance.Warning($"No rigs found for {algorithm}");
                    return new List<RigListing>();
                }

                // Filter by price and availability
                var rigs = response.Data.Records
                    .Where(r => !r.Rented && r.Price <= maxPrice)
                    .OrderBy(r => r.Price)
                    .ToList();

                Logger.Instance.Info($"Found {rigs.Count} available rigs for {algorithm}");
                return rigs;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to search rigs for {algorithm}");
                return new List<RigListing>();
            }
        }

        /// <summary>
        /// Get cheapest available rig for an algorithm
        /// </summary>
        public async Task<RigListing> GetCheapestRigAsync(
            string algorithm,
            decimal minHashrate = 0,
            CancellationToken cancellationToken = default)
        {
            var rigs = await SearchRigsAsync(algorithm, minHashrate, cancellationToken: cancellationToken);
            return rigs.FirstOrDefault();
        }

        /// <summary>
        /// Get average rental price for an algorithm
        /// </summary>
        public async Task<decimal> GetAveragePriceAsync(
            string algorithm,
            int sampleSize = 50,
            CancellationToken cancellationToken = default)
        {
            var rigs = await SearchRigsAsync(algorithm, limit: sampleSize, cancellationToken: cancellationToken);

            if (!rigs.Any())
                return 0m;

            return rigs.Average(r => r.Price);
        }

        /// <summary>
        /// Create a rental
        /// </summary>
        public async Task<Rental> CreateRentalAsync(
            int rigId,
            decimal lengthHours,
            MrrPool pool,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    rig = rigId,
                    length = lengthHours,
                    profile = new
                    {
                        host = pool.Host,
                        port = pool.Port,
                        user = pool.User,
                        pass = pool.Pass
                    }
                };

                var response = await _client.PostAsync<JObject>("/rental", payload, requiresAuth: true, cancellationToken);

                if (response["success"]?.ToString() == "true")
                {
                    Logger.Instance.Info($"Created rental for rig {rigId} ({lengthHours}h)");
                    return response["data"].ToObject<Rental>();
                }

                Logger.Instance.Error($"Failed to create rental: {response}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, $"Failed to create rental for rig {rigId}");
                return null;
            }
        }

        /// <summary>
        /// Get my active rentals
        /// </summary>
        public async Task<List<Rental>> GetMyRentalsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.GetAsync<JObject>("/rental", requiresAuth: true, cancellationToken);

                if (response["success"]?.ToString() == "true")
                {
                    return response["data"]["records"].ToObject<List<Rental>>();
                }

                return new List<Rental>();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to get rentals");
                return new List<Rental>();
            }
        }
    }
}
