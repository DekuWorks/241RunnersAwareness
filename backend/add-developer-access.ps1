# Add Developer Access to SQL Server
# This script adds developer IP addresses to the SQL Server firewall

param(
    [Parameter(Mandatory=$false)]
    [string]$DeveloperName,
    
    [Parameter(Mandatory=$false)]
    [string]$IpAddress,
    
    [string]$ResourceGroupName = "241runnersawareness-rg",
    [string]$SqlServerName = "241runners-sql-server-2025"
)

# Function to get public IP address
function Get-PublicIP {
    try {
        $response = Invoke-RestMethod -Uri "https://api.ipify.org" -Method Get
        return $response
    } catch {
        Write-Host "Could not get public IP automatically. Please provide it manually."
        return $null
    }
}

# If no parameters provided, add both developers
if (-not $DeveloperName -and -not $IpAddress) {
    Write-Host "Adding access for both developers..."
    
    # Get current public IP
    $currentIP = Get-PublicIP
    if ($currentIP) {
        Write-Host "Detected public IP: $currentIP"
        
        # Add Marcus Brown
        Write-Host "Adding Marcus Brown..."
        & "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd" sql server firewall-rule create `
            --resource-group $ResourceGroupName `
            --server $SqlServerName `
            --name "AllowMarcusBrown" `
            --start-ip-address $currentIP `
            --end-ip-address $currentIP
        
        # Add Daniel Carey
        Write-Host "Adding Daniel Carey..."
        & "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd" sql server firewall-rule create `
            --resource-group $ResourceGroupName `
            --server $SqlServerName `
            --name "AllowDanielCarey" `
            --start-ip-address $currentIP `
            --end-ip-address $currentIP
        
        Write-Host "SUCCESS: Added firewall rules for both developers"
        Write-Host "Database connection details:"
        Write-Host "Server: $SqlServerName.database.windows.net"
        Write-Host "Database: RunnersDb"
        Write-Host "Username: sqladmin"
        Write-Host "Password: YourStrongPassword123!"
        Write-Host ""
        Write-Host "Admin Login Credentials:"
        Write-Host "Marcus Brown: dekuworks1@gmail.com / marcus2025"
        Write-Host "Daniel Carey: danielcarey9770@gmail.com / daniel2025"
    } else {
        Write-Host "Please run with specific parameters:"
        Write-Host ".\add-developer-access.ps1 -DeveloperName 'MarcusBrown' -IpAddress 'YOUR_IP'"
    }
} else {
    # Original functionality for specific developer
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
}
