# ============================================================================
# TeknoSOS - Production Startup Script
# Run as Administrator for ports 80/443
# ============================================================================

param(
    [string]$Domain = "localhost",
    [switch]$ServiceMode,
    [switch]$Debug
)

$ErrorActionPreference = "Stop"
$ProjectPath = Split-Path -Parent $PSScriptRoot

function Write-Status($message, $color = "Cyan") {
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] $message" -ForegroundColor $color
}

# Check admin for production ports
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "WARNING: Running without admin rights. Ports 80/443 may not bind." -ForegroundColor Yellow
    Write-Host "         Use ports 5000/5001 or run as Administrator." -ForegroundColor Yellow
}

Write-Status "Starting TeknoSOS in Production Mode" "Green"
Write-Host "=" * 50

# Set environment
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:DOTNET_ENVIRONMENT = "Production"

if ($Debug) {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    Write-Status "Debug mode enabled" "Yellow"
}

# Navigate to project
Set-Location $ProjectPath

# Check if build is needed
$dllPath = "$ProjectPath\bin\Release\net8.0\TeknoSOS.WebApp.dll"
$csprojPath = "$ProjectPath\TeknoSOS.WebApp.csproj"

if (-not (Test-Path $dllPath) -or ((Get-Item $csprojPath).LastWriteTime -gt (Get-Item $dllPath).LastWriteTime)) {
    Write-Status "Building application..." "Cyan"
    dotnet build -c Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Status "Build failed!" "Red"
        exit 1
    }
}

# Display configuration
Write-Status "Configuration:" "White"
Write-Host "  Environment: $env:ASPNETCORE_ENVIRONMENT"
Write-Host "  Domain: $Domain"
Write-Host "  Project: $ProjectPath"

# Start application
Write-Host ""
Write-Status "Starting Kestrel web server..." "Green"
Write-Host ""

if ($ServiceMode) {
    # Run detached (for service mode)
    Start-Process -FilePath "dotnet" -ArgumentList "run --no-build -c Release" -WorkingDirectory $ProjectPath -WindowStyle Hidden
    Write-Status "Application started in background" "Green"
} else {
    # Run interactively
    Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
    Write-Host ""
    
    dotnet run --no-build -c Release --launch-profile production-local
}
