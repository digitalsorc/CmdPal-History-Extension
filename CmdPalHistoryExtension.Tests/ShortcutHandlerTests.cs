using System.Windows.Input;
using Xunit;
using CmdPalHistoryExtension.Services;

namespace CmdPalHistoryExtension.Tests
{
    public class ShortcutHandlerTests
    {
        private readonly ShortcutHandler _shortcutHandler;

        public ShortcutHandlerTests()
        {
            _shortcutHandler = new ShortcutHandler();
        }

        [Theory]
        [InlineData("Up", Key.Up, ModifierKeys.None)]
        [InlineData("Down", Key.Down, ModifierKeys.None)]
        [InlineData("Ctrl+H", Key.H, ModifierKeys.Control)]
        [InlineData("Alt+F4", Key.F4, ModifierKeys.Alt)]
        [InlineData("Shift+Tab", Key.Tab, ModifierKeys.Shift)]
        public void ParseShortcut_WithValidShortcut_ShouldParseCorrectly(
            string shortcutString, Key expectedKey, ModifierKeys expectedModifiers)
        {
            // Act
            var success = _shortcutHandler.ParseShortcut(shortcutString, out var key, out var modifiers);

            // Assert
            Assert.True(success);
            Assert.Equal(expectedKey, key);
            Assert.Equal(expectedModifiers, modifiers);
        }

        [Theory]
        [InlineData("Ctrl+Shift+A", Key.A, ModifierKeys.Control | ModifierKeys.Shift)]
        [InlineData("Ctrl+Alt+Delete", Key.Delete, ModifierKeys.Control | ModifierKeys.Alt)]
        public void ParseShortcut_WithMultipleModifiers_ShouldParseCorrectly(
            string shortcutString, Key expectedKey, ModifierKeys expectedModifiers)
        {
            // Act
            var success = _shortcutHandler.ParseShortcut(shortcutString, out var key, out var modifiers);

            // Assert
            Assert.True(success);
            Assert.Equal(expectedKey, key);
            Assert.Equal(expectedModifiers, modifiers);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("InvalidKey")]
        [InlineData("Ctrl+")]
        [InlineData("+H")]
        public void ParseShortcut_WithInvalidShortcut_ShouldReturnFalse(string shortcutString)
        {
            // Act
            var success = _shortcutHandler.ParseShortcut(shortcutString, out var key, out var modifiers);

            // Assert
            Assert.False(success);
        }

        [Theory]
        [InlineData(Key.Tab, ModifierKeys.Alt, true)]
        [InlineData(Key.F4, ModifierKeys.Alt, true)]
        [InlineData(Key.L, ModifierKeys.Windows, true)]
        [InlineData(Key.H, ModifierKeys.Control, false)]
        [InlineData(Key.Up, ModifierKeys.None, false)]
        public void IsSystemShortcut_ShouldDetectSystemConflicts(
            Key key, ModifierKeys modifiers, bool expectedIsSystemShortcut)
        {
            // Act
            var isSystemShortcut = _shortcutHandler.IsSystemShortcut(key, modifiers);

            // Assert
            Assert.Equal(expectedIsSystemShortcut, isSystemShortcut);
        }

        [Fact]
        public void MatchesShortcut_WithMatchingKeys_ShouldReturnTrue()
        {
            // Arrange
            var shortcutString = "Ctrl+H";
            var pressedKey = Key.H;
            var pressedModifiers = ModifierKeys.Control;

            // Act
            var matches = _shortcutHandler.MatchesShortcut(pressedKey, pressedModifiers, shortcutString);

            // Assert
            Assert.True(matches);
        }

        [Fact]
        public void MatchesShortcut_WithDifferentKeys_ShouldReturnFalse()
        {
            // Arrange
            var shortcutString = "Ctrl+H";
            var pressedKey = Key.G;
            var pressedModifiers = ModifierKeys.Control;

            // Act
            var matches = _shortcutHandler.MatchesShortcut(pressedKey, pressedModifiers, shortcutString);

            // Assert
            Assert.False(matches);
        }

        [Fact]
        public void FormatShortcut_ShouldFormatCorrectly()
        {
            // Arrange
            var key = Key.H;
            var modifiers = ModifierKeys.Control | ModifierKeys.Alt;

            // Act
            var formatted = _shortcutHandler.FormatShortcut(key, modifiers);

            // Assert
            Assert.Contains("Ctrl", formatted);
            Assert.Contains("Alt", formatted);
            Assert.Contains("H", formatted);
        }
    }
}
