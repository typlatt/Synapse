using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Synapse.SignalBooster.Models
{
    /// <summary>
    /// Represents extracted DME information from a physician note.
    /// </summary>
    public class DmeExtraction
    {
        [JsonPropertyName("device")]
        public string Device { get; set; } = "Unknown";

        [JsonPropertyName("mask_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MaskType { get; set; }

        [JsonPropertyName("add_ons")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? AddOns { get; set; }

        [JsonPropertyName("qualifier")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Qualifier { get; set; }

        [JsonPropertyName("ordering_provider")]
        public string OrderingProvider { get; set; } = "Unknown";

        [JsonPropertyName("liters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Liters { get; set; }

        [JsonPropertyName("usage")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Usage { get; set; }

        [JsonPropertyName("diagnosis")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Diagnosis { get; set; }

        [JsonPropertyName("patient_name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PatientName { get; set; }

        [JsonPropertyName("dob")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Dob { get; set; }

        public DmeExtraction()
        {
            AddOns = new List<string>();
        }
    }
}