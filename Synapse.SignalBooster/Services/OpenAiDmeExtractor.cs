using System;
using System.ClientModel;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using Synapse.SignalBooster.Models;

namespace Synapse.SignalBooster.Services
{
    /// <summary>
    /// Extracts DME information from physician notes using OpenAI's API.
    /// </summary>
    public class OpenAiDmeExtractor
    {
        private readonly ChatClient _chatClient;
        private readonly ILogger _logger;

        public OpenAiDmeExtractor(string apiKey, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("OpenAI API key cannot be empty", nameof(apiKey));
            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _chatClient = new ChatClient("gpt-4o-mini", new ApiKeyCredential(apiKey));
        }

        /// <summary>
        /// Extracts DME requirements from a physician note using OpenAI.
        /// </summary>
        /// <param name="noteContent">The raw physician note text</param>
        /// <returns>Structured DME extraction data</returns>
        public async Task<DmeExtraction> ExtractAsync(string noteContent)
        {
            if (string.IsNullOrWhiteSpace(noteContent))
            {
                throw new ArgumentException("Note content cannot be empty", nameof(noteContent));
            }

            string systemPrompt = @"You are a medical document extraction specialist. Extract DME (Durable Medical Equipment) information from physician notes.

Extract the following information in JSON format:
- device: The type of medical equipment (e.g., ""CPAP"", ""Oxygen Tank"", ""Wheelchair"", or ""Unknown"")
- mask_type: For CPAP, the type of mask (e.g., ""full face"", ""nasal"")
- add_ons: Array of additional equipment (e.g., [""humidifier""])
- qualifier: Any qualifying medical information (e.g., ""AHI > 20"")
- ordering_provider: The doctor's name (e.g., ""Dr. Smith"")
- liters: For oxygen, the flow rate (e.g., ""2 L"")
- usage: For oxygen, when it's used (e.g., ""sleep and exertion"")
- diagnosis: The patient's diagnosis (e.g., ""COPD"", ""Severe sleep apnea"")
- patient_name: The patient's full name
- dob: The patient's date of birth in the format found in the note

Return ONLY valid JSON. Omit null or empty fields.";

            string userPrompt = $"Extract DME information from this physician note:\n\n{noteContent}";

            try
            {
                _logger.LogDebug("Sending request to OpenAI API");

                ChatCompletion completion = await _chatClient.CompleteChatAsync(
                    new ChatMessage[]
                    {
                        new SystemChatMessage(systemPrompt),
                        new UserChatMessage(userPrompt)
                    }
                );

                string responseContent = completion.Content[0].Text;
                _logger.LogDebug("Received response from OpenAI: {Response}", responseContent);

                // Parse JSON response
                var extraction = JsonSerializer.Deserialize<DmeExtraction>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (extraction == null)
                {
                    _logger.LogWarning("Failed to deserialize OpenAI response, returning default extraction");
                    return new DmeExtraction();
                }

                _logger.LogInformation("Successfully extracted DME using OpenAI: {Device}", extraction.Device);
                return extraction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API");
                throw;
            }
        }
    }
}
