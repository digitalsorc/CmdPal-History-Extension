using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CmdPalHistoryExtension.Models;
using CmdPalHistoryExtension.Services;
using CmdPalHistoryExtension.UI;

namespace CmdPalHistoryExtension
{
    /// <summary>
    /// Main plugin class for CmdPal History Extension
    /// Provides command history browsing functionality for PowerToys
    /// </summary>
    public class CmdPalPlugin : IDisposable
    {
        private readonly ConfigManager _configManager;
        private readonly HistoryManager _historyManager;
        private readonly ShortcutHandler _shortcutHandler;
        private readonly HistoryNavigator _historyNavigator;
        private PluginConfig _config;
        private bool _disposed;

        public string Name => "Command History";
        public string Description => "Browse and search through your command history";
        public string ActionKeyword => "history";

        public CmdPalPlugin()
        {
            _configManager = new ConfigManager();
            _config = _configManager.LoadConfig();
            
            var dbPath = _config.Storage.DatabasePath;
            _historyManager = new HistoryManager(
                dbPath,
                _config.Storage.MaxHistoryEntries,
                _config.Behavior.DeduplicateEntries);
            
            _shortcutHandler = new ShortcutHandler();
            _historyNavigator = new HistoryNavigator(_historyManager);
        }

        /// <summary>
        /// Processes a query from the user
        /// </summary>
        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            try
            {
                if (string.IsNullOrWhiteSpace(query.Search))
                {
                    // Show recent history when no search term
                    var recentHistory = _historyManager.GetAllHistory(10);
                    foreach (var entry in recentHistory)
                    {
                        results.Add(CreateResult(entry));
                    }

                    // Add option to open history browser
                    results.Add(new Result
                    {
                        Title = "Open History Browser",
                        SubTitle = "Browse all command history with keyboard navigation",
                        IcoPath = "Images\\icon.png",
                        Action = context =>
                        {
                            OpenHistoryBrowser();
                            return true;
                        }
                    });
                }
                else
                {
                    // Search history
                    var searchResults = _historyManager.SearchHistory(
                        query.Search,
                        _config.Behavior.CaseSensitiveSearch);

                    foreach (var entry in searchResults.Take(20))
                    {
                        results.Add(CreateResult(entry));
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add(new Result
                {
                    Title = "Error",
                    SubTitle = $"Failed to query history: {ex.Message}",
                    IcoPath = "Images\\icon.png"
                });
            }

            return results;
        }

        /// <summary>
        /// Creates a result object from a history entry
        /// </summary>
        private Result CreateResult(HistoryEntry entry)
        {
            var subtitle = _config.UI.ShowTimestamps
                ? $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss} | Executed {entry.ExecutionCount} time(s)"
                : $"Executed {entry.ExecutionCount} time(s)";

            return new Result
            {
                Title = entry.Command,
                SubTitle = subtitle,
                IcoPath = "Images\\icon.png",
                Action = context =>
                {
                    ExecuteCommand(entry.Command);
                    return true;
                }
            };
        }

        /// <summary>
        /// Opens the history browser window
        /// </summary>
        public void OpenHistoryBrowser()
        {
            try
            {
                var window = new HistoryWindow(_historyManager);
                if (window.ShowDialog() == true && !string.IsNullOrEmpty(window.SelectedCommand))
                {
                    ExecuteCommand(window.SelectedCommand);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening history browser: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes a command (adds to history)
        /// </summary>
        public void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            try
            {
                _historyManager.AddCommand(command);
                _historyNavigator.Reset();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the previous command in history
        /// </summary>
        public string? GetPreviousCommand(string currentInput)
        {
            _historyNavigator.Initialize(currentInput);
            return _historyNavigator.GetPrevious();
        }

        /// <summary>
        /// Gets the next command in history
        /// </summary>
        public string? GetNextCommand()
        {
            return _historyNavigator.GetNext();
        }

        /// <summary>
        /// Handles keyboard shortcuts
        /// </summary>
        public bool HandleKeyPress(Key key, ModifierKeys modifiers)
        {
            var previousKey = _config.Keybindings.PreviousCommand;
            var nextKey = _config.Keybindings.NextCommand;
            var openListKey = _config.Keybindings.OpenHistoryList;

            if (_shortcutHandler.MatchesShortcut(key, modifiers, previousKey))
            {
                // Handle previous command
                return true;
            }
            else if (_shortcutHandler.MatchesShortcut(key, modifiers, nextKey))
            {
                // Handle next command
                return true;
            }
            else if (_shortcutHandler.MatchesShortcut(key, modifiers, openListKey))
            {
                OpenHistoryBrowser();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets plugin statistics
        /// </summary>
        public PluginStats GetStats()
        {
            return new PluginStats
            {
                TotalCommands = _historyManager.GetHistoryCount(),
                Version = _config.Version
            };
        }

        /// <summary>
        /// Reloads configuration
        /// </summary>
        public void ReloadConfig()
        {
            _configManager.ReloadConfig();
            _config = _configManager.LoadConfig();
        }

        /// <summary>
        /// Clears all history
        /// </summary>
        public void ClearHistory()
        {
            _historyManager.ClearHistory();
            _historyNavigator.Reset();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _historyManager?.Dispose();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Query object for processing user input
    /// </summary>
    public class Query
    {
        public string Search { get; set; } = string.Empty;
        public string RawQuery { get; set; } = string.Empty;
        public List<string> Terms { get; set; } = new();
    }

    /// <summary>
    /// Result object for displaying search results
    /// </summary>
    public class Result
    {
        public string Title { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;
        public string IcoPath { get; set; } = string.Empty;
        public Func<ActionContext, bool>? Action { get; set; }
        public int Score { get; set; } = 100;
    }

    /// <summary>
    /// Action context for result actions
    /// </summary>
    public class ActionContext
    {
        public Dictionary<string, object> SpecialKeyState { get; set; } = new();
    }

    /// <summary>
    /// Plugin statistics
    /// </summary>
    public class PluginStats
    {
        public int TotalCommands { get; set; }
        public string Version { get; set; } = string.Empty;
    }
}
