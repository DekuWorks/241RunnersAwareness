# 241 Runners Awareness - Deployment Status Checker
# This script checks the status of all deployment components

param(
    [switch]$Verbose,
    [switch]$TestAPI
)

Write-Host "241 Runners Awareness - Deployment Status Check" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# Configuration
$FrontendUrl = "https://241runnersawareness.org"
$BackendUrl = "https://241runnersawareness-api.azurewebsites.net"
$ApiHealthUrl = "$BackendUrl/health"
$ApiTestUrl = "$BackendUrl/api/auth/test"

# Function to test URL
function Test-Url {
    param(
        [string]$Url,
        [string]$Description
    )
    
    try {
        Write-Host "Testing $Description..." -NoNewline
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Host " [OK]" -ForegroundColor Green
            if ($Verbose) {
                Write-Host "   Status: $($response.StatusCode)" -ForegroundColor Gray
                Write-Host "   Content Length: $($response.Content.Length) bytes" -ForegroundColor Gray
            }
            return $true
        } else {
            Write-Host " [WARNING - Status: $($response.StatusCode)]" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host " [FAILED]" -ForegroundColor Red
        if ($Verbose) {
            Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        }
        return $false
    }
}

# Function to test API endpoint
function Test-ApiEndpoint {
    param(
        [string]$Url,
        [string]$Description
    )
    
    try {
        Write-Host "Testing API: $Description..." -NoNewline
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Host " [OK]" -ForegroundColor Green
            if ($Verbose) {
                $content = $response.Content | ConvertFrom-Json
                Write-Host "   Response: $($content | ConvertTo-Json -Compress)" -ForegroundColor Gray
            }
            return $true
        } else {
            Write-Host " [WARNING - Status: $($response.StatusCode)]" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host " [FAILED]" -ForegroundColor Red
        if ($Verbose) {
            Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        }
        return $false
    }
}

# Test Frontend
Write-Host "`nFrontend Status:" -ForegroundColor Yellow
$frontendStatus = Test-Url -Url $FrontendUrl -Description "Frontend (GitHub Pages)"

# Test Backend Health
Write-Host "`nBackend Status:" -ForegroundColor Yellow
$backendHealthStatus = Test-Url -Url $ApiHealthUrl -Description "Backend Health Check"

# Test API Endpoints
if ($TestAPI) {
    Write-Host "`nAPI Endpoints:" -ForegroundColor Yellow
    $apiTestStatus = Test-ApiEndpoint -Url $ApiTestUrl -Description "Auth Controller Test"
}

# Summary
Write-Host "`nDeployment Summary:" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan

$allTests = @($frontendStatus, $backendHealthStatus)
if ($TestAPI) {
    $allTests += $apiTestStatus
}

$passedTests = ($allTests | Where-Object { $_ -eq $true }).Count
$totalTests = $allTests.Count

Write-Host "Frontend (GitHub Pages): $(if ($frontendStatus) { 'ONLINE' } else { 'OFFLINE' })" -ForegroundColor $(if ($frontendStatus) { 'Green' } else { 'Red' })
Write-Host "Backend (Azure App Service): $(if ($backendHealthStatus) { 'ONLINE' } else { 'OFFLINE' })" -ForegroundColor $(if ($backendHealthStatus) { 'Green' } else { 'Red' })
if ($TestAPI) {
    Write-Host "API Endpoints: $(if ($apiTestStatus) { 'WORKING' } else { 'FAILED' })" -ForegroundColor $(if ($apiTestStatus) { 'Green' } else { 'Red' })
}

Write-Host "`nOverall Status: $passedTests/$totalTests tests passed" -ForegroundColor $(if ($passedTests -eq $totalTests) { 'Green' } else { 'Yellow' })

# URLs
Write-Host "`nURLs:" -ForegroundColor Cyan
Write-Host "Frontend: $FrontendUrl" -ForegroundColor Gray
Write-Host "Backend API: $BackendUrl" -ForegroundColor Gray
Write-Host "Health Check: $ApiHealthUrl" -ForegroundColor Gray
Write-Host "Swagger Docs: $BackendUrl/swagger" -ForegroundColor Gray

# Recommendations
Write-Host "`nRecommendations:" -ForegroundColor Cyan
if (-not $frontendStatus) {
    Write-Host "• Check GitHub Pages settings and deployment" -ForegroundColor Yellow
}
if (-not $backendHealthStatus) {
    Write-Host "• Check Azure App Service status and logs" -ForegroundColor Yellow
}
if ($TestAPI -and -not $apiTestStatus) {
    Write-Host "• Check API configuration and database connection" -ForegroundColor Yellow
}

if ($passedTests -eq $totalTests) {
    Write-Host "`nAll systems are operational!" -ForegroundColor Green
} else {
    Write-Host "`nSome components need attention" -ForegroundColor Yellow
}

Write-Host "`n==================================================" -ForegroundColor Cyan
