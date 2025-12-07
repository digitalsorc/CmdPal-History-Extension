# CmdPal History Extension - Setup Guide

Complete guide for installing and using the CmdPal History Extension with Windows Command Palette (CmdPal).

## Overview

This extension integrates with **CmdPal** (Windows Command Palette), not PowerToys Run. CmdPal is the next generation command palette for Windows that allows you to start anything from a single interface.

## Prerequisites

- **Windows 10** (version 19041) or **Windows 11** (version 22000 or later)
- **CmdPal** installed (Windows Command Palette)
- **.NET 8.0 Runtime** or later

## Architecture

The CmdPal History Extension uses the CmdPal Extension SDK, which provides:

- **ICommandProvider**: Main extension interface
- **IListPage**: For displaying searchable lists
- **ICommand**: For executable actions
- **COM Activation**: Extensions run as separate processes

## Installation

### Method 1: Using Package Manager (Recommended)

If available through winget or CmdPal's extension manager:

```powershell
# Via winget (if published)
winget install CmdPalHistoryExtension

# Via CmdPal extension manager
# Open CmdPal -> Settings -> Extensions -> Browse Extensions -> Install "Command History"
```

### Method 2: Manual Installation

1. **Build or Download** the extension package
   ```powershell
   # Build from source
   dotnet build -c Release
   dotnet publish -c Release -r win-x64 --self-contained false
   ```

2. **Install the Package**
   - If you have an `.msix` or `.appx` package:
     ```powershell
     Add-AppxPackage -Path "CmdPalHistoryExtension.msix"
     ```

3. **Register COM Server** (for development/testing)
   - Extensions must be registered as COM servers
   - The Package.appxmanifest handles this automatically when packaged
   - For development, you may need to manually register

### Method 3: Development Setup

For developers working on the extension:

1. **Clone the repository**
   ```powershell
   git clone https://github.com/digitalsorc/CmdPal-History-Extension.git
   cd CmdPal-History-Extension
   ```

2. **Build the project**
   ```powershell
   dotnet restore
   dotnet build
   ```

3. **Package for deployment**
   ```powershell
   # Create MSIX package (requires Windows SDK)
   # This will handle COM registration automatically
   ```

## Configuration

### Extension Settings

The extension can be configured through:

1. **CmdPal Settings UI**
   - Open CmdPal
   - Navigate to Settings → Extensions → Command History
   - Adjust preferences

2. **Configuration File**
   - Location: `%USERPROFILE%\.cmdpal\config.json`
   - Contains keybindings and behavior settings

### Default Configuration

```json
{
  "keybindings": {
    "previousCommand": "Up",
    "nextCommand": "Down",
    "openHistoryList": "Ctrl+H"
  },
  "storage": {
    "maxHistoryEntries": 10000,
    "databasePath": "%USERPROFILE%\\.cmdpal\\history.db"
  },
  "behavior": {
    "deduplicateEntries": true,
    "caseSensitiveSearch": false
  },
  "ui": {
    "showTimestamps": true
  }
}
```

## Usage

### Basic Usage

1. **Open CmdPal**
   - Default hotkey: `Win+R` or configured hotkey
   - Or search for "Command Palette" in Start Menu

2. **Access History**
   - Type `history` in CmdPal
   - The Command History extension will appear

3. **Browse History**
   - Navigate with arrow keys
   - Type to search/filter commands
   - Press Enter to execute a command
   - Right-click or use context menu for more options

### Features

- **Search**: Type to filter command history in real-time
- **Execute**: Select and press Enter to copy command to clipboard
- **Delete**: Remove individual commands from history
- **Copy**: Copy commands to clipboard via context menu
- **Statistics**: View execution counts and timestamps

### Context Menu Actions

For each history item:
- **Execute**: Copy and execute the command
- **Copy to clipboard**: Copy command text
- **Delete from history**: Remove this entry

## Troubleshooting

### Extension Not Appearing

1. **Check CmdPal Version**
   - Ensure you have CmdPal installed (not PowerToys Run)
   - Check for updates

2. **Verify Installation**
   - Open PowerShell as Administrator:
     ```powershell
     Get-AppxPackage | Where-Object {$_.Name -like "*CmdPal*"}
     ```

3. **Check Extension Registration**
   - Extensions are registered via Package.appxmanifest
   - Verify COM server GUID matches: `{12345678-1234-1234-1234-123456789012}`

### Extension Not Loading

1. **Check Logs**
   - CmdPal logs: `%LOCALAPPDATA%\CmdPal\Logs`
   - Extension logs: Written to console output

2. **Verify Dependencies**
   - Ensure .NET 8.0 Runtime is installed
   - Check that SQLite database is accessible

3. **Reinstall Extension**
   ```powershell
   # Remove existing package
   Remove-AppxPackage -Package "CmdPalHistoryExtension_*"
   
   # Reinstall
   Add-AppxPackage -Path "CmdPalHistoryExtension.msix"
   ```

### Database Issues

If history is not being saved:

1. **Check Database Path**
   - Default: `%USERPROFILE%\.cmdpal\history.db`
   - Ensure directory exists and is writable

2. **Reset Database**
   ```powershell
   Remove-Item "$env:USERPROFILE\.cmdpal\history.db" -Force
   # Extension will recreate on next launch
   ```

### Performance Issues

1. **Database Size**
   - Check history entry count
   - Consider reducing `maxHistoryEntries` in config
   - Or manually clear old entries

2. **Search Performance**
   - Large datasets (>10,000 entries) may slow searches
   - Database is indexed but performance depends on hardware

## Development

### Building from Source

```powershell
# Clone repository
git clone https://github.com/digitalsorc/CmdPal-History-Extension.git
cd CmdPal-History-Extension

# Restore dependencies
dotnet restore

# Build
dotnet build -c Release

# Run tests
dotnet test

# Publish
dotnet publish -c Release -r win-x64
```

### Project Structure

```
CmdPalHistoryExtension/
├── src/
│   ├── SDK/               # CmdPal SDK interfaces
│   │   ├── ICmdPalExtension.cs
│   │   └── Toolkit.cs
│   ├── CmdPal/            # CmdPal implementation
│   │   ├── HistoryCommandProvider.cs
│   │   └── HistoryListPage.cs
│   ├── Services/          # Core services
│   ├── Models/            # Data models
│   └── Program.cs         # Entry point
├── Package.appxmanifest   # App manifest (COM registration)
├── plugin.json            # Extension metadata
└── CmdPalHistoryExtension.csproj
```

### Extension Architecture

The extension implements the CmdPal SDK:

1. **HistoryCommandProvider** (ICommandProvider)
   - Main entry point
   - Provides top-level commands
   - Manages extension lifecycle

2. **HistoryListPage** (IListPage)
   - Displays command history as searchable list
   - Handles filtering and sorting
   - Provides context menu actions

3. **Commands** (IInvokableCommand)
   - ExecuteHistoryCommand: Executes selected command
   - CopyCommand: Copies to clipboard
   - DeleteHistoryCommand: Removes from history

### Testing

```powershell
# Run unit tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~HistoryManager"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Additional Resources

- **CmdPal Documentation**: See `initial-sdk-spec.md` for full SDK details
- **GitHub Repository**: https://github.com/digitalsorc/CmdPal-History-Extension
- **Issues & Support**: https://github.com/digitalsorc/CmdPal-History-Extension/issues

## Key Differences from PowerToys Run

This extension is built for **CmdPal**, not PowerToys Run:

| Feature | PowerToys Run | CmdPal |
|---------|--------------|---------|
| API | `Query()`, `Result` | `ICommandProvider`, `IListPage` |
| Packaging | DLL Plugin | MSIX Package with COM |
| Registration | plugin.json only | Package.appxmanifest + COM |
| Lifecycle | In-process | Out-of-process (COM) |
| UI Model | PowerToys specific | CmdPal SDK |

## FAQ

**Q: Can I use this with PowerToys Run?**  
A: No, this is specifically for CmdPal. A different version would be needed for PowerToys Run.

**Q: Where is my command history stored?**  
A: In an SQLite database at `%USERPROFILE%\.cmdpal\history.db`

**Q: How do I clear all history?**  
A: Delete the database file or use the Clear History command in the extension.

**Q: Does this work on Linux or macOS?**  
A: No, this is Windows-only as it uses Windows-specific APIs and COM.

**Q: Can I sync history across machines?**  
A: Not in the current version, but this could be a future enhancement.

## License

MIT License - See LICENSE file for details

## Contributing

Contributions welcome! See CONTRIBUTING.md for guidelines.

---

**Made with ❤️ for CmdPal**
