# Simple Deployment Test Script
Write-Host "Testing 241 Runners Awareness Deployment..." -ForegroundColor Green

# Test Frontend
Write-Host "`n1. Testing Frontend..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://241runnersawareness.org" -UseBasicParsing -TimeoutSec 10
    Write-Host "   ✅ Frontend is accessible" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Frontend not accessible: $($_.Exception.Message)" -ForegroundColor Red
}

# Test Backend Health
Write-Host "`n2. Testing Backend Health..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://241runnersawareness-api.azurewebsites.net/health" -UseBasicParsing -TimeoutSec 10
    Write-Host "   ✅ Backend health check passed" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Backend health check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test Auth Endpoint
Write-Host "`n3. Testing Auth Endpoint..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://241runnersawareness-api.azurewebsites.net/api/auth/test" -UseBasicParsing -TimeoutSec 10
    Write-Host "   ✅ Auth endpoint working" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Auth endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nDeployment test completed!" -ForegroundColor Green
