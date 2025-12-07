using System;
using System.ComponentModel;

namespace Microsoft.CommandPalette.Extensions.Toolkit
{
    /// <summary>
    /// Base implementation for Icon Info
    /// </summary>
    public class IconInfo : IIconInfo
    {
        public IconData Light { get; set; }
        public IconData Dark { get; set; }

        public IconInfo(string icon)
        {
            Light = new IconData(icon);
            Dark = new IconData(icon);
        }

        public IconInfo(string lightIcon, string darkIcon)
        {
            Light = new IconData(lightIcon);
            Dark = new IconData(darkIcon);
        }
    }

    /// <summary>
    /// Simple command result implementation
    /// </summary>
    public class CommandResult : ICommandResult
    {
        public CommandResultKind Kind { get; set; }

        public CommandResult(CommandResultKind kind)
        {
            Kind = kind;
        }

        public static CommandResult Dismiss() => new(CommandResultKind.Dismiss);
        public static CommandResult GoHome() => new(CommandResultKind.GoHome);
        public static CommandResult GoBack() => new(CommandResultKind.GoBack);
        public static CommandResult Hide() => new(CommandResultKind.Hide);
        public static CommandResult KeepOpen() => new(CommandResultKind.KeepOpen);
    }

    /// <summary>
    /// Base implementation for Tag
    /// </summary>
    public class Tag : ITag
    {
        public IIconInfo? Icon { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? ToolTip { get; set; }
    }

    /// <summary>
    /// Base class for invokable commands
    /// </summary>
    public abstract class InvokableCommand : IInvokableCommand, INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private IIconInfo? _icon;

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public virtual string? Id { get; set; }

        public virtual IIconInfo? Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract ICommandResult Invoke(object? sender);

        // Convenience method without sender
        public ICommandResult Invoke() => Invoke(null);
    }

    /// <summary>
    /// Command context item implementation
    /// </summary>
    public class CommandContextItem : ICommandContextItem
    {
        public ICommand Command { get; set; }
        public IIconInfo? Icon { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public bool IsCritical { get; set; }

        public CommandContextItem(ICommand command)
        {
            Command = command;
        }
    }

    /// <summary>
    /// Command item implementation
    /// </summary>
    public class CommandItem : ICommandItem
    {
        public ICommand Command { get; set; }
        public IContextItem[]? MoreCommands { get; set; }
        public IIconInfo? Icon { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }

        public CommandItem(ICommand command)
        {
            Command = command;
        }
    }

    /// <summary>
    /// List item implementation
    /// </summary>
    public class ListItem : IListItem
    {
        public ICommand Command { get; set; }
        public IContextItem[]? MoreCommands { get; set; }
        public IIconInfo? Icon { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public ITag[]? Tags { get; set; }
        public string? Section { get; set; }

        public ListItem(ICommand command)
        {
            Command = command;
        }
    }

    /// <summary>
    /// Base class for list pages
    /// </summary>
    public abstract class ListPage : IListPage, INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string? _title;
        private bool _isLoading;

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public virtual string? Id { get; set; }
        public virtual IIconInfo? Icon { get; set; }

        public virtual string? Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public virtual bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public virtual string? SearchText { get; set; }
        public virtual string? PlaceholderText { get; set; }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract IListItem[] GetItems();
    }

    /// <summary>
    /// Base class for command providers
    /// </summary>
    public abstract class CommandProvider : ICommandProvider
    {
        private bool _disposed;

        public virtual string? Id { get; set; }
        public virtual string DisplayName { get; set; } = string.Empty;
        public virtual IIconInfo? Icon { get; set; }
        public virtual bool Frozen { get; set; } = true;

        public virtual void Initialize()
        {
            // Override in derived classes if needed
        }

        public abstract ICommandItem[] TopLevelCommands();

        public virtual ICommand? GetCommand(string id)
        {
            // Default implementation - search through top level commands
            foreach (var item in TopLevelCommands())
            {
                if (item.Command.Id == id)
                {
                    return item.Command;
                }
            }
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed state
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
