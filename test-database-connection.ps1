# Test Database Connection and Admin User Creation
# This script tests the backend API endpoints to diagnose NULL values issue

$BaseUrl = "https://241runnersawareness-api.azurewebsites.net"

Write-Host "Testing Database Connection and Admin User Creation" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
Write-Host ""

# Test 1: Health Check
Write-Host "1. Testing Health Endpoint..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-WebRequest -Uri "$BaseUrl/health" -Method GET
    Write-Host "   ✅ Health endpoint: $($healthResponse.StatusCode)" -ForegroundColor Green
    $healthContent = $healthResponse.Content | ConvertFrom-Json
    Write-Host "   📊 Status: $($healthContent.status)" -ForegroundColor Cyan
    Write-Host "   🔍 Checks: $($healthContent.checks.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "   ❌ Health endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 2: Database Connection Test
Write-Host "2. Testing Database Connection..." -ForegroundColor Yellow
try {
    $dbResponse = Invoke-WebRequest -Uri "$BaseUrl/api/auth/test-db" -Method GET
    Write-Host "   ✅ Database test: $($dbResponse.StatusCode)" -ForegroundColor Green
    $dbContent = $dbResponse.Content | ConvertFrom-Json
    Write-Host "   📊 Message: $($dbContent.message)" -ForegroundColor Cyan
    Write-Host "   👥 User Count: $($dbContent.userCount)" -ForegroundColor Cyan
    Write-Host "   ✍️  Test Write: $($dbContent.testWrite)" -ForegroundColor Cyan
} catch {
    Write-Host "   ❌ Database test failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 3: Admin User Creation
Write-Host "3. Testing Admin User Creation..." -ForegroundColor Yellow
$adminData = @{
    email = "testadmin@241runnersawareness.org"
    firstName = "Test"
    lastName = "Admin"
    password = "TestAdmin123!"
    role = "admin"
    organization = "241 Runners Awareness"
    credentials = "Test Administrator"
    specialization = "Testing"
    yearsOfExperience = "1+"
} | ConvertTo-Json

try {
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    $adminResponse = Invoke-WebRequest -Uri "$BaseUrl/api/admin/create-admin" -Method POST -Body $adminData -Headers $headers
    Write-Host "   ✅ Admin creation: $($adminResponse.StatusCode)" -ForegroundColor Green
    $adminContent = $adminResponse.Content | ConvertFrom-Json
    Write-Host "   📊 Message: $($adminContent.message)" -ForegroundColor Cyan
    Write-Host "   🆔 User ID: $($adminContent.userId)" -ForegroundColor Cyan
    Write-Host "   📧 Email: $($adminContent.email)" -ForegroundColor Cyan
} catch {
    Write-Host "   ❌ Admin creation failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorContent = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorContent)
        $errorBody = $reader.ReadToEnd()
        Write-Host "   📄 Error Details: $errorBody" -ForegroundColor Red
    }
}
Write-Host ""

# Test 4: Admin Login
Write-Host "4. Testing Admin Login..." -ForegroundColor Yellow
$loginData = @{
    email = "testadmin@241runnersawareness.org"
    password = "TestAdmin123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginData -Headers $headers
    Write-Host "   ✅ Admin login: $($loginResponse.StatusCode)" -ForegroundColor Green
    $loginContent = $loginResponse.Content | ConvertFrom-Json
    Write-Host "   📊 Success: $($loginContent.success)" -ForegroundColor Cyan
    Write-Host "   📄 Message: $($loginContent.message)" -ForegroundColor Cyan
    if ($loginContent.token) {
        Write-Host "   🔑 Token received: $($loginContent.token.Substring(0, 20))..." -ForegroundColor Cyan
    }
} catch {
    Write-Host "   ❌ Admin login failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorContent = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorContent)
        $errorBody = $reader.ReadToEnd()
        Write-Host "   📄 Error Details: $errorBody" -ForegroundColor Red
    }
}
Write-Host ""

Write-Host "Test completed!" -ForegroundColor Green
Write-Host "Check the logs above for any issues with database connection or NULL values." -ForegroundColor Cyan
