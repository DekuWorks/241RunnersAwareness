# Fix Database Schema Script
Write-Host "Fixing 241 Runners Awareness Database Schema" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# SQL commands to add missing columns
$sqlCommands = @(
    "ALTER TABLE Users ADD TwoFactorEnabled bit NOT NULL DEFAULT 0;",
    "ALTER TABLE Users ADD TwoFactorSecret nvarchar(MAX) NULL;",
    "ALTER TABLE Users ADD TwoFactorBackupCodes nvarchar(MAX) NULL;",
    "ALTER TABLE Users ADD TwoFactorSetupDate datetime2 NULL;",
    "ALTER TABLE Users ADD RefreshToken nvarchar(MAX) NULL;",
    "ALTER TABLE Users ADD RefreshTokenExpiry datetime2 NULL;",
    "ALTER TABLE Users ADD EmailVerificationToken nvarchar(MAX) NULL;",
    "ALTER TABLE Users ADD EmailVerificationExpiry datetime2 NULL;",
    "ALTER TABLE Users ADD PhoneVerificationCode nvarchar(MAX) NULL;",
    "ALTER TABLE Users ADD PhoneVerificationExpiry datetime2 NULL;"
)

Write-Host "`nAdding missing columns to Users table..." -ForegroundColor Yellow

foreach ($sql in $sqlCommands) {
    try {
        $result = sqlcmd -S localhost -d RunnersDb -Q $sql
        Write-Host "✅ Executed: $($sql.TrimEnd(';'))" -ForegroundColor Green
    } catch {
        if ($_.Exception.Message -like "*already exists*" -or $_.Exception.Message -like "*specified more than once*") {
            Write-Host "⚠️  Column already exists: $($sql.TrimEnd(';'))" -ForegroundColor Yellow
        } else {
            Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "`nDatabase schema fix completed!" -ForegroundColor Green
