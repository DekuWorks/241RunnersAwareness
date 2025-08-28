# Get Azure App Service Publish Profile using Azure PowerShell
# This script uses Azure PowerShell module to download the publish profile

param(
    [string]$ResourceGroupName = "241runnersawareness-rg",
    [string]$AppServiceName = "241runnersawareness-api"
)

Write-Host "Getting publish profile using Azure PowerShell..." -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Yellow
Write-Host "App Service: $AppServiceName" -ForegroundColor Yellow

# Check if Azure PowerShell module is installed
try {
    $azModule = Get-Module -ListAvailable -Name Az
    if ($azModule) {
        Write-Host "✅ Azure PowerShell module found: $($azModule.Version)" -ForegroundColor Green
    } else {
        Write-Host "❌ Azure PowerShell module not found. Installing..." -ForegroundColor Yellow
        Install-Module -Name Az -AllowClobber -Force
        Write-Host "✅ Azure PowerShell module installed" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Failed to install Azure PowerShell module: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please run: Install-Module -Name Az -AllowClobber -Force" -ForegroundColor Yellow
    exit 1
}

# Import the module
Import-Module Az

# Check if logged in
try {
    $context = Get-AzContext
    Write-Host "✅ Logged in as: $($context.Account.Id)" -ForegroundColor Green
} catch {
    Write-Host "❌ Not logged into Azure. Please run: Connect-AzAccount" -ForegroundColor Red
    exit 1
}

# Download publish profile
Write-Host "`nDownloading publish profile..." -ForegroundColor Yellow
try {
    $profile = Get-AzWebAppPublishingProfile -ResourceGroupName $ResourceGroupName -Name $AppServiceName -OutputFile "publish-profile.xml"
    Write-Host "✅ Publish profile downloaded successfully!" -ForegroundColor Green
    Write-Host "📁 Profile saved to: publish-profile.xml" -ForegroundColor Green
    
    # Read and display the profile content
    $profileContent = Get-Content "publish-profile.xml" -Raw
    Write-Host "`n📋 Publish Profile Content:" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Gray
    Write-Host $profileContent -ForegroundColor White
    Write-Host "==========================================" -ForegroundColor Gray
    
    Write-Host "`n💡 Copy this content to your GitHub secret: AZURE_WEBAPP_PUBLISH_PROFILE" -ForegroundColor Yellow
    
} catch {
    Write-Host "❌ Failed to download publish profile: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nAlternative methods:" -ForegroundColor Yellow
    Write-Host "1. Enable basic authentication in Azure Portal" -ForegroundColor Gray
    Write-Host "2. Use Azure CLI: az webapp deployment list-publishing-profiles" -ForegroundColor Gray
    Write-Host "3. Generate from Visual Studio" -ForegroundColor Gray
}
