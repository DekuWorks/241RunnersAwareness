# Test Database Connection and Run Cleanup
# This script tests the database connection and runs cleanup operations

param(
    [string]$ConnectionString = "Server=tcp:241runners-sql-server-2025.database.windows.net,1433;Initial Catalog=RunnersDb;Persist Security Info=False;User ID=sqladmin;Password=YourStrongPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
)

Write-Host "Testing database connection..." -ForegroundColor Green

# Test connection using sqlcmd if available
try {
    # Try to use sqlcmd to test connection
    $testQuery = "SELECT 1 as TestResult"
    $result = sqlcmd -S "241runners-sql-server-2025.database.windows.net" -U "sqladmin" -P "YourStrongPassword123!" -Q $testQuery -d "RunnersDb" -h -1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database connection successful!" -ForegroundColor Green
        
        # Check if admin user exists
        Write-Host "Checking for admin user..." -ForegroundColor Yellow
        $adminCheck = sqlcmd -S "241runners-sql-server-2025.database.windows.net" -U "sqladmin" -P "YourStrongPassword123!" -Q "SELECT COUNT(*) as AdminCount FROM Users WHERE Role = 'admin'" -d "RunnersDb" -h -1
        Write-Host "Admin users found: $adminCheck" -ForegroundColor Cyan
        
        # Check for test data
        Write-Host "Checking for test data..." -ForegroundColor Yellow
        $testDataCheck = sqlcmd -S "241runners-sql-server-2025.database.windows.net" -U "sqladmin" -P "YourStrongPassword123!" -Q "SELECT COUNT(*) as TestCount FROM Users WHERE Email LIKE '%@test.com%' OR Email LIKE '%@example.com%'" -d "RunnersDb" -h -1
        Write-Host "Test users found: $testDataCheck" -ForegroundColor Cyan
        
    } else {
        Write-Host "Database connection failed!" -ForegroundColor Red
    }
} catch {
    Write-Host "sqlcmd not available or connection failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please install SQL Server Command Line Utilities or use Azure Data Studio" -ForegroundColor Yellow
}

Write-Host "Database connection test completed!" -ForegroundColor Green
