# PowerShell script to create database schema
# This script will execute the SQL commands against Azure SQL Database

$connectionString = "Server=241runners-sql-server-2025.database.windows.net;Database=RunnersDb;User Id=sqladmin;Password=NewStrongPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"

try {
    # Create SQL connection
    $connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $connection.Open()
    Write-Host "Connected to Azure SQL Database successfully!" -ForegroundColor Green
    
    # Read the SQL script
    $sqlScript = Get-Content "create-sql-server-schema.sql" -Raw
    
    # Execute the entire script as one command
    try {
        $sqlCommand = New-Object System.Data.SqlClient.SqlCommand $sqlScript, $connection
        $sqlCommand.CommandTimeout = 300  # 5 minutes timeout
        $sqlCommand.ExecuteNonQuery() | Out-Null
        Write-Host "Executed SQL script successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "Error executing script: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    # Verify tables were created
    $verifyCommand = New-Object System.Data.SqlClient.SqlCommand "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES", $connection
    $tableCount = $verifyCommand.ExecuteScalar()
    Write-Host "Number of tables in database: $tableCount" -ForegroundColor Cyan
    
    Write-Host "Database schema creation completed!" -ForegroundColor Green
}
catch {
    Write-Host "Database operation failed: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    if ($connection -and $connection.State -eq 'Open') {
        $connection.Close()
        Write-Host "Database connection closed." -ForegroundColor Gray
    }
}
