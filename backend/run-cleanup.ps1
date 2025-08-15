# Database Cleanup Script
# This script authenticates as admin and runs cleanup operations

param(
    [string]$ApiUrl = "https://241runnersawareness-api.azurewebsites.net",
    [string]$AdminEmail = "admin@241runners.org",
    [string]$AdminPassword = "admin123"
)

Write-Host "Starting database cleanup operations..." -ForegroundColor Green

# Step 1: Authenticate as admin
Write-Host "Authenticating as admin..." -ForegroundColor Yellow
$loginBody = @{
    email = $AdminEmail
    password = $AdminPassword
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$ApiUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "Authentication successful!" -ForegroundColor Green
} catch {
    Write-Host "Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Set up headers with JWT token
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Step 2: Remove test data
Write-Host "Removing test data..." -ForegroundColor Yellow
try {
    $testDataResponse = Invoke-RestMethod -Uri "$ApiUrl/api/cleanup/remove-test-data" -Method POST -Headers $headers
    Write-Host "Test data removal: $($testDataResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "Test data removal failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Remove duplicates
Write-Host "Removing duplicate users..." -ForegroundColor Yellow
try {
    $duplicatesResponse = Invoke-RestMethod -Uri "$ApiUrl/api/cleanup/remove-duplicates" -Method POST -Headers $headers
    Write-Host "Duplicate removal: $($duplicatesResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "Duplicate removal failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 4: Remove orphaned records
Write-Host "Removing orphaned records..." -ForegroundColor Yellow
try {
    $orphanedResponse = Invoke-RestMethod -Uri "$ApiUrl/api/cleanup/remove-orphaned" -Method POST -Headers $headers
    Write-Host "Orphaned records removal: $($orphanedResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "Orphaned records removal failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 5: Run full cleanup report
Write-Host "Generating full cleanup report..." -ForegroundColor Yellow
try {
    $fullCleanupResponse = Invoke-RestMethod -Uri "$ApiUrl/api/cleanup/full-cleanup" -Method POST -Headers $headers
    Write-Host "Full cleanup completed!" -ForegroundColor Green
    Write-Host "Report: $($fullCleanupResponse | ConvertTo-Json -Depth 3)" -ForegroundColor Cyan
} catch {
    Write-Host "Full cleanup failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Database cleanup operations completed!" -ForegroundColor Green
