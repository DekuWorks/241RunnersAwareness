# 241 Runners Awareness - Backend Server Startup Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "241 Runners Awareness - Backend Server" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Starting backend server..." -ForegroundColor Yellow
Write-Host ""

# Change to backend directory
Set-Location -Path "backend"

# Start the backend server
dotnet run

# Keep window open if there's an error
Read-Host "Press Enter to continue..."
