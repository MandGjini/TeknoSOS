# ============================================================================
# TeknoSOS - Cloudflare Tunnel Setup (No Port Forwarding Required!)
# ============================================================================

param(
    [string]$Domain = "teknosos.app"
)

$ErrorActionPreference = "Stop"
$ProjectPath = Split-Path -Parent $PSScriptRoot

function Write-Status($message, $color = "Cyan") {
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] $message" -ForegroundColor $color
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host " Cloudflare Tunnel Setup - Pa Port Forward!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

# ============================================================================
# Step 1: Check/Install cloudflared
# ============================================================================
$cloudflaredPath = "$env:ProgramFiles\cloudflared\cloudflared.exe"
$cloudflaredAlt = "C:\cloudflared\cloudflared.exe"

$cloudflared = $null
if (Test-Path $cloudflaredPath) {
    $cloudflared = $cloudflaredPath
} elseif (Test-Path $cloudflaredAlt) {
    $cloudflared = $cloudflaredAlt
} else {
    # Check if in PATH
    $cloudflared = Get-Command cloudflared -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
}

if (-not $cloudflared) {
    Write-Status "Duke instaluar cloudflared..." "Cyan"
    
    $installDir = "C:\cloudflared"
    if (-not (Test-Path $installDir)) {
        New-Item -ItemType Directory -Path $installDir -Force | Out-Null
    }
    
    # Download cloudflared
    $downloadUrl = "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe"
    $cloudflared = "$installDir\cloudflared.exe"
    
    Write-Status "Duke shkarkuar nga GitHub..." "Cyan"
    Invoke-WebRequest -Uri $downloadUrl -OutFile $cloudflared -UseBasicParsing
    
    # Add to PATH
    $currentPath = [Environment]::GetEnvironmentVariable("PATH", "Machine")
    if ($currentPath -notlike "*$installDir*") {
        [Environment]::SetEnvironmentVariable("PATH", "$currentPath;$installDir", "Machine")
        $env:PATH = "$env:PATH;$installDir"
    }
    
    Write-Status "cloudflared u instalua me sukses!" "Green"
} else {
    Write-Status "cloudflared është i instaluar: $cloudflared" "Green"
}

# ============================================================================
# Step 2: Login and Setup Instructions
# ============================================================================
Write-Host ""
Write-Host "============================================" -ForegroundColor Yellow
Write-Host " HAPAT E ARDHSHËM:" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. LOGO NË CLOUDFLARE:" -ForegroundColor White
Write-Host "   Ekzekuto këtë komandë dhe ndiq udhëzimet:" -ForegroundColor Gray
Write-Host ""
Write-Host "   cloudflared tunnel login" -ForegroundColor Cyan
Write-Host ""
Write-Host "   (Do hapet browser-i për autorizim)" -ForegroundColor Gray
Write-Host ""
Write-Host "2. KRIJO TUNELIN:" -ForegroundColor White
Write-Host ""
Write-Host "   cloudflared tunnel create teknosos" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. KONFIGURO DNS NË CLOUDFLARE:" -ForegroundColor White
Write-Host "   - Shko te Cloudflare Dashboard" -ForegroundColor Gray
Write-Host "   - Shto domain-in $Domain" -ForegroundColor Gray
Write-Host "   - Ndrysho Nameservers në Namecheap me ato të Cloudflare" -ForegroundColor Gray
Write-Host ""
Write-Host "4. LIDH TUNELIN ME DOMAIN:" -ForegroundColor White
Write-Host ""
Write-Host "   cloudflared tunnel route dns teknosos $Domain" -ForegroundColor Cyan
Write-Host "   cloudflared tunnel route dns teknosos www.$Domain" -ForegroundColor Cyan
Write-Host ""
Write-Host "5. NIS TUNELIN:" -ForegroundColor White
Write-Host ""
Write-Host "   cloudflared tunnel run teknosos" -ForegroundColor Cyan
Write-Host ""
Write-Host "============================================" -ForegroundColor Green

# Ask if user wants to proceed with login
Write-Host ""
$proceed = Read-Host "Dëshiron të vazhdosh me login tani? (P/J)"

if ($proceed -eq "P" -or $proceed -eq "p" -or $proceed -eq "Y" -or $proceed -eq "y") {
    Write-Host ""
    Write-Status "Duke hapur Cloudflare login..." "Cyan"
    Write-Host "Ndiq udhëzimet në browser dhe autorizoje..." -ForegroundColor Yellow
    Write-Host ""
    & $cloudflared tunnel login
}
