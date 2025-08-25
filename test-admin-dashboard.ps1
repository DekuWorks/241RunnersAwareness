# Test Admin Dashboard
# This script tests the admin dashboard functionality

Write-Host "Testing Admin Dashboard..." -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green

$baseUrl = "https://241runnersawareness-api.azurewebsites.net"

Write-Host "1. Testing Admin Dashboard Access..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/admin" -Method Get
    Write-Host "   ✅ Admin Dashboard: ACCESSIBLE" -ForegroundColor Green
    Write-Host "   Dashboard is now available at: $baseUrl/admin" -ForegroundColor Cyan
} catch {
    Write-Host "   ❌ Admin Dashboard: NOT ACCESSIBLE" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "2. Testing API Health..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
    Write-Host "   ✅ API Health: PASSED" -ForegroundColor Green
    Write-Host "   Status: $($response.status)" -ForegroundColor Cyan
} catch {
    Write-Host "   ❌ API Health: FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Admin Dashboard URLs:" -ForegroundColor Green
Write-Host "=====================" -ForegroundColor Green
Write-Host "Main Admin Dashboard: $baseUrl/admin" -ForegroundColor Cyan
Write-Host "API Health Check: $baseUrl/health" -ForegroundColor Cyan
Write-Host "Swagger Documentation: $baseUrl/swagger" -ForegroundColor Cyan

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Open $baseUrl/admin in your browser" -ForegroundColor White
Write-Host "2. Test all admin dashboard features" -ForegroundColor White
Write-Host "3. Configure DNS for admin subdomain if needed" -ForegroundColor White
Write-Host "4. Set up authentication for admin access" -ForegroundColor White
