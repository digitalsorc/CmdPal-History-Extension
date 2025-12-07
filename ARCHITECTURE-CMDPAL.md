# CmdPal Extension Architecture

## Overview

This document describes the architecture of the CmdPal History Extension, built on the Windows Command Palette (CmdPal) Extension SDK.

## What is CmdPal?

CmdPal (Command Palette) is the next-generation command launcher for Windows, evolved from PowerToys Run. It provides:

- Extensible plugin architecture via COM
- Rich UI components (lists, forms, content pages)
- Out-of-process extension hosting
- MSIX packaging with proper isolation
- Fine-grained permission model

## Key Differences from PowerToys Run

| Aspect | PowerToys Run | CmdPal |
|--------|--------------|---------|
| **Extension Model** | In-process DLL plugins | Out-of-process COM servers |
| **API** | `IPlugin`, `Query()`, `Result` | `ICommandProvider`, `ICommand`, `IListPage` |
| **Packaging** | Loose files + plugin.json | MSIX package + manifest |
| **Lifecycle** | Loaded on startup | On-demand activation |
| **UI Components** | Limited result list | Rich pages (List, Form, Content) |
| **Registration** | File-based discovery | COM + AppExtension catalog |

## CmdPal SDK Components

### Core Interfaces

```csharp
// Main extension interface
ICommandProvider : IDisposable
{
    string DisplayName { get; }
    IIconInfo Icon { get; }
    bool Frozen { get; }  // Can be cached?
    
    ICommandItem[] TopLevelCommands();
    ICommand GetCommand(string id);
    void Initialize();
}

// Commands represent actions or pages
ICommand
{
    string Name { get; }
    string Id { get; }
    IIconInfo Icon { get; }
}

// Invokable actions
IInvokableCommand : ICommand
{
    ICommandResult Invoke(object sender);
}

// Pages display content
IPage : ICommand
{
    string Title { get; }
    bool IsLoading { get; }
}

// List pages show searchable lists
IListPage : IPage
{
    string SearchText { get; }
    string PlaceholderText { get; }
    IListItem[] GetItems();
}

// List items represent rows
IListItem : ICommandItem
{
    ITag[] Tags { get; }
    string Section { get; }
}
```

## Extension Architecture

### 1. Command Provider (Entry Point)

**File**: `src/CmdPal/HistoryCommandProvider.cs`

The `HistoryCommandProvider` is the main entry point:

```csharp
public class HistoryCommandProvider : CommandProvider
{
    public override ICommandItem[] TopLevelCommands()
    {
        // Return top-level commands user sees
        return new ICommandItem[]
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

**Responsibilities**:
- Provide top-level commands
- Manage extension lifecycle
- Handle resource disposal
- Initialize services (HistoryManager, etc.)

### 2. List Page (UI Component)

**File**: `src/CmdPal/HistoryListPage.cs`

The `HistoryListPage` displays command history:

```csharp
public class HistoryListPage : ListPage
{
    public override IListItem[] GetItems()
    {
        // Fetch and return history entries
        var entries = _searchText.IsNullOrEmpty()
            ? _historyManager.GetAllHistory(50)
            : _historyManager.SearchHistory(_searchText);
            
        return entries.Select(CreateListItem).ToArray();
    }
}
```

**Features**:
- Real-time search filtering
- Dynamic list updates
- Context menu actions
- Execution count tags

### 3. Commands (Actions)

**File**: `src/CmdPal/HistoryListPage.cs` (inner classes)

Commands handle user actions:

```csharp
// Execute a history command
internal class ExecuteHistoryCommand : InvokableCommand
{
    public override ICommandResult Invoke(object sender)
    {
        _historyManager.AddCommand(_entry.Command);
        Clipboard.SetText(_entry.Command);
        return CommandResult.Dismiss();
    }
}

// Copy to clipboard
internal class CopyCommand : InvokableCommand
{
    public override ICommandResult Invoke(object sender)
    {
        Clipboard.SetText(_text);
        return CommandResult.Dismiss();
    }
}

// Delete from history
internal class DeleteHistoryCommand : InvokableCommand
{
    public override ICommandResult Invoke(object sender)
    {
        _historyManager.DeleteCommand(_entry.Id);
        return CommandResult.GoBack();
    }
}
```

### 4. Services Layer

**Files**: `src/Services/`

Services handle business logic:

- **HistoryManager**: SQLite database operations
- **ConfigManager**: Configuration file management
- **ShortcutHandler**: Keyboard shortcut parsing (legacy)
- **HistoryNavigator**: History navigation (legacy)

### 5. Models

**Files**: `src/Models/`

Data models:
- **HistoryEntry**: Represents a command in history
- **PluginConfig**: Configuration settings

## Extension Lifecycle

### 1. Registration

Extensions register via `Package.appxmanifest`:

```xml
<Extensions>
  <!-- COM Server -->
  <com:Extension Category="windows.comServer">
    <com:ComServer>
      <com:ExeServer Executable="CmdPalHistoryExtension.exe" 
                     Arguments="-RegisterProcessAsComServer">
        <com:Class Id="{12345678-1234-1234-1234-123456789012}" />
      </com:ExeServer>
    </com:ComServer>
  </com:Extension>
  
  <!-- CmdPal Extension -->
  <uap3:Extension Category="windows.appExtension">
    <uap3:AppExtension Name="com.microsoft.commandpalette">
      <uap3:Properties>
        <CmdPalProvider>
          <Activation>
            <CreateInstance ClassId="{12345678-1234-1234-1234-123456789012}" />
          </Activation>
          <SupportedInterfaces>
            <Commands />
          </SupportedInterfaces>
        </CmdPalProvider>
      </uap3:Properties>
    </uap3:AppExtension>
  </uap3:Extension>
</Extensions>
```

### 2. Discovery

On startup, CmdPal:
1. Queries `AppExtensionCatalog` for extensions
2. Reads manifest properties
3. Caches extension metadata
4. Creates stub entries for frozen extensions

### 3. Activation

When user interacts:
1. CmdPal calls `CoCreateInstance` with CLSID
2. Extension process starts with `-RegisterProcessAsComServer`
3. COM server initializes
4. `ICommandProvider.Initialize()` is called
5. CmdPal calls `TopLevelCommands()`
6. Extension returns command items

### 4. User Interaction

1. User types "history" in CmdPal
2. CmdPal filters top-level commands
3. User selects "Browse Command History"
4. CmdPal navigates to `HistoryListPage`
5. `GetItems()` is called to populate list
6. User can search, select, execute commands

### 5. Disposal

When CmdPal closes or extension is inactive:
1. CmdPal releases COM objects
2. Extension process can exit
3. Resources are cleaned up via `Dispose()`

## Caching Strategy

### Frozen vs Fresh Extensions

**Frozen (default)**:
- CmdPal caches `TopLevelCommands()` results
- Extension process not needed after first load
- Faster startup, lower memory usage
- Good for static command lists

**Fresh (this extension)**:
```csharp
Frozen = false;  // Stay running to update history
```

Why not frozen:
- Command history changes over time
- Need to track command execution
- Provide real-time search results

### Command Reheating

For frozen extensions, CmdPal can "reheat":
1. User activates cached stub command
2. CmdPal creates extension COM object
3. Calls `GetCommand(id)` to load command
4. Command is executed or page displayed

## Data Flow

```
User Input
    ↓
CmdPal Host
    ↓
COM Boundary
    ↓
HistoryCommandProvider.TopLevelCommands()
    ↓
HistoryListPage (returned as command)
    ↓
User navigates to page
    ↓
HistoryListPage.GetItems()
    ↓
HistoryManager.SearchHistory()
    ↓
SQLite Database
    ↓
Results → IListItem[]
    ↓
Displayed in CmdPal
    ↓
User selects item
    ↓
ExecuteHistoryCommand.Invoke()
    ↓
Action executed
```

## UI Components

### List Items

Each history entry becomes a list item:

```
[Icon] git commit -m "Initial commit"
       2024-12-07 10:30:00 | Executed 3 time(s)
       [×3]
       
Context menu:
  ↲ Execute
  📋 Copy to clipboard
  🗑️ Delete from history
```

Components:
- **Title**: Command text
- **Subtitle**: Timestamp + execution count
- **Icon**: Terminal/command icon
- **Tags**: Execution count badge
- **Context Menu**: Additional actions

### Search/Filter

Search is handled by:
1. CmdPal captures user input
2. Sets `HistoryListPage.SearchText`
3. Calls `GetItems()` again
4. `HistoryManager.SearchHistory()` filters results
5. New items returned and displayed

## Configuration

### Extension Metadata

**File**: `plugin.json`

```json
{
  "id": "cmdpal-history-extension",
  "name": "Command History",
  "extensionType": "command",
  "activationKeyword": "history",
  "executable": "CmdPalHistoryExtension.exe",
  "clsid": "{12345678-1234-1234-1234-123456789012}"
}
```

### User Configuration

**File**: `%USERPROFILE%\.cmdpal\config.json`

```json
{
  "storage": {
    "maxHistoryEntries": 10000,
    "databasePath": "%USERPROFILE%\\.cmdpal\\history.db"
  },
  "behavior": {
    "deduplicateEntries": true,
    "caseSensitiveSearch": false
  }
}
```

## Security & Isolation

### Process Isolation

- Extension runs in separate process
- COM marshalling provides boundary
- Process can be terminated independently
- No direct memory access to host

### Permissions

Declared in manifest:
```xml
<Capabilities>
  <Capability Name="internetClient" />
  <rescap:Capability Name="runFullTrust" />
</Capabilities>
```

### Data Isolation

- Extension data in user profile
- SQLite database with file permissions
- No network access required
- All data stays local

## Performance Considerations

### Startup Time

- Process creation: ~50-100ms
- COM activation: ~10-20ms
- Database connection: ~5-10ms
- First query: ~50ms
- Total: ~100-200ms

### Search Performance

- Indexed SQLite queries: ~50ms for 10,000 entries
- Real-time filtering in UI
- Lazy loading of large result sets

### Memory Usage

- Base process: ~20-30 MB
- Per history entry: ~200 bytes
- 10,000 entries: ~2-3 MB in database
- Total footprint: ~25-35 MB

## Testing

### Unit Tests

Test business logic without COM:
```csharp
[Fact]
public void HistoryManager_AddCommand_Success()
{
    var manager = new HistoryManager(":memory:");
    manager.AddCommand("test command");
    var history = manager.GetAllHistory();
    Assert.Single(history);
}
```

### Integration Tests

Test with actual COM activation (requires Windows):
```csharp
// Create COM object
var provider = (ICommandProvider)Activator.CreateInstance(
    Type.GetTypeFromCLSID(guid));
    
provider.Initialize();
var commands = provider.TopLevelCommands();
Assert.NotEmpty(commands);
```

## Debugging

### Attach to Process

1. Start CmdPal
2. Trigger extension activation
3. In Visual Studio: Debug → Attach to Process
4. Find `CmdPalHistoryExtension.exe`
5. Attach and set breakpoints

### Logging

Extension logs to:
- Console output (when run with `-RegisterProcessAsComServer`)
- CmdPal log files (if host implements logging)
- Custom log file (can be added)

### Common Issues

1. **Extension not appearing**
   - Check COM registration
   - Verify CLSID matches
   - Ensure manifest is correct

2. **Crashes on activation**
   - Check .NET runtime version
   - Verify dependencies
   - Look for unhandled exceptions

3. **Search not working**
   - Verify database path
   - Check SQLite version
   - Test queries directly

## Future Enhancements

### Potential Features

1. **Fallback Commands**: Allow "history <query>" to search directly
2. **Settings Page**: Use `IFormPage` for configuration UI
3. **Export/Import**: Use `IContentPage` to display export options
4. **Cloud Sync**: Store history in cloud (requires network permission)
5. **Command Snippets**: Create reusable command templates

### SDK Evolution

As CmdPal SDK evolves, we can add:
- `IDetailsElement` for rich item previews
- `IFilters` for category-based filtering
- `IDynamicListPage` for server-side search
- `IStatusMessage` for progress feedback

## References

- **CmdPal SDK Spec**: `initial-sdk-spec.md`
- **GitHub Repository**: https://github.com/digitalsorc/CmdPal-History-Extension
- **CmdPal Documentation**: See Microsoft Learn (when published)

---

**Last Updated**: December 7, 2024
