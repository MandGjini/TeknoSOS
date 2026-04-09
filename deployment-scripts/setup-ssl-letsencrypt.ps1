# ============================================================================
# TeknoSOS - SSL Certificate Setup with Let's Encrypt (Windows)
# Using win-acme (WACS) - https://www.win-acme.com/
# Run as Administrator
# ============================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$Domain,
    
    [string]$Email = "admin@$Domain",
    
    [switch]$Staging,  # Use Let's Encrypt staging server for testing
    
    [string]$WebRoot = "C:\inetpub\teknosos\wwwroot",
    
    [string]$CertPath = "C:\Certificates"
)

$ErrorActionPreference = "Stop"

function Write-Status($message, $color = "Cyan") {
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] $message" -ForegroundColor $color
}

# Check for admin privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "ERROR: This script requires Administrator privileges!" -ForegroundColor Red
    exit 1
}

Write-Status "TeknoSOS SSL Certificate Setup" "Green"
Write-Status "Domain: $Domain" "White"
Write-Status "Email: $Email" "White"
Write-Host ""

# ============================================================================
# 1. Install win-acme if not present
# ============================================================================
$wacsPath = "C:\win-acme"
$wacsExe = "$wacsPath\wacs.exe"

if (-not (Test-Path $wacsExe)) {
    Write-Status "Downloading win-acme..." "Cyan"
    
    # Create directory
    New-Item -ItemType Directory -Path $wacsPath -Force | Out-Null
    
    # Download latest release
    $wacsUrl = "https://github.com/win-acme/win-acme/releases/download/v2.2.9.1701/win-acme.v2.2.9.1701.x64.pluggable.zip"
    $wacsZip = "$wacsPath\win-acme.zip"
    
    Invoke-WebRequest -Uri $wacsUrl -OutFile $wacsZip
    Expand-Archive -Path $wacsZip -DestinationPath $wacsPath -Force
    Remove-Item $wacsZip
    
    Write-Status "win-acme installed to $wacsPath" "Green"
}

# ============================================================================
# 2. Create certificate directory
# ============================================================================
if (-not (Test-Path $CertPath)) {
    New-Item -ItemType Directory -Path $CertPath -Force | Out-Null
    Write-Status "Created certificate directory: $CertPath" "Green"
}

# ============================================================================
# 3. Request Certificate
# ============================================================================
Write-Status "Requesting SSL certificate for $Domain..." "Cyan"

$serverArg = ""
if ($Staging) {
    $serverArg = "--test"
    Write-Status "Using Let's Encrypt STAGING server (for testing)" "Yellow"
}

# Build arguments for win-acme
$wacsArgs = @(
    "--target", "manual",
    "--host", "$Domain,www.$Domain",
    "--validation", "selfhosting",
    "--validationport", "80",
    "--store", "pemfiles",
    "--pemfilespath", $CertPath,
    "--emailaddress", $Email,
    "--accepttos"
)

if ($Staging) {
    $wacsArgs += "--test"
}

Write-Status "Running win-acme..." "Cyan"
& $wacsExe $wacsArgs

if ($LASTEXITCODE -ne 0) {
    Write-Status "Certificate request may have failed. Check above for errors." "Yellow"
    Write-Status "Common issues:" "Yellow"
    Write-Host "  - Port 80 must be accessible from the internet" -ForegroundColor White
    Write-Host "  - DNS must already be pointing to this server" -ForegroundColor White
    Write-Host "  - Firewall must allow inbound port 80" -ForegroundColor White
}

# ============================================================================
# 4. Configure ASP.NET Core to use the certificate
# ============================================================================
$certFile = "$CertPath\$Domain-chain.pem"
$keyFile = "$CertPath\$Domain-key.pem"

if ((Test-Path $certFile) -and (Test-Path $keyFile)) {
    Write-Status "Certificate files created:" "Green"
    Write-Host "  Certificate: $certFile" -ForegroundColor White
    Write-Host "  Private Key: $keyFile" -ForegroundColor White
    
    # Update appsettings.Production.json
    $appSettingsPath = "c:\Users\CAS\Desktop\TeknoSOS.WebApp\appsettings.Production.json"
    if (Test-Path $appSettingsPath) {
        $config = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        
        # Update certificate paths
        $config.Kestrel.Endpoints.Https.Certificate.Path = $certFile
        $config.Kestrel.Endpoints.Https.Certificate.KeyPath = $keyFile
        
        # Update domain
        $config.ApplicationSettings.Domain = $Domain
        $config.AllowedHosts = "$Domain;www.$Domain;localhost"
        
        $config | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
        Write-Status "Updated appsettings.Production.json with certificate paths" "Green"
    }
    
    Write-Host ""
    Write-Status "SSL Certificate Setup Complete!" "Green"
    Write-Host "============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Ensure DNS is configured (A record -> your public IP)" -ForegroundColor White
    Write-Host "2. Open ports 80 and 443 on your router" -ForegroundColor White
    Write-Host "3. Start the application:" -ForegroundColor White
    Write-Host "   dotnet run --launch-profile production-local" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Your site will be available at:" -ForegroundColor Yellow
    Write-Host "   https://$Domain" -ForegroundColor Green
    Write-Host "   https://www.$Domain" -ForegroundColor Green
    
} else {
    Write-Status "Certificate files not found. Please check the win-acme output above." "Red"
}

# ============================================================================
# 5. Schedule Auto-Renewal (Optional)
# ============================================================================
Write-Host ""
Write-Status "Setting up auto-renewal task..." "Cyan"

$taskName = "TeknoSOS SSL Renewal"
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

if (-not $existingTask) {
    $action = New-ScheduledTaskAction -Execute $wacsExe -Argument "--renew --baseuri https://acme-v02.api.letsencrypt.org/"
    $trigger = New-ScheduledTaskTrigger -Daily -At "3:00AM"
    $principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
    $settings = New-ScheduledTaskSettingsSet -ExecutionTimeLimit (New-TimeSpan -Hours 1)
    
    Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Description "Auto-renew SSL certificate for TeknoSOS"
    Write-Status "Created scheduled task for auto-renewal (daily at 3:00 AM)" "Green"
} else {
    Write-Status "Auto-renewal task already exists" "Yellow"
}
