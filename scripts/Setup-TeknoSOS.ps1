#!/usr/bin/env pwsh
<#
.SYNOPSIS
    TeknoSOS Platform - Setup & Execution Script
    Përgatit dhe ekzekuton platformën për testim lokal

.DESCRIPTION
    Script-i automatik për:
    - Kontroll .NET SDK
    - Clean & Restore packages
    - Build project
    - Setup database
    - Start application

.AUTHOR
    TeknoSOS Development Team

.VERSION
    1.0 | 20 Janar 2026
#>

param(
    [switch]$SkipBuild = $false,
    [switch]$SkipDatabase = $false,
    [switch]$SkipRun = $false
)

# Colors for output
$colors = @{
    Success = 'Green'
    Warning = 'Yellow'
    Error   = 'Red'
    Info    = 'Cyan'
    Header  = 'Magenta'
}

function Write-Header {
    param([string]$Message)
    Write-Host "`n$('=' * 80)" -ForegroundColor $colors.Header
    Write-Host "  $Message" -ForegroundColor $colors.Header
    Write-Host "$('=' * 80)`n" -ForegroundColor $colors.Header
}

function Write-Step {
    param([string]$Step, [string]$Message)
    Write-Host "[$Step] $Message" -ForegroundColor $colors.Info
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor $colors.Success
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor $colors.Warning
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor $colors.Error
}

# Start
Write-Header "TEKNOSOS PLATFORM - SETUP & EXECUTION WIZARD"

# Set working directory
$projectPath = "C:\Users\arman\source\repos\TeknoSOS.WebApp"
Set-Location $projectPath
Write-Step "1" "Working directory: $projectPath"

# Step 1: Check .NET
Write-Step "2" "Checking .NET Installation..."
try {
    $dotnetVersion = dotnet --version
    Write-Success ".NET Version: $dotnetVersion"
    
    if ($dotnetVersion -notmatch "8\.") {
        Write-Warning ".NET 8 recommended but got: $dotnetVersion"
    }
} catch {
    Write-Error ".NET CLI not found!"
    Write-Host "Please install .NET 8 SDK from: https://dotnet.microsoft.com/download"
    Read-Host "Press ENTER to exit"
    exit 1
}

# Step 2: Check project file
Write-Step "3" "Checking project structure..."
if (-not (Test-Path "TeknoSOS.WebApp.csproj")) {
    Write-Error "Project file not found: TeknoSOS.WebApp.csproj"
    Read-Host "Press ENTER to exit"
    exit 1
}
Write-Success "Project file found: TeknoSOS.WebApp.csproj"

# Step 3: Build
if (-not $SkipBuild) {
    Write-Step "4" "Cleaning previous build..."
    try {
        dotnet clean | Out-Null
        Write-Success "Clean completed"
    } catch {
        Write-Warning "Clean failed, continuing..."
    }
    
    # Remove directories
    if (Test-Path "bin") { Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue }
    if (Test-Path "obj") { Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue }
    Write-Success "Directories cleaned"
    
    Write-Step "5" "Restoring NuGet packages..."
    try {
        dotnet restore
        Write-Success "Packages restored"
    } catch {
        Write-Error "Package restore failed!"
        Read-Host "Press ENTER to exit"
        exit 1
    }
    
    Write-Step "6" "Building project..."
    try {
        dotnet build
        Write-Success "Build completed successfully"
    } catch {
        Write-Error "Build failed!"
        Read-Host "Press ENTER to exit"
        exit 1
    }
} else {
    Write-Step "4" "Build skipped (--SkipBuild)"
}

# Step 4: Database
if (-not $SkipDatabase) {
    Write-Step "7" "Database setup..."
    
    # Check for LocalDB
    $localdbExists = $null -ne (Get-Command sqllocaldb -ErrorAction SilentlyContinue)
    
    if ($localdbExists) {
        Write-Success "SQL Server LocalDB found"
        
        Write-Host "Dropping existing database..."
        try {
            dotnet ef database drop -f --no-build 2>$null
            Write-Success "Old database dropped"
        } catch {
            Write-Warning "No existing database to drop"
        }
        
        Write-Host "Creating fresh database with migrations..."
        try {
            dotnet ef database update --no-build
            Write-Success "Database setup completed"
        } catch {
            Write-Warning "Database setup issues - check connection string in appsettings.json"
        }
    } else {
        Write-Warning "SQL Server LocalDB not found"
        Write-Host "Ensure SQL Server is installed or configure connection string in appsettings.json"
    }
} else {
    Write-Step "7" "Database setup skipped (--SkipDatabase)"
}

# Step 5: Display info
Write-Header "SETUP COMPLETED - APPLICATION READY"

$info = @"
📊 PLATFORM INFORMATION:
   Name:        TeknoSOS
   Version:     3.0 (MVP Production Ready)
   Status:      ✅ READY FOR TESTING

🔗 ACCESS URLS:
   Web:         https://localhost:5001
   HTTP:        http://localhost:5000
   
👤 TEST CREDENTIALS:
   Email:       admin@teknosos.local
   Password:    Admin#2024
   Role:        Administrator
   
📝 DEFAULT USERS:
   Citizen:     citizen@teknosos.local / Citizen#2024
   Professional: pro@teknosos.local / Pro#2024

📂 PROJECT STRUCTURE:
   Domain/        - Entities & Business Logic
   Application/   - Services & Repositories
   Pages/         - UI & Views
   Controllers/   - REST API Endpoints
   Data/          - Database Context

🔧 USEFUL COMMANDS:
   Build:    dotnet build
   Test:     dotnet test
   Clean:    dotnet clean
   Watch:    dotnet watch run
   
📚 DOCUMENTATION:
   - PLATFORM_OVERVIEW_COMPLETE.md    (Teknik Komplet)
   - TEKNOSOS_PRESENTATION_COMPLETE.md (Prezantim)
   - API_DOCUMENTATION.txt             (API Endpoints)
   
✨ FEATURES READY:
   ✓ User Authentication (3 Roles)
   ✓ Service Requests (CRUD)
   ✓ Professional Matching
   ✓ Real-time Status Tracking
   ✓ Reviews & Ratings (5-Star)
   ✓ Payment Integration (Stripe ready)
   ✓ Admin Dashboard
   ✓ Real-time Notifications

"@

Write-Host $info -ForegroundColor $colors.Success

# Step 6: Run if not skipped
if (-not $SkipRun) {
    Write-Step "8" "Starting application..."
    Write-Host "`n🚀 Launching TeknoSOS Platform...`n" -ForegroundColor Green
    
    Write-Host "
╔══════════════════════════════════════════════════════════════════╗
║                                                                  ║
║   Application will start at https://localhost:5001              ║
║   Press CTRL+C to stop the application                          ║
║                                                                  ║
║   Browser will NOT open automatically.                          ║
║   Open it manually and navigate to: https://localhost:5001      ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
`n" -ForegroundColor Yellow
    
    Read-Host "Press ENTER to start the application"
    
    try {
        dotnet run
    } catch {
        Write-Error "Application failed to start"
    }
} else {
    Write-Step "8" "Application run skipped (--SkipRun)"
    Write-Host "`nTo start the application, run: dotnet run`n" -ForegroundColor Yellow
}

Write-Success "Setup wizard completed!"
