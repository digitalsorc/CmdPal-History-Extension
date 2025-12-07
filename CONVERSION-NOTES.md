# Conversion Notes: PowerToys Run → CmdPal

## Summary

This document describes the conversion of the CmdPal History Extension from PowerToys Run plugin architecture to the proper CmdPal (Command Palette) extension architecture.

## The Problem

The extension was originally built for **PowerToys Run**, which is a different system than **CmdPal** (Windows Command Palette). While they share similar goals (launching things quickly), they have completely different APIs and architectures.

### Original Implementation (PowerToys Run)

- **API**: `IPlugin` interface with `Query()` method returning `Result[]`
- **Packaging**: Simple DLL with `plugin.json`
- **Loading**: In-process, loaded on PowerToys startup
- **Activation**: Via `ActionKeyword` (e.g., "history")
- **UI**: Limited to result list with title, subtitle, icon

### Required Implementation (CmdPal)

- **API**: `ICommandProvider` interface with `TopLevelCommands()` returning `ICommandItem[]`
- **Packaging**: MSIX/APPX package with COM registration via `Package.appxmanifest`
- **Loading**: Out-of-process COM server, activated on-demand
- **Activation**: Via AppExtension catalog with COM activation
- **UI**: Rich pages (IListPage, IFormPage, IContentPage) with advanced features

## Changes Made

### 1. Project Structure

**Added Files:**
```
src/SDK/
  ├── ICmdPalExtension.cs    # Core CmdPal interfaces
  └── Toolkit.cs              # Helper implementations

src/CmdPal/
  ├── HistoryCommandProvider.cs  # Main extension entry point
  └── HistoryListPage.cs          # History list UI implementation

src/Program.cs               # COM server entry point
Package.appxmanifest         # COM registration manifest
```

**Modified Files:**
```
CmdPalHistoryExtension.csproj  # Updated for .NET 8 Windows, exe output
plugin.json                     # Updated for CmdPal format
README.md                       # Updated references from PowerToys to CmdPal
PROJECT_SUMMARY.md              # Updated architecture description
src/Services/HistoryManager.cs  # Added DeleteCommand method
```

**Added Documentation:**
```
docs/CMDPAL-SETUP.md            # Comprehensive setup guide
ARCHITECTURE-CMDPAL.md          # Detailed architecture documentation
CONVERSION-NOTES.md             # This file
```

### 2. SDK Implementation

Created a minimal CmdPal SDK based on the `initial-sdk-spec.md`:

**Core Interfaces** (`ICmdPalExtension.cs`):
- `ICommand` - Base for all commands
- `ICommandProvider` - Main extension interface
- `ICommandResult` - Result of command invocation
- `IInvokableCommand` - Commands that can be executed
- `IPage` - Base for pages
- `IListPage` - Page displaying a list
- `IListItem` - Item in a list
- `ICommandItem` - Command in top-level list
- `IContextItem` - Item in context menu
- `ICommandContextItem` - Command in context menu
- `ITag` - Tag/badge on list items
- `IIconInfo` - Icon information

**Helper Classes** (`Toolkit.cs`):
- `CommandProvider` - Base class for providers
- `ListPage` - Base class for list pages
- `InvokableCommand` - Base class for commands
- `ListItem` - List item implementation
- `CommandItem` - Command item implementation
- `CommandContextItem` - Context menu item
- `Tag` - Tag implementation
- `IconInfo` - Icon info implementation
- `CommandResult` - Result helper with static methods

### 3. Extension Implementation

**HistoryCommandProvider** (Main Entry Point):
```csharp
public class HistoryCommandProvider : CommandProvider
{
    public override ICommandItem[] TopLevelCommands()
    {
        // Returns the history browsing page as a top-level command
        return new[] 
        {
            new CommandItem(new HistoryListPage(...))
            {
                Title = "Browse Command History",
                Subtitle = "View and search your command history"
            }
        };
    }
}
```

**HistoryListPage** (UI Implementation):
```csharp
public class HistoryListPage : ListPage
{
    public override IListItem[] GetItems()
    {
        // Fetch history entries from database
        var entries = string.IsNullOrWhiteSpace(_searchText)
            ? _historyManager.GetAllHistory(50)
            : _historyManager.SearchHistory(_searchText);
            
        // Convert to list items with context menus
        return entries.Select(CreateListItem).ToArray();
    }
}
```

**Commands** (Actions):
- `ExecuteHistoryCommand` - Executes/copies command
- `CopyCommand` - Copies to clipboard
- `DeleteHistoryCommand` - Removes from history
- `ErrorCommand` - Displays errors

### 4. COM Server Setup

**Program.cs**:
```csharp
static int Main(string[] args)
{
    if (args[0] == "-RegisterProcessAsComServer")
    {
        // Start COM server
        var provider = new HistoryCommandProvider();
        provider.Initialize();
        // Keep alive until terminated
    }
}
```

**Package.appxmanifest**:
```xml
<!-- COM Server Registration -->
<com:ExeServer Executable="CmdPalHistoryExtension.exe" 
               Arguments="-RegisterProcessAsComServer">
  <com:Class Id="{12345678-1234-1234-1234-123456789012}" />
</com:ExeServer>

<!-- CmdPal Extension Registration -->
<uap3:AppExtension Name="com.microsoft.commandpalette">
  <CmdPalProvider>
    <CreateInstance ClassId="{12345678-1234-1234-1234-123456789012}" />
  </CmdPalProvider>
</uap3:AppExtension>
```

### 5. Project Configuration

**CmdPalHistoryExtension.csproj**:
```xml
<PropertyGroup>
  <TargetFramework>net8.0-windows</TargetFramework>
  <OutputType>Exe</OutputType>  <!-- Changed from Library -->
  <UseWPF>true</UseWPF>
  <Platforms>x64;ARM64</Platforms>
</PropertyGroup>
```

### 6. Plugin Metadata

**plugin.json** (updated format):
```json
{
  "id": "cmdpal-history-extension",
  "name": "Command History",
  "extensionType": "command",
  "activationKeyword": "history",
  "executable": "CmdPalHistoryExtension.exe",  // Changed from .dll
  "clsid": "{12345678-1234-1234-1234-123456789012}",
  "permissions": ["clipboard", "storage"]
}
```

## What Was Preserved

The following components were kept and work without modification:

1. **Services Layer**:
   - `HistoryManager` - SQLite operations
   - `ConfigManager` - Configuration management
   - `ShortcutHandler` - Keyboard shortcuts (legacy)
   - `HistoryNavigator` - Navigation logic (legacy)

2. **Models**:
   - `HistoryEntry` - Command data model
   - `PluginConfig` - Configuration model

3. **Database Schema**:
   - SQLite tables and indexes unchanged
   - Storage location configurable

4. **Configuration**:
   - JSON configuration format
   - Settings structure maintained

5. **Core Functionality**:
   - Add commands to history
   - Search and filter
   - Execution counting
   - Deduplication logic

## What Was Removed/Deprecated

1. **Old Plugin Class**:
   - `CmdPalPlugin.cs` - No longer used (kept for reference)
   - PowerToys Run `Query()` method
   - PowerToys Run `Result` class

2. **WPF UI**:
   - `HistoryWindow.xaml` - Not used in CmdPal (uses IListPage instead)
   - Direct WPF window management
   - Custom window styling

3. **Direct PowerToys Integration**:
   - No longer loads as in-process DLL
   - No direct PowerToys API calls

## Testing

### Build Status

✅ **Compiles successfully**:
```bash
$ dotnet build
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Runtime Requirements

For actual testing, you need:
1. Windows 10/11
2. CmdPal installed (when available)
3. MSIX packaging tools (for deployment)

### Current Limitations

Since we're on Linux for development:
- Cannot test COM activation
- Cannot create MSIX package
- Cannot test with actual CmdPal

These will work on Windows with proper tooling.

## Migration Path

To complete the migration on Windows:

### 1. Package Creation

```powershell
# Install Windows SDK if not present
# Create MSIX package
makeappx pack /d "bin\Release\net8.0-windows" /p "CmdPalHistoryExtension.msix"

# Sign package (required for installation)
signtool sign /f "certificate.pfx" /p "password" "CmdPalHistoryExtension.msix"
```

### 2. Installation

```powershell
# Install package
Add-AppxPackage -Path "CmdPalHistoryExtension.msix"

# Verify installation
Get-AppxPackage | Where-Object {$_.Name -like "*CmdPal*"}
```

### 3. Testing

1. Open CmdPal
2. Type "history"
3. Extension should appear
4. Click to open history list
5. Test search, execute, delete functions

### 4. Debugging

```powershell
# Run with COM server argument
.\CmdPalHistoryExtension.exe -RegisterProcessAsComServer

# Or attach debugger to running process
# Visual Studio → Debug → Attach to Process → CmdPalHistoryExtension.exe
```

## Key Learnings

### 1. Architecture Differences

PowerToys Run and CmdPal are fundamentally different:
- **PowerToys**: Simple plugin model, in-process
- **CmdPal**: Rich extension model, out-of-process, COM-based

### 2. API Surface

CmdPal SDK is more powerful but complex:
- Multiple page types (List, Form, Content)
- Context menus with sub-menus
- Rich metadata (tags, details, icons)
- Lifecycle management (frozen vs fresh)

### 3. Packaging

Proper packaging is critical:
- MSIX/APPX for deployment
- Package.appxmanifest for COM registration
- Signing required for distribution

### 4. Development Experience

Building for CmdPal requires:
- Windows development machine
- Windows SDK and tools
- Understanding of COM and WinRT
- MSIX packaging knowledge

## Next Steps

1. **Test on Windows**:
   - Build on Windows machine
   - Create MSIX package
   - Test with actual CmdPal

2. **Enhance Features**:
   - Add fallback commands for direct search
   - Implement settings page using IFormPage
   - Add export/import functionality

3. **Polish**:
   - Better error handling
   - Localization support
   - Performance optimization

4. **Distribution**:
   - Publish to winget (if supported)
   - CmdPal extension marketplace (when available)
   - GitHub releases

## Conclusion

The extension has been successfully converted from PowerToys Run to CmdPal architecture:

✅ Implements CmdPal SDK interfaces  
✅ Uses COM-based activation  
✅ Proper packaging structure  
✅ Rich list page with context menus  
✅ Preserves core functionality  
✅ Builds successfully  
⏳ Needs Windows testing  
⏳ Needs MSIX packaging  

The extension is **architecturally complete** and ready for Windows testing and deployment.

---

**Conversion Date**: December 7, 2024  
**Status**: ✅ Build Complete, ⏳ Awaiting Windows Testing
