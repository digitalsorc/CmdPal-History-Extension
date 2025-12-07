using System;

namespace CmdPalHistoryExtension.Models
{
    /// <summary>
    /// Represents a single command history entry
    /// </summary>
    public class HistoryEntry
    {
        public int Id { get; set; }
        public string Command { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Context { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }

        public HistoryEntry()
        {
            Timestamp = DateTime.UtcNow;
            ExecutionCount = 1;
        }

        public HistoryEntry(string command, string context = "") : this()
        {
            Command = command;
            Context = context;
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Command}";
        }
    }
}
