# Set Azure Environment Variables for 241 Runners Awareness Backend
# This script sets up all the necessary environment variables in Azure App Service

$ResourceGroupName = "241runnersawareness-rg"
$AppServiceName = "241runnersawareness-api"

Write-Host "Setting up environment variables for Azure App Service..."
Write-Host "================================================================"

# Set environment variables
$settings = @{
    "ASPNETCORE_ENVIRONMENT" = "Production"
    "DB_CONNECTION_STRING" = "Server=241runners-sql-server-2025.database.windows.net;Database=RunnersDb;User Id=sqladmin;Password=YourStrongPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;Command Timeout=30;"
    "JWT_SECRET_KEY" = "241RunnersAwareness-Production-Secret-Key-2025-Change-In-Production-64-Chars-Minimum"
    "JWT_ISSUER" = "241RunnersAwareness"
    "JWT_AUDIENCE" = "241RunnersAwareness"
    "JWT_EXPIRY_IN_MINUTES" = "60"
    "JWT_REFRESH_TOKEN_EXPIRY_IN_DAYS" = "30"
    "GOOGLE_CLIENT_ID" = "933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com"
    "GOOGLE_CLIENT_SECRET" = "your-google-client-secret"
    "SENDGRID_API_KEY" = "your-sendgrid-api-key"
    "TWILIO_ACCOUNT_SID" = "your-twilio-account-sid"
    "TWILIO_AUTH_TOKEN" = "your-twilio-auth-token"
    "TWILIO_FROM_NUMBER" = "your-twilio-phone-number"
    "APP_BASE_URL" = "https://241runnersawareness.org"
    "APP_API_URL" = "https://241runnersawareness-api.azurewebsites.net"
    "APP_ENVIRONMENT" = "Production"
    "APP_VERSION" = "1.0.0"
    "RATE_LIMITING_PERMIT_LIMIT" = "100"
    "RATE_LIMITING_WINDOW" = "00:01:00"
    "HEALTH_CHECKS_MEMORY_THRESHOLD" = "1024"
}

# Convert settings to Azure CLI format
$azureSettings = @()
foreach ($key in $settings.Keys) {
    $azureSettings += "$key=$($settings[$key])"
}

# Set all environment variables
Write-Host "Setting environment variables..."
az webapp config appsettings set --resource-group $ResourceGroupName --name $AppServiceName --settings $azureSettings

Write-Host "Environment variables set successfully!"
Write-Host "Restarting App Service..."

# Restart the app service
az webapp restart --resource-group $ResourceGroupName --name $AppServiceName

Write-Host "App Service restarted!"
Write-Host "================================================================"
Write-Host "Environment variables have been set and the app has been restarted."
Write-Host "The API should now be accessible at: https://241runnersawareness-api.azurewebsites.net"
