# 241 Runners Awareness - Frontend Deployment to Netlify
# This script builds and deploys the frontend to Netlify

param(
    [string]$Environment = "production",
    [string]$NetlifySiteId = "",
    [string]$NetlifyToken = ""
)

Write-Host "Frontend Deployment to Netlify" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green
Write-Host ""

# Check prerequisites
Write-Host "Checking Prerequisites..." -ForegroundColor Cyan

# Check if Node.js is installed
try {
    $nodeVersion = node --version
    Write-Host "Node.js version: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "Node.js not found. Please install Node.js first." -ForegroundColor Red
    exit 1
}

# Check if npm is installed
try {
    $npmVersion = npm --version
    Write-Host "npm version: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "npm not found. Please install npm first." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Building Frontend..." -ForegroundColor Yellow

# Navigate to frontend directory
Set-Location frontend

# Install dependencies
Write-Host "Installing dependencies..." -ForegroundColor Cyan
try {
    npm install
    Write-Host "Dependencies installed successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error installing dependencies: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Build the application
Write-Host "Building application..." -ForegroundColor Cyan
try {
    npm run build
    Write-Host "Application built successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error building application: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check if build was successful
if (-not (Test-Path "dist")) {
    Write-Host "Build failed - dist directory not found" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Deploying to Netlify..." -ForegroundColor Yellow

# If Netlify CLI is not installed, install it
try {
    netlify --version | Out-Null
    Write-Host "Netlify CLI found" -ForegroundColor Green
} catch {
    Write-Host "Installing Netlify CLI..." -ForegroundColor Cyan
    npm install -g netlify-cli
}

# Deploy to Netlify
try {
    if ($NetlifySiteId -and $NetlifyToken) {
        # Deploy with specific site ID and token
        netlify deploy --prod --dir=dist --site=$NetlifySiteId --auth=$NetlifyToken
    } else {
        # Interactive deployment
        netlify deploy --prod --dir=dist
    }
    Write-Host "Deployment completed successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error deploying to Netlify: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "You can also deploy manually by:" -ForegroundColor Yellow
    Write-Host "1. Going to https://app.netlify.com" -ForegroundColor White
    Write-Host "2. Dragging the 'dist' folder to the deploy area" -ForegroundColor White
    exit 1
}

# Return to root directory
Set-Location ..

Write-Host ""
Write-Host "Frontend Deployment Complete!" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "Frontend built successfully" -ForegroundColor White
Write-Host "Deployed to Netlify" -ForegroundColor White
Write-Host "Connected to backend API" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Test the deployed frontend" -ForegroundColor White
Write-Host "2. Configure custom domain if needed" -ForegroundColor White
Write-Host "3. Set up environment variables in Netlify" -ForegroundColor White
Write-Host "4. Test all functionality" -ForegroundColor White
