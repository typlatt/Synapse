namespace Synapse.SignalBooster.Configuration
{
    /// <summary>
    /// Configuration for application logging.
    /// </summary>
    public class LoggingConfig
    {
        /// <summary>
        /// Enable logging to a local file.
        /// </summary>
        public bool LogToFile { get; set; } = false;

        /// <summary>
        /// Path to the log file when LogToFile is true.
        /// Serilog will automatically add date suffixes and handle rotation.
        /// </summary>
        public string LogFilePath { get; set; } = "logs/app.log";

        /// <summary>
        /// Minimum log level (Trace, Debug, Information, Warning, Error, Critical, None).
        /// Maps to Serilog's LogEventLevel.
        /// </summary>
        public string MinimumLevel { get; set; } = "Information";
    }
}
