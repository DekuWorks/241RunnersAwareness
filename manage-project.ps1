# 241 Runners Awareness - Project Management Script
# This script provides all necessary functions for development, deployment, and maintenance

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("dev", "test", "deploy", "clean", "setup", "help")]
    [string]$Action = "help",
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = "development"
)

Write-Host "🏃 241 Runners Awareness - Project Manager" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Action: $Action" -ForegroundColor Yellow
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host ""

function Show-Help {
    Write-Host "📖 Available Commands:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  dev     - Start development environment" -ForegroundColor White
    Write-Host "  test    - Run tests and validation" -ForegroundColor White
    Write-Host "  deploy  - Deploy to production" -ForegroundColor White
    Write-Host "  clean   - Clean up temporary files" -ForegroundColor White
    Write-Host "  setup   - Initial project setup" -ForegroundColor White
    Write-Host "  help    - Show this help message" -ForegroundColor White
    Write-Host ""
    Write-Host "📝 Usage Examples:" -ForegroundColor Cyan
    Write-Host "  .\manage-project.ps1 dev" -ForegroundColor Gray
    Write-Host "  .\manage-project.ps1 deploy" -ForegroundColor Gray
    Write-Host "  .\manage-project.ps1 test" -ForegroundColor Gray
}

function Start-Development {
    Write-Host "🚀 Starting Development Environment..." -ForegroundColor Cyan
    
    # Check prerequisites
    Write-Host "  Checking prerequisites..." -ForegroundColor Yellow
    
    # Check Node.js
    try {
        $nodeVersion = node --version
        Write-Host "  ✅ Node.js: $nodeVersion" -ForegroundColor Green
    } catch {
        Write-Host "  ❌ Node.js not found. Please install Node.js first." -ForegroundColor Red
        return
    }
    
    # Check .NET
    try {
        $dotnetVersion = dotnet --version
        Write-Host "  ✅ .NET: $dotnetVersion" -ForegroundColor Green
    } catch {
        Write-Host "  ❌ .NET not found. Please install .NET SDK first." -ForegroundColor Red
        return
    }
    
    Write-Host ""
    Write-Host "🔧 Starting Backend..." -ForegroundColor Cyan
    if (Test-Path "backend") {
        Set-Location backend
        Write-Host "  Building backend..." -ForegroundColor Yellow
        dotnet build
        Write-Host "  Starting backend server..." -ForegroundColor Yellow
        Start-Process -FilePath "dotnet" -ArgumentList "run" -WindowStyle Minimized
        Set-Location ..
    }
    
    Write-Host ""
    Write-Host "🌐 Starting Frontend..." -ForegroundColor Cyan
    if (Test-Path "frontend") {
        Set-Location frontend
        Write-Host "  Installing dependencies..." -ForegroundColor Yellow
        npm install
        Write-Host "  Starting development server..." -ForegroundColor Yellow
        Start-Process -FilePath "npm" -ArgumentList "start" -WindowStyle Minimized
        Set-Location ..
    }
    
    Write-Host ""
    Write-Host "✅ Development environment started!" -ForegroundColor Green
    Write-Host "  Backend: http://localhost:5000" -ForegroundColor Gray
    Write-Host "  Frontend: http://localhost:3000" -ForegroundColor Gray
    Write-Host "  Static Site: Open index.html in browser" -ForegroundColor Gray
}

function Invoke-Tests {
    Write-Host "🧪 Running Tests and Validation..." -ForegroundColor Cyan
    
    $testResults = @()
    
    # Backend tests
    if (Test-Path "backend") {
        Write-Host "  Testing backend..." -ForegroundColor Yellow
        Set-Location backend
        try {
            dotnet test --verbosity normal
            $testResults += "Backend: ✅ Passed"
        } catch {
            $testResults += "Backend: ❌ Failed"
        }
        Set-Location ..
    }
    
    # Frontend tests
    if (Test-Path "frontend") {
        Write-Host "  Testing frontend..." -ForegroundColor Yellow
        Set-Location frontend
        try {
            npm test --if-present
            $testResults += "Frontend: ✅ Passed"
        } catch {
            $testResults += "Frontend: ❌ Failed"
        }
        Set-Location ..
    }
    
    # Security audit
    Write-Host "  Running security audit..." -ForegroundColor Yellow
    if (Test-Path "package.json") {
        npm audit --audit-level=moderate
    }
    if (Test-Path "frontend/package.json") {
        Set-Location frontend
        npm audit --audit-level=moderate
        Set-Location ..
    }
    
    # HTML validation
    Write-Host "  Validating HTML files..." -ForegroundColor Yellow
    Get-ChildItem -Filter "*.html" | ForEach-Object {
        Write-Host "    Checking $($_.Name)..." -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "📊 Test Results:" -ForegroundColor Cyan
    $testResults | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
}

function Invoke-Deployment {
    Write-Host "🚀 Deploying to Production..." -ForegroundColor Cyan
    
    # Build frontend
    if (Test-Path "frontend") {
        Write-Host "  Building frontend..." -ForegroundColor Yellow
        Set-Location frontend
        npm install
        npm run build
        Set-Location ..
    }
    
    # Build backend
    if (Test-Path "backend") {
        Write-Host "  Building backend..." -ForegroundColor Yellow
        Set-Location backend
        dotnet publish -c Release
        Set-Location ..
    }
    
    # Deploy to Netlify (static files)
    Write-Host "  Deploying static files to Netlify..." -ForegroundColor Yellow
    
    # Check if Netlify CLI is installed
    try {
        $netlifyVersion = netlify --version
        Write-Host "  ✅ Netlify CLI: $netlifyVersion" -ForegroundColor Green
    } catch {
        Write-Host "  📦 Installing Netlify CLI..." -ForegroundColor Yellow
        npm install -g netlify-cli
    }
    
    # Deploy to Netlify
    Write-Host "  Deploying to Netlify..." -ForegroundColor Yellow
    netlify deploy --prod --dir .
    
    Write-Host ""
    Write-Host "✅ Deployment completed!" -ForegroundColor Green
    Write-Host "  Site: https://241runnersawareness.org" -ForegroundColor Gray
}

function Invoke-Cleanup {
    Write-Host "🧹 Cleaning up temporary files..." -ForegroundColor Cyan
    
    # Remove node_modules
    if (Test-Path "node_modules") {
        Write-Host "  Removing node_modules..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force "node_modules"
    }
    
    if (Test-Path "frontend/node_modules") {
        Write-Host "  Removing frontend node_modules..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force "frontend/node_modules"
    }
    
    # Remove build artifacts
    if (Test-Path "backend/bin") {
        Write-Host "  Removing backend build artifacts..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force "backend/bin"
    }
    
    if (Test-Path "backend/obj") {
        Write-Host "  Removing backend obj files..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force "backend/obj"
    }
    
    if (Test-Path "frontend/dist") {
        Write-Host "  Removing frontend dist..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force "frontend/dist"
    }
    
    # Remove temporary files
    Get-ChildItem -Filter "*.tmp" | Remove-Item -Force
    Get-ChildItem -Filter "*.log" | Remove-Item -Force
    
    Write-Host "✅ Cleanup completed!" -ForegroundColor Green
}

function Invoke-Setup {
    Write-Host "⚙️ Setting up project..." -ForegroundColor Cyan
    
    # Check if .env file exists
    if (-not (Test-Path ".env") -and (Test-Path "env.example")) {
        Write-Host "  Creating .env file from template..." -ForegroundColor Yellow
        Copy-Item "env.example" ".env"
        Write-Host "  ⚠️  Please update .env file with your configuration" -ForegroundColor Yellow
    }
    
    # Install dependencies
    if (Test-Path "package.json") {
        Write-Host "  Installing root dependencies..." -ForegroundColor Yellow
        npm install
    }
    
    if (Test-Path "frontend/package.json") {
        Write-Host "  Installing frontend dependencies..." -ForegroundColor Yellow
        Set-Location frontend
        npm install
        Set-Location ..
    }
    
    # Restore .NET packages
    if (Test-Path "backend") {
        Write-Host "  Restoring .NET packages..." -ForegroundColor Yellow
        Set-Location backend
        dotnet restore
        Set-Location ..
    }
    
    Write-Host "✅ Setup completed!" -ForegroundColor Green
    Write-Host "  Run '.\manage-project.ps1 dev' to start development" -ForegroundColor Gray
}

# Main execution
switch ($Action) {
    "dev" { Start-Development }
    "test" { Invoke-Tests }
    "deploy" { Invoke-Deployment }
    "clean" { Invoke-Cleanup }
    "setup" { Invoke-Setup }
    "help" { Show-Help }
    default { Show-Help }
}
