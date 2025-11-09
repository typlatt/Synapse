# Secrets Management Guide

This document explains how to securely configure sensitive settings like API keys in Synapse.SignalBooster.

## ⚠️ Security Notice

**NEVER commit API keys or secrets to source control!** The `appsettings.json` file should only contain non-sensitive default values.

## Configuration Priority

The application loads configuration from multiple sources in this priority order (highest to lowest):

1. **Environment Variables** (with `SYNAPSE_` prefix)
2. **User Secrets** (Development only)
3. **appsettings.{Environment}.json**
4. **appsettings.json**

Higher priority sources override lower priority sources.

---

## Local Development Setup

### Option 1: User Secrets (Recommended for Development)

User secrets are stored outside your project directory and are never committed to source control.

#### Initialize User Secrets

```bash
cd Synapse.SignalBooster
dotnet user-secrets init
```

This adds a `UserSecretsId` to your `.csproj` file.

#### Set Your OpenAI API Key

```bash
dotnet user-secrets set "OpenAiApiKey" "sk-proj-YOUR-ACTUAL-API-KEY-HERE"
```

#### View All Secrets

```bash
dotnet user-secrets list
```

#### Remove a Secret

```bash
dotnet user-secrets remove "OpenAiApiKey"
```

#### Clear All Secrets

```bash
dotnet user-secrets clear
```

---

### Option 2: Environment Variables

Set environment variables with the `SYNAPSE_` prefix:

#### macOS/Linux (Bash/Zsh)

```bash
export SYNAPSE_OpenAiApiKey="sk-proj-YOUR-ACTUAL-API-KEY-HERE"
export SYNAPSE_OpenAiModel="gpt-4o-mini"
```

For nested configuration (e.g., Logging settings), use double underscores:

```bash
export SYNAPSE_Logging__MinimumLevel="Debug"
export SYNAPSE_Logging__LogToFile="true"
```

#### Windows (PowerShell)

```powershell
$env:SYNAPSE_OpenAiApiKey="sk-proj-YOUR-ACTUAL-API-KEY-HERE"
$env:SYNAPSE_OpenAiModel="gpt-4o-mini"
```

#### Windows (Command Prompt)

```cmd
set SYNAPSE_OpenAiApiKey=sk-proj-YOUR-ACTUAL-API-KEY-HERE
set SYNAPSE_OpenAiModel=gpt-4o-mini
```

---

## Production Deployment

### Option 1: Environment Variables (Docker/Kubernetes)

#### Docker

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "Synapse.SignalBooster.dll"]
```

```bash
# Run with environment variable
docker run -e SYNAPSE_OpenAiApiKey="your-key" synapse-signalbooster
```

Or use an `.env` file (NOT committed to source control):

```bash
# .env file
SYNAPSE_OpenAiApiKey=sk-proj-YOUR-ACTUAL-API-KEY-HERE
SYNAPSE_NotesFolder=/app/notes
SYNAPSE_Logging__MinimumLevel=Information
```

```bash
docker run --env-file .env synapse-signalbooster
```

#### Kubernetes Secret

```bash
# Create secret
kubectl create secret generic synapse-secrets \
  --from-literal=openai-api-key='sk-proj-YOUR-ACTUAL-API-KEY-HERE'
```

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: synapse-signalbooster
spec:
  template:
    spec:
      containers:
      - name: app
        image: synapse-signalbooster:latest
        env:
        - name: SYNAPSE_OpenAiApiKey
          valueFrom:
            secretKeyRef:
              name: synapse-secrets
              key: openai-api-key
```

---

### Option 2: Azure Key Vault (Recommended for Azure)

#### Install Package

```bash
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add package Azure.Identity
```

#### Update ConfigurationService.cs

```csharp
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

// In LoadConfiguration method:
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI")))
{
    var keyVaultUri = new Uri(Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI")!);
    configBuilder.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}
```

#### Azure Setup

1. **Create Key Vault**:
   ```bash
   az keyvault create --name synapse-kv --resource-group my-rg --location eastus
   ```

2. **Store Secret**:
   ```bash
   az keyvault secret set --vault-name synapse-kv \
     --name OpenAiApiKey \
     --value "sk-proj-YOUR-ACTUAL-API-KEY-HERE"
   ```

3. **Configure App**:
   ```bash
   export AZURE_KEY_VAULT_URI="https://synapse-kv.vault.azure.net/"
   ```

4. **Grant Access** (Managed Identity or Service Principal):
   ```bash
   az keyvault set-policy --name synapse-kv \
     --object-id <managed-identity-id> \
     --secret-permissions get list
   ```

---

## Configuration Examples

### Example 1: Local Development with User Secrets

```bash
dotnet user-secrets set "OpenAiApiKey" "sk-proj-abc123..."
dotnet user-secrets set "Logging:MinimumLevel" "Debug"
dotnet run
```

### Example 2: Production with Environment Variables

```bash
export SYNAPSE_OpenAiApiKey="sk-proj-xyz789..."
export SYNAPSE_Logging__MinimumLevel="Information"
export SYNAPSE_NotesFolder="/var/app/notes"
dotnet Synapse.SignalBooster.dll
```

### Example 3: CI/CD Pipeline (GitHub Actions)

```yaml
# .github/workflows/deploy.yml
name: Deploy
on: [push]
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build and deploy
        env:
          SYNAPSE_OpenAiApiKey: ${{ secrets.OPENAI_API_KEY }}
        run: |
          dotnet build
          dotnet run
```

Store `OPENAI_API_KEY` in GitHub repository secrets (Settings > Secrets and variables > Actions).

---

## Verification

To verify your configuration is loaded correctly:

1. **Check environment**:
   ```bash
   echo $DOTNET_ENVIRONMENT  # Should be "Development" or "Production"
   ```

2. **Run the app** - it will log which configuration sources are active

3. **Test with invalid key** - the app should fail gracefully with a clear error message

---

## Troubleshooting

### "OpenAI API key cannot be empty"

**Cause**: No API key found in any configuration source.

**Solution**: Set the key using one of the methods above.

### User secrets not loading

**Ensure**:
- `DOTNET_ENVIRONMENT=Development` is set
- User secrets are initialized: `dotnet user-secrets list`
- Running from correct directory

### Environment variable not working

**Check**:
- Use correct prefix: `SYNAPSE_` (not `Synapse_` or `synapse_`)
- Use double underscore for nested config: `SYNAPSE_Logging__MinimumLevel`
- Variable is exported in current shell session

---

## Best Practices

✅ **DO**:
- Use User Secrets for local development
- Use environment variables or Key Vault for production
- Rotate API keys regularly
- Use different keys for dev/staging/production
- Document required secrets in README

❌ **DON'T**:
- Commit API keys to Git
- Share production keys in chat/email
- Log sensitive values
- Hard-code secrets in source code
- Use same key across all environments

---

## Additional Resources

- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/)
- [Environment Variables in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#environment-variable-configuration-provider)
