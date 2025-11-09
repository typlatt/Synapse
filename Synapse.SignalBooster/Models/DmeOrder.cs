using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Synapse.SignalBooster.Models
{
    /// <summary>
    /// Represents a DME (Durable Medical Equipment) order from a physician note.
    /// </summary>
    public class DmeOrder
    {
        [JsonPropertyName("device")]
        public string Device { get; set; } = "Unknown";

        [JsonPropertyName("ordering_provider")]
        public string OrderingProvider { get; set; } = "Unknown";

        [JsonPropertyName("liters")]
        public string Liters { get; set; } = "";

        [JsonPropertyName("usage")]
        public string Usage { get; set; } = "";

        [JsonPropertyName("diagnosis")]
        public string Diagnosis { get; set; } = "";

        [JsonPropertyName("patient_name")]
        public string PatientName { get; set; } = "";

        [JsonPropertyName("dob")]
        public string Dob { get; set; } = "";
    }
}