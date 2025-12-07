# CmdPal History Extension

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

A powerful CmdPal (Command Palette) extension that brings terminal-style command history browsing to Windows. Navigate through your previous commands using keyboard shortcuts, search your history, and boost your productivity.

## ✨ Features

- 🔄 **Up/Down Arrow Navigation** - Browse command history like in a terminal
- 🔍 **Fast Full-Text Search** - Find commands instantly
- ⌨️ **Customizable Shortcuts** - Configure your preferred key bindings
- 💾 **Persistent Storage** - History saved across sessions
- 🎨 **Modern UI** - Dark-themed history browser with keyboard navigation
- ⚡ **High Performance** - Sub-100ms queries on 10,000+ entries
- 🔒 **Privacy First** - All data stays local, no telemetry

## 🚀 Quick Start

### Installation

1. **Download** the latest release: `CmdPalHistoryExtension-v1.0.0.zip`
2. **Extract** to CmdPal extensions directory:
   ```
   %LOCALAPPDATA%\Microsoft\CmdPal\Extensions\CmdPalHistoryExtension\
   ```
3. **Restart** CmdPal
4. **Try it**: Open CmdPal and type `history`

📖 **Detailed instructions**: See [docs/INSTALL.md](docs/INSTALL.md)

## 🎮 Usage

| Shortcut | Action |
|----------|--------|
| `Up Arrow` | Previous command |
| `Down Arrow` | Next command |
| `Ctrl+H` | Open history browser |
| `Enter` | Execute command |

**Example:**
```
1. Open CmdPal (default: Win+R or configured hotkey)
2. Type 'history'
3. Browse through your command history
4. Press Enter to execute
```

## 📚 Documentation

- **[User Guide](docs/README.md)** - Complete features and usage
- **[Installation Guide](docs/INSTALL.md)** - Step-by-step setup
- **[Architecture](docs/ARCHITECTURE.md)** - Technical details

## 🔧 Building from Source

### Prerequisites
- .NET 8.0 SDK
- PowerShell 7+

### Build
```powershell
# Clone repository
git clone https://github.com/digitalsorc/CmdPal-History-Extension.git
cd CmdPal-History-Extension

# Build and package
.\build.ps1 -Configuration Release -Package
```

### Run Tests
```powershell
dotnet test
```

## ⚙️ Configuration

Default config location: `%USERPROFILE%\.cmdpal\config.json`

```json
{
  "keybindings": {
    "previousCommand": "Up",
    "nextCommand": "Down",
    "openHistoryList": "Ctrl+H"
  },
  "storage": {
    "maxHistoryEntries": 10000
  }
}
```

## 🧪 Testing

Comprehensive test coverage using xUnit:

```powershell
dotnet test CmdPalHistoryExtension.Tests.csproj
```

**Test Coverage:**
- HistoryManager: CRUD, search, cleanup
- ConfigManager: Load, save, validation
- ShortcutHandler: Key parsing, conflicts
- HistoryNavigator: Up/down navigation

## 🗺️ Project Structure

```
CmdPalHistoryExtension/
├── src/
│   ├── Models/          # Data models
│   ├── Services/        # Core business logic
│   ├── UI/              # WPF user interface
│   └── CmdPalPlugin.cs  # Main plugin entry
├── tests/               # xUnit tests
├── docs/                # Documentation
├── config/              # Default configuration
└── build.ps1            # Build script
```

## 🤝 Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests
5. Submit a pull request

## 📝 License

MIT License - see LICENSE file for details

## 🙏 Acknowledgments

- CmdPal team for the extension framework
- SQLite for the database engine
- .NET team for the runtime

## 📮 Support

- **Issues**: https://github.com/digitalsorc/CmdPal-History-Extension/issues
- **Discussions**: https://github.com/digitalsorc/CmdPal-History-Extension/discussions

## 📅 Changelog

### v1.0.0 (2024-12-07)
- ✅ Command history browsing with up/down arrows
- ✅ Full history browser UI with search
- ✅ Customizable keyboard shortcuts
- ✅ SQLite-based persistence
- ✅ Comprehensive test coverage
- ✅ Complete documentation

---

**Made with ❤️ for CmdPal users**

*Boost your productivity with command history at your fingertips!*
