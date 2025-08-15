# Azure Deployment Script for 241 Runners Awareness Backend
# This script handles the complete Azure deployment process

param(
    [string]$ResourceGroupName = "241runnersawareness-rg",
    [string]$Location = "East US",
    [string]$AppServiceName = "241runnersawareness-api",
    [string]$AppServicePlanName = "241runners-api-plan",
    [string]$SqlServerName = "241runners-sql-server",
    [string]$DatabaseName = "RunnersDb",
    [string]$SqlAdminUser = "sqladmin",
    [string]$SqlAdminPassword = "YourStrongPassword123!",
    [switch]$SkipLogin,
    [switch]$SkipResourceCreation,
    [switch]$DeployOnly
)

# Function to find Azure CLI
function Find-AzureCLI {
    $possiblePaths = @(
        "C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\wbin\az.exe",
        "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.exe",
        "C:\Users\$env:USERNAME\AppData\Local\Programs\Azure CLI\az.exe"
    )
    
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    
    return $null
}

# Function to execute Azure CLI commands
function Invoke-AzureCLI {
    param([string]$Arguments)
    
    $azPath = Find-AzureCLI
    if (-not $azPath) {
        Write-Error "Azure CLI not found. Please install Azure CLI first."
        exit 1
    }
    
    Write-Host "Executing: $azPath $Arguments"
    $result = & $azPath $Arguments.Split(' ')
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Azure CLI command failed: $Arguments"
        exit 1
    }
    
    return $result
}

# Function to check if user is logged in
function Test-AzureLogin {
    try {
        $result = Invoke-AzureCLI "account show"
        Write-Host "✅ Already logged into Azure"
        return $true
    }
    catch {
        Write-Host "❌ Not logged into Azure"
        return $false
    }
}

# Function to login to Azure
function Login-Azure {
    Write-Host "🔐 Logging into Azure..."
    Invoke-AzureCLI "login"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Successfully logged into Azure"
    } else {
        Write-Error "❌ Failed to login to Azure"
        exit 1
    }
}

# Function to create resource group
function New-AzureResourceGroup {
    Write-Host "🏗️ Creating resource group: $ResourceGroupName"
    
    $existing = Invoke-AzureCLI "group show --name $ResourceGroupName --query name -o tsv" 2>$null
    if ($existing) {
        Write-Host "✅ Resource group already exists: $ResourceGroupName"
        return
    }
    
    Invoke-AzureCLI "group create --name $ResourceGroupName --location $Location --tags Project=241RunnersAwareness Environment=Production"
    Write-Host "✅ Resource group created: $ResourceGroupName"
}

# Function to create App Service Plan
function New-AzureAppServicePlan {
    Write-Host "📋 Creating App Service Plan: $AppServicePlanName"
    
    $existing = Invoke-AzureCLI "appservice plan show --name $AppServicePlanName --resource-group $ResourceGroupName --query name -o tsv" 2>$null
    if ($existing) {
        Write-Host "✅ App Service Plan already exists: $AppServicePlanName"
        return
    }
    
    Invoke-AzureCLI "appservice plan create --name $AppServicePlanName --resource-group $ResourceGroupName --sku B1 --is-linux"
    Write-Host "✅ App Service Plan created: $AppServicePlanName"
}

# Function to create App Service
function New-AzureAppService {
    Write-Host "🌐 Creating App Service: $AppServiceName"
    
    $existing = Invoke-AzureCLI "webapp show --name $AppServiceName --resource-group $ResourceGroupName --query name -o tsv" 2>$null
    if ($existing) {
        Write-Host "✅ App Service already exists: $AppServiceName"
        return
    }
    
    Invoke-AzureCLI "webapp create --name $AppServiceName --resource-group $ResourceGroupName --plan $AppServicePlanName --runtime DOTNETCORE:8.0"
    Write-Host "✅ App Service created: $AppServiceName"
}

# Function to create SQL Server
function New-AzureSqlServer {
    Write-Host "🗄️ Creating SQL Server: $SqlServerName"
    
    $existing = Invoke-AzureCLI "sql server show --name $SqlServerName --resource-group $ResourceGroupName --query name -o tsv" 2>$null
    if ($existing) {
        Write-Host "✅ SQL Server already exists: $SqlServerName"
        return
    }
    
    Invoke-AzureCLI "sql server create --name $SqlServerName --resource-group $ResourceGroupName --location $Location --admin-user $SqlAdminUser --admin-password $SqlAdminPassword"
    Write-Host "✅ SQL Server created: $SqlServerName"
}

# Function to create SQL Database
function New-AzureSqlDatabase {
    Write-Host "📊 Creating SQL Database: $DatabaseName"
    
    $existing = Invoke-AzureCLI "sql db show --name $DatabaseName --server $SqlServerName --resource-group $ResourceGroupName --query name -o tsv" 2>$null
    if ($existing) {
        Write-Host "✅ SQL Database already exists: $DatabaseName"
        return
    }
    
    Invoke-AzureCLI "sql db create --name $DatabaseName --resource-group $ResourceGroupName --server $SqlServerName --edition Basic --capacity 5"
    Write-Host "✅ SQL Database created: $DatabaseName"
}

# Function to configure SQL Server firewall
function Set-AzureSqlFirewall {
    Write-Host "🔥 Configuring SQL Server firewall rules"
    
    # Allow Azure services
    Invoke-AzureCLI "sql server firewall-rule create --resource-group $ResourceGroupName --server $SqlServerName --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0"
    
    # Get current IP and allow it
    $currentIP = Invoke-AzureCLI "rest --method GET --url 'https://api.ipify.org' --query ip -o tsv"
    Invoke-AzureCLI "sql server firewall-rule create --resource-group $ResourceGroupName --server $SqlServerName --name AllowMyIP --start-ip-address $currentIP --end-ip-address $currentIP"
    
    Write-Host "✅ SQL Server firewall configured"
}

# Function to configure App Service settings
function Set-AzureAppServiceSettings {
    Write-Host "⚙️ Configuring App Service settings"
    
    $connectionString = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=$DatabaseName;Persist Security Info=False;User ID=$SqlAdminUser;Password=$SqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    
    # Set settings one by one to avoid array syntax issues
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
    
    Write-Host "✅ App Service settings configured"
}

# Function to build and deploy the application
function Deploy-AzureApplication {
    Write-Host "🚀 Building and deploying application"
    
    # Change to backend directory
    Set-Location "backend"
    
    # Build the application
    Write-Host "📦 Building application..."
    dotnet publish -c Release -o ./publish
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Build failed"
        exit 1
    }
    
    # Create deployment package
    Write-Host "📦 Creating deployment package..."
    if (Test-Path "deployment.zip") {
        Remove-Item "deployment.zip" -Force
    }
    
    Compress-Archive -Path "./publish/*" -DestinationPath "deployment.zip"
    
    # Deploy to Azure
    Write-Host "🚀 Deploying to Azure..."
    Invoke-AzureCLI "webapp deployment source config-zip --resource-group $ResourceGroupName --name $AppServiceName --src deployment.zip"
    
    # Clean up
    Remove-Item "deployment.zip" -Force
    Remove-Item "publish" -Recurse -Force
    
    Write-Host "✅ Application deployed successfully"
}

# Function to run database migrations
function Invoke-AzureDatabaseMigration {
    Write-Host "🗄️ Running database migrations"
    
    $connectionString = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=$DatabaseName;Persist Security Info=False;User ID=$SqlAdminUser;Password=$SqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    
    # SSH into the app service and run migrations
    Write-Host "🔧 Running EF migrations..."
    Invoke-AzureCLI "webapp ssh --name $AppServiceName --resource-group $ResourceGroupName --command 'cd /home/site/wwwroot && dotnet ef database update --connection `"$connectionString`"'"
    
    Write-Host "✅ Database migrations completed"
}

# Function to verify deployment
function Test-AzureDeployment {
    Write-Host "🔍 Verifying deployment"
    
    $appUrl = "https://$AppServiceName.azurewebsites.net"
    
    Write-Host "🌐 App Service URL: $appUrl"
    Write-Host "📚 Swagger UI: $appUrl/swagger"
    Write-Host "❤️ Health Check: $appUrl/health"
    
    # Test health endpoint
    try {
        $response = Invoke-WebRequest -Uri "$appUrl/health" -UseBasicParsing -TimeoutSec 30
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Health check passed"
        } else {
            Write-Host "⚠️ Health check returned status: $($response.StatusCode)"
        }
    }
    catch {
        Write-Host "⚠️ Health check failed: $($_.Exception.Message)"
    }
    
    Write-Host "✅ Deployment verification completed"
}

# Main execution
Write-Host "🚀 Starting Azure deployment for 241 Runners Awareness Backend"
Write-Host "================================================================"

# Check Azure CLI
$azPath = Find-AzureCLI
if (-not $azPath) {
    Write-Error "❌ Azure CLI not found. Please install Azure CLI first."
    Write-Host "💡 You can install it using: winget install Microsoft.AzureCLI"
    exit 1
}

Write-Host "✅ Azure CLI found at: $azPath"

# Login to Azure (if not skipped)
if (-not $SkipLogin) {
    if (-not (Test-AzureLogin)) {
        Login-Azure
    }
}

# Create Azure resources (if not skipped)
if (-not $SkipResourceCreation) {
    New-AzureResourceGroup
    New-AzureAppServicePlan
    New-AzureAppService
    New-AzureSqlServer
    New-AzureSqlDatabase
    Set-AzureSqlFirewall
    Set-AzureAppServiceSettings
}

# Deploy application
if (-not $DeployOnly) {
    Deploy-AzureApplication
    Invoke-AzureDatabaseMigration
    Test-AzureDeployment
}

Write-Host "🎉 Azure deployment completed successfully!"
Write-Host "================================================================"
Write-Host "🌐 Your API is now available at: https://$AppServiceName.azurewebsites.net"
Write-Host "📚 Swagger documentation: https://$AppServiceName.azurewebsites.net/swagger"
Write-Host "❤️ Health check: https://$AppServiceName.azurewebsites.net/health"
