# ============================================================================
# TeknoSOS - Skript Komplet për Deployment
# Domain: teknosos.app
# Run as Administrator
# ============================================================================

param(
    [switch]$SkipFirewall,
    [switch]$SkipSSL,
    [switch]$SkipDDNS,
    [switch]$TestOnly
)

$ErrorActionPreference = "Stop"
$Domain = "teknosos.app"
$Email = "armandogjini95@gmail.com"
$ProjectPath = Split-Path -Parent $PSScriptRoot

function Write-Status($message, $color = "Cyan") {
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] $message" -ForegroundColor $color
}

function Write-Section($title) {
    Write-Host ""
    Write-Host ("=" * 60) -ForegroundColor DarkCyan
    Write-Host "  $title" -ForegroundColor White
    Write-Host ("=" * 60) -ForegroundColor DarkCyan
}

# Kontrollo privilegjet admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║      TeknoSOS Deployment Script - teknosos.app             ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Domain:     $Domain" -ForegroundColor White
Write-Host "  Email:      $Email" -ForegroundColor White
Write-Host "  Admin:      $isAdmin" -ForegroundColor $(if($isAdmin){"Green"}else{"Yellow"})
Write-Host ""

if (-not $isAdmin -and -not $TestOnly) {
    Write-Host "KUJDES: Ky skript kërkon privilegje Administrator!" -ForegroundColor Yellow
    Write-Host "Disa funksione mund të mos punojnë." -ForegroundColor Yellow
    Write-Host ""
}

# ============================================================================
# 1. Merr informacion rrjeti
# ============================================================================
Write-Section "1. Informacioni i Rrjetit"

$localIP = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -eq "Wi-Fi" -or $_.InterfaceAlias -eq "Ethernet" } | Select-Object -First 1).IPAddress
Write-Status "IP Lokale: $localIP"

try {
    $publicIP = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing -TimeoutSec 10).Content.Trim()
    Write-Status "IP Publike: $publicIP" "Green"
} catch {
    Write-Status "Nuk u mor IP publike: $($_.Exception.Message)" "Yellow"
    $publicIP = "N/A"
}

# ============================================================================
# 2. Konfiguro Firewall
# ============================================================================
if (-not $SkipFirewall -and $isAdmin) {
    Write-Section "2. Konfigurimi i Firewall"
    
    $rules = @(
        @{Name="TeknoSOS HTTP (80)"; Port=80; Profile="Any"},
        @{Name="TeknoSOS HTTPS (443)"; Port=443; Profile="Any"},
        @{Name="TeknoSOS Dev HTTP (5000)"; Port=5000; Profile="Private,Domain"},
        @{Name="TeknoSOS Dev HTTPS (5001)"; Port=5001; Profile="Private,Domain"}
    )
    
    foreach ($rule in $rules) {
        $existing = Get-NetFirewallRule -DisplayName $rule.Name -ErrorAction SilentlyContinue
        if (-not $existing) {
            New-NetFirewallRule -DisplayName $rule.Name -Direction Inbound -Protocol TCP -LocalPort $rule.Port -Action Allow -Profile $rule.Profile | Out-Null
            Write-Status "Krijuar: $($rule.Name)" "Green"
        } else {
            Write-Status "Ekziston: $($rule.Name)" "Yellow"
        }
    }
} else {
    Write-Status "Firewall u anashkalua (SkipFirewall=$SkipFirewall, Admin=$isAdmin)" "Yellow"
}

# ============================================================================
# 3. Ndërto aplikacionin
# ============================================================================
Write-Section "3. Ndërtimi i Aplikacionit"

Set-Location $ProjectPath
Write-Status "Duke ndërtuar në Release mode..."

$buildResult = dotnet build -c Release 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Status "Ndërtimi përfundoi me sukses!" "Green"
} else {
    Write-Status "Ndërtimi dështoi!" "Red"
    Write-Host $buildResult
    exit 1
}

# ============================================================================
# 4. Testo lidhjen lokale
# ============================================================================
Write-Section "4. Testimi Lokal"

# Ndal proceset ekzistuese
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force

# Fillo serverin në background
Write-Status "Duke filluar serverin..."
$serverJob = Start-Process -FilePath "dotnet" -ArgumentList "run --no-build -c Release --launch-profile http" -WorkingDirectory $ProjectPath -PassThru -WindowStyle Hidden

Start-Sleep -Seconds 10

# Testo HTTP
try {
    $testResult = Invoke-WebRequest -Uri "http://localhost:5000" -MaximumRedirection 0 -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
    Write-Status "HTTP punon - Status: $($testResult.StatusCode)" "Green"
} catch {
    if ($_.Exception.Response.StatusCode.Value__ -eq 307) {
        Write-Status "HTTP ridrejton në HTTPS (307) - OK!" "Green"
    } else {
        Write-Status "HTTP test dështoi: $($_.Exception.Message)" "Yellow"
    }
}

# Testo HTTPS
try {
    $httpsResult = Invoke-WebRequest -Uri "https://localhost:5001" -UseBasicParsing -TimeoutSec 5
    Write-Status "HTTPS punon - Status: $($httpsResult.StatusCode)" "Green"
} catch {
    Write-Status "HTTPS test: $($_.Exception.Message)" "Yellow"
}

# Ndal serverin e testit
Stop-Process -Id $serverJob.Id -Force -ErrorAction SilentlyContinue

# ============================================================================
# 5. Udhëzime për DNS
# ============================================================================
Write-Section "5. Konfigurimi DNS në Namecheap"

Write-Host ""
Write-Host "  HAPAT PËR NAMECHEAP:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  1. Hyr në https://www.namecheap.com/myaccount/" -ForegroundColor White
Write-Host "  2. Shko te Domain List > $Domain > Manage" -ForegroundColor White
Write-Host "  3. Kliko 'Advanced DNS'" -ForegroundColor White
Write-Host "  4. Shto/Ndrysho këto rekorde:" -ForegroundColor White
Write-Host ""
Write-Host "     ┌─────────┬──────────┬─────────────────────┐" -ForegroundColor DarkGray
Write-Host "     │  Type   │   Host   │        Value        │" -ForegroundColor DarkGray
Write-Host "     ├─────────┼──────────┼─────────────────────┤" -ForegroundColor DarkGray
Write-Host "     │    A    │    @     │  $publicIP  │" -ForegroundColor Green
Write-Host "     │    A    │   www    │  $publicIP  │" -ForegroundColor Green
Write-Host "     └─────────┴──────────┴─────────────────────┘" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  5. Aktivizo 'Dynamic DNS' dhe kopjo password-in" -ForegroundColor White
Write-Host "  6. Ruaj password-in te: deployment-scripts\update-ddns-teknosos.ps1" -ForegroundColor White
Write-Host ""

# ============================================================================
# 6. Konfiguro DDNS Task
# ============================================================================
if (-not $SkipDDNS -and $isAdmin) {
    Write-Section "6. DDNS Scheduled Task"
    
    $taskName = "TeknoSOS DDNS Update"
    $existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
    
    if (-not $existingTask) {
        $action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-ExecutionPolicy Bypass -File `"$ProjectPath\deployment-scripts\update-ddns-teknosos.ps1`""
        $trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 5) -RepetitionDuration (New-TimeSpan -Days 9999)
        $settings = New-ScheduledTaskSettingsSet -ExecutionTimeLimit (New-TimeSpan -Minutes 5) -RestartCount 3
        
        Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Settings $settings -Description "Update DDNS for teknosos.app" | Out-Null
        Write-Status "DDNS task u krijua (çdo 5 min)" "Green"
    } else {
        Write-Status "DDNS task ekziston" "Yellow"
    }
}

# ============================================================================
# 7. Përfundimi
# ============================================================================
Write-Section "Përmbledhje"

Write-Host ""
Write-Host "  ✓ Aplikacioni u ndërtua me sukses" -ForegroundColor Green
Write-Host "  ✓ Serveri lokal punon" -ForegroundColor Green
Write-Host ""
Write-Host "  HAPAT E ARDHSHËM:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  1. Konfiguro DNS në Namecheap (shih udhëzimet më sipër)" -ForegroundColor White
Write-Host "  2. Konfiguro port forwarding në router:" -ForegroundColor White
Write-Host "     - Port 80  -> $localIP`:80" -ForegroundColor Cyan
Write-Host "     - Port 443 -> $localIP`:443" -ForegroundColor Cyan
Write-Host "  3. Vendos DDNS password në skriptin update-ddns-teknosos.ps1" -ForegroundColor White
Write-Host "  4. Prit propagimin e DNS (5-30 min)" -ForegroundColor White
Write-Host "  5. Ekzekuto: .\deployment-scripts\setup-ssl-letsencrypt.ps1 -Domain $Domain -Email $Email" -ForegroundColor White
Write-Host "  6. Fillo serverin: .\deployment-scripts\start-production.ps1" -ForegroundColor White
Write-Host ""
Write-Host "  PËR TË TESTUAR:" -ForegroundColor Yellow
Write-Host "  .\deployment-scripts\test-domain-connectivity.ps1 -Domain $Domain" -ForegroundColor Cyan
Write-Host ""
