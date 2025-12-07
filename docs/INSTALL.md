# Installation Guide - CmdPal History Extension

This guide will walk you through installing the CmdPal History Extension for PowerToys step by step.

## 📋 Prerequisites

Before installing, ensure you have:

1. **Windows 10 or Windows 11** (64-bit)
2. **PowerToys** installed (v0.75.0 or later)
   - Download from: https://github.com/microsoft/PowerToys/releases
3. **.NET 8.0 Runtime** (usually installed with PowerToys)
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0

## 🔍 Check Your System

### Verify PowerToys is Installed

1. Look for PowerToys icon in your system tray (bottom-right corner)
2. Or search for "PowerToys" in Start Menu
3. If not installed, download and install from the link above

### Check PowerToys Version

1. Right-click PowerToys icon in system tray
2. Click "Settings"
3. Check version number in the top-left corner
4. Should be **v0.75.0** or higher

## 📦 Installation Methods

### Method 1: Pre-built Package (Recommended)

#### Step 1: Download the Package

1. Go to the Releases page
2. Download `CmdPalHistoryExtension-v1.0.0.zip`
3. Save to your Downloads folder

#### Step 2: Extract the Archive

1. Right-click the downloaded ZIP file
2. Select "Extract All..."
3. Choose a temporary location (e.g., Desktop)
4. Click "Extract"

#### Step 3: Locate PowerToys Plugin Directory

Open File Explorer and navigate to:
```
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\
```

**Shortcut:**
1. Press `Win+R` to open Run dialog
2. Paste: `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\`
3. Press Enter

#### Step 4: Copy Plugin Files

1. In the extracted folder, find `CmdPalHistoryExtension` folder
2. Copy the entire folder
3. Paste it into the PowerToys Plugins directory from Step 3
4. The final path should be:
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\CmdPalHistoryExtension\
   ```

#### Step 5: Restart PowerToys

1. Right-click PowerToys icon in system tray
2. Click "Exit PowerToys"
3. Wait a few seconds
4. Open Start Menu and search for "PowerToys"
5. Click to launch PowerToys

#### Step 6: Verify Installation

1. Press `Alt+Space` to open PowerToys Run
2. Type `history`
3. You should see the CmdPal History Extension activate

🎉 **Congratulations!** The extension is now installed.

### Method 2: Build from Source

For developers who want to build from source:

#### Prerequisites
- Visual Studio 2022 or .NET 8.0 SDK
- Git for Windows
- PowerShell 7+

#### Steps

1. **Clone the repository**
   ```powershell
   git clone https://github.com/digitalsorc/CmdPal-History-Extension.git
   cd CmdPal-History-Extension
   ```

2. **Build the project**
   ```powershell
   .\build.ps1 -Configuration Release -Package
   ```

3. **Find the output**
   ```
   The build creates: publish\CmdPalHistoryExtension-v1.0.0.zip
   ```

4. **Follow Method 1 steps** starting from Step 2

## 🔧 Post-Installation Configuration

### First Run Setup

1. **Open PowerToys Run** (`Alt+Space`)
2. Type `history`
3. The extension creates its configuration automatically:
   - Config: `%USERPROFILE%\.cmdpal\config.json`
   - Database: `%USERPROFILE%\.cmdpal\history.db`

### Customize Settings

1. **Open config file**
   ```
   Press Win+R, paste:
   %USERPROFILE%\.cmdpal\config.json
   ```

2. **Edit with Notepad or your preferred editor**

3. **Save and restart PowerToys** for changes to take effect

### Example Customizations

**Change keyboard shortcuts:**
```json
{
  "keybindings": {
    "previousCommand": "Ctrl+Up",
    "nextCommand": "Ctrl+Down",
    "openHistoryList": "Ctrl+Shift+H"
  }
}
```

**Adjust history limits:**
```json
{
  "storage": {
    "maxHistoryEntries": 5000
  }
}
```

## ✅ Verification Checklist

After installation, verify everything works:

- [ ] PowerToys is running (icon in system tray)
- [ ] Plugin files are in correct directory
- [ ] Typing `history` in PowerToys Run shows the extension
- [ ] Config file created at `%USERPROFILE%\.cmdpal\config.json`
- [ ] Can open history browser with `Ctrl+H`
- [ ] Up/Down arrows navigate through history

## 🐛 Troubleshooting

### Extension Not Showing

**Problem:** Typing `history` doesn't activate the extension

**Solutions:**
1. Verify files are in correct location
2. Check PowerToys settings → PowerToys Run → Plugins
3. Ensure plugin is enabled
4. Restart PowerToys completely (Exit and relaunch)
5. Check PowerToys logs for errors

### Permission Errors

**Problem:** "Access denied" when copying files

**Solutions:**
1. Run File Explorer as Administrator:
   - Right-click File Explorer icon
   - Select "Run as administrator"
2. Ensure you have write permissions to the plugins directory
3. Temporarily disable antivirus if it's blocking

### Missing Dependencies

**Problem:** Plugin crashes or doesn't load

**Solutions:**
1. Install .NET 8.0 Runtime from Microsoft
2. Repair PowerToys installation
3. Reinstall Visual C++ Redistributables
4. Check Windows Update for pending updates

### Database Creation Fails

**Problem:** History not saving

**Solutions:**
1. Create directory manually:
   ```powershell
   New-Item -ItemType Directory -Path "$env:USERPROFILE\.cmdpal" -Force
   ```
2. Check disk space (need at least 10 MB)
3. Verify folder permissions
4. Try different database path in config.json

### Shortcut Conflicts

**Problem:** Keyboard shortcuts not working

**Solutions:**
1. Check for conflicts with other applications
2. Try different key combinations
3. Verify config.json syntax is valid
4. Reset to default shortcuts

## 🔄 Updating

### Update to New Version

1. **Download new version** from Releases page
2. **Exit PowerToys** completely
3. **Delete old plugin folder**
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\CmdPalHistoryExtension\
   ```
4. **Follow installation steps** with new version
5. **Your history is preserved** (stored separately in user profile)
6. **Restart PowerToys**

### Preserve Custom Configuration

Your config and history are safe during updates:
- Config: `%USERPROFILE%\.cmdpal\config.json` (not touched)
- History: `%USERPROFILE%\.cmdpal\history.db` (preserved)

## 🗑️ Uninstallation

### Remove Plugin

1. **Exit PowerToys**
2. **Delete plugin folder:**
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\CmdPalHistoryExtension\
   ```
3. **Restart PowerToys**

### Remove All Data (Optional)

To completely remove all traces:

1. **Delete plugin folder** (see above)
2. **Delete user data:**
   ```
   %USERPROFILE%\.cmdpal\
   ```
   This removes your config and history

## 📞 Getting Help

If you encounter issues:

1. **Check troubleshooting section** above
2. **Review documentation** in docs/README.md
3. **Search existing issues** on GitHub
4. **Create new issue** with:
   - PowerToys version
   - Windows version
   - Error messages
   - Steps to reproduce

## 🎓 Next Steps

After successful installation:

1. **Read the User Guide** (docs/README.md)
2. **Try example workflows**
3. **Customize keyboard shortcuts** to your preference
4. **Check configuration options** for advanced features

## 📚 Additional Resources

- **Full Documentation**: docs/README.md
- **Architecture Guide**: docs/ARCHITECTURE.md
- **Build Instructions**: build.ps1
- **GitHub Repository**: https://github.com/digitalsorc/CmdPal-History-Extension

---

**Installation complete!** You're ready to boost your productivity with command history browsing.
