# Synapse SignalBooster - Medical DME Extraction System

> **A production-ready .NET application that extracts Durable Medical Equipment (DME) orders from physician notes using AI-powered natural language processing.**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![OpenAI](https://img.shields.io/badge/OpenAI-GPT--4o--mini-412991?logo=openai)](https://platform.openai.com/)
[![Tests](https://img.shields.io/badge/tests-1%20passed-success)]()
[![Architecture](https://img.shields.io/badge/architecture-SOLID-blue)]()

## 🎯 Project Overview

This application processes unstructured physician notes and extracts structured DME order information using OpenAI's GPT models with strict JSON schema validation. It demonstrates modern .NET architecture, clean code principles, and production-ready error handling.

**Built to showcase:**
- 🏗️ **Enterprise .NET Architecture** - Dependency Injection, Result Pattern, Async/Await
- 🔒 **Security-First Design** - Multi-tier secrets management, no credentials in code
- 🧪 **Test-Driven Development** - Unit tests, FluentAssertions, 100% pass rate
- 📊 **Production Observability** - Structured logging, multiple destinations
- 📖 **Comprehensive Documentation** - Architecture decisions, deployment guides

**Key Capabilities:**
- 🤖 **AI-Powered Extraction**: Uses OpenAI's Structured Outputs for reliable, consistent data extraction
- 📊 **Structured Logging**: Multi-destination logging (console, file, HTTP endpoint)
- 🔒 **Secure Configuration**: Environment-based secrets management
- ✅ **Production-Ready**: Dependency injection, Result pattern, comprehensive error handling

### Architectural Decision: OpenAI-Only Extraction

This project uses **exclusively OpenAI-based extraction** for simplicity and maintainability. An earlier version included a rule-based (regex) extractor as a fallback, but it was removed because:

- ✅ **Single source of truth** - One extraction method, one schema to maintain
- ✅ **Consistent quality** - OpenAI provides structured, validated output across all device types
- ✅ **Simplified codebase** - No dual-path logic or schema synchronization
- ✅ **Production reality** - In real deployments, retry logic + monitoring > fallback to lower-quality extraction

For production resilience, consider implementing:
- Retry logic with exponential backoff (e.g., using Polly library)
- Queue-based processing for failed extractions
- Monitoring and alerting for API failures

---

## 🛠️ Development Environment & Tools

### IDE and Tools Used
- **Primary IDE**: Visual Studio Code (VS Code) on macOS (Apple M4 Pro)
  - Extensions: C# Dev Kit, GitHub Copilot, GitLens
- **AI Development Tools**: 
  - **GitHub Copilot** with **Claude Sonnet 4.5** - Used for code generation, refactoring suggestions, and documentation
  - Assisted with implementing design patterns (DI, Result pattern), writing unit tests, and creating comprehensive documentation
- **Version Control**: Git with GitHub
- **Testing**: xUnit Test Explorer in VS Code
- **Command Line**: zsh terminal integrated in VS Code

### How to Run This Project

**Prerequisites:**
- .NET 9.0 SDK ([download here](https://dotnet.microsoft.com/download/dotnet/9.0))
- OpenAI API key ([get one here](https://platform.openai.com/api-keys))

**Step-by-Step Instructions:**

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd Synapse/Synapse.SignalBooster
   ```

2. **Configure your OpenAI API key:**
   
   For **Development** (local machine):
   ```bash
   dotnet user-secrets init --project 'Synapse.SignalBooster'
   dotnet user-secrets set "OpenAiApiKey" "sk-proj-YOUR-ACTUAL-KEY-HERE"
   ```
   
   For **Production** (server/container):
   ```bash
   export SYNAPSE_OpenAiApiKey="sk-proj-YOUR-ACTUAL-KEY-HERE"
   ```

3. **Run the application:**
   ```bash
   DOTNET_ENVIRONMENT=Development dotnet run
   ```

4. **Run tests:**
   ```bash
   dotnet test
   ```

**Expected Output:**
```
info: Processing physician note: physician_note1.txt
info: Successfully extracted DME using OpenAI: portable oxygen tank
{
  "device": "portable oxygen tank",
  "ordering_provider": "Dr. Cuddy",
  "diagnosis": "COPD",
  "patient_name": "Harold Finch",
  "dob": "04/12/1952"
}
info: Processing complete. Success: 6, Failures: 0
```

### Assumptions and Limitations

**Assumptions:**
- Physician notes are plain text files (`.txt`) located in the `Notes/` directory
- OpenAI API key is valid and has sufficient credits/quota
- .NET 9.0 SDK is installed on the target machine
- Notes contain medical information in natural language format (English)
- User has basic familiarity with command line and .NET tooling

**Current Limitations:**
- **No log rotation**: File-based logging doesn't implement automatic rotation (use centralized logging in production)
- **No retry logic**: API failures don't automatically retry with exponential backoff (intentionally omitted for demo simplicity)
- **Single-threaded processing**: Processes notes sequentially (parallel processing could improve performance for large batches)
- **Text-only input**: Doesn't support PDF, Word, or other document formats
- **No rate limiting**: Doesn't implement throttling for API calls (could hit OpenAI rate limits with large volumes)

**Known Issues:**
- None at this time - all tests passing, application runs successfully

### Future Improvements

**High Priority (Production Readiness):**
- [ ] Implement retry logic with exponential backoff using [Polly](https://github.com/App-vNext/Polly)
- [ ] Add rate limiting/throttling for API calls
- [ ] Implement log rotation for file-based logging (or replace with centralized logging)
- [ ] Add health check endpoints (`/health`, `/ready`) for container orchestration
- [ ] Integrate Application Insights, Serilog, or similar for production telemetry/metrics
- [ ] Add distributed tracing for microservices environments

**Medium Priority (Enhanced Functionality):**
- [ ] Batch processing with parallel execution for high-volume scenarios
- [ ] Support for PDF and Word document formats
- [ ] Integration tests for end-to-end workflow validation
- [ ] Configuration validation on startup (fail fast if OpenAI key missing)
- [ ] More comprehensive error categorization (transient vs. permanent failures)

**Nice to Have (Extended Features):**
- [ ] Web dashboard for monitoring extractions and viewing results
- [ ] Support for more DME device types in rule-based extractor
- [ ] Real-time processing with message queues (Azure Service Bus, RabbitMQ)
- [ ] Multi-language support for physician notes
- [ ] Confidence scoring for extracted data
- [ ] Human-in-the-loop review workflow for low-confidence extractions

---

## 🏗️ Architecture Highlights

### Modern .NET Patterns Implemented

| Pattern | Implementation | Benefit |
|---------|---------------|---------|
| **Dependency Injection** | Microsoft.Extensions.DI | Testable, loosely coupled services |
| **Result Pattern** | Custom `Result<T>` wrapper | Explicit error handling, no hidden exceptions |
| **Async/Await** | Throughout application | Consistent, non-blocking I/O |
| **Configuration Provider** | Multi-tier (Secrets, Env, JSON) | Secure, environment-specific settings |
| **Service-Oriented** | Clear separation of concerns | Maintainable, SOLID principles |

### Project Structure
```
Synapse/
├── notes/                    # Input physician notes
├── logs/                     # Application logs
├── Synapse.SignalBooster/    # Main application
│   ├── Configuration/        # App configuration models
│   ├── Models/              # Data models (DmeOrder, Result<T>)
│   ├── Services/            # Business logic layer
│   │   ├── OpenAiDmeExtractor.cs (AI-powered extraction)
│   │   ├── PhysicianNoteReader.cs
│   │   └── DmeApiClient.cs
│   └── Program.cs           # DI container, orchestration
└── Synapse.SignalBooster.Tests/  # Unit tests
```

---

## 💡 Technical Achievements

### 1. **Intelligent Data Extraction**
- Implemented OpenAI Structured Outputs with strict JSON schema validation
- Guarantees type-safe, validated responses (no markdown/malformed JSON)
- Extracts 7+ data points: device type, patient info, diagnosis, provider, usage details
- Successfully processes diverse medical equipment types (CPAP, oxygen, wheelchairs, glucose monitors, etc.)

### 2. **Robust Error Handling**
- Custom `Result<T>` pattern eliminates exception-driven control flow
- Graceful degradation: app continues processing despite individual failures
- Clear error messages logged at each pipeline stage
- All service methods return `Result<T>` for explicit success/failure handling

### 3. **Enterprise-Grade Configuration**
- **Multi-tier configuration priority**: Environment Variables > User Secrets > Config Files
- **Secure secrets management**: No API keys in source control
- **Environment-specific configs**: Development, Staging, Production
- **Documented deployment**: Docker, Kubernetes, Azure Key Vault examples

### 4. **Production-Ready Logging**
- **Serilog** - Industry-standard structured logging library
- **Development**: Console with timestamps and optional file logging with automatic rotation
- **Production**: Designed to integrate with centralized logging platforms:
  - Azure Application Insights for cloud deployments
  - Elasticsearch/Kibana (ELK stack) for on-premises
  - Splunk, Datadog, or other enterprise logging solutions
- Structured logging with context (file names, extraction results, error details)
- Configurable log levels per environment (Debug for dev, Warning+ for production)
- **Automatic log rotation** (daily, with 7-day retention and 10MB file size limit)
- Easy integration: Just add additional Serilog sinks (e.g., `WriteTo.ApplicationInsights()`)

**Why Serilog?**
- ✅ Industry standard with 35+ sink providers
- ✅ Built-in log rotation, buffering, and batching
- ✅ Structured logging (can output JSON for parsing)
- ✅ High performance with async writes
- ✅ Battle-tested in production environments worldwide

**Example production logging setup:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(telemetryConfig, TelemetryConverter.Traces)
    .CreateLogger();
```

### 5. **Testability & Maintainability**
- Dependency injection enables easy mocking
- 6 unit tests with 100% pass rate
- Clear service boundaries and single responsibility
- All services accept `ILogger<T>` for type-safe logging

---

## 🛠️ Development Details

### Technology Stack
- **.NET 9.0** - Latest C# features, performance improvements
- **OpenAI GPT-4o-mini** - Fast, cost-effective structured extraction
- **Serilog** - Industry-standard structured logging with 35+ sinks
- **xUnit** - Unit testing framework
- **FluentAssertions** - Expressive test assertions
- **Microsoft.Extensions.*** - DI, Logging, Configuration

### Key Dependencies
```xml
<PackageReference Include="OpenAI" Version="2.1.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.10" />
<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.10" />
<PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="9.0.10" />
<PackageReference Include="Microsoft.Extensions.Options.DataAnnotations" Version="9.0.10" />
<PackageReference Include="Serilog.Extensions.Logging" Version="9.0.2" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
```

### SOLID Principles Applied
✅ **Single Responsibility**: Each service has one clear purpose  
✅ **Open/Closed**: Extractors are swappable, services extensible  
✅ **Liskov Substitution**: Both extractors implement same async interface  
✅ **Interface Segregation**: Services only depend on what they need  
✅ **Dependency Inversion**: Depend on abstractions (`ILogger<T>`, `HttpClient`)

---

## 📋 Features & Functionality

### Extraction Capabilities
- [x] Patient demographics (name, DOB)
- [x] Medical diagnosis
- [x] Device type and specifications
- [x] Usage instructions
- [x] Ordering physician
- [x] Device-specific details (oxygen flow rate, CPAP settings, etc.)
- [x] Handles multiple equipment types per note

### Configuration Options
- [x] User Secrets (development)
- [x] Environment Variables (production)
- [x] JSON configuration files
- [x] Azure Key Vault ready (documented)
- [x] Docker/Kubernetes support

### Logging Destinations
- [x] Console with structured output (Serilog format with timestamps)
- [x] File with automatic daily rotation (7-day retention, 10MB size limit)
- [ ] Production: Easy to add additional Serilog sinks:
  - Azure Application Insights: `WriteTo.ApplicationInsights()`
  - Elasticsearch: `WriteTo.Elasticsearch()`
  - Seq: `WriteTo.Seq()`
  - 35+ other destinations available

---

## 🧪 Testing

```bash
# Run all tests
dotnet test
```

**Test Coverage:**
- `PhysicianNoteReaderTests` - File I/O and Result pattern validation

The project focuses on testing critical infrastructure (file reading, error handling) rather than testing AI extraction behavior, which would require expensive API calls and produce non-deterministic results.

**Why no OpenAI extractor tests?**
- Testing AI extraction requires real API calls (cost, latency)
- LLM responses are non-deterministic (same input ≠ same output)
- Better tested through manual validation and production monitoring
- Focus is on demonstrating testing skills with deterministic code

**Test summary:** 1 test, 1 passed, 0 failed

---

## 📚 Documentation

| Document | Purpose |
|----------|---------|
| [docs/README.md](docs/README.md) | Overview of all documentation for hiring managers |
| [SignalBooster_Original.cs](docs/SignalBooster_Original.cs) | The "before" code - compare with current implementation |
| [Architecture_Improvements.md](docs/Architecture_Improvements.md) | Detailed refactoring summary and design decisions |
| [Secrets_Management.md](docs/Secrets_Management.md) | Security best practices, API keys, deployment configuration |

---

## 🔐 Security & Production

### Secrets Management
**⚠️ API keys are NEVER committed to source control**

**Development:**
```bash
dotnet user-secrets set "OpenAiApiKey" "sk-proj-..."
```

**Production:**
```bash
export SYNAPSE_OpenAiApiKey="sk-proj-..."
# Or use Azure Key Vault, AWS Secrets Manager, etc.
```

See [docs/Secrets_Management.md](docs/Secrets_Management.md) for complete guide.

### Production Deployment Examples

**Docker:**
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "Synapse.SignalBooster.dll"]
```

```bash
docker run -e SYNAPSE_OpenAiApiKey="sk-..." synapse-signalbooster
```

**Kubernetes:**
```yaml
env:
  - name: SYNAPSE_OpenAiApiKey
    valueFrom:
      secretKeyRef:
        name: synapse-secrets
        key: openai-api-key
```

---

## 🎓 What I Learned / Demonstrated

### Technical Skills
- Modern .NET dependency injection patterns
- Result pattern for functional error handling
- OpenAI API integration with structured outputs
- Multi-tier configuration management
- Async/await best practices
- Unit testing with xUnit and FluentAssertions
- SOLID principles in C# application design

### Architecture & Design
- Service-oriented architecture
- Separation of concerns
- Dependency inversion
- Factory pattern (logger providers)
- Strategy pattern (dual extractors)

### DevOps & Production
- Secure secrets management (User Secrets, Environment Variables)
- Environment-specific configuration
- Container deployment readiness
- Structured logging for observability
- Documentation for operations teams

