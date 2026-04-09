# ============================================================================
# TeknoSOS - Namecheap Dynamic DNS Update Script
# Schedule this to run every 5 minutes if you have a dynamic IP
# ============================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$Domain,
    
    [Parameter(Mandatory=$true)]
    [string]$DDNSPassword,
    
    [string]$Host = "@",
    
    [switch]$UpdateWWW
)

$LogFile = "C:\Logs\ddns-update.log"
$LastIPFile = "C:\Logs\last-ip.txt"

# Ensure log directory exists
$logDir = Split-Path $LogFile -Parent
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

function Write-Log($message) {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp - $message" | Add-Content -Path $LogFile
    Write-Host $message
}

try {
    # Get current public IP
    $currentIP = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing -TimeoutSec 10).Content.Trim()
    
    # Check if IP has changed
    $lastIP = ""
    if (Test-Path $LastIPFile) {
        $lastIP = Get-Content $LastIPFile -Raw
        $lastIP = $lastIP.Trim()
    }
    
    if ($currentIP -eq $lastIP) {
        Write-Log "IP unchanged: $currentIP"
        exit 0
    }
    
    Write-Log "IP changed from '$lastIP' to '$currentIP' - Updating DNS..."
    
    # Update main record (@)
    $url = "https://dynamicdns.park-your-domain.com/update?host=$Host&domain=$Domain&password=$DDNSPassword&ip=$currentIP"
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30
    
    if ($response.Content -match "<ErrCount>0</ErrCount>") {
        Write-Log "Successfully updated $Host.$Domain to $currentIP"
    } else {
        Write-Log "ERROR updating $Host.$Domain : $($response.Content)"
    }
    
    # Optionally update www record
    if ($UpdateWWW) {
        $wwwUrl = "https://dynamicdns.park-your-domain.com/update?host=www&domain=$Domain&password=$DDNSPassword&ip=$currentIP"
        $wwwResponse = Invoke-WebRequest -Uri $wwwUrl -UseBasicParsing -TimeoutSec 30
        
        if ($wwwResponse.Content -match "<ErrCount>0</ErrCount>") {
            Write-Log "Successfully updated www.$Domain to $currentIP"
        } else {
            Write-Log "ERROR updating www.$Domain : $($wwwResponse.Content)"
        }
    }
    
    # Save current IP
    $currentIP | Set-Content -Path $LastIPFile
    
} catch {
    Write-Log "ERROR: $($_.Exception.Message)"
    exit 1
}
