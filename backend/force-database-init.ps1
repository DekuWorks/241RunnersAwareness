# Force Database Initialization Script
# This script ensures the database is properly created and seeded

Write-Host "Forcing Database Initialization..." -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Green

# Navigate to backend directory
Set-Location "backend"

Write-Host "1. Removing existing database..." -ForegroundColor Yellow
if (Test-Path "RunnersDb.db") {
    Remove-Item "RunnersDb.db" -Force
    Write-Host "   SUCCESS: Existing database removed" -ForegroundColor Green
} else {
    Write-Host "   INFO: No existing database found" -ForegroundColor Cyan
}

Write-Host "2. Dropping existing migrations..." -ForegroundColor Yellow
try {
    dotnet ef database drop --force
    Write-Host "   SUCCESS: Existing migrations dropped" -ForegroundColor Green
} catch {
    Write-Host "   INFO: No existing migrations to drop" -ForegroundColor Cyan
}

Write-Host "3. Removing existing migrations..." -ForegroundColor Yellow
if (Test-Path "Migrations") {
    Remove-Item "Migrations" -Recurse -Force
    Write-Host "   SUCCESS: Existing migrations removed" -ForegroundColor Green
} else {
    Write-Host "   INFO: No existing migrations folder found" -ForegroundColor Cyan
}

Write-Host "4. Creating new migration..." -ForegroundColor Yellow
try {
    dotnet ef migrations add InitialCreate
    Write-Host "   SUCCESS: New migration created" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: Failed to create migration: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "5. Applying migration to database..." -ForegroundColor Yellow
try {
    dotnet ef database update
    Write-Host "   SUCCESS: Database migration applied" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: Failed to apply migration: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "6. Building the application..." -ForegroundColor Yellow
try {
    dotnet build -c Release
    Write-Host "   SUCCESS: Application built successfully" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "7. Publishing to Azure..." -ForegroundColor Yellow
try {
    dotnet publish -c Release -o ./publish
    Compress-Archive -Path "./publish/*" -DestinationPath "./publish.zip" -Force
    az webapp deployment source config-zip --resource-group 241runnersawareness-rg --name 241runnersawareness-api --src ./publish.zip
    Write-Host "   SUCCESS: Application deployed to Azure" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Database initialization complete!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Wait 2-3 minutes for Azure to restart the app" -ForegroundColor White
Write-Host "2. Test the API endpoints with: .\test-api-endpoints.ps1" -ForegroundColor White
Write-Host "3. Check the health endpoint: https://241runnersawareness-api.azurewebsites.net/health" -ForegroundColor White

# Return to root directory
Set-Location ".."
