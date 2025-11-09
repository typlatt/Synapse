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
    /// Client for submitting DME orders to external API.
    /// </summary>
    public class DmeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DmeApiClient> _logger;

        public DmeApiClient(HttpClient httpClient, ILogger<DmeApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Submits a DME order to the specified API endpoint.
        /// </summary>
        /// <param name="endpoint">API endpoint URL</param>
        /// <param name="order">DME order data to submit</param>
        /// <returns>Result indicating success or failure with error message</returns>
        public async Task<Result<bool>> SubmitExtractionAsync(string endpoint, DmeOrder order)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return Result<bool>.Failure("Endpoint cannot be empty");
            
            if (order == null)
                return Result<bool>.Failure("Order cannot be null");

            try
            {
                string jsonContent = JsonSerializer.Serialize(order, new JsonSerializerOptions
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
                    return Result<bool>.Failure($"API request failed with status {response.StatusCode}: {errorBody}");
                }

                _logger.LogInformation("Successfully submitted extraction to API");
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting to API");
                return Result<bool>.Failure($"Error submitting to API: {ex.Message}");
            }
        }
    }
}