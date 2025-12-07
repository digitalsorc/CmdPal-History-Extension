#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Build script for CmdPal History Extension
.DESCRIPTION
    Builds, tests, and packages the PowerToys CmdPal History Extension
.PARAMETER Configuration
    Build configuration (Debug or Release)
.PARAMETER SkipTests
    Skip running tests
.PARAMETER Package
    Create a deployment package
#>

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [Parameter()]
    [switch]$SkipTests,
    
    [Parameter()]
    [switch]$Package
)

$ErrorActionPreference = 'Stop'

# Colors for output
function Write-Info($message) {
    Write-Host "ℹ️  $message" -ForegroundColor Cyan
}

function Write-Success($message) {
    Write-Host "✅ $message" -ForegroundColor Green
}

function Write-Error($message) {
    Write-Host "❌ $message" -ForegroundColor Red
}

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Info "Starting build process..."

# Clean previous builds
Write-Info "Cleaning previous builds..."
if (Test-Path "bin") {
    Remove-Item -Recurse -Force "bin"
}
if (Test-Path "obj") {
    Remove-Item -Recurse -Force "obj"
}
if (Test-Path "publish") {
    Remove-Item -Recurse -Force "publish"
}

Write-Success "Clean completed"

# Restore dependencies
Write-Info "Restoring NuGet packages..."
dotnet restore CmdPalHistoryExtension.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Error "Package restore failed"
    exit $LASTEXITCODE
}
Write-Success "Packages restored"

# Build the project
Write-Info "Building project ($Configuration)..."
dotnet build CmdPalHistoryExtension.csproj --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit $LASTEXITCODE
}
Write-Success "Build completed"

# Run tests if not skipped
if (-not $SkipTests) {
    Write-Info "Running tests..."
    dotnet test CmdPalHistoryExtension.Tests/CmdPalHistoryExtension.Tests.csproj --configuration $Configuration --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed"
        exit $LASTEXITCODE
    }
    Write-Success "Tests passed"
}

# Create package if requested
if ($Package) {
    Write-Info "Creating deployment package..."
    
    $PublishDir = Join-Path $ScriptDir "publish"
    $OutputDir = Join-Path $PublishDir "CmdPalHistoryExtension"
    
    # Publish the project
    dotnet publish CmdPalHistoryExtension.csproj `
        --configuration $Configuration `
        --output $OutputDir `
        --no-build `
        --verbosity quiet
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Publish failed"
        exit $LASTEXITCODE
    }
    
    # Copy additional files
    Copy-Item "config\default-config.json" -Destination "$OutputDir\config.json" -Force
    
    # Create Images directory if it doesn't exist
    $ImagesDir = Join-Path $OutputDir "Images"
    if (-not (Test-Path $ImagesDir)) {
        New-Item -ItemType Directory -Path $ImagesDir | Out-Null
    }
    
    # Create a ZIP package
    $ZipPath = Join-Path $PublishDir "CmdPalHistoryExtension-v1.0.0.zip"
    if (Test-Path $ZipPath) {
        Remove-Item $ZipPath -Force
    }
    
    Compress-Archive -Path $OutputDir -DestinationPath $ZipPath -CompressionLevel Optimal
    
    Write-Success "Package created at: $ZipPath"
    Write-Info "Package size: $([math]::Round((Get-Item $ZipPath).Length / 1MB, 2)) MB"
}

Write-Success "Build process completed successfully!"
Write-Info "Build output: bin\$Configuration\net8.0-windows\"

if ($Package) {
    Write-Info "Package output: publish\CmdPalHistoryExtension-v1.0.0.zip"
    Write-Info ""
    Write-Info "To install:"
    Write-Info "1. Extract the ZIP file"
    Write-Info "2. Copy to PowerToys plugins directory"
    Write-Info "3. Restart PowerToys"
}
