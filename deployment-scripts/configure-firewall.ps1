# ============================================================================
# TeknoSOS - Windows Firewall Configuration Script
# Run as Administrator
# ============================================================================

param(
    [switch]$Remove,
    [string]$AppPath = "C:\inetpub\teknosos"
)

$ErrorActionPreference = "Stop"

function Write-Status($message, $color = "Cyan") {
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] $message" -ForegroundColor $color
}

# Check for admin privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "ERROR: This script requires Administrator privileges!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

if ($Remove) {
    Write-Status "Removing TeknoSOS firewall rules..." "Yellow"
    
    $rules = @(
        "TeknoSOS HTTP (Port 80)",
        "TeknoSOS HTTPS (Port 443)",
        "TeknoSOS Development HTTP (Port 5000)",
        "TeknoSOS Development HTTPS (Port 5001)"
    )
    
    foreach ($rule in $rules) {
        $existing = Get-NetFirewallRule -DisplayName $rule -ErrorAction SilentlyContinue
        if ($existing) {
            Remove-NetFirewallRule -DisplayName $rule
            Write-Status "Removed: $rule" "Green"
        }
    }
    
    Write-Status "Firewall rules removed successfully!" "Green"
    exit 0
}

Write-Status "Configuring Windows Firewall for TeknoSOS..." "Cyan"

# ============================================================================
# 1. HTTP Port 80 (Production)
# ============================================================================
$ruleName = "TeknoSOS HTTP (Port 80)"
$existing = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Status "Rule already exists: $ruleName" "Yellow"
} else {
    New-NetFirewallRule -DisplayName $ruleName `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort 80 `
        -Action Allow `
        -Profile Any `
        -Description "Allow HTTP traffic for TeknoSOS web application"
    Write-Status "Created: $ruleName" "Green"
}

# ============================================================================
# 2. HTTPS Port 443 (Production)
# ============================================================================
$ruleName = "TeknoSOS HTTPS (Port 443)"
$existing = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Status "Rule already exists: $ruleName" "Yellow"
} else {
    New-NetFirewallRule -DisplayName $ruleName `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort 443 `
        -Action Allow `
        -Profile Any `
        -Description "Allow HTTPS traffic for TeknoSOS web application"
    Write-Status "Created: $ruleName" "Green"
}

# ============================================================================
# 3. Development HTTP Port 5000
# ============================================================================
$ruleName = "TeknoSOS Development HTTP (Port 5000)"
$existing = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Status "Rule already exists: $ruleName" "Yellow"
} else {
    New-NetFirewallRule -DisplayName $ruleName `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort 5000 `
        -Action Allow `
        -Profile Private,Domain `
        -Description "Allow development HTTP traffic for TeknoSOS"
    Write-Status "Created: $ruleName" "Green"
}

# ============================================================================
# 4. Development HTTPS Port 5001
# ============================================================================
$ruleName = "TeknoSOS Development HTTPS (Port 5001)"
$existing = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Status "Rule already exists: $ruleName" "Yellow"
} else {
    New-NetFirewallRule -DisplayName $ruleName `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort 5001 `
        -Action Allow `
        -Profile Private,Domain `
        -Description "Allow development HTTPS traffic for TeknoSOS"
    Write-Status "Created: $ruleName" "Green"
}

# ============================================================================
# 5. URL ACL Reservations (for non-admin execution)
# ============================================================================
Write-Status "Configuring URL ACL reservations..." "Cyan"

$currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name

# Reserve URLs for the current user
$urls = @(
    "http://+:80/",
    "https://+:443/",
    "http://+:5000/",
    "https://+:5001/"
)

foreach ($url in $urls) {
    # First, try to delete existing reservation
    & netsh http delete urlacl url=$url 2>$null
    
    # Add new reservation
    $result = & netsh http add urlacl url=$url user="$currentUser"
    if ($LASTEXITCODE -eq 0) {
        Write-Status "Reserved URL: $url for $currentUser" "Green"
    } else {
        Write-Status "Could not reserve URL: $url (may already be reserved)" "Yellow"
    }
}

# ============================================================================
# 6. Verify Configuration
# ============================================================================
Write-Status "`nFirewall Configuration Summary:" "Cyan"
Write-Host "================================" -ForegroundColor Cyan

$rules = Get-NetFirewallRule -DisplayName "TeknoSOS*" -ErrorAction SilentlyContinue
if ($rules) {
    $rules | Format-Table DisplayName, Enabled, Direction, Action -AutoSize
} else {
    Write-Host "No TeknoSOS firewall rules found!" -ForegroundColor Red
}

Write-Status "`nURL Reservations:" "Cyan"
& netsh http show urlacl | Select-String "teknosos|5000|5001|:80|:443" -Context 0,1

Write-Host "`n============================================" -ForegroundColor Green
Write-Host "Firewall configuration completed!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Open ports 80/443 on your router (port forwarding)" -ForegroundColor White
Write-Host "2. Configure your domain DNS to point to your public IP" -ForegroundColor White
Write-Host "3. Run the SSL certificate setup script" -ForegroundColor White
