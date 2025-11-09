using System.ComponentModel.DataAnnotations;

namespace Synapse.SignalBooster.Configuration
{
    /// <summary>
    /// Application configuration settings.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// OpenAI API key for DME extraction.
        /// </summary>
        [Required(ErrorMessage = "OpenAI API key is required. Set it in user secrets or environment variables.")]
        [MinLength(20, ErrorMessage = "OpenAI API key appears to be invalid")]
        public string OpenAiApiKey { get; set; } = string.Empty;

        /// <summary>
        /// OpenAI model to use for extraction (e.g., "gpt-4o-mini", "gpt-4o", "gpt-3.5-turbo").
        /// </summary>
        [Required]
        public string OpenAiModel { get; set; } = "gpt-4o-mini";

        /// <summary>
        /// System prompt for OpenAI extraction. If empty, uses default prompt.
        /// </summary>
        public string? OpenAiSystemPrompt { get; set; }

        /// <summary>
        /// JSON Schema for OpenAI structured output. If empty, uses default schema.
        /// Must be a valid JSON Schema definition.
        /// </summary>
        public string? OpenAiJsonSchema { get; set; }

        /// <summary>
        /// Folder path containing physician notes to process.
        /// </summary>
        [Required(ErrorMessage = "NotesFolder path is required")]
        public string NotesFolder { get; set; } = string.Empty;

        /// <summary>
        /// API endpoint URL for submitting DME extractions.
        /// </summary>
        [Required(ErrorMessage = "API URL is required")]
        [Url(ErrorMessage = "API URL must be a valid URL")]
        public string ApiUrl { get; set; } = string.Empty;

        /// <summary>
        /// Logging configuration.
        /// </summary>
        public LoggingConfig Logging { get; set; } = new LoggingConfig();
    }
}
