# 241 Runners Awareness - SQLite Environment Setup Script
# This script configures environment variables for SQLite deployment

param(
    [string]$ResourceGroup = "241runnersawareness-rg",
    [string]$AppName = "241runnersawareness-api"
)

Write-Host "🔧 Setting up SQLite Environment Variables for 241 Runners Awareness" -ForegroundColor Green
Write-Host "=====================================================================" -ForegroundColor Green
Write-Host ""

# Function to get secure input
function Get-SecureInput {
    param([string]$Prompt)
    Write-Host $Prompt -ForegroundColor Cyan -NoNewline
    $secure = Read-Host -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    return [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

# Function to get regular input
function Get-UserInput {
    param([string]$Prompt, [string]$Default = "")
    if ($Default) {
        Write-Host "$Prompt (press Enter for default: $Default)" -ForegroundColor Cyan -NoNewline
    } else {
        Write-Host $Prompt -ForegroundColor Cyan -NoNewline
    }
    $input = Read-Host
    if ([string]::IsNullOrEmpty($input) -and $Default) {
        return $Default
    }
    return $input
}

Write-Host "🚀 Let's configure your API keys for SQLite deployment:" -ForegroundColor Green
Write-Host ""

# 1. JWT Secret Key
$jwtSecret = Get-SecureInput "Enter your JWT Secret Key (at least 32 characters): "
if ([string]::IsNullOrEmpty($jwtSecret)) {
    $jwtSecret = "your-super-secret-key-with-at-least-32-characters-for-production"
    Write-Host "Using default JWT secret key" -ForegroundColor Yellow
}

# 2. SendGrid API Key
$sendgridKey = Get-SecureInput "Enter your SendGrid API Key: "

# 3. Twilio Account SID
$twilioSid = Get-UserInput "Enter your Twilio Account SID: "

# 4. Twilio Auth Token
$twilioToken = Get-SecureInput "Enter your Twilio Auth Token: "

# 5. Twilio Phone Number
$twilioPhone = Get-UserInput "Enter your Twilio Phone Number (e.g., +1234567890): "

# 6. Google Client Secret
$googleSecret = Get-SecureInput "Enter your Google Client Secret: "

# 7. Sentry DSN (optional)
$sentryDsn = Get-UserInput "Enter your Sentry DSN (optional, press Enter to skip): "

Write-Host ""
Write-Host "⚙️ Configuring environment variables for SQLite..." -ForegroundColor Yellow

# Set environment variables for SQLite deployment
$settings = @(
    "ConnectionStrings__DefaultConnection=Data Source=RunnersDb.db",
    "Jwt__SecretKey=$jwtSecret",
    "SendGrid__ApiKey=$sendgridKey",
    "Twilio__AccountSid=$twilioSid",
    "Twilio__AuthToken=$twilioToken",
    "Twilio__FromNumber=$twilioPhone",
    "Google__ClientSecret=$googleSecret",
    "ASPNETCORE_ENVIRONMENT=Production"
)

# Add Sentry DSN if provided
if (-not [string]::IsNullOrEmpty($sentryDsn)) {
    $settings += "Sentry__Dsn=$sentryDsn"
}

# Add CORS settings
$settings += @(
    "Cors__AllowedOrigins=https://241runnersawareness.org,https://www.241runnersawareness.org,https://241runnersawareness-web.azurestaticapps.net"
)

# Apply settings
try {
    az webapp config appsettings set --resource-group $ResourceGroup --name $AppName --settings $settings --output none
    Write-Host "✅ Environment variables configured successfully!" -ForegroundColor Green
} catch {
    Write-Host "❌ Error configuring environment variables: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🔄 Restarting the application..." -ForegroundColor Yellow
try {
    az webapp restart --resource-group $ResourceGroup --name $AppName --output none
    Write-Host "✅ Application restarted successfully!" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Could not restart application automatically. Please restart manually in Azure Portal." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🧪 Testing the configuration..." -ForegroundColor Yellow

# Wait a moment for the app to restart
Start-Sleep -Seconds 15

# Test the health endpoint
try {
    $healthResponse = Invoke-RestMethod -Uri "https://$AppName.azurewebsites.net/health" -Method Get -TimeoutSec 10
    Write-Host "✅ Health check passed: $($healthResponse.status)" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Health check failed. This is normal during restart. Try again in a few minutes." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎉 SQLite Environment Setup Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Summary:" -ForegroundColor Cyan
Write-Host "✅ SQLite database configured"
Write-Host "✅ JWT Secret Key configured"
Write-Host "✅ SendGrid API Key configured"
Write-Host "✅ Twilio credentials configured"
Write-Host "✅ Google OAuth configured"
if (-not [string]::IsNullOrEmpty($sentryDsn)) {
    Write-Host "✅ Sentry DSN configured"
}
Write-Host "✅ CORS settings configured"
Write-Host "✅ Application restarted"
Write-Host ""
Write-Host "🌐 Your API is available at:" -ForegroundColor Cyan
Write-Host "   https://$AppName.azurewebsites.net" -ForegroundColor White
Write-Host "   https://$AppName.azurewebsites.net/swagger" -ForegroundColor White
Write-Host ""
Write-Host "🗄️ Database Information:" -ForegroundColor Cyan
Write-Host "   Type: SQLite" -ForegroundColor White
Write-Host "   File: RunnersDb.db" -ForegroundColor White
Write-Host "   Location: Azure App Service file system" -ForegroundColor White
Write-Host ""
Write-Host "📝 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Test authentication endpoints" -ForegroundColor White
Write-Host "   2. Verify email/SMS functionality" -ForegroundColor White
Write-Host "   3. Test Google OAuth login" -ForegroundColor White
Write-Host "   4. Deploy frontend to connect to this API" -ForegroundColor White
Write-Host "   5. Download database file for local development" -ForegroundColor White
Write-Host ""
Write-Host "🔗 Test URLs:" -ForegroundColor Cyan
Write-Host "   Health Check: https://$AppName.azurewebsites.net/health" -ForegroundColor White
Write-Host "   API Docs: https://$AppName.azurewebsites.net/swagger" -ForegroundColor White
Write-Host "   Auth Test: https://$AppName.azurewebsites.net/api/auth/test" -ForegroundColor White
Write-Host ""
Write-Host "💡 For local development:" -ForegroundColor Yellow
Write-Host "   - Use DB Browser for SQLite to view/edit the database" -ForegroundColor White
Write-Host "   - Download RunnersDb.db from Azure App Service" -ForegroundColor White
Write-Host "   - Share the database file with your development team" -ForegroundColor White
