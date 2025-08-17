# Simple Login Test
Write-Host "Testing Login with Marcus" -ForegroundColor Green

# Test 1: Check if API is accessible
Write-Host "`n1. Testing API connectivity..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5113/api/auth/test" -UseBasicParsing
    Write-Host "✅ API is running" -ForegroundColor Green
} catch {
    Write-Host "❌ API is not accessible" -ForegroundColor Red
    exit
}

# Test 2: Test login
Write-Host "`n2. Testing login..." -ForegroundColor Yellow
$loginBody = @{
    Email = "dekuworks1@gmail.com"
    Password = "marcus2025"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5113/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -UseBasicParsing
    Write-Host "✅ Login successful!" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Login failed" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    # Try to get detailed error response
    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Error Details: $responseBody" -ForegroundColor Red
        } catch {
            Write-Host "Could not read error details" -ForegroundColor Red
        }
    }
}

Write-Host "`nTest completed!" -ForegroundColor Green
