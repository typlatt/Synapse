# Synapse SignalBooster - Medical DME Extraction System

> **A .NET application that extracts Durable Medical Equipment (DME) orders from physician notes using AI-powered natural language processing.**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![OpenAI](https://img.shields.io/badge/OpenAI-GPT--4o--mini-412991?logo=openai)](https://platform.openai.com/)


## Prerequisites

- .NET 9 SDK
- OpenAI API key
- DME API access

## Setup

1. **Clone and navigate:**
   ```bash
   git clone <repository_url>
   cd Synapse/Synapse.SignalBooster
   ```

2. **Set up secrets:**
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "AppConfig:OpenAiApiKey" "your-openai-key"
   dotnet user-secrets set "AppConfig:DmeApiToken" "your-dme-token"
   ```

3. **Configure API URL:**
   Copy `appsettings.template.json` to `appsettings.json` and update the DME API URL.

## Run

```bash
# Process one note
dotnet run -- ../notes/physician_note1.txt

# Process all notes
dotnet run -- all
```

## Assumptions

- Notes are in plain text format
- .NET 9 SDK is installed
- API keys are properly configured

## Limitations

- Only processes DME orders
- Processes one note at a time
- Basic error handling

## Future Improvements

- Support multiple file formats (PDF, DOC, etc.)
- Batch processing capabilities
- Enhanced error handling and retry logic
- Support for additional medical order types
- Improved logging and monitoring
- Account for HIPPA Governance & Compliance

## Testing

```bash
# Run unit tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Example Output

When processing a physician's note, the app extracts structured data like:
```json
{
  "PatientName": "John Doe",
  "DateOfBirth": "1980-05-15",
  "Diagnosis": "Sleep Apnea",
  "Equipment": "CPAP Machine",
  "Supplier": "ABC Medical Supply"
}
```

## Troubleshooting

**API Key Issues:**
- Verify your OpenAI API key is set: `dotnet user-secrets list`
- Check API key permissions and billing status

**Network Errors:**
- Confirm DME API URL in `appsettings.json`
- Test API connectivity with a tool like curl or Postman

**File Not Found:**
- Ensure note files exist in the `notes/` directory
- Use relative paths from the `Synapse.SignalBooster` directory

**Build Errors:**
- Run `dotnet restore` to restore packages
- Verify .NET 9 SDK is installed: `dotnet --version`
