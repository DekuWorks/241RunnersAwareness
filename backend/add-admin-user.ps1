# Add Admin User Script
# This script creates an admin user in the database

param(
    [string]$Email = "admin@241runnersawareness.com",
    [string]$Password = "Admin123!",
    [string]$FullName = "System Administrator"
)

Write-Host "Adding admin user to database..." -ForegroundColor Green
Write-Host "Email: $Email" -ForegroundColor Yellow
Write-Host "Full Name: $FullName" -ForegroundColor Yellow

# Navigate to backend directory
Set-Location $PSScriptRoot

# Build the project
Write-Host "Building project..." -ForegroundColor Cyan
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create a simple C# program to add the admin user
$adminScript = @"
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using _241RunnersAwareness.BackendAPI.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RunnersDbContext>();
        optionsBuilder.UseSqlite("Data Source=RunnersDb.db");
        
        using var context = new RunnersDbContext(optionsBuilder.Options);
        
        // Create a simple configuration for AuthService
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"JWT_SECRET_KEY", "your-secret-key-here"},
                {"JWT_ISSUER", "241RunnersAwareness"},
                {"JWT_AUDIENCE", "241RunnersAwareness"}
            })
            .Build();
            
        var authService = new AuthService(config);
        
        // Check if admin user already exists
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "$Email");
        
        if (existingUser != null)
        {
            Console.WriteLine("Admin user already exists. Updating role to admin...");
            existingUser.Role = "admin";
            existingUser.IsActive = true;
            await context.SaveChangesAsync();
            Console.WriteLine("Admin user updated successfully!");
            return;
        }
        
        // Create new admin user
        var adminUser = new User
        {
            UserId = Guid.NewGuid(),
            Username = "$Email".Split('@')[0],
            Email = "$Email",
            FullName = "$FullName",
            PasswordHash = authService.HashPassword("$Password"),
            Role = "admin",
            EmailVerified = true,
            PhoneVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };
        
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
        
        Console.WriteLine("Admin user created successfully!");
        Console.WriteLine($"Email: $Email");
        Console.WriteLine($"Password: $Password");
        Console.WriteLine("Role: admin");
    }
}
"@

# Write the script to a temporary file
$adminScript | Out-File -FilePath "temp-admin-script.cs" -Encoding UTF8

# Create a temporary project file
$projectFile = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="241RunnersAwareness.BackendAPI.csproj" />
  </ItemGroup>
</Project>
"@

$projectFile | Out-File -FilePath "temp-admin-project.csproj" -Encoding UTF8

try {
    # Run the script
    Write-Host "Running admin user creation script..." -ForegroundColor Cyan
    dotnet run --project temp-admin-project.csproj
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Admin user created/updated successfully!" -ForegroundColor Green
        Write-Host "You can now login with:" -ForegroundColor Yellow
        Write-Host "Email: $Email" -ForegroundColor White
        Write-Host "Password: $Password" -ForegroundColor White
    } else {
        Write-Host "Failed to create admin user!" -ForegroundColor Red
    }
}
finally {
    # Clean up temporary files
    if (Test-Path "temp-admin-script.cs") { Remove-Item "temp-admin-script.cs" }
    if (Test-Path "temp-admin-project.csproj") { Remove-Item "temp-admin-project.csproj" }
    if (Test-Path "temp-admin-project.csproj.user") { Remove-Item "temp-admin-project.csproj.user" }
    if (Test-Path "bin") { Remove-Item "bin" -Recurse -Force }
    if (Test-Path "obj") { Remove-Item "obj" -Recurse -Force }
}

Write-Host "Script completed!" -ForegroundColor Green
