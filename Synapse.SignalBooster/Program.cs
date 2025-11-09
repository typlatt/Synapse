using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Synapse.SignalBooster.Configuration;
using Synapse.SignalBooster.Models;
using Synapse.SignalBooster.Services;

namespace Synapse.SignalBooster
{
    /// <summary>
    /// Extracts DME (Durable Medical Equipment) requirements from physician notes
    /// and submits them to an external API.
    /// </summary>
    class Program
    {
        // Logger for the application.
        private static readonly ILogger<Program> _logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<Program>();

        static async Task<int> Main(string[] args)
        {
            try
            {
                _logger.LogInformation("Starting DME extraction process");

                // Load configuration
                var config = LoadConfiguration();
                _logger.LogInformation("Loaded configuration");

                // Validate configuration
                if (string.IsNullOrWhiteSpace(config.NotesFolder))
                {
                    _logger.LogError("NotesFolder is not configured in appsettings.json");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(config.ApiUrl))
                {
                    _logger.LogError("ApiUrl is not configured in appsettings.json");
                    return 1;
                }

                // Get all .txt files from the notes folder
                // Resolve path relative to the project directory, not the bin directory
                string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
                string notesFolder = Path.IsPathRooted(config.NotesFolder) 
                    ? config.NotesFolder 
                    : Path.Combine(projectDirectory, config.NotesFolder);
                
                if (!Directory.Exists(notesFolder))
                {
                    _logger.LogError("Notes folder does not exist: {NotesFolder}", notesFolder);
                    return 1;
                }

                string[] noteFiles = Directory.GetFiles(notesFolder, "*.txt");
                if (noteFiles.Length == 0)
                {
                    _logger.LogWarning("No .txt files found in {NotesFolder}", notesFolder);
                    return 0;
                }

                _logger.LogInformation("Found {Count} note file(s) to process", noteFiles.Length);

                // Determine which extractor to use
                bool useOpenAi = !string.IsNullOrWhiteSpace(config.OpenAiApiKey) && 
                                 config.OpenAiApiKey != "your-openai-api-key-here";

                if (useOpenAi)
                {
                    _logger.LogInformation("Using OpenAI-based DME extraction");
                }
                else
                {
                    _logger.LogInformation("Using rule-based DME extraction (OpenAI API key not configured)");
                }

                // Process each note file
                DmeExtractor? ruleBasedExtractor = useOpenAi ? null : new DmeExtractor(_logger);
                OpenAiDmeExtractor? openAiExtractor = useOpenAi ? new OpenAiDmeExtractor(config.OpenAiApiKey, _logger) : null;
                var apiClient = new DmeApiClient(new HttpClient(), _logger);
                int successCount = 0;
                int failureCount = 0;

                foreach (string noteFile in noteFiles)
                {
                    try
                    {
                        _logger.LogInformation("Processing {FileName}", Path.GetFileName(noteFile));

                        // Read physician note
                        string noteContent = await ReadPhysicianNoteAsync(noteFile);

                        // Extract DME information using the appropriate extractor
                        DmeExtraction extraction;
                        if (useOpenAi && openAiExtractor != null)
                        {
                            extraction = await openAiExtractor.ExtractAsync(noteContent);
                        }
                        else if (ruleBasedExtractor != null)
                        {
                            extraction = ruleBasedExtractor.Extract(noteContent);
                        }
                        else
                        {
                            throw new InvalidOperationException("No extractor available");
                        }
                        
                        _logger.LogInformation("Extracted DME: {DeviceType}", extraction.Device);

                        // Submit to API
                        await apiClient.SubmitExtractionAsync(config.ApiUrl, extraction);
                        _logger.LogInformation("Successfully submitted DME extraction for {FileName}", Path.GetFileName(noteFile));
                        
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing {FileName}", Path.GetFileName(noteFile));
                        failureCount++;
                    }
                }

                _logger.LogInformation("Processing complete. Success: {SuccessCount}, Failures: {FailureCount}", successCount, failureCount);
                return failureCount > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error during DME extraction process");
                return 1;
            }
        }

        /// <summary>
        /// Loads application configuration from appsettings.json.
        /// </summary>
        /// <returns>Application configuration</returns>
        private static AppConfig LoadConfiguration()
        {
            // Get the directory where the project file is located (not the bin directory)
            string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(projectDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var config = new AppConfig();
            configuration.Bind(config);
            return config;
        }

        /// <summary>
        /// Reads physician note content from a file, handling both plain text and JSON formats.
        /// </summary>
        /// <param name="filePath">Path to the physician note file</param>
        /// <returns>The note content as a string</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
        private static async Task<string> ReadPhysicianNoteAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Physician note file not found: {filePath}");
            }

            string content = await File.ReadAllTextAsync(filePath);

            // Handle JSON-wrapped notes (e.g., { "data": "..." })
            if (content.TrimStart().StartsWith("{"))
            {
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out JsonElement dataElement))
                    {
                        return dataElement.GetString() ?? content;
                    }
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Failed to parse JSON format, treating as plain text");
                }
            }

            return content;
        }
    }
}