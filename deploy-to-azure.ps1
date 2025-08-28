# 241 Runners Awareness - Azure Deployment Script
# This script deploys the backend to Azure App Service

Write-Host "Deploying 241 Runners Awareness Backend to Azure..." -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Green

# Configuration
$ResourceGroupName = "241runnersawareness-rg"
$AppServiceName = "241runnersawareness-api"
$BackendPath = "./backend"

# Check if Azure CLI is installed
try {
    az --version | Out-Null
    Write-Host "✅ Azure CLI is installed" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI is not installed. Please install it from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Red
    exit 1
}

# Check if logged in to Azure
try {
    $account = az account show 2>$null | ConvertFrom-Json
    if ($account) {
        Write-Host "✅ Logged in to Azure as: $($account.user.name)" -ForegroundColor Green
    } else {
        Write-Host "❌ Not logged in to Azure. Please run: az login" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Not logged in to Azure. Please run: az login" -ForegroundColor Red
    exit 1
}

# Build the backend
Write-Host "Building backend..." -ForegroundColor Yellow
Set-Location $BackendPath
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Backend build failed" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Backend built successfully" -ForegroundColor Green

# Deploy to Azure
Write-Host "Deploying to Azure App Service..." -ForegroundColor Yellow
az webapp deployment source config-zip --resource-group $ResourceGroupName --name $AppServiceName --src ./publish.zip

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Backend deployed successfully to Azure!" -ForegroundColor Green
    Write-Host "🌐 Backend URL: https://$AppServiceName.azurewebsites.net" -ForegroundColor Cyan
} else {
    Write-Host "❌ Deployment failed" -ForegroundColor Red
}

# Return to root directory
Set-Location ..
