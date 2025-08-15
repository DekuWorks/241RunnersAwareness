# Add Developer Access to SQL Server
# This script adds developer IP addresses to the SQL Server firewall

param(
    [Parameter(Mandatory=$true)]
    [string]$DeveloperName,
    
    [Parameter(Mandatory=$true)]
    [string]$IpAddress,
    
    [string]$ResourceGroupName = "241runnersawareness-rg",
    [string]$SqlServerName = "241runners-sql-server-2025"
)

Write-Host "Adding developer access for $DeveloperName with IP: $IpAddress"

# Add firewall rule for the developer
& "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd" sql server firewall-rule create `
    --resource-group $ResourceGroupName `
    --server $SqlServerName `
    --name "Allow$DeveloperName" `
    --start-ip-address $IpAddress `
    --end-ip-address $IpAddress

if ($LASTEXITCODE -eq 0) {
    Write-Host "SUCCESS: Added firewall rule for $DeveloperName"
    Write-Host "Developer can now connect using:"
    Write-Host "Server: $SqlServerName.database.windows.net"
    Write-Host "Database: RunnersDb"
    Write-Host "Username: sqladmin"
    Write-Host "Password: YourStrongPassword123!"
} else {
    Write-Host "ERROR: Failed to add firewall rule"
}
