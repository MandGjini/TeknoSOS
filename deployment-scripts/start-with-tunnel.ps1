# ============================================================================
# TeknoSOS - Start with Cloudflare Tunnel
# Nis aplikacionin dhe tunelin Cloudflare në të njëjtën kohë
# ============================================================================

param(
    [string]$TunnelName = "teknosos",
    [switch]$SetupMode
)

$ErrorActionPreference = "Stop"
$ProjectPath = Split-Path -Parent $PSScriptRoot

function Write-Status($message, $color = "Cyan") {
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] $message" -ForegroundColor $color
}

# Find cloudflared
$cloudflared = Get-Command cloudflared -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
if (-not $cloudflared) {
    $cloudflared = "C:\cloudflared\cloudflared.exe"
}

if (-not (Test-Path $cloudflared)) {
    Write-Host "GABIM: cloudflared nuk është i instaluar!" -ForegroundColor Red
    Write-Host "Ekzekuto: .\setup-cloudflare-tunnel.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host " TeknoSOS + Cloudflare Tunnel" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

# Check if tunnel exists
$tunnelList = & $cloudflared tunnel list 2>&1
if ($tunnelList -notmatch $TunnelName) {
    Write-Host "GABIM: Tuneli '$TunnelName' nuk ekziston!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Krijo tunelin me komandat:" -ForegroundColor Yellow
    Write-Host "  cloudflared tunnel login" -ForegroundColor Cyan
    Write-Host "  cloudflared tunnel create $TunnelName" -ForegroundColor Cyan
    exit 1
}

# Create config file
$configDir = "$env:USERPROFILE\.cloudflared"
$configFile = "$configDir\config.yml"

if (-not (Test-Path $configDir)) {
    New-Item -ItemType Directory -Path $configDir -Force | Out-Null
}

# Get tunnel ID
$tunnelId = & $cloudflared tunnel list --output json 2>$null | ConvertFrom-Json | Where-Object { $_.name -eq $TunnelName } | Select-Object -ExpandProperty id

if ($tunnelId) {
    $configContent = @"
tunnel: $tunnelId
credentials-file: $configDir\$tunnelId.json

ingress:
  - hostname: teknosos.app
    service: http://localhost:5000
  - hostname: www.teknosos.app
    service: http://localhost:5000
  - service: http_status:404
"@

    $configContent | Set-Content $configFile -Encoding UTF8
    Write-Status "Krijova config.yml" "Green"
}

# Start application in background
Write-Status "Duke nisur TeknoSOS..." "Cyan"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5000"

$appJob = Start-Job -ScriptBlock {
    param($path)
    Set-Location $path
    dotnet run -c Release
} -ArgumentList $ProjectPath

Start-Sleep -Seconds 5

# Check if app started
if ($appJob.State -eq "Running") {
    Write-Status "TeknoSOS u nis në http://localhost:5000" "Green"
} else {
    Write-Host "GABIM: Aplikacioni nuk u nis!" -ForegroundColor Red
    Receive-Job $appJob
    exit 1
}

# Start tunnel
Write-Host ""
Write-Status "Duke nisur Cloudflare Tunnel..." "Cyan"
Write-Host ""
Write-Host "Faqja do jetë e disponueshme në:" -ForegroundColor Yellow
Write-Host "  https://teknosos.app" -ForegroundColor Green
Write-Host "  https://www.teknosos.app" -ForegroundColor Green
Write-Host ""
Write-Host "Shtyp Ctrl+C për të ndalur" -ForegroundColor Gray
Write-Host ""

try {
    & $cloudflared tunnel run $TunnelName
} finally {
    Write-Status "Duke ndalur aplikacionin..." "Yellow"
    Stop-Job $appJob -ErrorAction SilentlyContinue
    Remove-Job $appJob -ErrorAction SilentlyContinue
}
