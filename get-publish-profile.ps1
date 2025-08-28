# Get Azure App Service Publish Profile
# This script uses Azure CLI to download the publish profile

param(
    [string]$ResourceGroupName = "241runnersawareness-rg",
    [string]$AppServiceName = "241runnersawareness-api"
)

Write-Host "Getting publish profile for Azure App Service..." -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Yellow
Write-Host "App Service: $AppServiceName" -ForegroundColor Yellow

# Check if Azure CLI is installed
try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-Host "✅ Azure CLI found: $($azVersion.'azure-cli')" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI not found. Please install it first:" -ForegroundColor Red
    Write-Host "   winget install Microsoft.AzureCLI" -ForegroundColor Yellow
    Write-Host "   Or download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Check if logged in
try {
    $account = az account show --output json | ConvertFrom-Json
    Write-Host "✅ Logged in as: $($account.user.name)" -ForegroundColor Green
} catch {
    Write-Host "❌ Not logged into Azure. Please run: az login" -ForegroundColor Red
    exit 1
}

# Download publish profile
Write-Host "`nDownloading publish profile..." -ForegroundColor Yellow
try {
    $profile = az webapp deployment list-publishing-profiles --resource-group $ResourceGroupName --name $AppServiceName --xml --output tsv
    Write-Host "✅ Publish profile downloaded successfully!" -ForegroundColor Green
    
    # Save to file
    $profilePath = "publish-profile.xml"
    $profile | Out-File -FilePath $profilePath -Encoding UTF8
    Write-Host "📁 Profile saved to: $profilePath" -ForegroundColor Green
    
    # Display the profile content
    Write-Host "`n📋 Publish Profile Content:" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Gray
    Write-Host $profile -ForegroundColor White
    Write-Host "==========================================" -ForegroundColor Gray
    
    Write-Host "`n💡 Copy this content to your GitHub secret: AZURE_WEBAPP_PUBLISH_PROFILE" -ForegroundColor Yellow
    
} catch {
    Write-Host "❌ Failed to download publish profile: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nAlternative methods:" -ForegroundColor Yellow
    Write-Host "1. Try enabling basic authentication in Azure Portal" -ForegroundColor Gray
    Write-Host "2. Use Azure PowerShell: Get-AzWebAppPublishingProfile" -ForegroundColor Gray
    Write-Host "3. Generate a new publish profile from Visual Studio" -ForegroundColor Gray
}
