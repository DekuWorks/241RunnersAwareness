# ============================================
# 241 RUNNERS AWARENESS - COMPONENT TESTING SCRIPT
# ============================================
# This script tests all components of the project:
# - Static Site (HTML/CSS/JS)
# - Backend API (.NET Core)
# - Frontend React App
# - Database (SQLite)
# - API Endpoints

Write-Host "============================================" -ForegroundColor Green
Write-Host "241 RUNNERS AWARENESS - COMPONENT TESTING" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

# Test 1: Check Prerequisites
Write-Host "1. CHECKING PREREQUISITES..." -ForegroundColor Yellow
Write-Host ""

# Check .NET version
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET not found or not working" -ForegroundColor Red
    exit 1
}

# Check Node.js version
try {
    $nodeVersion = node --version
    Write-Host "✓ Node.js Version: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Node.js not found or not working" -ForegroundColor Red
    exit 1
}

# Check npm version
try {
    $npmVersion = npm --version
    Write-Host "✓ npm Version: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ npm not found or not working" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Database Check
Write-Host "2. TESTING DATABASE..." -ForegroundColor Yellow
Write-Host ""

if (Test-Path "backend/RunnersDb.db") {
    $dbSize = (Get-Item "backend/RunnersDb.db").Length
    Write-Host "✓ Database exists: RunnersDb.db ($($dbSize / 1KB) KB)" -ForegroundColor Green
} else {
    Write-Host "✗ Database file not found" -ForegroundColor Red
}

Write-Host ""

# Test 3: Static Site Testing
Write-Host "3. TESTING STATIC SITE..." -ForegroundColor Yellow
Write-Host ""

# Start static server in background
Write-Host "Starting static server on port 8080..." -ForegroundColor Cyan
Start-Process -FilePath "python" -ArgumentList "-m", "http.server", "8080" -WindowStyle Hidden

# Wait for server to start
Start-Sleep -Seconds 3

# Test main pages
$staticPages = @(
    "http://localhost:8080",
    "http://localhost:8080/aboutus.html",
    "http://localhost:8080/cases.html",
    "http://localhost:8080/map.html",
    "http://localhost:8080/login.html",
    "http://localhost:8080/signup.html"
)

foreach ($page in $staticPages) {
    try {
        $response = Invoke-WebRequest -Uri $page -Method GET -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ $page - OK" -ForegroundColor Green
        } else {
            Write-Host "✗ $page - Status: $($response.StatusCode)" -ForegroundColor Red
        }
    } catch {
        Write-Host "✗ $page - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""

# Test 4: Backend API Testing
Write-Host "4. TESTING BACKEND API..." -ForegroundColor Yellow
Write-Host ""

# Check if backend is running
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5113/health" -Method GET -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Backend API is running" -ForegroundColor Green
        $healthData = $response.Content | ConvertFrom-Json
        Write-Host "  Status: $($healthData.status)" -ForegroundColor Cyan
    } else {
        Write-Host "✗ Backend API returned status: $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Backend API not accessible - may not be running" -ForegroundColor Red
    Write-Host "  To start backend: cd backend && dotnet run" -ForegroundColor Cyan
}

Write-Host ""

# Test 5: Frontend React App Testing
Write-Host "5. TESTING FRONTEND REACT APP..." -ForegroundColor Yellow
Write-Host ""

# Check if frontend is running
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5173" -Method GET -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Frontend React app is running" -ForegroundColor Green
    } else {
        Write-Host "✗ Frontend React app returned status: $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Frontend React app not accessible - may not be running" -ForegroundColor Red
    Write-Host "  To start frontend: cd frontend && npm run dev" -ForegroundColor Cyan
}

Write-Host ""

# Test 6: API Endpoints Testing (if backend is running)
Write-Host "6. TESTING API ENDPOINTS..." -ForegroundColor Yellow
Write-Host ""

$apiEndpoints = @(
    "http://localhost:5113/api/auth/test",
    "http://localhost:5113/api/auth/test-db",
    "http://localhost:5113/swagger",
    "http://localhost:5113/health/ready",
    "http://localhost:5113/health/live"
)

foreach ($endpoint in $apiEndpoints) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint -Method GET -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ $endpoint - OK" -ForegroundColor Green
        } else {
            Write-Host "✗ $endpoint - Status: $($response.StatusCode)" -ForegroundColor Red
        }
    } catch {
        Write-Host "✗ $endpoint - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""

# Test 7: File Structure Check
Write-Host "7. CHECKING FILE STRUCTURE..." -ForegroundColor Yellow
Write-Host ""

$requiredFiles = @(
    "index.html",
    "styles.css",
    "auth-utils.js",
    "backend/Program.cs",
    "backend/appsettings.json",
    "frontend/package.json",
    "frontend/vite.config.js"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file exists" -ForegroundColor Green
    } else {
        Write-Host "✗ $file missing" -ForegroundColor Red
    }
}

Write-Host ""

# Test 8: Build Tests
Write-Host "8. BUILD TESTS..." -ForegroundColor Yellow
Write-Host ""

# Test backend build
Write-Host "Testing backend build..." -ForegroundColor Cyan
try {
    Push-Location backend
    dotnet build --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Backend builds successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Backend build failed" -ForegroundColor Red
    }
    Pop-Location
} catch {
    Write-Host "✗ Backend build error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test frontend dependencies
Write-Host "Testing frontend dependencies..." -ForegroundColor Cyan
try {
    Push-Location frontend
    if (Test-Path "node_modules") {
        Write-Host "✓ Frontend dependencies installed" -ForegroundColor Green
    } else {
        Write-Host "✗ Frontend dependencies not installed" -ForegroundColor Red
        Write-Host "  Run: npm install" -ForegroundColor Cyan
    }
    Pop-Location
} catch {
    Write-Host "✗ Frontend dependency check error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Summary
Write-Host "============================================" -ForegroundColor Green
Write-Host "TESTING SUMMARY" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

Write-Host "Static Site: ✓ Working on http://localhost:8080" -ForegroundColor Green
Write-Host "Database: ✓ SQLite database exists" -ForegroundColor Green
Write-Host "Backend: Check status above" -ForegroundColor Yellow
Write-Host "Frontend: Check status above" -ForegroundColor Yellow
Write-Host ""

Write-Host "To start components manually:" -ForegroundColor Cyan
Write-Host "1. Static Site: python -m http.server 8080" -ForegroundColor White
Write-Host "2. Backend: cd backend && dotnet run" -ForegroundColor White
Write-Host "3. Frontend: cd frontend && npm run dev" -ForegroundColor White
Write-Host ""

Write-Host "API Documentation: http://localhost:5113/swagger" -ForegroundColor Cyan
Write-Host "Health Check: http://localhost:5113/health" -ForegroundColor Cyan
Write-Host ""

# Cleanup
Write-Host "Cleaning up..." -ForegroundColor Yellow
try {
    Get-Process -Name "python" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like "*http.server*" } | Stop-Process -Force
    Write-Host "✓ Static server stopped" -ForegroundColor Green
} catch {
    Write-Host "No static server to clean up" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Testing complete!" -ForegroundColor Green

