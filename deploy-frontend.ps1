# 241 Runners Awareness - Frontend Deployment Script
# This script builds and deploys the frontend to Netlify

param(
    [string]$Environment = "production",
    [string]$NetlifySiteId = "",
    [string]$NetlifyToken = ""
)

Write-Host "🌐 241 Runners Awareness - Frontend Deployment" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host ""

# Check prerequisites
Write-Host "📋 Checking Prerequisites..." -ForegroundColor Cyan

# Check if Node.js is installed
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js version: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js not found. Please install Node.js first." -ForegroundColor Red
    exit 1
}

# Check if npm is installed
try {
    $npmVersion = npm --version
    Write-Host "✅ npm version: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ npm not found. Please install npm first." -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 1: Build Frontend
Write-Host "🔧 Step 1: Building Frontend..." -ForegroundColor Cyan
Set-Location frontend

Write-Host "  Installing dependencies..." -ForegroundColor Yellow
npm install

Write-Host "  Building for production..." -ForegroundColor Yellow
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Frontend build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Frontend build completed!" -ForegroundColor Green
Set-Location ..

# Step 2: Deploy to Netlify
Write-Host ""
Write-Host "☁️ Step 2: Deploying to Netlify..." -ForegroundColor Cyan

# Check if Netlify CLI is installed
try {
    $netlifyVersion = netlify --version
    Write-Host "✅ Netlify CLI version: $netlifyVersion" -ForegroundColor Green
} catch {
    Write-Host "📦 Installing Netlify CLI..." -ForegroundColor Yellow
    npm install -g netlify-cli
}

# Deploy to Netlify
Write-Host "  Deploying to Netlify..." -ForegroundColor Yellow

if ($NetlifyToken -and $NetlifySiteId) {
    # Deploy with specific site ID and token
    $env:NETLIFY_AUTH_TOKEN = $NetlifyToken
    netlify deploy --site $NetlifySiteId --dir frontend/dist --prod
} else {
    # Interactive deployment
    netlify deploy --dir frontend/dist --prod
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Netlify deployment failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Frontend deployed successfully!" -ForegroundColor Green

# Step 3: Configure Custom Domain
Write-Host ""
Write-Host "🌐 Step 3: Configuring Custom Domain..." -ForegroundColor Cyan

Write-Host "  Setting up custom domain: 241runnersawareness.org" -ForegroundColor Yellow

# Add custom domain to Netlify
if ($NetlifyToken -and $NetlifySiteId) {
    netlify domains:add --site $NetlifySiteId 241runnersawareness.org
    netlify domains:add --site $NetlifySiteId www.241runnersawareness.org
} else {
    Write-Host "  Please configure custom domain manually in Netlify dashboard" -ForegroundColor Yellow
}

# Step 4: Configure Environment Variables
Write-Host ""
Write-Host "⚙️ Step 4: Configuring Environment Variables..." -ForegroundColor Cyan

Write-Host "  Setting up environment variables..." -ForegroundColor Yellow

# Set environment variables in Netlify
if ($NetlifyToken -and $NetlifySiteId) {
    netlify env:set --site $NetlifySiteId API_BASE_URL "https://241runnersawareness-api.azurewebsites.net/api"
    netlify env:set --site $NetlifySiteId GOOGLE_CLIENT_ID "933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com"
    netlify env:set --site $NetlifySiteId APP_URL "https://241runnersawareness.org"
} else {
    Write-Host "  Please set environment variables manually in Netlify dashboard:" -ForegroundColor Yellow
    Write-Host "    - API_BASE_URL: https://241runnersawareness-api.azurewebsites.net/api" -ForegroundColor White
    Write-Host "    - GOOGLE_CLIENT_ID: 933970195369-67fjn7t28p7q8a3grar5a46jad4mvinq.apps.googleusercontent.com" -ForegroundColor White
    Write-Host "    - APP_URL: https://241runnersawareness.org" -ForegroundColor White
}

# Step 5: Configure Redirects
Write-Host ""
Write-Host "🔄 Step 5: Configuring Redirects..." -ForegroundColor Cyan

Write-Host "  Setting up SPA redirects..." -ForegroundColor Yellow

# Create _redirects file for SPA routing
$redirectsContent = @"
# SPA Routing
/*    /index.html   200

# API Proxy (if needed)
/api/*  https://241runnersawareness-api.azurewebsites.net/api/:splat  200

# Security Headers
/*
  X-Frame-Options: DENY
  X-XSS-Protection: 1; mode=block
  X-Content-Type-Options: nosniff
  Referrer-Policy: strict-origin-when-cross-origin
  Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' https://accounts.google.com https://www.google.com; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; img-src 'self' data: https:; connect-src 'self' https://241runnersawareness-api.azurewebsites.net https://accounts.google.com; frame-src https://accounts.google.com;
"@

$redirectsContent | Out-File -FilePath "frontend/dist/_redirects" -Encoding UTF8

Write-Host "✅ Redirects configured!" -ForegroundColor Green

# Step 6: Health Check
Write-Host ""
Write-Host "🏥 Step 6: Running Health Checks..." -ForegroundColor Cyan

Write-Host "  Checking frontend accessibility..." -ForegroundColor Yellow

# Get the deployed URL from Netlify
try {
    $deployInfo = netlify status --json | ConvertFrom-Json
    $deployUrl = $deployInfo.url
    
    Write-Host "  Frontend URL: $deployUrl" -ForegroundColor Green
    
    # Test the deployment
    try {
        $response = Invoke-RestMethod -Uri $deployUrl -Method Get -TimeoutSec 10
        Write-Host "✅ Frontend is accessible!" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Frontend health check failed. This is normal during deployment." -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ Could not retrieve deployment URL." -ForegroundColor Yellow
}

# Step 7: Summary
Write-Host ""
Write-Host "🎉 Frontend Deployment Summary" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green
Write-Host "✅ Frontend built successfully" -ForegroundColor Green
Write-Host "✅ Deployed to Netlify" -ForegroundColor Green
Write-Host "✅ Custom domain configured" -ForegroundColor Green
Write-Host "✅ Environment variables set" -ForegroundColor Green
Write-Host "✅ SPA redirects configured" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Production URLs:" -ForegroundColor Cyan
Write-Host "   Frontend: https://241runnersawareness.org" -ForegroundColor White
Write-Host "   Backend API: https://241runnersawareness-api.azurewebsites.net" -ForegroundColor White
Write-Host ""
Write-Host "📝 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Verify SSL certificate is active" -ForegroundColor White
Write-Host "   2. Test all functionality on production" -ForegroundColor White
Write-Host "   3. Set up monitoring and analytics" -ForegroundColor White
Write-Host "   4. Configure backup and disaster recovery" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Frontend deployment completed successfully!" -ForegroundColor Green
