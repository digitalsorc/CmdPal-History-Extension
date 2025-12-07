using System;
using System.Linq;
using CmdPalHistoryExtension.Models;
using CmdPalHistoryExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHistoryExtension.CmdPal
{
    /// <summary>
    /// List page that displays command history
    /// </summary>
    public class HistoryListPage : ListPage
    {
        private readonly HistoryManager _historyManager;
        private readonly PluginConfig _config;
        private string _searchText = string.Empty;

        public HistoryListPage(HistoryManager historyManager, PluginConfig config)
        {
            _historyManager = historyManager;
            _config = config;

            Id = "history.browse";
            Name = "Command History";
            Title = "Browse Command History";
            Icon = new IconInfo("\uE81C");
            PlaceholderText = "Search command history...";
        }

        public override string? SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value ?? string.Empty;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        public override IListItem[] GetItems()
        {
            try
            {
                var entries = string.IsNullOrWhiteSpace(_searchText)
                    ? _historyManager.GetAllHistory(50)
                    : _historyManager.SearchHistory(_searchText, _config.Behavior.CaseSensitiveSearch);

                return entries.Select(entry => CreateListItem(entry)).ToArray();
            }
            catch (Exception ex)
            {
                // Return error item
                return new IListItem[]
                {
                    new ListItem(new ErrorCommand($"Error loading history: {ex.Message}"))
                    {
                        Title = "Error",
                        Subtitle = ex.Message,
                        Icon = new IconInfo("\uE783") // Error icon
                    }
                };
            }
        }

        private IListItem CreateListItem(HistoryEntry entry)
        {
            var command = new ExecuteHistoryCommand(entry, _historyManager);
            
            var subtitle = _config.UI.ShowTimestamps
                ? $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss} | Executed {entry.ExecutionCount} time(s)"
                : $"Executed {entry.ExecutionCount} time(s)";

            var tags = new[]
            {
                new Tag { Text = $"×{entry.ExecutionCount}", ToolTip = "Execution count" }
            };

            var moreCommands = new IContextItem[]
            {
                new CommandContextItem(new CopyCommand(entry.Command))
                {
                    Title = "Copy to clipboard",
                    Icon = new IconInfo("\uE8C8")
                },
                new CommandContextItem(new DeleteHistoryCommand(entry, _historyManager))
                {
                    Title = "Delete from history",
                    Icon = new IconInfo("\uE74D"),
                    IsCritical = true
                }
            };

            return new ListItem(command)
            {
                Title = entry.Command,
                Subtitle = subtitle,
                Icon = new IconInfo("\uE756"), // Command icon
                Tags = tags,
                MoreCommands = moreCommands
            };
        }
    }

    /// <summary>
    /// Command to execute a history entry
    /// </summary>
    internal class ExecuteHistoryCommand : InvokableCommand
    {
        private readonly HistoryEntry _entry;
        private readonly HistoryManager _historyManager;

        public ExecuteHistoryCommand(HistoryEntry entry, HistoryManager historyManager)
        {
            _entry = entry;
            _historyManager = historyManager;
            Name = "Execute";
            Icon = new IconInfo("\uE768"); // Play icon
        }

        public override ICommandResult Invoke(object? sender)
        {
            try
            {
                // Add to history again (updates execution count)
                _historyManager.AddCommand(_entry.Command);
                
                // TODO: Actually execute the command if needed
                // For now, we just copy to clipboard
                System.Windows.Clipboard.SetText(_entry.Command);
                
                return CommandResult.Dismiss();
            }
            catch
            {
                return CommandResult.KeepOpen();
            }
        }
    }

    /// <summary>
    /// Command to copy text to clipboard
    /// </summary>
    internal class CopyCommand : InvokableCommand
    {
        private readonly string _text;

        public CopyCommand(string text)
        {
            _text = text;
            Name = "Copy";
            Icon = new IconInfo("\uE8C8");
        }

        public override ICommandResult Invoke(object? sender)
        {
            try
            {
                System.Windows.Clipboard.SetText(_text);
                return CommandResult.Dismiss();
            }
            catch
            {
                return CommandResult.KeepOpen();
            }
        }
    }

    /// <summary>
    /// Command to delete a history entry
    /// </summary>
    internal class DeleteHistoryCommand : InvokableCommand
    {
        private readonly HistoryEntry _entry;
        private readonly HistoryManager _historyManager;

        public DeleteHistoryCommand(HistoryEntry entry, HistoryManager historyManager)
        {
            _entry = entry;
            _historyManager = historyManager;
            Name = "Delete";
            Icon = new IconInfo("\uE74D");
        }

        public override ICommandResult Invoke(object? sender)
        {
            try
            {
                _historyManager.DeleteCommand(_entry.Id);
                return CommandResult.GoBack();
            }
            catch
            {
                return CommandResult.KeepOpen();
            }
        }
    }

    /// <summary>
    /// Error command for displaying errors
    /// </summary>
    internal class ErrorCommand : InvokableCommand
    {
        public ErrorCommand(string message)
        {
            Name = $"Error: {message}";
            Icon = new IconInfo("\uE783");
        }

        public override ICommandResult Invoke(object? sender)
        {
            return CommandResult.KeepOpen();
        }
    }
}
