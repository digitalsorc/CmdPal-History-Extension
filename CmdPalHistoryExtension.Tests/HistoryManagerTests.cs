using System;
using System.IO;
using Xunit;
using CmdPalHistoryExtension.Services;
using CmdPalHistoryExtension.Models;

namespace CmdPalHistoryExtension.Tests
{
    public class HistoryManagerTests : IDisposable
    {
        private const int DisposalDelayMs = 100;
        private const int MaxRetries = 3;
        private const int InitialRetryDelayMs = 50;
        
        private readonly string _testDbPath;
        private readonly HistoryManager _historyManager;

        public HistoryManagerTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_history_{Guid.NewGuid()}.db");
            _historyManager = new HistoryManager(_testDbPath, maxEntries: 100, deduplicate: true);
        }

        [Fact]
        public void AddCommand_ShouldAddCommandToHistory()
        {
            // Arrange
            var command = "test command";

            // Act
            _historyManager.AddCommand(command);

            // Assert
            var history = _historyManager.GetAllHistory();
            Assert.Single(history);
            Assert.Equal(command, history[0].Command);
        }

        [Fact]
        public void AddCommand_WithDuplication_ShouldUpdateExistingEntry()
        {
            // Arrange
            var command = "duplicate command";

            // Act
            _historyManager.AddCommand(command);
            _historyManager.AddCommand(command);

            // Assert
            var history = _historyManager.GetAllHistory();
            Assert.Single(history);
            Assert.Equal(2, history[0].ExecutionCount);
        }

        [Fact]
        public void GetAllHistory_ShouldReturnInReverseChronologicalOrder()
        {
            // Arrange
            _historyManager.AddCommand("first");
            System.Threading.Thread.Sleep(10); // Ensure different timestamps
            _historyManager.AddCommand("second");
            System.Threading.Thread.Sleep(10);
            _historyManager.AddCommand("third");

            // Act
            var history = _historyManager.GetAllHistory();

            // Assert
            Assert.Equal(3, history.Count);
            Assert.Equal("third", history[0].Command);
            Assert.Equal("second", history[1].Command);
            Assert.Equal("first", history[2].Command);
        }

        [Fact]
        public void SearchHistory_ShouldFindMatchingCommands()
        {
            // Arrange
            _historyManager.AddCommand("git status");
            _historyManager.AddCommand("git commit");
            _historyManager.AddCommand("npm install");

            // Act
            var results = _historyManager.SearchHistory("git");

            // Assert
            Assert.Equal(2, results.Count);
            Assert.All(results, r => Assert.Contains("git", r.Command));
        }

        [Fact]
        public void SearchHistory_CaseInsensitive_ShouldFindMatches()
        {
            // Arrange
            _historyManager.AddCommand("Git Status");

            // Act
            var results = _historyManager.SearchHistory("git", caseSensitive: false);

            // Assert
            Assert.Single(results);
            Assert.Equal("Git Status", results[0].Command);
        }

        [Fact]
        public void GetHistoryCount_ShouldReturnCorrectCount()
        {
            // Arrange
            _historyManager.AddCommand("command1");
            _historyManager.AddCommand("command2");
            _historyManager.AddCommand("command3");

            // Act
            var count = _historyManager.GetHistoryCount();

            // Assert
            Assert.Equal(3, count);
        }

        [Fact]
        public void ClearHistory_ShouldRemoveAllEntries()
        {
            // Arrange
            _historyManager.AddCommand("command1");
            _historyManager.AddCommand("command2");

            // Act
            _historyManager.ClearHistory();

            // Assert
            var count = _historyManager.GetHistoryCount();
            Assert.Equal(0, count);
        }

        [Fact]
        public void AddCommand_WithMaxEntries_ShouldRemoveOldestEntries()
        {
            // Arrange
            using var limitedManager = new HistoryManager(_testDbPath + "_limited", maxEntries: 3);

            // Act
            limitedManager.AddCommand("cmd1");
            limitedManager.AddCommand("cmd2");
            limitedManager.AddCommand("cmd3");
            limitedManager.AddCommand("cmd4");

            // Assert
            var history = limitedManager.GetAllHistory();
            Assert.Equal(3, history.Count);
            Assert.DoesNotContain(history, h => h.Command == "cmd1");
        }

        [Fact]
        public void AddCommand_WithEmptyString_ShouldNotAddToHistory()
        {
            // Act
            _historyManager.AddCommand("");
            _historyManager.AddCommand("   ");

            // Assert
            var count = _historyManager.GetHistoryCount();
            Assert.Equal(0, count);
        }

        public void Dispose()
        {
            // Step 1: Dispose the manager to release all database connections
            _historyManager?.Dispose();
            
            // Step 2: Small delay to ensure OS releases file locks
            System.Threading.Thread.Sleep(DisposalDelayMs);
            
            // Step 3: Delete the temporary database files with retry logic
            TryDeleteFile(_testDbPath);
            TryDeleteFile(_testDbPath + "_limited");
        }

        private static void TryDeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            int delay = InitialRetryDelayMs;
            
            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    File.Delete(filePath);
                    return;
                }
                catch (IOException) when (i < MaxRetries - 1)
                {
                    System.Threading.Thread.Sleep(delay);
                    delay *= 2; // Exponential backoff
                }
                catch (UnauthorizedAccessException) when (i < MaxRetries - 1)
                {
                    System.Threading.Thread.Sleep(delay);
                    delay *= 2; // Exponential backoff
                }
                catch
                {
                    // Ignore cleanup failures to prevent test failures
                    return;
                }
            }
        }
    }
}
