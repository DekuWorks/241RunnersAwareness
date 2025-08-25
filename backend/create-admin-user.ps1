# Create Admin User Script
# This script creates an admin user using the existing backend API

param(
    [string]$Email = "admin@241runnersawareness.com",
    [string]$Password = "Admin123!",
    [string]$FullName = "System Administrator"
)

Write-Host "Creating admin user..." -ForegroundColor Green
Write-Host "Email: $Email" -ForegroundColor Yellow
Write-Host "Full Name: $FullName" -ForegroundColor Yellow

# API endpoint
$apiUrl = "https://241runnersawareness-api.azurewebsites.net/api/auth/register"

# Create the request body
$requestBody = @{
    email = $Email
    password = $Password
    fullName = $FullName
    role = "admin"
} | ConvertTo-Json

try {
    # Make the API call
    $response = Invoke-RestMethod -Uri $apiUrl -Method POST -Body $requestBody -ContentType "application/json"
    
    if ($response.success) {
        Write-Host "Admin user created successfully!" -ForegroundColor Green
        Write-Host "You can now login with:" -ForegroundColor Yellow
        Write-Host "Email: $Email" -ForegroundColor White
        Write-Host "Password: $Password" -ForegroundColor White
    } else {
        Write-Host "Failed to create admin user: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "Error creating admin user: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "This might be because the user already exists or the API is not ready." -ForegroundColor Yellow
}

Write-Host "Script completed!" -ForegroundColor Green
