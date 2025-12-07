# Deployment Checklist

## IMPORTANT: Before Production Deployment

### 1. Generate Unique GUID

⚠️ **CRITICAL**: The extension currently uses a placeholder GUID that MUST be replaced.

**Current GUID (PLACEHOLDER - DO NOT USE IN PRODUCTION)**:
```
12345678-1234-1234-1234-123456789012
```

**Generate a new GUID:**

```powershell
# In PowerShell
[guid]::NewGuid()

# Or use guidgen.exe from Visual Studio
# Or use: https://www.guidgenerator.com/
```

**Update GUID in these files:**
1. `Package.appxmanifest` - In `<com:Class Id="...">`
2. `plugin.json` - In `"clsid"` field

**Example:**
```xml
<!-- Package.appxmanifest -->
<com:Class Id="A1B2C3D4-E5F6-7890-ABCD-EF1234567890" DisplayName="History Command Provider" />
```

```json
// plugin.json
{
  "clsid": "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}"
}
```

### 2. Code Signing Certificate

For MSIX distribution, you need a code signing certificate:

**Options:**
1. **Production**: Purchase certificate from trusted CA (e.g., DigiCert, Sectigo)
2. **Testing**: Create self-signed certificate (not for public distribution)

**Create self-signed certificate (testing only):**
```powershell
New-SelfSignedCertificate -Type CodeSigningCert `
  -Subject "CN=YourCompany" `
  -KeyUsage DigitalSignature `
  -FriendlyName "CmdPal History Extension" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
```

### 3. Build Release Version

```powershell
# Clean previous builds
dotnet clean

# Build release
dotnet build -c Release

# Publish for specific platform
dotnet publish -c Release -r win-x64 --self-contained false
```

### 4. Create MSIX Package

```powershell
# Navigate to publish directory
cd bin\Release\net8.0-windows\win-x64\publish

# Create MSIX package
makeappx pack /d . /p ..\CmdPalHistoryExtension.msix /l

# Sign the package
signtool sign /fd SHA256 /a /f "path\to\certificate.pfx" /p "certificate_password" ..\CmdPalHistoryExtension.msix
```

### 5. Version Management

Update version numbers in:
1. `CmdPalHistoryExtension.csproj` - `<Version>1.0.0</Version>`
2. `Package.appxmanifest` - `Version="1.0.0.0"`
3. `plugin.json` - `"version": "1.0.0"`
4. `README.md` - Version badge

### 6. Testing Checklist

Before release, test:
- [ ] Installation on clean Windows 10
- [ ] Installation on clean Windows 11
- [ ] CmdPal integration works
- [ ] History commands are saved
- [ ] Search functionality works
- [ ] Context menu actions work
- [ ] Clipboard operations work
- [ ] Uninstallation is clean
- [ ] No console windows appear
- [ ] Performance is acceptable (< 200ms startup)

### 7. Documentation Review

Ensure these files are up-to-date:
- [ ] README.md - Installation instructions
- [ ] CMDPAL-SETUP.md - Setup guide
- [ ] ARCHITECTURE-CMDPAL.md - Technical docs
- [ ] CHANGELOG.md - Version history
- [ ] LICENSE - Legal terms

### 8. Distribution

**Via GitHub Releases:**
1. Create GitHub release with tag (e.g., v1.0.0)
2. Attach signed MSIX file
3. Include SHA256 hash of package
4. Provide installation instructions

**Via winget (if supported):**
1. Create winget manifest
2. Submit PR to winget-pkgs repository
3. Wait for approval

**Via CmdPal Extension Store (when available):**
1. Follow CmdPal store submission process
2. Provide required metadata and screenshots
3. Submit for review

### 9. Security Considerations

- [ ] No hardcoded secrets or API keys
- [ ] Database file has proper permissions
- [ ] No execution of arbitrary commands
- [ ] Input validation on all user input
- [ ] Error messages don't leak sensitive info
- [ ] Dependencies are up-to-date and secure

### 10. Privacy & Compliance

- [ ] Privacy policy if collecting any data
- [ ] GDPR compliance if applicable
- [ ] Clear data storage location documentation
- [ ] Uninstall cleanly removes all data

## Build Script

Here's a complete build and package script:

```powershell
# build-release.ps1
param(
    [string]$Version = "1.0.0",
    [string]$Platform = "win-x64"
)

Write-Host "Building CmdPal History Extension v$Version" -ForegroundColor Green

# Clean
Write-Host "Cleaning..." -ForegroundColor Yellow
dotnet clean

# Build
Write-Host "Building..." -ForegroundColor Yellow
dotnet build -c Release

# Publish
Write-Host "Publishing for $Platform..." -ForegroundColor Yellow
dotnet publish -c Release -r $Platform --self-contained false

# Create package
Write-Host "Creating MSIX package..." -ForegroundColor Yellow
$publishDir = "bin\Release\net8.0-windows\$Platform\publish"
$outputFile = "bin\Release\CmdPalHistoryExtension-v$Version-$Platform.msix"

makeappx pack /d $publishDir /p $outputFile /l

Write-Host "Package created: $outputFile" -ForegroundColor Green
Write-Host "Don't forget to sign it!" -ForegroundColor Yellow

# Calculate hash
$hash = (Get-FileHash $outputFile -Algorithm SHA256).Hash
Write-Host "SHA256: $hash" -ForegroundColor Cyan
```

## Verification

After packaging, verify:

```powershell
# Inspect package contents
makeappx unpack /p CmdPalHistoryExtension.msix /d extracted /l

# Verify signature
signtool verify /pa /v CmdPalHistoryExtension.msix

# Check manifest
Get-AppxPackageManifest -Package extracted\Package.appxmanifest
```

## Rollback Plan

If issues are found post-deployment:
1. Remove package: `Remove-AppxPackage -Package "CmdPalHistoryExtension_*"`
2. Fix issues in code
3. Increment version number
4. Rebuild and redeploy

## Support

After deployment, monitor:
- GitHub Issues for bug reports
- Performance metrics (if available)
- User feedback
- Crash reports (Windows Error Reporting)

## Release Notes Template

```markdown
## CmdPal History Extension v1.0.0

### Features
- Browse command history with arrow keys
- Real-time search and filtering
- Context menu actions (Execute, Copy, Delete)
- Execution count tracking
- Configurable settings

### Installation
1. Download CmdPalHistoryExtension-v1.0.0-win-x64.msix
2. Run: `Add-AppxPackage -Path "CmdPalHistoryExtension-v1.0.0-win-x64.msix"`
3. Open CmdPal and type "history"

### System Requirements
- Windows 10 (19041) or Windows 11
- .NET 8.0 Runtime
- CmdPal installed

### Known Issues
- None

### SHA256
[Package hash here]
```

---

**Last Updated**: December 7, 2024
