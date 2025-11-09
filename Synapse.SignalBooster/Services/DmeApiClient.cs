using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Synapse.SignalBooster.Models;

namespace Synapse.SignalBooster.Services
{
    /// <summary>
    /// Client for submitting DME extractions to external API.
    /// </summary>
    public class DmeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public DmeApiClient(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Submits a DME extraction to the specified API endpoint.
        /// </summary>
        /// <param name="endpoint">API endpoint URL</param>
        /// <param name="extraction">DME extraction data to submit</param>
        /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
        public async Task SubmitExtractionAsync(string endpoint, DmeExtraction extraction)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint cannot be empty", nameof(endpoint));
            if (extraction == null)
                throw new ArgumentNullException(nameof(extraction));

            string jsonContent = JsonSerializer.Serialize(extraction, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogDebug("Submitting to {Endpoint}: {Json}", endpoint, jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "API request failed with status {StatusCode}: {ErrorBody}",
                    response.StatusCode,
                    errorBody
                );
                response.EnsureSuccessStatusCode();
            }

            _logger.LogInformation("Successfully submitted extraction to API");
        }
    }
}