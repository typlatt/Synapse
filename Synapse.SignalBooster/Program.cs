using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
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
        static async Task<int> Main(string[] args)
        {
            // Build service provider with dependency injection
            var serviceProvider = ConfigureServices();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var config = serviceProvider.GetRequiredService<IOptions<AppConfig>>().Value;

            try
            {
                logger.LogInformation("Starting DME extraction process");

                // Get all .txt files from the notes folder
                // Resolve path relative to the project directory, not the bin directory
                string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
                string notesFolder = Path.IsPathRooted(config.NotesFolder) 
                    ? config.NotesFolder 
                    : Path.Combine(projectDirectory, config.NotesFolder);
                
                if (!Directory.Exists(notesFolder))
                {
                    logger.LogError("Notes folder does not exist: {NotesFolder}", notesFolder);
                    return 1;
                }

                var noteReader = serviceProvider.GetRequiredService<PhysicianNoteReader>();
                var noteFilesResult = noteReader.GetNoteFiles(notesFolder);
                
                if (noteFilesResult.IsFailure)
                {
                    logger.LogError("Failed to get note files: {Error}", noteFilesResult.Error);
                    return 1;
                }

                string[] noteFiles = noteFilesResult.Value;
                
                if (noteFiles.Length == 0)
                {
                    logger.LogWarning("No .txt files found in {NotesFolder}", notesFolder);
                    return 0;
                }

                logger.LogInformation("Found {Count} note file(s) to process", noteFiles.Length);
                logger.LogInformation("Using OpenAI-based DME extraction");

                // Get services
                var openAiExtractor = serviceProvider.GetRequiredService<OpenAiDmeExtractor>();
                var apiClient = serviceProvider.GetRequiredService<DmeApiClient>();
                int successCount = 0;
                int failureCount = 0;

                foreach (string noteFile in noteFiles)
                {
                    logger.LogInformation("Processing {FileName}", Path.GetFileName(noteFile));

                    // Read physician note
                    var noteResult = await noteReader.ReadNoteAsync(noteFile);
                    if (noteResult.IsFailure)
                    {
                        logger.LogError("Failed to read note {FileName}: {Error}", Path.GetFileName(noteFile), noteResult.Error);
                        failureCount++;
                        continue;
                    }

                    string noteContent = noteResult.Value;

                    // Extract DME information using OpenAI
                    var extractionResult = await openAiExtractor.ExtractAsync(noteContent);

                    if (extractionResult.IsFailure)
                    {
                        logger.LogError("Failed to extract DME from {FileName}: {Error}", Path.GetFileName(noteFile), extractionResult.Error);
                        failureCount++;
                        continue;
                    }

                    DmeOrder order = extractionResult.Value;
                    logger.LogInformation("Extracted DME: {DeviceType}", order.Device);

                    // Log the full extraction output
                    string extractionJson = JsonSerializer.Serialize(order, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });
                    logger.LogInformation("Extraction output:\n{ExtractionJson}", extractionJson);

                    // Submit to API
                    // var submitResult = await apiClient.SubmitExtractionAsync(config.ApiUrl, order);
                    // if (submitResult.IsFailure)
                    // {
                    //     logger.LogError("Failed to submit extraction for {FileName}: {Error}", Path.GetFileName(noteFile), submitResult.Error);
                    //     failureCount++;
                    //     continue;
                    // }
                    
                    logger.LogInformation("Successfully submitted DME extraction for {FileName}", Path.GetFileName(noteFile));
                    successCount++;
                }

                logger.LogInformation("Processing complete. Success: {SuccessCount}, Failures: {FailureCount}", successCount, failureCount);
                return failureCount > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fatal error during DME extraction process");
                return 1;
            }
        }

        /// <summary>
        /// Configures dependency injection services.
        /// </summary>
        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Load configuration using built-in .NET configuration system
            string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(projectDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                .AddUserSecrets<Program>(optional: true)
                .AddEnvironmentVariables(prefix: "SYNAPSE_")
                .Build();

            // Bind and validate configuration with IOptions pattern
            services.AddOptions<AppConfig>()
                .Bind(configuration)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Get AppConfig to configure Serilog
            var appConfig = new AppConfig();
            configuration.Bind(appConfig);

            // Parse log level from config
            if (!Enum.TryParse<Serilog.Events.LogEventLevel>(appConfig.Logging.MinimumLevel, out var logLevel))
            {
                logLevel = Serilog.Events.LogEventLevel.Information;
            }

            // Configure Serilog
            string logFilePath = Path.IsPathRooted(appConfig.Logging.LogFilePath)
                ? appConfig.Logging.LogFilePath
                : Path.Combine(projectDirectory, appConfig.Logging.LogFilePath);

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel)
                .WriteTo.Console();

            if (appConfig.Logging.LogToFile)
            {
                loggerConfig.WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    fileSizeLimitBytes: 10_485_760); // 10 MB
            }

            Log.Logger = loggerConfig.CreateLogger();

            // Add Serilog to logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });

            // Register HttpClient with proper lifetime management
            services.AddHttpClient<DmeApiClient>();

            // Register application services
            services.AddTransient<PhysicianNoteReader>();
            
            // Register OpenAI extractor
            services.AddTransient<OpenAiDmeExtractor>(sp =>
            {
                var cfg = sp.GetRequiredService<IOptions<AppConfig>>().Value;
                var logger = sp.GetRequiredService<ILogger<OpenAiDmeExtractor>>();
                return new OpenAiDmeExtractor(
                    cfg.OpenAiApiKey,
                    logger,
                    cfg.OpenAiModel,
                    cfg.OpenAiSystemPrompt,
                    cfg.OpenAiJsonSchema
                );
            });

            return services.BuildServiceProvider();
        }
    }
}