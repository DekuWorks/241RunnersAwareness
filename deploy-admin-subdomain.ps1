# Deploy Admin Dashboard to Netlify Subdomain
# This script deploys the admin dashboard as a subdomain

Write-Host "Deploying Admin Dashboard to Netlify Subdomain..." -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Check if netlify-cli is installed
try {
    $netlifyVersion = netlify --version
    Write-Host "Netlify CLI found: $netlifyVersion" -ForegroundColor Green
} catch {
    Write-Host "Installing Netlify CLI..." -ForegroundColor Yellow
    npm install -g netlify-cli
}

# Create admin directory structure
Write-Host "Creating admin directory structure..." -ForegroundColor Yellow
if (!(Test-Path "admin")) {
    New-Item -ItemType Directory -Path "admin"
}

# Copy admin dashboard files
Write-Host "Copying admin dashboard files..." -ForegroundColor Yellow
Copy-Item "admin-dashboard.html" "admin/index.html" -Force
Copy-Item "styles.css" "admin/styles.css" -Force
Copy-Item "241-logo.jpg" "admin/241-logo.jpg" -Force

# Create admin-specific configuration
Write-Host "Creating admin configuration..." -ForegroundColor Yellow

# Create admin CNAME file
$adminCNAME = "admin.241runnersawareness.com"
Set-Content -Path "admin/CNAME" -Value $adminCNAME

# Create admin README
$adminREADME = @"
# 241 Runners Awareness - Admin Dashboard

This is the admin dashboard for the 241 Runners Awareness platform.

## Features
- Case Management
- User Management  
- Analytics Dashboard
- System Settings

## API Endpoint
- Backend: https://241runnersawareness-api.azurewebsites.net

## Deployment
This dashboard is deployed as a subdomain: admin.241runnersawareness.com

## Security
- Admin authentication required
- Secure API communication
- CSP headers configured
"@

Set-Content -Path "admin/README.md" -Value $adminREADME

# Navigate to admin directory
Set-Location "admin"

# Initialize Netlify site (if not already done)
Write-Host "Initializing Netlify site..." -ForegroundColor Yellow
try {
    netlify status
} catch {
    Write-Host "Please run 'netlify login' and 'netlify init' to set up the admin site" -ForegroundColor Red
    Write-Host "Then run this script again" -ForegroundColor Red
    exit 1
}

# Deploy to Netlify
Write-Host "Deploying to Netlify..." -ForegroundColor Yellow
try {
    netlify deploy --prod --dir=.
    Write-Host "Admin dashboard deployed successfully!" -ForegroundColor Green
} catch {
    Write-Host "Deployment failed. Please check your Netlify configuration." -ForegroundColor Red
    exit 1
}

# Return to root directory
Set-Location ".."

Write-Host ""
Write-Host "Admin Dashboard Deployment Complete!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Admin Dashboard URL: https://admin.241runnersawareness.com" -ForegroundColor Cyan
Write-Host "API Endpoint: https://241runnersawareness-api.azurewebsites.net" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Configure DNS for admin.241runnersawareness.com" -ForegroundColor White
Write-Host "2. Set up admin authentication" -ForegroundColor White
Write-Host "3. Test all admin functions" -ForegroundColor White
Write-Host "4. Configure SSL certificate" -ForegroundColor White
