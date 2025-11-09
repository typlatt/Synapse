using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Synapse.SignalBooster.Models;

namespace Synapse.SignalBooster.Services
{
    /// <summary>
    /// Service responsible for reading physician notes from files.
    /// </summary>
    public class PhysicianNoteReader
    {
        private readonly ILogger<PhysicianNoteReader>? _logger;

        /// <summary>
        /// Initializes a new instance of PhysicianNoteReader.
        /// </summary>
        /// <param name="logger">Optional logger for diagnostics</param>
        public PhysicianNoteReader(ILogger<PhysicianNoteReader>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Reads physician note content from a file, handling both plain text and JSON formats.
        /// </summary>
        /// <param name="filePath">Path to the physician note file</param>
        /// <returns>Result containing the note content or an error message</returns>
        public async Task<Result<string>> ReadNoteAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Result<string>.Failure("File path cannot be null or empty");

            if (!File.Exists(filePath))
                return Result<string>.Failure($"Physician note file not found: {filePath}");

            try
            {
                string content = await File.ReadAllTextAsync(filePath);

                // Handle JSON-wrapped notes (e.g., { "data": "..." })
                if (content.TrimStart().StartsWith("{"))
                {
                    try
                    {
                        using JsonDocument doc = JsonDocument.Parse(content);
                        if (doc.RootElement.TryGetProperty("data", out JsonElement dataElement))
                        {
                            string? extractedData = dataElement.GetString();
                            return Result<string>.Success(extractedData ?? content);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger?.LogWarning(ex, "Failed to parse JSON format for {FilePath}, treating as plain text", filePath);
                    }
                }

                return Result<string>.Success(content);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error reading file {FilePath}", filePath);
                return Result<string>.Failure($"Error reading file: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all .txt files from a directory.
        /// </summary>
        /// <param name="directoryPath">Path to the directory to search</param>
        /// <returns>Result containing array of file paths or an error message</returns>
        public Result<string[]> GetNoteFiles(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return Result<string[]>.Failure("Directory path cannot be null or empty");

            if (!Directory.Exists(directoryPath))
                return Result<string[]>.Failure($"Directory not found: {directoryPath}");

            try
            {
                string[] files = Directory.GetFiles(directoryPath, "*.txt");
                return Result<string[]>.Success(files);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error reading directory {DirectoryPath}", directoryPath);
                return Result<string[]>.Failure($"Error reading directory: {ex.Message}");
            }
        }
    }
}
