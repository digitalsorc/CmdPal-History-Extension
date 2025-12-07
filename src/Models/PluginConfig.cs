using System.Collections.Generic;

namespace CmdPalHistoryExtension.Models
{
    /// <summary>
    /// Configuration model for the plugin
    /// </summary>
    public class PluginConfig
    {
        public string Version { get; set; } = "1.0.0";
        public KeyBindings Keybindings { get; set; } = new();
        public StorageConfig Storage { get; set; } = new();
        public UIConfig UI { get; set; } = new();
        public BehaviorConfig Behavior { get; set; } = new();
    }

    public class KeyBindings
    {
        public string PreviousCommand { get; set; } = "Up";
        public string NextCommand { get; set; } = "Down";
        public string OpenHistoryList { get; set; } = "Ctrl+H";
    }

    public class StorageConfig
    {
        public int MaxHistoryEntries { get; set; } = 10000;
        public string PersistenceType { get; set; } = "sqlite";
        public string DatabasePath { get; set; } = "%USERPROFILE%\\.cmdpal\\history.db";
    }

    public class UIConfig
    {
        public bool ShowTimestamps { get; set; } = true;
        public int ItemsPerPage { get; set; } = 50;
        public string Theme { get; set; } = "auto";
    }

    public class BehaviorConfig
    {
        public bool DeduplicateEntries { get; set; } = true;
        public bool CaseSensitiveSearch { get; set; } = false;
        public int AutoSaveInterval { get; set; } = 60;
    }
}
