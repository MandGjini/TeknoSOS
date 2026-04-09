# ============================================================================
# TeknoSOS - Self-Hosting Setup Script
# Configures everything needed to host TeknoSOS from your local machine
# Run as Administrator
# ============================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$Domain,          # e.g., "teknosos.al" or "yourdomain.com"
    
    [Parameter(Mandatory=$true)]
    [string]$DDNSPassword,    # Namecheap Dynamic DNS Password (NOT your account password)
    
    [string]$Email = "",
    
    [switch]$SkipSSL,         # Skip SSL certificate setup (use HTTP only)
    
    [switch]$TestMode         # Use Let's Encrypt staging server
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectPath = Split-Path -Parent $ScriptDir

function Write-Status($message, $color = "Cyan") {
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] $message" -ForegroundColor $color
}

function Write-Section($title) {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Green
    Write-Host " $title" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
}

# Check for admin privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "GABIM: Ky skript kërkon të drejta Administratori!" -ForegroundColor Red
    Write-Host "Kliko djathtas PowerShell dhe zgjidh 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host " ______     __  __     __   __     ______     ______     ______     ______   " -ForegroundColor Cyan
Write-Host "/\__  _\   /\ \_\ \   /\ \ / /    /\  ___\   /\  ___\   /\  __ \   /\  ___\  " -ForegroundColor Cyan
Write-Host "\/_/\ \/   \ \  __ \  \ \ \'/     \ \___  \  \ \  __\   \ \  __ \  \ \___  \ " -ForegroundColor Cyan
Write-Host "   \ \_\    \ \_\ \_\  \ \__|      \/\_____\  \ \_____\  \ \_\ \_\  \/\_____\" -ForegroundColor Cyan
Write-Host "    \/_/     \/_/\/_/   \/_/        \/_____/   \/_____/   \/_/\/_/   \/_____/" -ForegroundColor Cyan
Write-Host ""
Write-Host "                    SELF-HOSTING SETUP                    " -ForegroundColor Yellow
Write-Host ""

Write-Status "Domain: $Domain"
Write-Status "Project: $ProjectPath"

# ============================================================================
# STEP 1: Configure Windows Firewall
# ============================================================================
Write-Section "Hapi 1: Konfigurimi i Firewall"

Write-Status "Duke hapur portet 80 dhe 443..."
& "$ScriptDir\configure-firewall.ps1"

# ============================================================================
# STEP 2: Update Dynamic DNS
# ============================================================================
Write-Section "Hapi 2: Përditësimi i DNS"

Write-Status "Duke përditësuar Namecheap DNS me IP-në tënde publike..."
& "$ScriptDir\update-ddns-namecheap.ps1" -Domain $Domain -DDNSPassword $DDNSPassword -Host "@" -UpdateWWW

# ============================================================================
# STEP 3: Get current public IP
# ============================================================================
$publicIP = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing -TimeoutSec 10).Content.Trim()
Write-Status "IP-ja jote publike: $publicIP" "Yellow"

# ============================================================================
# STEP 4: Update appsettings.Production.json
# ============================================================================
Write-Section "Hapi 3: Konfigurimi i Aplikacionit"

$appSettingsPath = "$ProjectPath\appsettings.Production.json"
$config = Get-Content $appSettingsPath -Raw | ConvertFrom-Json

# Update domain
$config.ApplicationSettings.Domain = $Domain
$config.ApplicationSettings.AllowedOrigins = @(
    "https://$Domain",
    "https://www.$Domain",
    "http://$Domain",
    "http://www.$Domain"
)
$config.AllowedHosts = "$Domain;www.$Domain;localhost"

$config | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
Write-Status "Përditësova appsettings.Production.json" "Green"

# ============================================================================
# STEP 5: SSL Certificate (optional)
# ============================================================================
if (-not $SkipSSL) {
    Write-Section "Hapi 4: Certifikata SSL (Let's Encrypt)"
    
    if ([string]::IsNullOrEmpty($Email)) {
        $Email = "admin@$Domain"
    }
    
    Write-Status "Duke konfiguruar SSL për $Domain..."
    Write-Host "SHËNIM: Sigurohu që:" -ForegroundColor Yellow
    Write-Host "  1. Port 80 është i hapur në router" -ForegroundColor White
    Write-Host "  2. DNS është përditësuar (prit 5-10 minuta)" -ForegroundColor White
    Write-Host ""
    
    $confirm = Read-Host "Dëshiron të vazhdosh me SSL? (P/J)"
    if ($confirm -eq "P" -or $confirm -eq "p" -or $confirm -eq "Y" -or $confirm -eq "y") {
        $sslArgs = @{
            Domain = $Domain
            Email = $Email
        }
        if ($TestMode) {
            $sslArgs.Add("Staging", $true)
        }
        & "$ScriptDir\setup-ssl-letsencrypt.ps1" @sslArgs
    } else {
        Write-Status "SSL u anashkalua. Aplikacioni do punojë vetëm me HTTP." "Yellow"
    }
} else {
    Write-Status "SSL u anashkalua (përdor -SkipSSL)" "Yellow"
}

# ============================================================================
# STEP 6: Create scheduled task for DDNS updates
# ============================================================================
Write-Section "Hapi 5: Përditësim Automatik i DNS"

$taskName = "TeknoSOS-DDNS-Update"
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

if (-not $existingTask) {
    $action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
        -Argument "-ExecutionPolicy Bypass -File `"$ScriptDir\update-ddns-namecheap.ps1`" -Domain $Domain -DDNSPassword $DDNSPassword -Host @ -UpdateWWW"
    
    $trigger = New-ScheduledTaskTrigger -RepetitionInterval (New-TimeSpan -Minutes 5) -Once -At (Get-Date)
    
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
    
    Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Settings $settings -Description "Përditëson IP-në dinamike për TeknoSOS"
    Write-Status "U krijua detyrë e planifikuar për përditësim DNS çdo 5 minuta" "Green"
} else {
    Write-Status "Detyra e planifikuar ekziston tashmë" "Yellow"
}

# ============================================================================
# SUMMARY
# ============================================================================
Write-Section "Konfigurimi Përfundoi!"

Write-Host ""
Write-Host "PËRMBLEDHJE:" -ForegroundColor Green
Write-Host "  Domain:    $Domain" -ForegroundColor White
Write-Host "  IP Publike: $publicIP" -ForegroundColor White
Write-Host ""
Write-Host "HAPAT E ARDHSHËM:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Konfiguro PORT FORWARDING në routerin tënd:" -ForegroundColor White
Write-Host "   - Port 80 (HTTP)  -> $((Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -like '192.168.*' -or $_.IPAddress -like '10.*'} | Select-Object -First 1).IPAddress):80" -ForegroundColor Cyan
Write-Host "   - Port 443 (HTTPS) -> $((Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -like '192.168.*' -or $_.IPAddress -like '10.*'} | Select-Object -First 1).IPAddress):443" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Prit 5-10 minuta që DNS të përditësohet" -ForegroundColor White
Write-Host ""
Write-Host "3. Nis aplikacionin me komandën:" -ForegroundColor White
Write-Host "   cd `"$ProjectPath`"" -ForegroundColor Cyan
Write-Host "   dotnet run -c Release --launch-profile production-local" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. Hape në browser:" -ForegroundColor White
if ($SkipSSL) {
    Write-Host "   http://$Domain" -ForegroundColor Green
} else {
    Write-Host "   https://$Domain" -ForegroundColor Green
}
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
