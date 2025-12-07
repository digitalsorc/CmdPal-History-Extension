using System;
using System.Collections.Generic;

namespace Microsoft.CommandPalette.Extensions
{
    /// <summary>
    /// Base interface for all commands in CmdPal
    /// </summary>
    public interface ICommand
    {
        string Name { get; }
        string? Id { get; }
        IIconInfo? Icon { get; }
    }

    /// <summary>
    /// Represents icon information
    /// </summary>
    public interface IIconInfo
    {
        IconData Light { get; }
        IconData Dark { get; }
    }

    /// <summary>
    /// Icon data structure
    /// </summary>
    public struct IconData
    {
        public string Icon { get; set; }
        
        public IconData(string iconString)
        {
            Icon = iconString;
        }
    }

    /// <summary>
    /// Result of command invocation
    /// </summary>
    public enum CommandResultKind
    {
        Dismiss,
        GoHome,
        GoBack,
        Hide,
        KeepOpen,
        GoToPage,
        ShowToast,
        Confirm
    }

    /// <summary>
    /// Command result
    /// </summary>
    public interface ICommandResult
    {
        CommandResultKind Kind { get; }
    }

    /// <summary>
    /// A command that can be invoked
    /// </summary>
    public interface IInvokableCommand : ICommand
    {
        ICommandResult Invoke(object? sender);
    }

    /// <summary>
    /// Represents a page in CmdPal
    /// </summary>
    public interface IPage : ICommand
    {
        string? Title { get; }
        bool IsLoading { get; }
    }

    /// <summary>
    /// Tag for list items
    /// </summary>
    public interface ITag
    {
        IIconInfo? Icon { get; }
        string Text { get; }
        string? ToolTip { get; }
    }

    /// <summary>
    /// Base for context menu items
    /// </summary>
    public interface IContextItem
    {
    }

    /// <summary>
    /// Command in context menu
    /// </summary>
    public interface ICommandContextItem : IContextItem
    {
        ICommand Command { get; }
        IIconInfo? Icon { get; }
        string? Title { get; }
        string? Subtitle { get; }
        bool IsCritical { get; }
    }

    /// <summary>
    /// Base interface for command items
    /// </summary>
    public interface ICommandItem
    {
        ICommand Command { get; }
        IContextItem[]? MoreCommands { get; }
        IIconInfo? Icon { get; }
        string? Title { get; }
        string? Subtitle { get; }
    }

    /// <summary>
    /// List item in a list page
    /// </summary>
    public interface IListItem : ICommandItem
    {
        ITag[]? Tags { get; }
        string? Section { get; }
    }

    /// <summary>
    /// A page that displays a list of items
    /// </summary>
    public interface IListPage : IPage
    {
        string? SearchText { get; }
        string? PlaceholderText { get; }
        IListItem[] GetItems();
    }

    /// <summary>
    /// Command provider - main extension interface
    /// </summary>
    public interface ICommandProvider : IDisposable
    {
        string? Id { get; }
        string DisplayName { get; }
        IIconInfo? Icon { get; }
        bool Frozen { get; }

        ICommandItem[] TopLevelCommands();
        ICommand? GetCommand(string id);
        void Initialize();
    }
}
