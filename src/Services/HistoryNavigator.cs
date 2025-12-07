using System.Collections.Generic;
using CmdPalHistoryExtension.Models;

namespace CmdPalHistoryExtension.Services
{
    /// <summary>
    /// Manages navigation through command history (up/down arrow functionality)
    /// </summary>
    public class HistoryNavigator
    {
        private readonly HistoryManager _historyManager;
        private List<HistoryEntry> _currentHistory;
        private int _currentIndex;
        private string _originalInput;

        public HistoryNavigator(HistoryManager historyManager)
        {
            _historyManager = historyManager;
            _currentHistory = new List<HistoryEntry>();
            _currentIndex = -1;
            _originalInput = string.Empty;
        }

        /// <summary>
        /// Initializes navigation with current input
        /// </summary>
        public void Initialize(string currentInput)
        {
            _originalInput = currentInput;
            _currentHistory = _historyManager.GetAllHistory();
            _currentIndex = -1;
        }

        /// <summary>
        /// Gets the previous command in history
        /// </summary>
        public string? GetPrevious()
        {
            if (_currentHistory.Count == 0)
            {
                return null;
            }

            _currentIndex++;
            if (_currentIndex >= _currentHistory.Count)
            {
                _currentIndex = _currentHistory.Count - 1;
            }

            return _currentHistory[_currentIndex].Command;
        }

        /// <summary>
        /// Gets the next command in history (moving forward)
        /// </summary>
        public string? GetNext()
        {
            if (_currentHistory.Count == 0)
            {
                return null;
            }

            _currentIndex--;
            if (_currentIndex < 0)
            {
                _currentIndex = -1;
                return _originalInput;
            }

            return _currentHistory[_currentIndex].Command;
        }

        /// <summary>
        /// Resets navigation state
        /// </summary>
        public void Reset()
        {
            _currentIndex = -1;
            _originalInput = string.Empty;
        }

        /// <summary>
        /// Gets the current position in history
        /// </summary>
        public int CurrentPosition => _currentIndex;

        /// <summary>
        /// Gets the total history count
        /// </summary>
        public int TotalCount => _currentHistory.Count;
    }
}
