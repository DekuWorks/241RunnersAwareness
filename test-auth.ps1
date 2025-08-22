# Test Authentication Endpoints
Write-Host "Testing 241 Runners Awareness API Authentication" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green

# Test 1: Check if API is running
Write-Host "`n1. Testing API connectivity..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5113/api/auth/test" -UseBasicParsing
    Write-Host "✅ API is running: $($response.Content)" -ForegroundColor Green
} catch {
    Write-Host "❌ API is not accessible: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# Test 2: Test login with seeded user
Write-Host "`n2. Testing login with seeded user (Marcus)..." -ForegroundColor Yellow
$loginBody = @{
    Email = "dekuworks1@gmail.com"
    Password = "marcus2025"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5113/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -UseBasicParsing
    Write-Host "✅ Login successful!" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Error details: $responseBody" -ForegroundColor Red
    }
}

# Test 3: Test registration
Write-Host "`n3. Testing user registration..." -ForegroundColor Yellow
$registerBody = @{
    Email = "newuser@example.com"
    Password = "TestPassword123!"
    FullName = "New Test User"
    Role = "user"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5113/api/auth/register-simple" -Method POST -Body $registerBody -ContentType "application/json" -UseBasicParsing
    Write-Host "✅ Registration successful!" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Registration failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Error details: $responseBody" -ForegroundColor Red
    }
}

# Test 4: Test login with newly registered user
Write-Host "`n4. Testing login with newly registered user..." -ForegroundColor Yellow
$newLoginBody = @{
    Email = "newuser@example.com"
    Password = "TestPassword123!"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5113/api/auth/login" -Method POST -Body $newLoginBody -ContentType "application/json" -UseBasicParsing
    Write-Host "✅ New user login successful!" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ New user login failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Error details: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`nAuthentication testing completed!" -ForegroundColor Green
