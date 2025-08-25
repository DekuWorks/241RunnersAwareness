# 241 Runners Awareness - Quick Environment Setup
# This script sets up environment variables with default values for testing

param(
    [string]$ResourceGroup = "241runnersawareness-rg",
    [string]$AppName = "241runnersawareness-api"
)

Write-Host "Quick Environment Setup for 241 Runners Awareness" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Green
Write-Host ""

# Set default environment variables for testing
$settings = @(
    "ConnectionStrings__DefaultConnection=Data Source=RunnersDb.db",
    "Jwt__SecretKey=your-super-secret-key-with-at-least-32-characters-for-production-testing",
    "SendGrid__ApiKey=SG.your-sendgrid-api-key-here",
    "Twilio__AccountSid=your-twilio-account-sid-here",
    "Twilio__AuthToken=your-twilio-auth-token-here",
    "Twilio__FromNumber=+1234567890",
    "Google__ClientSecret=your-google-client-secret-here",
    "ASPNETCORE_ENVIRONMENT=Production",
    "Cors__AllowedOrigins=https://241runnersawareness.org,https://www.241runnersawareness.org,https://241runnersawareness-web.azurestaticapps.net,http://localhost:3000,http://localhost:5173"
)

Write-Host "Configuring environment variables..." -ForegroundColor Yellow

# Apply settings
try {
    az webapp config appsettings set --resource-group $ResourceGroup --name $AppName --settings $settings --output none
    Write-Host "Environment variables configured successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error configuring environment variables: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Restarting the application..." -ForegroundColor Yellow
try {
    az webapp restart --resource-group $ResourceGroup --name $AppName --output none
    Write-Host "Application restarted successfully!" -ForegroundColor Green
} catch {
    Write-Host "Could not restart application automatically. Please restart manually in Azure Portal." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Testing the configuration..." -ForegroundColor Yellow

# Wait a moment for the app to restart
Start-Sleep -Seconds 15

# Test the health endpoint
try {
    $healthResponse = Invoke-RestMethod -Uri "https://$AppName.azurewebsites.net/health" -Method Get -TimeoutSec 10
    Write-Host "Health check passed: $($healthResponse.status)" -ForegroundColor Green
} catch {
    Write-Host "Health check failed. This is normal during restart. Try again in a few minutes." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Quick Setup Complete!" -ForegroundColor Green
Write-Host "=======================" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "SQLite database configured" -ForegroundColor White
Write-Host "Default JWT Secret Key configured" -ForegroundColor White
Write-Host "Placeholder API keys configured" -ForegroundColor White
Write-Host "CORS settings configured" -ForegroundColor White
Write-Host "Application restarted" -ForegroundColor White
Write-Host ""
Write-Host "Your API is available at:" -ForegroundColor Cyan
Write-Host "   https://$AppName.azurewebsites.net" -ForegroundColor White
Write-Host "   https://$AppName.azurewebsites.net/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Important:" -ForegroundColor Yellow
Write-Host "   - Replace placeholder API keys with real ones for production" -ForegroundColor White
Write-Host "   - Update JWT secret key for security" -ForegroundColor White
Write-Host "   - Configure SendGrid, Twilio, and Google OAuth properly" -ForegroundColor White
Write-Host ""
Write-Host "Test URLs:" -ForegroundColor Cyan
Write-Host "   Health Check: https://$AppName.azurewebsites.net/health" -ForegroundColor White
Write-Host "   API Docs: https://$AppName.azurewebsites.net/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Test the map functionality" -ForegroundColor White
Write-Host "   2. Configure real API keys when ready" -ForegroundColor White
Write-Host "   3. Deploy frontend to connect to this API" -ForegroundColor White
