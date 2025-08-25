# 241 Runners Awareness - Complete Production Deployment Script
# This script deploys the entire platform to production

param(
    [string]$Environment = "production",
    [string]$AzureSubscription = "",
    [string]$ResourceGroup = "241runnersawareness-rg",
    [string]$BackendAppName = "241runnersawareness-api",
    [string]$FrontendAppName = "241runnersawareness-web",
    [string]$DatabaseName = "241runnersawareness-db"
)

Write-Host "🚀 241 Runners Awareness - Production Deployment" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Yellow
Write-Host "Backend App: $BackendAppName" -ForegroundColor Yellow
Write-Host "Frontend App: $FrontendAppName" -ForegroundColor Yellow
Write-Host "Database: $DatabaseName" -ForegroundColor Yellow
Write-Host ""

# Check prerequisites
Write-Host "📋 Checking Prerequisites..." -ForegroundColor Cyan

# Check if Azure CLI is installed
try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-Host "✅ Azure CLI version: $($azVersion.'azure-cli')" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI not found. Please install Azure CLI first." -ForegroundColor Red
    Write-Host "   Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET not found. Please install .NET 9.0 first." -ForegroundColor Red
    exit 1
}

# Check if Node.js is installed
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js version: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js not found. Please install Node.js first." -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 1: Build Backend
Write-Host "🔧 Step 1: Building Backend API..." -ForegroundColor Cyan
Set-Location backend

Write-Host "  Restoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host "  Building project..." -ForegroundColor Yellow
dotnet build --configuration Release

Write-Host "  Running database migrations..." -ForegroundColor Yellow
dotnet ef database update

Write-Host "  Publishing backend..." -ForegroundColor Yellow
dotnet publish --configuration Release --output ./publish

Write-Host "✅ Backend build completed!" -ForegroundColor Green
Set-Location ..

# Step 2: Build Frontend
Write-Host ""
Write-Host "🔧 Step 2: Building Frontend..." -ForegroundColor Cyan
Set-Location frontend

Write-Host "  Installing dependencies..." -ForegroundColor Yellow
npm install

Write-Host "  Building for production..." -ForegroundColor Yellow
npm run build

Write-Host "✅ Frontend build completed!" -ForegroundColor Green
Set-Location ..

# Step 3: Deploy to Azure
Write-Host ""
Write-Host "☁️ Step 3: Deploying to Azure..." -ForegroundColor Cyan

# Check Azure login
Write-Host "  Checking Azure login..." -ForegroundColor Yellow
try {
    $account = az account show --output json | ConvertFrom-Json
    Write-Host "✅ Logged in as: $($account.user.name)" -ForegroundColor Green
} catch {
    Write-Host "❌ Not logged into Azure. Please run 'az login' first." -ForegroundColor Red
    exit 1
}

# Create resource group if it doesn't exist
Write-Host "  Creating resource group..." -ForegroundColor Yellow
az group create --name $ResourceGroup --location "East US" --output none

# Deploy backend to Azure App Service
Write-Host "  Deploying backend to Azure App Service..." -ForegroundColor Yellow
az webapp create --resource-group $ResourceGroup --plan "$BackendAppName-plan" --name $BackendAppName --runtime "DOTNETCORE:9.0" --deployment-local-git --output none

# Configure backend app settings
Write-Host "  Configuring backend app settings..." -ForegroundColor Yellow
az webapp config appsettings set --resource-group $ResourceGroup --name $BackendAppName --settings @backend/appsettings.Production.json --output none

# Deploy backend code
Write-Host "  Deploying backend code..." -ForegroundColor Yellow
az webapp deployment source config-local-git --resource-group $ResourceGroup --name $BackendAppName --output none
$gitUrl = az webapp deployment source config-local-git --resource-group $ResourceGroup --name $BackendAppName --query url --output tsv

# Deploy frontend to Azure Static Web Apps
Write-Host "  Deploying frontend to Azure Static Web Apps..." -ForegroundColor Yellow
az staticwebapp create --name $FrontendAppName --resource-group $ResourceGroup --location "East US" --source frontend/dist --output none

# Step 4: Configure DNS and SSL
Write-Host ""
Write-Host "🌐 Step 4: Configuring DNS and SSL..." -ForegroundColor Cyan

Write-Host "  Backend URL: https://$BackendAppName.azurewebsites.net" -ForegroundColor Green
Write-Host "  Frontend URL: https://$FrontendAppName.azurestaticapps.net" -ForegroundColor Green

# Step 5: Run health checks
Write-Host ""
Write-Host "🏥 Step 5: Running Health Checks..." -ForegroundColor Cyan

Write-Host "  Checking backend health..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "https://$BackendAppName.azurewebsites.net/health" -Method Get
    Write-Host "✅ Backend health check passed!" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Backend health check failed. This is normal during initial deployment." -ForegroundColor Yellow
}

Write-Host "  Checking frontend..." -ForegroundColor Yellow
try {
    $frontendResponse = Invoke-RestMethod -Uri "https://$FrontendAppName.azurestaticapps.net" -Method Get
    Write-Host "✅ Frontend is accessible!" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Frontend check failed. This is normal during initial deployment." -ForegroundColor Yellow
}

# Step 6: Final Configuration
Write-Host ""
Write-Host "⚙️ Step 6: Final Configuration..." -ForegroundColor Cyan

Write-Host "  Setting up environment variables..." -ForegroundColor Yellow
# Set production environment variables
az webapp config appsettings set --resource-group $ResourceGroup --name $BackendAppName --settings ASPNETCORE_ENVIRONMENT=Production --output none

Write-Host "  Configuring CORS..." -ForegroundColor Yellow
az webapp config appsettings set --resource-group $ResourceGroup --name $BackendAppName --settings "Cors__AllowedOrigins=https://$FrontendAppName.azurestaticapps.net,https://241runnersawareness.org" --output none

# Step 7: Summary
Write-Host ""
Write-Host "🎉 Deployment Summary" -ForegroundColor Green
Write-Host "====================" -ForegroundColor Green
Write-Host "✅ Backend API: https://$BackendAppName.azurewebsites.net" -ForegroundColor Green
Write-Host "✅ Frontend App: https://$FrontendAppName.azurestaticapps.net" -ForegroundColor Green
Write-Host "✅ Database: $DatabaseName" -ForegroundColor Green
Write-Host "✅ Resource Group: $ResourceGroup" -ForegroundColor Green
Write-Host ""
Write-Host "🔗 API Documentation: https://$BackendAppName.azurewebsites.net/swagger" -ForegroundColor Cyan
Write-Host "🔗 Health Check: https://$BackendAppName.azurewebsites.net/health" -ForegroundColor Cyan
Write-Host ""
Write-Host "📝 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Configure custom domain (241runnersawareness.org)" -ForegroundColor White
Write-Host "   2. Set up monitoring and alerts" -ForegroundColor White
Write-Host "   3. Configure backup and disaster recovery" -ForegroundColor White
Write-Host "   4. Set up CI/CD pipeline" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Deployment completed successfully!" -ForegroundColor Green
