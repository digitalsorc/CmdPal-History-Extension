using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CmdPalHistoryExtension.Models;
using CmdPalHistoryExtension.Services;

namespace CmdPalHistoryExtension.UI
{
    /// <summary>
    /// Interaction logic for HistoryWindow.xaml
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private readonly HistoryManager _historyManager;
        private List<HistoryEntry> _allHistory;
        private List<HistoryEntry> _filteredHistory;
        public string? SelectedCommand { get; private set; }

        public HistoryWindow(HistoryManager historyManager)
        {
            InitializeComponent();
            _historyManager = historyManager;
            _allHistory = new List<HistoryEntry>();
            _filteredHistory = new List<HistoryEntry>();
            LoadHistory();
            SearchTextBox.Focus();
        }

        /// <summary>
        /// Loads history from the manager
        /// </summary>
        private void LoadHistory()
        {
            try
            {
                _allHistory = _historyManager.GetAllHistory();
                _filteredHistory = new List<HistoryEntry>(_allHistory);
                UpdateHistoryList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading history: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates the history list display
        /// </summary>
        private void UpdateHistoryList()
        {
            HistoryListBox.ItemsSource = _filteredHistory;
            StatusTextBlock.Text = $"{_filteredHistory.Count} command{(_filteredHistory.Count != 1 ? "s" : "")}";

            if (_filteredHistory.Count > 0 && HistoryListBox.SelectedIndex < 0)
            {
                HistoryListBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Handles search text changes
        /// </summary>
        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredHistory = new List<HistoryEntry>(_allHistory);
            }
            else
            {
                _filteredHistory = _allHistory
                    .Where(entry => entry.Command.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            UpdateHistoryList();
        }

        /// <summary>
        /// Clears the search box
        /// </summary>
        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            SearchTextBox.Focus();
        }

        /// <summary>
        /// Handles window key down events
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    DialogResult = false;
                    Close();
                    break;

                case Key.Enter:
                    SelectCurrentCommand();
                    break;

                case Key.Up:
                    if (HistoryListBox.SelectedIndex > 0)
                    {
                        HistoryListBox.SelectedIndex--;
                        HistoryListBox.ScrollIntoView(HistoryListBox.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.Down:
                    if (HistoryListBox.SelectedIndex < HistoryListBox.Items.Count - 1)
                    {
                        HistoryListBox.SelectedIndex++;
                        HistoryListBox.ScrollIntoView(HistoryListBox.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.PageUp:
                    if (HistoryListBox.SelectedIndex > 0)
                    {
                        HistoryListBox.SelectedIndex = Math.Max(0, HistoryListBox.SelectedIndex - 10);
                        HistoryListBox.ScrollIntoView(HistoryListBox.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.PageDown:
                    if (HistoryListBox.SelectedIndex < HistoryListBox.Items.Count - 1)
                    {
                        HistoryListBox.SelectedIndex = Math.Min(
                            HistoryListBox.Items.Count - 1, 
                            HistoryListBox.SelectedIndex + 10);
                        HistoryListBox.ScrollIntoView(HistoryListBox.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.Home:
                    if (HistoryListBox.Items.Count > 0)
                    {
                        HistoryListBox.SelectedIndex = 0;
                        HistoryListBox.ScrollIntoView(HistoryListBox.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.End:
                    if (HistoryListBox.Items.Count > 0)
                    {
                        HistoryListBox.SelectedIndex = HistoryListBox.Items.Count - 1;
                        HistoryListBox.ScrollIntoView(HistoryListBox.SelectedItem);
                    }
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Handles double-click on history item
        /// </summary>
        private void HistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectCurrentCommand();
        }

        /// <summary>
        /// Handles selection change in the list
        /// </summary>
        private void HistoryListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Update UI based on selection if needed
        }

        /// <summary>
        /// Selects the current command and closes the window
        /// </summary>
        private void SelectCurrentCommand()
        {
            if (HistoryListBox.SelectedItem is HistoryEntry entry)
            {
                SelectedCommand = entry.Command;
                DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// Handles select button click
        /// </summary>
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentCommand();
        }

        /// <summary>
        /// Handles close button click
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
