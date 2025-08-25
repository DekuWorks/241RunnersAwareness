# 241 Runners Awareness - Download Database Script
# This script downloads the SQLite database from Azure for local development

param(
    [string]$ResourceGroup = "241runnersawareness-rg",
    [string]$AppName = "241runnersawareness-api",
    [string]$LocalPath = "./RunnersDb.db"
)

Write-Host "📥 Downloading SQLite Database from Azure" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

Write-Host "🔍 Checking if database exists on Azure..." -ForegroundColor Yellow

# Check if the database file exists on Azure
try {
    $files = az webapp ssh --name $AppName --resource-group $ResourceGroup --command "ls -la RunnersDb.db" --output json | ConvertFrom-Json
    Write-Host "✅ Database file found on Azure" -ForegroundColor Green
} catch {
    Write-Host "❌ Database file not found on Azure. Please ensure the application has been deployed and run at least once." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "📥 Downloading database file..." -ForegroundColor Yellow

# Download the database file using Azure CLI
try {
    az webapp ssh --name $AppName --resource-group $ResourceGroup --command "cp RunnersDb.db /tmp/RunnersDb.db"
    az webapp files download --name $AppName --resource-group $ResourceGroup --source-path "/tmp/RunnersDb.db" --target-path $LocalPath
    Write-Host "✅ Database downloaded successfully to: $LocalPath" -ForegroundColor Green
} catch {
    Write-Host "❌ Error downloading database: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 Alternative method: Use Azure Portal to download the file" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "🔍 Verifying database file..." -ForegroundColor Yellow

# Check if the file was downloaded
if (Test-Path $LocalPath) {
    $fileSize = (Get-Item $LocalPath).Length
    Write-Host "✅ Database file verified: $fileSize bytes" -ForegroundColor Green
} else {
    Write-Host "❌ Database file not found locally" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🎉 Database Download Complete!" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Summary:" -ForegroundColor Cyan
Write-Host "✅ Database downloaded from Azure" -ForegroundColor White
Write-Host "✅ File saved to: $LocalPath" -ForegroundColor White
Write-Host "✅ File size: $fileSize bytes" -ForegroundColor White
Write-Host ""
Write-Host "💡 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Open the database with DB Browser for SQLite" -ForegroundColor White
Write-Host "   2. Explore the tables and data" -ForegroundColor White
Write-Host "   3. Share the database file with your development team" -ForegroundColor White
Write-Host "   4. Use for local development and testing" -ForegroundColor White
Write-Host ""
Write-Host "🔧 To open with DB Browser for SQLite:" -ForegroundColor Cyan
Write-Host "   - Download DB Browser for SQLite from: https://sqlitebrowser.org/" -ForegroundColor White
Write-Host "   - Open the application" -ForegroundColor White
Write-Host "   - Click 'Open Database' and select: $LocalPath" -ForegroundColor White
Write-Host ""
Write-Host "📊 Database Tables:" -ForegroundColor Cyan
Write-Host "   - Users (authentication and user management)" -ForegroundColor White
Write-Host "   - Individuals (missing persons data)" -ForegroundColor White
Write-Host "   - Cases (case management)" -ForegroundColor White
Write-Host "   - DNAReports (DNA tracking)" -ForegroundColor White
Write-Host "   - EmergencyContacts (emergency contact info)" -ForegroundColor White
Write-Host ""
Write-Host "⚠️ Important Notes:" -ForegroundColor Yellow
Write-Host "   - Keep the database file secure" -ForegroundColor White
Write-Host "   - Don't commit the database file to version control" -ForegroundColor White
Write-Host "   - Make regular backups of the database" -ForegroundColor White
Write-Host "   - Update the database file periodically from production" -ForegroundColor White
