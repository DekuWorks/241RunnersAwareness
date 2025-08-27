# 241 Runners Awareness - Admin User Setup Script
# This script creates admin users in the backend database using the new API endpoints

param(
    [string]$BackendUrl = "https://241runnersawareness-api.azurewebsites.net"
)

Write-Host "🔐 Setting up Admin Users for 241 Runners Awareness" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
Write-Host ""

# Admin users to create
$adminUsers = @(
    @{
        username = "marcus_brown"
        email = "dekuworks1@gmail.com"
        firstName = "Marcus"
        lastName = "Brown"
        password = "marcus2025"
        role = "admin"
        organization = "241 Runners Awareness"
        credentials = "Co-Founder"
        specialization = "Operations"
        yearsOfExperience = "3+"
    },
    @{
        username = "daniel_carey"
        email = "danielcarey9770@gmail.com"
        firstName = "Daniel"
        lastName = "Carey"
        password = "daniel2025"
        role = "admin"
        organization = "241 Runners Awareness"
        credentials = "Co-Founder"
        specialization = "Technology"
        yearsOfExperience = "4+"
    },
    @{
        username = "daniel_carey_yahoo"
        email = "danielcarey9770@yahoo.com"
        firstName = "Daniel"
        lastName = "Carey"
        password = "daniel2025"
        role = "admin"
        organization = "241 Runners Awareness"
        credentials = "Co-Founder"
        specialization = "Technology"
        yearsOfExperience = "4+"
    }
)

# Function to create admin user
function Create-AdminUser {
    param(
        [hashtable]$UserData
    )
    
    try {
        Write-Host "Creating admin user: $($UserData.email)" -ForegroundColor Yellow
        
        $body = @{
            username = $UserData.username
            email = $UserData.email
            firstName = $UserData.firstName
            lastName = $UserData.lastName
            password = $UserData.password
            role = $UserData.role
            organization = $UserData.organization
            credentials = $UserData.credentials
            specialization = $UserData.specialization
            yearsOfExperience = $UserData.yearsOfExperience
        } | ConvertTo-Json -Depth 10

        $headers = @{
            "Content-Type" = "application/json"
        }

        $response = Invoke-RestMethod -Uri "$BackendUrl/api/admin/create-admin" -Method POST -Body $body -Headers $headers

        Write-Host "✅ Successfully created admin user: $($UserData.email)" -ForegroundColor Green
        return $true
    }
    catch {
        $errorMessage = $_.Exception.Message
        Write-Host "❌ Failed to create admin user $($UserData.email): $errorMessage" -ForegroundColor Red
        return $false
    }
}

# Function to test admin login
function Test-AdminLogin {
    param(
        [string]$Email,
        [string]$Password
    )
    
    try {
        Write-Host "Testing login for: $Email" -ForegroundColor Yellow
        
        $body = @{
            email = $Email
            password = $Password
        } | ConvertTo-Json

        $headers = @{
            "Content-Type" = "application/json"
        }

        $response = Invoke-RestMethod -Uri "$BackendUrl/api/auth/login" -Method POST -Body $body -Headers $headers

        if ($response.user.role -eq "admin" -or $response.user.role -eq "superadmin") {
            Write-Host "✅ Login successful for: $Email" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ User is not an admin: $Email" -ForegroundColor Red
            return $false
        }
    }
    catch {
        $errorMessage = $_.Exception.Message
        Write-Host "❌ Login failed for $Email: $errorMessage" -ForegroundColor Red
        return $false
    }
}

# Function to get existing admin users
function Get-ExistingAdminUsers {
    try {
        Write-Host "Checking existing admin users..." -ForegroundColor Yellow
        
        $headers = @{
            "Content-Type" = "application/json"
        }

        $response = Invoke-RestMethod -Uri "$BackendUrl/api/admin/admins" -Method GET -Headers $headers

        $count = $response.Count
        Write-Host "Found $count existing admin users" -ForegroundColor Green
        return $response
    }
    catch {
        Write-Host "❌ Failed to get existing admin users: $($_.Exception.Message)" -ForegroundColor Red
        return @()
    }
}

# Main execution
Write-Host "🚀 Starting admin user setup..." -ForegroundColor Cyan

# Check existing admin users
$existingUsers = Get-ExistingAdminUsers
$existingEmails = $existingUsers | ForEach-Object { $_.email }

$successCount = 0
$totalUsers = $adminUsers.Count

foreach ($user in $adminUsers) {
    if ($existingEmails -contains $user.email) {
        Write-Host "⚠️  Admin user already exists: $($user.email)" -ForegroundColor Yellow
        $successCount++
    } else {
        if (Create-AdminUser -UserData $user) {
            $successCount++
        }
    }
    Write-Host ""
}

Write-Host "==================================================" -ForegroundColor Green
Write-Host "📊 Setup Summary:" -ForegroundColor Green
Write-Host "Total users: $totalUsers" -ForegroundColor White
Write-Host "Successfully created/verified: $successCount" -ForegroundColor Green
$failedCount = $totalUsers - $successCount
Write-Host "Failed: $failedCount" -ForegroundColor Red
Write-Host ""

# Test logins
Write-Host "🔍 Testing admin logins..." -ForegroundColor Cyan
$loginSuccessCount = 0

foreach ($user in $adminUsers) {
    if (Test-AdminLogin -Email $user.email -Password $user.password) {
        $loginSuccessCount++
    }
    Write-Host ""
}

Write-Host "==================================================" -ForegroundColor Green
Write-Host "📊 Login Test Summary:" -ForegroundColor Green
Write-Host "Total logins tested: $totalUsers" -ForegroundColor White
Write-Host "Successful logins: $loginSuccessCount" -ForegroundColor Green
$failedLogins = $totalUsers - $loginSuccessCount
Write-Host "Failed logins: $failedLogins" -ForegroundColor Red
Write-Host ""

# Display admin credentials
Write-Host "🔑 Admin User Credentials:" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
foreach ($user in $adminUsers) {
    Write-Host "Email: $($user.email)" -ForegroundColor White
    Write-Host "Password: $($user.password)" -ForegroundColor White
    Write-Host "Role: $($user.role)" -ForegroundColor White
    Write-Host "---" -ForegroundColor Gray
}
Write-Host ""

Write-Host "✅ Admin user setup completed!" -ForegroundColor Green
Write-Host "🌐 You can now log in at: https://241runnersawareness.org/admin-login.html" -ForegroundColor Cyan
