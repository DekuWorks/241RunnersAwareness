# Azure Deployment Script for 241 Runners Awareness Backend
param(
    [string]$ResourceGroupName = "241runnersawareness-rg",
    [string]$Location = "East US",
    [string]$AppServiceName = "241runnersawareness-api",
    [string]$AppServicePlanName = "241runners-api-plan",
    [string]$SqlServerName = "241runners-sql-server",
    [string]$DatabaseName = "RunnersDb",
    [string]$SqlAdminUser = "sqladmin",
    [string]$SqlAdminPassword = "YourStrongPassword123!"
)

Write-Host "Starting Azure deployment for 241 Runners Awareness Backend"
Write-Host "================================================================"

# Find Azure CLI
$azPath = $null
$possiblePaths = @(
    "C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\wbin\az.cmd",
    "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd",
    "C:\Users\$env:USERNAME\AppData\Local\Programs\Azure CLI\az.cmd"
)

foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $azPath = $path
        break
    }
}

if (-not $azPath) {
    Write-Host "ERROR: Azure CLI not found. Please install Azure CLI first."
    Write-Host "You can install it using: winget install Microsoft.AzureCLI"
    exit 1
}

Write-Host "SUCCESS: Azure CLI found at: $azPath"

# Function to execute Azure CLI commands
function Invoke-AzureCLI {
    param([string]$Arguments)
    Write-Host "Executing: $azPath $Arguments"
    & $azPath $Arguments.Split(' ')
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Azure CLI command failed: $Arguments"
        exit 1
    }
}

# Check if logged in
try {
    Invoke-AzureCLI "account show"
    Write-Host "SUCCESS: Already logged into Azure"
} catch {
    Write-Host "WARNING: Azure login check failed, but continuing..."
}

# Create Resource Group
Write-Host "Creating resource group: $ResourceGroupName"
try {
    Invoke-AzureCLI "group show --name $ResourceGroupName"
    Write-Host "SUCCESS: Resource group already exists"
} catch {
    Invoke-AzureCLI "group create --name $ResourceGroupName --location $Location --tags Project=241RunnersAwareness Environment=Production"
    Write-Host "SUCCESS: Resource group created"
}

# Create App Service Plan
Write-Host "Creating App Service Plan: $AppServicePlanName"
try {
    Invoke-AzureCLI "appservice plan show --name $AppServicePlanName --resource-group $ResourceGroupName"
    Write-Host "SUCCESS: App Service Plan already exists"
} catch {
    Invoke-AzureCLI "appservice plan create --name $AppServicePlanName --resource-group $ResourceGroupName --sku B1 --is-linux"
    Write-Host "SUCCESS: App Service Plan created"
}

# Create App Service
Write-Host "Creating App Service: $AppServiceName"
try {
    Invoke-AzureCLI "webapp show --name $AppServiceName --resource-group $ResourceGroupName"
    Write-Host "SUCCESS: App Service already exists"
} catch {
    Invoke-AzureCLI "webapp create --name $AppServiceName --resource-group $ResourceGroupName --plan $AppServicePlanName --runtime DOTNETCORE:8.0"
    Write-Host "SUCCESS: App Service created"
}

# Create SQL Server
Write-Host "Creating SQL Server: $SqlServerName"
try {
    Invoke-AzureCLI "sql server show --name $SqlServerName --resource-group $ResourceGroupName"
    Write-Host "SUCCESS: SQL Server already exists"
} catch {
    Invoke-AzureCLI "sql server create --name $SqlServerName --resource-group $ResourceGroupName --location $Location --admin-user $SqlAdminUser --admin-password $SqlAdminPassword"
    Write-Host "SUCCESS: SQL Server created"
}

# Create SQL Database
Write-Host "Creating SQL Database: $DatabaseName"
try {
    Invoke-AzureCLI "sql db show --name $DatabaseName --server $SqlServerName --resource-group $ResourceGroupName"
    Write-Host "SUCCESS: SQL Database already exists"
} catch {
    Invoke-AzureCLI "sql db create --name $DatabaseName --resource-group $ResourceGroupName --server $SqlServerName --edition Basic --capacity 5"
    Write-Host "SUCCESS: SQL Database created"
}

# Configure SQL Server firewall
Write-Host "Configuring SQL Server firewall rules"
try {
    Invoke-AzureCLI "sql server firewall-rule create --resource-group $ResourceGroupName --server $SqlServerName --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0"
    Write-Host "SUCCESS: Azure services firewall rule created"
} catch {
    Write-Host "WARNING: Azure services firewall rule might already exist"
}

# Get current IP and allow it
$currentIP = Invoke-AzureCLI "rest --method GET --url 'https://api.ipify.org' --query ip -o tsv"
try {
    Invoke-AzureCLI "sql server firewall-rule create --resource-group $ResourceGroupName --server $SqlServerName --name AllowMyIP --start-ip-address $currentIP --end-ip-address $currentIP"
    Write-Host "SUCCESS: Current IP firewall rule created"
} catch {
    Write-Host "WARNING: Current IP firewall rule might already exist"
}

# Configure App Service settings
Write-Host "Configuring App Service settings"
$connectionString = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=$DatabaseName;Persist Security Info=False;User ID=$SqlAdminUser;Password=$SqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings ASPNETCORE_ENVIRONMENT=Production"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings ConnectionStrings__DefaultConnection=`"$connectionString`""
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Jwt__SecretKey=241RunnersAwareness-Super-Secret-Key-2025-Production-Ready-Change-In-Production"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Jwt__Issuer=241RunnersAwareness"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Jwt__Audience=241RunnersAwareness"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Jwt__ExpiryInDays=7"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Jwt__RefreshTokenExpiryInDays=30"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings App__BaseUrl=https://241runnersawareness.org"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings App__ApiUrl=https://$AppServiceName.azurewebsites.net"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings App__Environment=Production"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings App__Version=1.0.0"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings App__MaxFileUploadSize=10485760"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings App__AllowedFileTypes=.jpg,.jpeg,.png,.pdf,.doc,.docx"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings Cors__AllowedOrigins=https://241runnersawareness.org,https://www.241runnersawareness.org,https://app.241runnersawareness.org"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings RateLimiting__PermitLimit=100"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings RateLimiting__Window=00:01:00"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings RateLimiting__SegmentsPerWindow=10"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings HealthChecks__DatabaseTimeout=00:00:05"
Invoke-AzureCLI "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings HealthChecks__MemoryThreshold=1024"

Write-Host "SUCCESS: App Service settings configured"

# Build and deploy application
Write-Host "Building and deploying application"
Set-Location "backend"

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
Invoke-AzureCLI "webapp deployment source config-zip --resource-group $ResourceGroupName --name $AppServiceName --src deployment.zip"

# Clean up
Remove-Item "deployment.zip" -Force
Remove-Item "publish" -Recurse -Force

Write-Host "SUCCESS: Application deployed successfully"

# Run database migrations
Write-Host "Running database migrations"
Invoke-AzureCLI "webapp ssh --name $AppServiceName --resource-group $ResourceGroupName --command 'cd /home/site/wwwroot && dotnet ef database update --connection `"$connectionString`"'"

Write-Host "SUCCESS: Database migrations completed"

# Verify deployment
Write-Host "Verifying deployment"
$appUrl = "https://$AppServiceName.azurewebsites.net"

Write-Host "App Service URL: $appUrl"
Write-Host "Swagger UI: $appUrl/swagger"
Write-Host "Health Check: $appUrl/health"

# Test health endpoint
try {
    $response = Invoke-WebRequest -Uri "$appUrl/health" -UseBasicParsing -TimeoutSec 30
    if ($response.StatusCode -eq 200) {
        Write-Host "SUCCESS: Health check passed"
    } else {
        Write-Host "WARNING: Health check returned status: $($response.StatusCode)"
    }
} catch {
    Write-Host "WARNING: Health check failed: $($_.Exception.Message)"
}

Write-Host "SUCCESS: Deployment verification completed"

Write-Host "Azure deployment completed successfully!"
Write-Host "================================================================"
Write-Host "Your API is now available at: https://$AppServiceName.azurewebsites.net"
Write-Host "Swagger documentation: https://$AppServiceName.azurewebsites.net/swagger"
Write-Host "Health check: https://$AppServiceName.azurewebsites.net/health"
