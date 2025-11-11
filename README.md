# Synapse SignalBooster  
**Medical DME Extraction System**

> A .NET 9 application that extracts **Durable Medical Equipment (DME)** orders from unstructured physician notes using AI-powered natural language processing and structured data mapping.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![OpenAI](https://img.shields.io/badge/OpenAI-2.1.0-412991?logo=openai)](https://platform.openai.com/)
[![Serilog](https://img.shields.io/badge/Serilog-9.0.2-1BA1E2?logo=serilog)](https://serilog.net/)
[![Microsoft Extensions](https://img.shields.io/badge/Microsoft%20Extensions-9.0.10-5C2D91?logo=microsoft)](https://docs.microsoft.com/en-us/dotnet/core/extensions/)

---

## 🧭 Overview

SignalBooster refactors a legacy note parser into a **modular, maintainable**, and **AI-integrated** data extraction pipeline.  
It reads physician notes, extracts relevant DME information (e.g., CPAP, oxygen tanks, wheelchairs), and posts structured JSON to a DME API endpoint.

---

## 🧠 Architecture Highlights

- **Layered Design**
  - `PhysicianNoteReader` – Handles input and preprocessing  
  - `OpenAiDmeExtractor` – Applies NLP logic and model integration  
  - `DmeApiClient` – Sends structured JSON to DME API  
  - `Program.cs` – Orchestrates workflow, configuration, and logging  
- **Structured Logging** with Serilog (console and file sinks)  
- **Config Management** via `.NET user-secrets` and `appsettings.json`  
- **AI Integration Ready** – OpenAI model can be swapped or extended  
- **Minimal Test Coverage** – Currently only basic file reading tests  

---

## 🗂️ Project Structure
```
Synapse/
├── Synapse.SignalBooster/
│   ├── Program.cs # Entry point orchestrating workflow
│   ├── Services/
│   │   ├── PhysicianNoteReader.cs # Handles file I/O and note retrieval
│   │   ├── OpenAiDmeExtractor.cs # OpenAI-powered DME extraction service
│   │   └── DmeApiClient.cs # Posts structured payloads to endpoint
│   ├── Models/
│   │   ├── DmeOrder.cs # Strongly-typed representation of DME data
│   │   └── Result.cs # Result pattern for error handling
│   ├── Configuration/
│   │   ├── AppConfig.cs # Application configuration model
│   │   └── LoggingConfig.cs # Logging configuration model
│   └── appsettings.json # Configuration file (endpoint, keys)
├── Synapse.SignalBooster.Tests/
│   └── PhysicianNoteReaderTests.cs # Unit tests for note reading functionality
├── notes/
│   ├── physician_note1.txt # Sample physician notes
│   └── physician_note2.txt
├── notes/
│ ├── physician_note1.txt
│ └── physician_note2.txt
|-- logs
└── README.md

```


This modular layout emphasizes **separation of concerns** — each class handles one part of the workflow for better testing and long-term maintainability.

---

## ⚙️ Development Environment

- MacBook Pro M4 Pro  
- Visual Studio Code  
- .NET 9 SDK  
- GitHub for version control  

**AI Tools**
- GitHub Copilot – pair programming  
- Claude Sonnet 4.5 – for design-level refactor suggestions  
*(Used for structure guidance, not direct code generation)*

---

## ⚖️ Assumptions
- Input physician notes are plain text files
- .NET 9 SDK is installed and accessible via CLI
- API keys for OpenAI and DME endpoint are valid and configured
- Only DME-related orders (CPAP, Oxygen, Wheelchair, etc.) are considered

## ❌ Limitations
- Currently handles only plain text notes, not PDFs or DOCX
- Basic error handling; failed API calls will log errors but do not retry
- No HIPAA-specific data masking or governance yet
- Limited unit test coverage
- No CI/CD pipeline configuration or setup that isn't required for production, but is a best practice

## 🧩 Future Enhancements
- Support additional file formats (PDF, DOCX, XML, etc.)
- Batch and async pipeline execution
- API retries
- Schema validation using LLM guardrails
- Take a more structured approach with LLM context stuffing, e.g., include a YML/JSON file or ontology
- LLM tuning that configuarble without a deployment lifecycle
- Structured metrics and observability
- HIPAA-compliant data handling & governance
- I imagine there is a lot more data fields that could be extracted in a real world scenario. 
- Depending on the confidence score of the LLM extraction, I would consider adding a fallback method based on pattern matching. ```Regex Baby!!```
- Logs would be pushed to a centralized logging platform or picked up by some sort of log ingestion

## 🚀 Setup

```bash
# Clone repository
git clone <repository_url>
cd Synapse/Synapse.SignalBooster

# Initialize user secrets
dotnet user-secrets init
dotnet user-secrets set "AppConfig:OpenAiApiKey" "your-openai-key"
dotnet user-secrets set "AppConfig:DmeApiToken" "your-dme-token"

# Configure endpoint in appsettings.json
# Update "ApiUrl" to your actual or mock endpoint
```
## ▶️ Run

```bash
# Process a single note (run from Synapse.SignalBooster directory)
dotnet run -- ../notes/physician_note1.txt

# Process all notes in directory
dotnet run -- all
```
> **Note:** The included API endpoint (```https://alert-api.com/DrExtract```) is a placeholder.
> To prevent runtime errors, use a mock endpoint like Webhook.site or update the `ApiUrl` in `appsettings.json` to point to your actual DME API endpoint.

## 🧪 Testing
```
dotnet test
# or
dotnet test --collect:"XPlat Code Coverage"
```

Current unit tests validate:
- Basic file reading functionality for physician notes

**Test Coverage Gaps:**
- No error handling tests (missing files, invalid paths)
- No OpenAI extraction logic tests
- No API client functionality tests
- No configuration validation tests
- No integration tests for the complete pipeline

*Note: Test coverage is currently minimal with only one basic test. Significant expansion needed for production readiness.*


## 📄 Example Output
```JSON
{
  "device": "Oxygen Tank",
  "liters": "2 L",
  "usage": "sleep and exertion",
  "diagnosis": "COPD",
  "ordering_provider": "Dr. Cuddy",
  "patient_name": "Harold Finch",
  "dob": "04/12/1952"
}
```

## 🔧 Troubleshooting
|Issue |	Fix |
|------|-----|
|API key errors |	Check secrets: `dotnet user-secrets list` |
|Network failures	| Verify `appsettings.json` DME API URL |
|Missing files | 	Confirm note paths relative to `Synapse.SignalBooster` directory|
|Build issues | 	Run `dotnet restore` and confirm .NET 9 SDK install |
