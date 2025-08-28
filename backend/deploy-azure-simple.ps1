# Simple Azure Deployment Script for 241 Runners Awareness Backend
param(
    [string]$ResourceGroupName = "241runnersawareness-rg",
    [string]$Location = "East US",
    [string]$AppServiceName = "241runnersawareness-api",
    [string]$AppServicePlanName = "241runners-api-plan"
)

Write-Host "Starting simple Azure deployment for 241 Runners Awareness Backend"
Write-Host "================================================================"

# Check if logged in
az account show
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Not logged into Azure. Please run 'az login' first."
    exit 1
}

# Create Resource Group (if it doesn't exist)
Write-Host "Creating/checking resource group: $ResourceGroupName"
az group create --name $ResourceGroupName --location $Location --tags Project=241RunnersAwareness Environment=Production

# Create App Service Plan (if it doesn't exist)
Write-Host "Creating/checking App Service Plan: $AppServicePlanName"
az appservice plan create --name $AppServicePlanName --resource-group $ResourceGroupName --sku B1 --is-linux

# Create App Service (if it doesn't exist)
Write-Host "Creating/checking App Service: $AppServiceName"
az webapp create --name $AppServiceName --resource-group $ResourceGroupName --plan $AppServicePlanName --runtime DOTNETCORE:8.0

# Configure App Service settings
Write-Host "Configuring App Service settings"
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings ASPNETCORE_ENVIRONMENT=Production
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Jwt__SecretKey="241RunnersAwareness-Super-Secret-Key-2025-Production-Ready-Change-In-Production"
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Jwt__Issuer="241RunnersAwareness"
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Jwt__Audience="241RunnersAwareness"
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Jwt__ExpiryInDays=7
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings App__BaseUrl="https://241runnersawareness.org"
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings App__ApiUrl="https://$AppServiceName.azurewebsites.net"
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings App__Environment="Production"
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Cors__AllowedOrigins="https://241runnersawareness.org,https://www.241runnersawareness.org,https://app.241runnersawareness.org"

Write-Host "SUCCESS: App Service settings configured"

# Build and deploy application
Write-Host "Building and deploying application"

# Build the application
Write-Host "Building application..."
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed"
    exit 1
}

# Create deployment package
Write-Host "Creating deployment package..."
if (Test-Path "deployment.zip") {
    Remove-Item "deployment.zip" -Force
}

Compress-Archive -Path "./publish/*" -DestinationPath "deployment.zip"

# Deploy to Azure
Write-Host "Deploying to Azure..."
az webapp deployment source config-zip --resource-group $ResourceGroupName --name $AppServiceName --src deployment.zip

# Clean up
Remove-Item "deployment.zip" -Force
Remove-Item "publish" -Recurse -Force

Write-Host "SUCCESS: Application deployed successfully"

# Verify deployment
Write-Host "Verifying deployment"
$appUrl = "https://$AppServiceName.azurewebsites.net"

Write-Host "App Service URL: $appUrl"
Write-Host "Swagger UI: $appUrl/swagger"
Write-Host "Health Check: $appUrl/health"

Write-Host "Azure deployment completed successfully!"
Write-Host "================================================================"
Write-Host "Your API is now available at: $appUrl"
Write-Host "Swagger documentation: $appUrl/swagger"
Write-Host "Health check: $appUrl/health"
