using System;
using System.Linq;
using CmdPalHistoryExtension.Models;
using CmdPalHistoryExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHistoryExtension.CmdPal
{
    /// <summary>
    /// Main command provider for CmdPal History Extension
    /// </summary>
    public class HistoryCommandProvider : CommandProvider
    {
        private readonly ConfigManager _configManager;
        private readonly HistoryManager _historyManager;
        private PluginConfig _config;
        private HistoryListPage? _historyPage;

        public HistoryCommandProvider()
        {
            DisplayName = "Command History";
            Icon = new IconInfo("\uE81C"); // History icon
            Frozen = false; // We need to stay running to update history
            
            _configManager = new ConfigManager();
            _config = _configManager.LoadConfig();
            
            var dbPath = _config.Storage.DatabasePath;
            _historyManager = new HistoryManager(
                dbPath,
                _config.Storage.MaxHistoryEntries,
                _config.Behavior.DeduplicateEntries);
        }

        public override void Initialize()
        {
            base.Initialize();
            // Perform any initialization needed
        }

        public override ICommandItem[] TopLevelCommands()
        {
            // Create the main history browsing page
            _historyPage = new HistoryListPage(_historyManager, _config);
            
            return new ICommandItem[]
            {
                new CommandItem(_historyPage)
                {
                    Title = "Browse Command History",
                    Subtitle = "View and search your command history",
                    Icon = new IconInfo("\uE81C")
                }
            };
        }

        public override ICommand? GetCommand(string id)
        {
            if (id == "history.browse")
            {
                return _historyPage ?? new HistoryListPage(_historyManager, _config);
            }
            return base.GetCommand(id);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _historyManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
