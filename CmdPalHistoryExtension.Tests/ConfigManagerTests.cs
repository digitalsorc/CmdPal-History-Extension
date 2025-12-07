using System;
using System.IO;
using Xunit;
using CmdPalHistoryExtension.Services;
using CmdPalHistoryExtension.Models;

namespace CmdPalHistoryExtension.Tests
{
    public class ConfigManagerTests : IDisposable
    {
        private readonly string _testConfigPath;
        private readonly ConfigManager _configManager;

        public ConfigManagerTests()
        {
            _testConfigPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
            _configManager = new ConfigManager(_testConfigPath);
        }

        [Fact]
        public void LoadConfig_WithNoFile_ShouldCreateDefaultConfig()
        {
            // Act
            var config = _configManager.LoadConfig();

            // Assert
            Assert.NotNull(config);
            Assert.Equal("1.0.0", config.Version);
            Assert.NotNull(config.Keybindings);
            Assert.NotNull(config.Storage);
            Assert.NotNull(config.UI);
            Assert.NotNull(config.Behavior);
        }

        [Fact]
        public void SaveConfig_ShouldPersistConfiguration()
        {
            // Arrange
            var config = new PluginConfig
            {
                Version = "2.0.0",
                Keybindings = new KeyBindings
                {
                    PreviousCommand = "Ctrl+Up"
                }
            };

            // Act
            _configManager.SaveConfig(config);
            var newManager = new ConfigManager(_testConfigPath);
            var loadedConfig = newManager.LoadConfig();

            // Assert
            Assert.Equal("2.0.0", loadedConfig.Version);
            Assert.Equal("Ctrl+Up", loadedConfig.Keybindings.PreviousCommand);
        }

        [Fact]
        public void ReloadConfig_ShouldLoadLatestConfiguration()
        {
            // Arrange
            var config1 = new PluginConfig { Version = "1.0.0" };
            _configManager.SaveConfig(config1);

            // Modify file externally
            var config2 = new PluginConfig { Version = "2.0.0" };
            var newManager = new ConfigManager(_testConfigPath);
            newManager.SaveConfig(config2);

            // Act
            _configManager.ReloadConfig();
            var reloadedConfig = _configManager.LoadConfig();

            // Assert
            Assert.Equal("2.0.0", reloadedConfig.Version);
        }

        [Fact]
        public void ExpandPath_ShouldExpandEnvironmentVariables()
        {
            // Arrange
            var pathWithEnvVar = "%USERPROFILE%\\.cmdpal\\config.json";

            // Act
            var expandedPath = ConfigManager.ExpandPath(pathWithEnvVar);

            // Assert
            Assert.DoesNotContain("%", expandedPath);
            Assert.Contains(".cmdpal", expandedPath);
        }

        [Fact]
        public void DefaultConfig_ShouldHaveCorrectDefaults()
        {
            // Act
            var config = _configManager.LoadConfig();

            // Assert
            Assert.Equal("Up", config.Keybindings.PreviousCommand);
            Assert.Equal("Down", config.Keybindings.NextCommand);
            Assert.Equal("Ctrl+H", config.Keybindings.OpenHistoryList);
            Assert.Equal(10000, config.Storage.MaxHistoryEntries);
            Assert.True(config.UI.ShowTimestamps);
            Assert.True(config.Behavior.DeduplicateEntries);
        }

        public void Dispose()
        {
            if (File.Exists(_testConfigPath))
            {
                File.Delete(_testConfigPath);
            }
        }
    }
}
