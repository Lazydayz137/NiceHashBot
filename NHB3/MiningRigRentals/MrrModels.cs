using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NHB3.MiningRigRentals
{
    /// <summary>
    /// MiningRigRentals rig listing
    /// </summary>
    public class RigListing
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } // "SHA256", "Scrypt", etc.

        [JsonProperty("status")]
        public string Status { get; set; } // "available", "rented", etc.

        [JsonProperty("price")]
        public decimal Price { get; set; } // BTC per MH per day

        [JsonProperty("hashrate")]
        public decimal Hashrate { get; set; } // In MH

        [JsonProperty("minhours")]
        public decimal MinHours { get; set; }

        [JsonProperty("maxhours")]
        public decimal MaxHours { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; } // "eu", "us", "asia", etc.

        [JsonProperty("rating")]
        public decimal Rating { get; set; }

        [JsonProperty("rented")]
        public bool Rented { get; set; }

        [JsonProperty("available_in_hours")]
        public decimal AvailableInHours { get; set; }
    }

    /// <summary>
    /// MRR rental
    /// </summary>
    public class Rental
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("rig")]
        public int RigId { get; set; }

        [JsonProperty("length")]
        public decimal Length { get; set; } // In hours

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; } // BTC per MH per day

        [JsonProperty("hashrate")]
        public decimal Hashrate { get; set; }

        [JsonProperty("cost")]
        public decimal Cost { get; set; } // Total rental cost in BTC

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    /// <summary>
    /// MRR algorithm info
    /// </summary>
    public class MrrAlgorithm
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("display")]
        public string Display { get; set; }

        [JsonProperty("suggested_diff")]
        public decimal SuggestedDiff { get; set; }
    }

    /// <summary>
    /// MRR pool configuration
    /// </summary>
    public class MrrPool
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("pass")]
        public string Pass { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Rig search/list response
    /// </summary>
    public class RigListResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public RigListData Data { get; set; }
    }

    public class RigListData
    {
        [JsonProperty("records")]
        public List<RigListing> Records { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
