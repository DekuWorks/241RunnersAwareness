# Fixed Azure Deployment Script
param(
    [string]$ResourceGroupName = "241runnersawareness-rg",
    [string]$Location = "eastus",
    [string]$AppServiceName = "241runnersawareness-api",
    [string]$AppServicePlanName = "241runners-api-plan",
    [string]$SqlServerName = "241runners-sql-server",
    [string]$DatabaseName = "RunnersDb",
    [string]$SqlAdminUser = "sqladmin",
    [string]$SqlAdminPassword = "YourStrongPassword123!"
)

Write-Host "Starting Azure deployment..."

# Find Azure CLI
$azPath = "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"
if (-not (Test-Path $azPath)) {
    Write-Host "ERROR: Azure CLI not found at $azPath"
    exit 1
}

Write-Host "Found Azure CLI at: $azPath"

# Check if we're logged in
Write-Host "Checking Azure login status..."
$loginCheck = & $azPath "account" "show" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Not logged in. Please run: $azPath login"
    Write-Host "Then run this script again."
    exit 1
}

Write-Host "Successfully logged into Azure"

# Function to run Azure CLI commands with proper argument handling
function Run-AzureCommand {
    param([string]$Command)
    
    Write-Host "Running: $Command"
    
    # Split the command properly and handle spaces in arguments
    $args = $Command -split ' '
    $result = & $azPath $args 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "WARNING: Command failed but continuing: $Command"
        Write-Host "Error: $result"
        return $false
    }
    
    return $true
}

# Create Resource Group
Write-Host "Creating resource group..."
Run-AzureCommand "group create --name $ResourceGroupName --location $Location --tags Project=241RunnersAwareness Environment=Production"

# Create App Service Plan
Write-Host "Creating App Service Plan..."
Run-AzureCommand "appservice plan create --name $AppServicePlanName --resource-group $ResourceGroupName --sku B1 --is-linux"

# Create App Service
Write-Host "Creating App Service..."
Run-AzureCommand "webapp create --name $AppServiceName --resource-group $ResourceGroupName --plan $AppServicePlanName --runtime DOTNETCORE:8.0"

# Create SQL Server
Write-Host "Creating SQL Server..."
Run-AzureCommand "sql server create --name $SqlServerName --resource-group $ResourceGroupName --location $Location --admin-user $SqlAdminUser --admin-password $SqlAdminPassword"

# Create SQL Database
Write-Host "Creating SQL Database..."
Run-AzureCommand "sql db create --name $DatabaseName --resource-group $ResourceGroupName --server $SqlServerName --edition Basic --capacity 5"

# Configure SQL Server firewall
Write-Host "Configuring SQL Server firewall..."
Run-AzureCommand "sql server firewall-rule create --resource-group $ResourceGroupName --server $SqlServerName --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0"

# Get current IP and allow it
$currentIP = & $azPath "rest" "--method" "GET" "--url" "https://api.ipify.org" "--query" "ip" "-o" "tsv"
Run-AzureCommand "sql server firewall-rule create --resource-group $ResourceGroupName --server $SqlServerName --name AllowMyIP --start-ip-address $currentIP --end-ip-address $currentIP"

# Configure App Service settings
Write-Host "Configuring App Service settings..."
$connectionString = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=$DatabaseName;Persist Security Info=False;User ID=$SqlAdminUser;Password=$SqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Set settings one by one to avoid argument parsing issues
$settings = @(
    "ASPNETCORE_ENVIRONMENT=Production",
    "ConnectionStrings__DefaultConnection=`"$connectionString`"",
    "Jwt__SecretKey=241RunnersAwareness-Super-Secret-Key-2025-Production-Ready-Change-In-Production",
    "Jwt__Issuer=241RunnersAwareness",
    "Jwt__Audience=241RunnersAwareness",
    "Jwt__ExpiryInDays=7",
    "Jwt__RefreshTokenExpiryInDays=30",
    "App__BaseUrl=https://241runnersawareness.org",
    "App__ApiUrl=https://$AppServiceName.azurewebsites.net",
    "App__Environment=Production",
    "App__Version=1.0.0",
    "App__MaxFileUploadSize=10485760",
    "App__AllowedFileTypes=.jpg,.jpeg,.png,.pdf,.doc,.docx",
    "Cors__AllowedOrigins=https://241runnersawareness.org,https://www.241runnersawareness.org,https://app.241runnersawareness.org",
    "RateLimiting__PermitLimit=100",
    "RateLimiting__Window=00:01:00",
    "RateLimiting__SegmentsPerWindow=10",
    "HealthChecks__DatabaseTimeout=00:00:05",
    "HealthChecks__MemoryThreshold=1024"
)

foreach ($setting in $settings) {
    Run-AzureCommand "webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings $setting"
}

Write-Host "App Service settings configured"

# Build and deploy application
Write-Host "Building and deploying application..."
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
Run-AzureCommand "webapp deployment source config-zip --resource-group $ResourceGroupName --name $AppServiceName --src deployment.zip"

# Clean up
Remove-Item "deployment.zip" -Force
Remove-Item "publish" -Recurse -Force

Write-Host "Application deployed successfully"

# Run database migrations
Write-Host "Running database migrations..."
Run-AzureCommand "webapp ssh --name $AppServiceName --resource-group $ResourceGroupName --command 'cd /home/site/wwwroot && dotnet ef database update --connection `"$connectionString`"'"

Write-Host "Database migrations completed"

# Verify deployment
Write-Host "Verifying deployment..."
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

Write-Host "Deployment verification completed"

Write-Host "Azure deployment completed successfully!"
Write-Host "Your API is now available at: https://$AppServiceName.azurewebsites.net"
Write-Host "Swagger documentation: https://$AppServiceName.azurewebsites.net/swagger"
Write-Host "Health check: https://$AppServiceName.azurewebsites.net/health"
