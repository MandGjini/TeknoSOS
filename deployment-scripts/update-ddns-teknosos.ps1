# ============================================================================
# TeknoSOS - DDNS Update for teknosos.app
# Kjo skript duhet të ekzekutohet çdo 5 minuta
# ============================================================================

# KONFIGURIMI - Vendos DDNS password nga Namecheap
$DDNSPassword = "e0fbdca91dd246afa7d8de8f341ef664"  # Merr nga Namecheap > Advanced DNS > Dynamic DNS

$Domain = "teknosos.app"
$LogFile = "C:\Logs\ddns-teknosos.log"
$LastIPFile = "C:\Logs\last-ip-teknosos.txt"

# Krijo direktorinë e log-ut
$logDir = Split-Path $LogFile -Parent
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

function Write-Log($message) {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp - $message" | Add-Content -Path $LogFile
}

try {
    # Merr IP publike aktuale
    $currentIP = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing -TimeoutSec 10).Content.Trim()
    
    # Kontrollo nëse IP ka ndryshuar
    $lastIP = ""
    if (Test-Path $LastIPFile) {
        $lastIP = (Get-Content $LastIPFile -Raw).Trim()
    }
    
    if ($currentIP -eq $lastIP) {
        # IP nuk ka ndryshuar
        exit 0
    }
    
    Write-Log "IP ndryshoi: $lastIP -> $currentIP - Duke përditësuar DNS..."
    
    # Përditëso @ record
    $url = "https://dynamicdns.park-your-domain.com/update?host=@&domain=$Domain&password=$DDNSPassword&ip=$currentIP"
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30
    
    if ($response.Content -match "<ErrCount>0</ErrCount>") {
        Write-Log "Sukses: @ record -> $currentIP"
    } else {
        Write-Log "GABIM: $($response.Content)"
    }
    
    # Përditëso www record
    $wwwUrl = "https://dynamicdns.park-your-domain.com/update?host=www&domain=$Domain&password=$DDNSPassword&ip=$currentIP"
    $wwwResponse = Invoke-WebRequest -Uri $wwwUrl -UseBasicParsing -TimeoutSec 30
    
    if ($wwwResponse.Content -match "<ErrCount>0</ErrCount>") {
        Write-Log "Sukses: www record -> $currentIP"
    } else {
        Write-Log "GABIM www: $($wwwResponse.Content)"
    }
    
    # Ruaj IP aktuale
    $currentIP | Set-Content -Path $LastIPFile
    
    Write-Host "DNS u përditësua: $Domain -> $currentIP" -ForegroundColor Green
    
} catch {
    Write-Log "GABIM: $($_.Exception.Message)"
    Write-Host "Gabim: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
