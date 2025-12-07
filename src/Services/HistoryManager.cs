using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using CmdPalHistoryExtension.Models;
using System.IO;

namespace CmdPalHistoryExtension.Services
{
    /// <summary>
    /// Manages command history storage and retrieval using SQLite
    /// </summary>
    public class HistoryManager : IDisposable
    {
        private readonly string _dbPath;
        private readonly int _maxEntries;
        private readonly bool _deduplicate;
        private SqliteConnection? _connection;
        private readonly object _lock = new();

        public HistoryManager(string dbPath, int maxEntries = 10000, bool deduplicate = true)
        {
            _dbPath = ConfigManager.ExpandPath(dbPath);
            _maxEntries = maxEntries;
            _deduplicate = deduplicate;
            EnsureDatabaseExists();
        }

        /// <summary>
        /// Ensures the database and schema exist
        /// </summary>
        private void EnsureDatabaseExists()
        {
            var directory = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connection = new SqliteConnection($"Data Source={_dbPath}");
            _connection.Open();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS history (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    command TEXT NOT NULL,
                    timestamp TEXT NOT NULL,
                    context TEXT,
                    execution_count INTEGER DEFAULT 1
                );
                CREATE INDEX IF NOT EXISTS idx_timestamp ON history(timestamp DESC);
                CREATE INDEX IF NOT EXISTS idx_command ON history(command);
            ";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Adds a command to history
        /// </summary>
        public void AddCommand(string command, string context = "")
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            lock (_lock)
            {
                try
                {
                    if (_deduplicate)
                    {
                        // Check if command already exists
                        using var checkCmd = _connection!.CreateCommand();
                        checkCmd.CommandText = "SELECT id, execution_count FROM history WHERE command = @command ORDER BY timestamp DESC LIMIT 1";
                        checkCmd.Parameters.AddWithValue("@command", command);

                        using var reader = checkCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            var existingId = reader.GetInt32(0);
                            var execCount = reader.GetInt32(1);
                            reader.Close();

                            // Update timestamp and execution count
                            using var updateCmd = _connection.CreateCommand();
                            updateCmd.CommandText = @"
                                UPDATE history 
                                SET timestamp = @timestamp, execution_count = @execCount, context = @context
                                WHERE id = @id
                            ";
                            updateCmd.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToString("O"));
                            updateCmd.Parameters.AddWithValue("@execCount", execCount + 1);
                            updateCmd.Parameters.AddWithValue("@context", context);
                            updateCmd.Parameters.AddWithValue("@id", existingId);
                            updateCmd.ExecuteNonQuery();
                            return;
                        }
                    }

                    // Insert new entry
                    using var insertCmd = _connection!.CreateCommand();
                    insertCmd.CommandText = @"
                        INSERT INTO history (command, timestamp, context, execution_count)
                        VALUES (@command, @timestamp, @context, 1)
                    ";
                    insertCmd.Parameters.AddWithValue("@command", command);
                    insertCmd.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToString("O"));
                    insertCmd.Parameters.AddWithValue("@context", context);
                    insertCmd.ExecuteNonQuery();

                    // Cleanup old entries if exceeding max
                    CleanupOldEntries();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding command to history: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gets all history entries in reverse chronological order
        /// </summary>
        public List<HistoryEntry> GetAllHistory(int limit = -1)
        {
            lock (_lock)
            {
                var entries = new List<HistoryEntry>();
                try
                {
                    using var cmd = _connection!.CreateCommand();
                    cmd.CommandText = limit > 0
                        ? "SELECT id, command, timestamp, context, execution_count FROM history ORDER BY timestamp DESC LIMIT @limit"
                        : "SELECT id, command, timestamp, context, execution_count FROM history ORDER BY timestamp DESC";
                    
                    if (limit > 0)
                    {
                        cmd.Parameters.AddWithValue("@limit", limit);
                    }

                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        entries.Add(new HistoryEntry
                        {
                            Id = reader.GetInt32(0),
                            Command = reader.GetString(1),
                            Timestamp = DateTime.Parse(reader.GetString(2)),
                            Context = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            ExecutionCount = reader.GetInt32(4)
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving history: {ex.Message}");
                }
                return entries;
            }
        }

        /// <summary>
        /// Searches history by command text
        /// </summary>
        public List<HistoryEntry> SearchHistory(string searchTerm, bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return GetAllHistory();
            }

            lock (_lock)
            {
                var entries = new List<HistoryEntry>();
                try
                {
                    using var cmd = _connection!.CreateCommand();
                    if (caseSensitive)
                    {
                        cmd.CommandText = @"
                            SELECT id, command, timestamp, context, execution_count 
                            FROM history 
                            WHERE command LIKE @search
                            ORDER BY timestamp DESC
                        ";
                        cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                    }
                    else
                    {
                        cmd.CommandText = @"
                            SELECT id, command, timestamp, context, execution_count 
                            FROM history 
                            WHERE LOWER(command) LIKE LOWER(@search)
                            ORDER BY timestamp DESC
                        ";
                        cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                    }

                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        entries.Add(new HistoryEntry
                        {
                            Id = reader.GetInt32(0),
                            Command = reader.GetString(1),
                            Timestamp = DateTime.Parse(reader.GetString(2)),
                            Context = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            ExecutionCount = reader.GetInt32(4)
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error searching history: {ex.Message}");
                }
                return entries;
            }
        }

        /// <summary>
        /// Gets history count
        /// </summary>
        public int GetHistoryCount()
        {
            lock (_lock)
            {
                try
                {
                    using var cmd = _connection!.CreateCommand();
                    cmd.CommandText = "SELECT COUNT(*) FROM history";
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting history count: {ex.Message}");
                    return 0;
                }
            }
        }

        /// <summary>
        /// Deletes a specific command by ID
        /// </summary>
        public void DeleteCommand(int id)
        {
            lock (_lock)
            {
                try
                {
                    using var cmd = _connection!.CreateCommand();
                    cmd.CommandText = "DELETE FROM history WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting command: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Clears all history
        /// </summary>
        public void ClearHistory()
        {
            lock (_lock)
            {
                try
                {
                    using var cmd = _connection!.CreateCommand();
                    cmd.CommandText = "DELETE FROM history";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing history: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Removes entries exceeding the maximum limit
        /// </summary>
        private void CleanupOldEntries()
        {
            try
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = @"
                    DELETE FROM history 
                    WHERE id NOT IN (
                        SELECT id FROM history 
                        ORDER BY timestamp DESC 
                        LIMIT @maxEntries
                    )
                ";
                cmd.Parameters.AddWithValue("@maxEntries", _maxEntries);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up old entries: {ex.Message}");
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _connection?.Close();
                _connection?.Dispose();
            }
        }
    }
}
