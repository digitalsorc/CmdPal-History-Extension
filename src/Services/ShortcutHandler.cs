using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace CmdPalHistoryExtension.Services
{
    /// <summary>
    /// Handles custom keyboard shortcut parsing and validation
    /// </summary>
    public class ShortcutHandler
    {
        private readonly Dictionary<string, Key> _keyMap = new();
        private readonly Dictionary<string, ModifierKeys> _modifierMap = new();

        public ShortcutHandler()
        {
            InitializeKeyMaps();
        }

        /// <summary>
        /// Initializes the key and modifier mappings
        /// </summary>
        private void InitializeKeyMaps()
        {
            // Basic keys
            _keyMap["Up"] = Key.Up;
            _keyMap["Down"] = Key.Down;
            _keyMap["Left"] = Key.Left;
            _keyMap["Right"] = Key.Right;
            _keyMap["Enter"] = Key.Enter;
            _keyMap["Escape"] = Key.Escape;
            _keyMap["Tab"] = Key.Tab;
            _keyMap["Space"] = Key.Space;
            _keyMap["Backspace"] = Key.Back;
            _keyMap["Delete"] = Key.Delete;
            _keyMap["Home"] = Key.Home;
            _keyMap["End"] = Key.End;
            _keyMap["PageUp"] = Key.PageUp;
            _keyMap["PageDown"] = Key.PageDown;

            // Letter keys
            for (char c = 'A'; c <= 'Z'; c++)
            {
                _keyMap[c.ToString()] = (Key)Enum.Parse(typeof(Key), c.ToString());
            }

            // Number keys
            for (int i = 0; i <= 9; i++)
            {
                _keyMap[$"D{i}"] = (Key)Enum.Parse(typeof(Key), $"D{i}");
                _keyMap[i.ToString()] = (Key)Enum.Parse(typeof(Key), $"D{i}");
            }

            // Function keys
            for (int i = 1; i <= 24; i++)
            {
                _keyMap[$"F{i}"] = (Key)Enum.Parse(typeof(Key), $"F{i}");
            }

            // Modifiers
            _modifierMap["Ctrl"] = ModifierKeys.Control;
            _modifierMap["Control"] = ModifierKeys.Control;
            _modifierMap["Alt"] = ModifierKeys.Alt;
            _modifierMap["Shift"] = ModifierKeys.Shift;
            _modifierMap["Win"] = ModifierKeys.Windows;
            _modifierMap["Windows"] = ModifierKeys.Windows;
        }

        /// <summary>
        /// Parses a shortcut string like "Ctrl+H" into Key and ModifierKeys
        /// </summary>
        public bool ParseShortcut(string shortcutString, out Key key, out ModifierKeys modifiers)
        {
            key = Key.None;
            modifiers = ModifierKeys.None;

            if (string.IsNullOrWhiteSpace(shortcutString))
            {
                return false;
            }

            var parts = shortcutString.Split('+');
            if (parts.Length == 0)
            {
                return false;
            }

            // Parse modifiers (all parts except the last one)
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var modifierStr = parts[i].Trim();
                if (_modifierMap.TryGetValue(modifierStr, out var modifier))
                {
                    modifiers |= modifier;
                }
                else
                {
                    return false; // Invalid modifier
                }
            }

            // Parse the main key (last part)
            var keyStr = parts[^1].Trim();
            if (_keyMap.TryGetValue(keyStr, out var parsedKey))
            {
                key = parsedKey;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validates if a shortcut conflicts with system shortcuts
        /// </summary>
        public bool IsSystemShortcut(Key key, ModifierKeys modifiers)
        {
            // Check for common system shortcuts that should be avoided
            var conflictingShortcuts = new HashSet<(Key, ModifierKeys)>
            {
                (Key.Tab, ModifierKeys.Alt), // Alt+Tab (window switching)
                (Key.F4, ModifierKeys.Alt), // Alt+F4 (close window)
                (Key.Escape, ModifierKeys.Control | ModifierKeys.Shift), // Ctrl+Shift+Esc (Task Manager)
                (Key.L, ModifierKeys.Windows), // Win+L (lock screen)
                (Key.D, ModifierKeys.Windows), // Win+D (show desktop)
                (Key.E, ModifierKeys.Windows), // Win+E (explorer)
            };

            return conflictingShortcuts.Contains((key, modifiers));
        }

        /// <summary>
        /// Checks if the given key press matches a shortcut
        /// </summary>
        public bool MatchesShortcut(Key pressedKey, ModifierKeys pressedModifiers, string shortcutString)
        {
            if (ParseShortcut(shortcutString, out var key, out var modifiers))
            {
                return pressedKey == key && pressedModifiers == modifiers;
            }
            return false;
        }

        /// <summary>
        /// Formats a shortcut for display
        /// </summary>
        public string FormatShortcut(Key key, ModifierKeys modifiers)
        {
            var parts = new List<string>();

            if (modifiers.HasFlag(ModifierKeys.Control))
                parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt))
                parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Shift))
                parts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Windows))
                parts.Add("Win");

            parts.Add(key.ToString());

            return string.Join("+", parts);
        }
    }
}
