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
        private readonly ILogger<OpenAiDmeExtractor> _logger;
        private readonly string _systemPrompt;
        private readonly string _jsonSchema;

        private const string DefaultSystemPrompt = @"You are a medical document extraction specialist. Extract DME (Durable Medical Equipment) information from physician notes.

        Use the following information as extraction guidelines:
        - device: The type of medical equipment from the Prescription field (e.g., ""CPAP"", ""Oxygen Tank"", ""Wheelchair"", ""Blood glucose monitoring kit"", ""Diabetic shoes"")
        - liters: The flow rate if specified (e.g., ""2 L""), or empty string if not applicable
        - usage: When/how the equipment is used (e.g., ""sleep and exertion"", ""3 times daily""), or empty string if not specified
        - diagnosis: The patient's diagnosis (e.g., ""COPD"", ""Type 2 Diabetes Mellitus"")
        - ordering_provider: The doctor's name (e.g., ""Dr. Smith"", ""Dr. House"")
        - patient_name: The patient's full name
        - dob: The patient's date of birth in the format found in the note";

        private const string DefaultJsonSchema = """
        {
            "type": "object",
            "properties": {
                "device": {
                    "type": "string",
                    "description": "The type of medical equipment"
                },
                "liters": {
                    "type": "string",
                    "description": "For oxygen, the flow rate (e.g., '2 L'), or empty string if not applicable"
                },
                "usage": {
                    "type": "string",
                    "description": "When/how the equipment is used (e.g., 'sleep and exertion', '3 times daily'), or empty string if not specified"
                },
                "diagnosis": {
                    "type": "string",
                    "description": "The patient's diagnosis"
                },
                "ordering_provider": {
                    "type": "string",
                    "description": "The doctor's name"
                },
                "patient_name": {
                    "type": "string",
                    "description": "The patient's full name"
                },
                "dob": {
                    "type": "string",
                    "description": "The patient's date of birth"
                }
            },
            "required": ["device", "ordering_provider", "patient_name", "dob", "diagnosis", "liters", "usage"],
            "additionalProperties": false
        }
        """;

        public OpenAiDmeExtractor(string apiKey, ILogger<OpenAiDmeExtractor> logger, string? model = null, string? systemPrompt = null, string? jsonSchema = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("OpenAI API key cannot be empty", nameof(apiKey));
            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            string modelToUse = string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini" : model;
            _chatClient = new ChatClient(modelToUse, new ApiKeyCredential(apiKey));
            _systemPrompt = string.IsNullOrWhiteSpace(systemPrompt) ? DefaultSystemPrompt : systemPrompt;
            _jsonSchema = string.IsNullOrWhiteSpace(jsonSchema) ? DefaultJsonSchema : jsonSchema;
            
            _logger.LogInformation("OpenAI extractor initialized with model: {Model}", modelToUse);
        }

        /// <summary>
        /// Extracts DME requirements from a physician note using OpenAI.
        /// </summary>
        /// <param name="noteContent">The raw physician note text</param>
        /// <returns>Result containing structured DME order data or an error message</returns>
        public async Task<Result<DmeOrder>> ExtractAsync(string noteContent)
        {
            if (string.IsNullOrWhiteSpace(noteContent))
            {
                _logger.LogWarning("Attempted to extract DME from empty note content");
                return Result<DmeOrder>.Failure("Note content cannot be empty");
            }

            string userPrompt = $"Extract DME information from this physician note:\n\n{noteContent}";
            _logger.LogDebug("Preparing OpenAI request with {CharCount} characters of note content", noteContent.Length);

            try
            {
                _logger.LogDebug("Sending request to OpenAI API with structured JSON schema");

                var requestStartTime = DateTime.UtcNow;

                // Create BinaryData from the JSON schema string
                var schemaBinaryData = BinaryData.FromString(_jsonSchema);

                var options = new ChatCompletionOptions
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "dme_order",
                        jsonSchema: schemaBinaryData,
                        jsonSchemaIsStrict: true
                    )
                };

                ChatCompletion completion = await _chatClient.CompleteChatAsync(
                    new ChatMessage[]
                    {
                        new SystemChatMessage(_systemPrompt),
                        new UserChatMessage(userPrompt)
                    },
                    options
                );

                var requestDuration = DateTime.UtcNow - requestStartTime;
                _logger.LogInformation("OpenAI API request completed in {DurationMs}ms", requestDuration.TotalMilliseconds);

                string responseContent = completion.Content[0].Text;
                _logger.LogDebug("Received {CharCount} characters from OpenAI", responseContent.Length);

                // Validate we have content
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    _logger.LogError("OpenAI returned empty response");
                    return Result<DmeOrder>.Failure("OpenAI returned empty response");
                }

                _logger.LogDebug("OpenAI response content: {Response}", responseContent);

                // Parse JSON response
                var order = JsonSerializer.Deserialize<DmeOrder>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (order == null)
                {
                    _logger.LogWarning("Failed to deserialize OpenAI response to DmeOrder object");
                    return Result<DmeOrder>.Failure("Failed to deserialize OpenAI response");
                }

                _logger.LogInformation("Successfully extracted DME: Device={Device}, Patient={PatientName}, Provider={Provider}", 
                    order.Device, 
                    order.PatientName, 
                    order.OrderingProvider);
                return Result<DmeOrder>.Success(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API. Exception type: {ExceptionType}", ex.GetType().Name);
                return Result<DmeOrder>.Failure($"Error calling OpenAI API: {ex.Message}");
            }
        }
    }
}
