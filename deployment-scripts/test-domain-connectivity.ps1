# ============================================================================
# TeknoSOS - Domain & Server Connectivity Test
# Tests DNS, HTTP, HTTPS, and SSL certificate status
# ============================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$Domain,
    
    [switch]$SkipSSL,
    [switch]$ShowHeaders
)

$ErrorActionPreference = "SilentlyContinue"

function Write-Status($message, $success = $true) {
    $icon = if ($success) { "[OK]" } else { "[X]" }
    $color = if ($success) { "Green" } else { "Red" }
    Write-Host "  $icon $message" -ForegroundColor $color
}

function Write-Section($title) {
    Write-Host ""
    Write-Host $title -ForegroundColor Yellow
    Write-Host ("-" * 40) -ForegroundColor DarkGray
}

Write-Host ""
Write-Host ("=" * 50) -ForegroundColor Cyan
Write-Host "  TeknoSOS Domain Connectivity Test" -ForegroundColor Cyan
Write-Host "  Domain: $Domain" -ForegroundColor White
Write-Host ("  Time: " + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')) -ForegroundColor DarkGray
Write-Host ("=" * 50) -ForegroundColor Cyan

$testResults = @{
    DNS = $false
    HTTP = $false
    HTTPS = $false
    SSL = $false
    Content = $false
}

# ============================================================================
# 1. DNS Resolution Test
# ============================================================================
Write-Section "1. DNS Resolution"

try {
    $dnsResult = Resolve-DnsName -Name $Domain -Type A -ErrorAction Stop
    $ip = $dnsResult | Where-Object { $_.Type -eq 'A' } | Select-Object -First 1 -ExpandProperty IPAddress
    
    if ($ip) {
        Write-Status "DNS resolves to: $ip"
        $testResults.DNS = $true
        
        # Test www subdomain
        $wwwDns = Resolve-DnsName -Name "www.$Domain" -Type A -ErrorAction SilentlyContinue
        if ($wwwDns) {
            $wwwIp = $wwwDns | Where-Object { $_.Type -eq 'A' } | Select-Object -First 1 -ExpandProperty IPAddress
            Write-Status "www.$Domain resolves to: $wwwIp"
        } else {
            Write-Status "www.$Domain not configured (optional)" $false
        }
    }
} catch {
    Write-Status "DNS resolution failed: $($_.Exception.Message)" $false
}

# ============================================================================
# 2. Port Connectivity Test
# ============================================================================
Write-Section "2. Port Connectivity"

# Test port 80
try {
    $tcpClient = New-Object System.Net.Sockets.TcpClient
    $tcpClient.Connect($Domain, 80)
    Write-Status "Port 80 (HTTP) is open"
    $tcpClient.Close()
} catch {
    Write-Status "Port 80 (HTTP) is not reachable" $false
}

# Test port 443
try {
    $tcpClient = New-Object System.Net.Sockets.TcpClient
    $tcpClient.Connect($Domain, 443)
    Write-Status "Port 443 (HTTPS) is open"
    $tcpClient.Close()
} catch {
    Write-Status "Port 443 (HTTPS) is not reachable" $false
}

# ============================================================================
# 3. HTTP Response Test
# ============================================================================
Write-Section "3. HTTP Response"

try {
    $httpResponse = Invoke-WebRequest -Uri "http://$Domain" -MaximumRedirection 0 -TimeoutSec 10 -UseBasicParsing
    Write-Status "HTTP returned status: $($httpResponse.StatusCode)"
    $testResults.HTTP = $true
} catch {
    if ($_.Exception.Response.StatusCode.Value__ -eq 301 -or $_.Exception.Response.StatusCode.Value__ -eq 302) {
        Write-Status "HTTP redirects to HTTPS (correct behavior)"
        $testResults.HTTP = $true
    } else {
        Write-Status "HTTP request failed: $($_.Exception.Message)" $false
    }
}

# ============================================================================
# 4. HTTPS Response Test
# ============================================================================
Write-Section "4. HTTPS Response"

if (-not $SkipSSL) {
    try {
        $httpsResponse = Invoke-WebRequest -Uri "https://$Domain" -TimeoutSec 15 -UseBasicParsing
        Write-Status "HTTPS returned status: $($httpsResponse.StatusCode)"
        $testResults.HTTPS = $true
        
        # Check for TeknoSOS in content
        if ($httpsResponse.Content -match "TeknoSOS|teknosos") {
            Write-Status "TeknoSOS content detected in response"
            $testResults.Content = $true
        }
        
        # Check response headers
        if ($ShowHeaders) {
            Write-Host ""
            Write-Host "  Response Headers:" -ForegroundColor DarkGray
            $httpsResponse.Headers.GetEnumerator() | ForEach-Object {
                Write-Host "    $($_.Key): $($_.Value)" -ForegroundColor DarkGray
            }
        }
    } catch {
        Write-Status "HTTPS request failed: $($_.Exception.Message)" $false
    }
}

# ============================================================================
# 5. SSL Certificate Test
# ============================================================================
Write-Section "5. SSL Certificate"

if (-not $SkipSSL) {
    try {
        $request = [System.Net.HttpWebRequest]::Create("https://$Domain")
        $request.AllowAutoRedirect = $false
        $request.Timeout = 15000
        
        $response = $request.GetResponse()
        $cert = $request.ServicePoint.Certificate
        
        if ($cert) {
            $certExpiry = [DateTime]::Parse($cert.GetExpirationDateString())
            $daysRemaining = ($certExpiry - (Get-Date)).Days
            
            Write-Status "Certificate Subject: $($cert.Subject)"
            Write-Status "Certificate Issuer: $($cert.Issuer)"
            $expiryMsg = "Expires: " + $certExpiry.ToString('yyyy-MM-dd') + " (" + $daysRemaining + " days remaining)"
            Write-Status $expiryMsg
            
            if ($daysRemaining -lt 30) {
                Write-Status "WARNING: Certificate expires soon!" $false
            } else {
                $testResults.SSL = $true
            }
        }
        
        $response.Close()
    } catch {
        Write-Status "SSL certificate check failed: $($_.Exception.Message)" $false
    }
}

# ============================================================================
# Summary
# ============================================================================
Write-Host ""
Write-Host ("=" * 50) -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host ("=" * 50) -ForegroundColor Cyan

$passed = ($testResults.Values | Where-Object { $_ -eq $true }).Count
$total = $testResults.Count

$resultColor = if ($passed -eq $total) { "Green" } else { "Yellow" }
Write-Host ""
Write-Host "  Results: $passed / $total tests passed" -ForegroundColor $resultColor
Write-Host ""

$testResults.GetEnumerator() | ForEach-Object {
    $icon = if ($_.Value) { "[OK]" } else { "[X]" }
    $color = if ($_.Value) { "Green" } else { "Red" }
    Write-Host "    $icon $($_.Key)" -ForegroundColor $color
}

Write-Host ""

if ($passed -eq $total) {
    Write-Host "  All tests passed! Your domain is ready." -ForegroundColor Green
} elseif ($testResults.DNS) {
    Write-Host "  DNS works but ports may need configuration." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Common fixes:" -ForegroundColor White
    Write-Host "    - Configure router port forwarding (80 and 443)" -ForegroundColor DarkGray
    Write-Host "    - Check Windows Firewall allows incoming connections" -ForegroundColor DarkGray
    Write-Host "    - Ensure application is running on the server" -ForegroundColor DarkGray
    Write-Host "    - SSL certificate may need to be configured" -ForegroundColor DarkGray
} else {
    Write-Host "  DNS not configured yet. Please check Namecheap settings." -ForegroundColor Yellow
}

Write-Host ""
