# ============================================================================
# TeknoSOS - Install Auto-Start Service
# Run as Administrator!
# ============================================================================

$ErrorActionPreference = "Stop"

function Write-Status($message, $color = "Cyan") {
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] $message" -ForegroundColor $color
}

# Check for admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "GABIM: Duhet të ekzekutohet si Administrator!" -ForegroundColor Red
    Write-Host "Kliko djathtas dhe zgjidh 'Run as Administrator'" -ForegroundColor Yellow
    pause
    exit 1
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host " TeknoSOS Auto-Start Installation" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

# ============================================================================
# 1. Install Cloudflared Service
# ============================================================================
Write-Status "Duke instaluar Cloudflared si Windows Service..." "Cyan"

try {
    & C:\cloudflared\cloudflared.exe service install 2>&1
    Write-Status "Cloudflared service u instalua!" "Green"
} catch {
    Write-Status "Cloudflared service mund të jetë instaluar tashmë" "Yellow"
}

# ============================================================================
# 2. Create TeknoSOS Application Service via Task Scheduler
# ============================================================================
Write-Status "Duke krijuar Scheduled Task për TeknoSOS..." "Cyan"

$taskName = "TeknoSOS-WebApp"
$taskExists = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

if ($taskExists) {
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
    Write-Status "U fshi task-u i vjetër" "Yellow"
}

# Create action to run dotnet
$action = New-ScheduledTaskAction -Execute "dotnet" `
    -Argument "run --launch-profile http" `
    -WorkingDirectory "c:\Users\CAS\Desktop\TeknoSOS.WebApp"

# Trigger on startup
$trigger = New-ScheduledTaskTrigger -AtStartup

# Settings
$settings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RestartCount 3 `
    -RestartInterval (New-TimeSpan -Minutes 1)

# Principal (run as current user)
$principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType S4U -RunLevel Highest

# Register task
Register-ScheduledTask -TaskName $taskName `
    -Action $action `
    -Trigger $trigger `
    -Settings $settings `
    -Principal $principal `
    -Description "Nis TeknoSOS Web Application automatikisht"

Write-Status "Scheduled Task '$taskName' u krijua!" "Green"

# ============================================================================
# 3. Start Services Now
# ============================================================================
Write-Status "Duke nisur shërbimet..." "Cyan"

# Start cloudflared service
Start-Service -Name "cloudflared" -ErrorAction SilentlyContinue

# Start the scheduled task
Start-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host " INSTALIMI PËRFUNDOI!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "TeknoSOS tani do niset automatikisht kur:" -ForegroundColor Yellow
Write-Host "  - Ndizet kompjuteri" -ForegroundColor White
Write-Host "  - Restart-ohet sistemi" -ForegroundColor White
Write-Host ""
Write-Host "Faqja: https://teknosos.app" -ForegroundColor Green
Write-Host ""

pause
