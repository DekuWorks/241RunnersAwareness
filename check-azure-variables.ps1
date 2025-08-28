# Check Azure App Service Environment Variables
# This script queries the current environment variables configured in Azure

$ResourceGroupName = "241runnersawareness-rg"
$AppServiceName = "241runnersawareness-api"

Write-Host "Checking Azure App Service Environment Variables" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green
Write-Host ""

try {
    # Get current app settings
    Write-Host "Fetching current environment variables..." -ForegroundColor Yellow
    $currentSettings = az webapp config appsettings list --resource-group $ResourceGroupName --name $AppServiceName --query "[].{name:name, value:value}" --output json | ConvertFrom-Json
    
    Write-Host "Current Environment Variables:" -ForegroundColor Cyan
    Write-Host "=============================" -ForegroundColor Cyan
    
    foreach ($setting in $currentSettings) {
        $value = $setting.value
        # Mask sensitive values
        if ($setting.name -like "*PASSWORD*" -or $setting.name -like "*SECRET*" -or $setting.name -like "*KEY*" -or $setting.name -like "*CONNECTION*") {
            if ($value -and $value.Length -gt 10) {
                $value = $value.Substring(0, 10) + "***"
            } else {
                $value = "***"
            }
        }
        
        Write-Host "  $($setting.name): $value" -ForegroundColor White
    }
    
    Write-Host ""
    Write-Host "Checking for required variables..." -ForegroundColor Yellow
    
    # Check for required variables
    $requiredVars = @(
        "ASPNETCORE_ENVIRONMENT",
        "DB_CONNECTION_STRING", 
        "JWT_SECRET_KEY"
    )
    
    $missingVars = @()
    foreach ($requiredVar in $requiredVars) {
        $found = $currentSettings | Where-Object { $_.name -eq $requiredVar }
        if ($found) {
            Write-Host "  ✅ $requiredVar: Found" -ForegroundColor Green
        } else {
            Write-Host "  ❌ $requiredVar: Missing" -ForegroundColor Red
            $missingVars += $requiredVar
        }
    }
    
    if ($missingVars.Count -gt 0) {
        Write-Host ""
        Write-Host "Missing required variables:" -ForegroundColor Red
        foreach ($missingVar in $missingVars) {
            Write-Host "  - $missingVar" -ForegroundColor Red
        }
    } else {
        Write-Host ""
        Write-Host "All required variables are present!" -ForegroundColor Green
    }
    
} catch {
    Write-Host "Error checking environment variables: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Script completed!" -ForegroundColor Green
