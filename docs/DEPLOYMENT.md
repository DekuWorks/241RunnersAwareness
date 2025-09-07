# Deployment Guide

## Prerequisites
- Azure CLI installed and configured
- .NET 8 SDK
- Access to Azure resources

## Deployment Steps

### 1. Build and Package
```bash
cd 241RunnersAwarenessAPI
dotnet build
dotnet publish -c Release -o publish
cd publish
zip -r ../deployment.zip . -x "*.pdb" "*.xml"
```

### 2. Deploy to Azure
```bash
az webapp deploy --resource-group 241runnersawareness-rg --name 241runners-api --src-path deployment.zip --type zip
```

### 3. Restart App Service
```bash
az webapp restart --name 241runners-api --resource-group 241runnersawareness-rg
```

## Environment Variables
All secrets are stored in Azure App Settings, not in the codebase.

## Security Notes
- Never commit secrets to the repository
- Use Azure Key Vault for sensitive data
- Rotate secrets regularly
