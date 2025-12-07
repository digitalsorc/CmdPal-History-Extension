using System;
using System.IO;
using System.Text.Json;
using CmdPalHistoryExtension.Models;

namespace CmdPalHistoryExtension.Services
{
    /// <summary>
    /// Manages plugin configuration loading and saving
    /// </summary>
    public class ConfigManager
    {
        private readonly string _configPath;
        private PluginConfig? _config;
        private static readonly object _lock = new();

        public ConfigManager(string? configPath = null)
        {
            _configPath = configPath ?? GetDefaultConfigPath();
            EnsureConfigDirectory();
        }

        /// <summary>
        /// Gets the default configuration path
        /// </summary>
        private static string GetDefaultConfigPath()
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userProfile, ".cmdpal", "config.json");
        }

        /// <summary>
        /// Ensures the configuration directory exists
        /// </summary>
        private void EnsureConfigDirectory()
        {
            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Loads configuration from file or creates default
        /// </summary>
        public PluginConfig LoadConfig()
        {
            lock (_lock)
            {
                if (_config != null)
                {
                    return _config;
                }

                try
                {
                    if (File.Exists(_configPath))
                    {
                        var json = File.ReadAllText(_configPath);
                        _config = JsonSerializer.Deserialize<PluginConfig>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            WriteIndented = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading config: {ex.Message}");
                }

                // Return default config if loading failed
                if (_config == null)
                {
                    _config = new PluginConfig();
                    SaveConfig(_config);
                }

                return _config;
            }
        }

        /// <summary>
        /// Saves configuration to file
        /// </summary>
        public void SaveConfig(PluginConfig config)
        {
            lock (_lock)
            {
                try
                {
                    EnsureConfigDirectory();
                    var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    });
                    File.WriteAllText(_configPath, json);
                    _config = config;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving config: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Reloads configuration from file
        /// </summary>
        public void ReloadConfig()
        {
            lock (_lock)
            {
                _config = null;
                LoadConfig();
            }
        }

        /// <summary>
        /// Expands environment variables in path
        /// </summary>
        public static string ExpandPath(string path)
        {
            return Environment.ExpandEnvironmentVariables(path);
        }
    }
}
