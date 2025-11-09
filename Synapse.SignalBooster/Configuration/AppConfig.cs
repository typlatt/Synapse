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
        public string OpenAiApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Folder path containing physician notes to process.
        /// </summary>
        public string NotesFolder { get; set; } = string.Empty;

        /// <summary>
        /// API endpoint URL for submitting DME extractions.
        /// </summary>
        public string ApiUrl { get; set; } = string.Empty;
    }
}
