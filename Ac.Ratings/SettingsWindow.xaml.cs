using System.Configuration;
using System.IO;
using System.Windows;
using Ac.Ratings.Services;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        private readonly MainWindow _mainWindow;
        public string SelectedPath { get; private set; } = string.Empty;
        private bool _isCanceling = false;
        private ConfigManager _configManager;

        public SettingsWindow(MainWindow mainWindow) {
            InitializeComponent();
            _mainWindow = mainWindow;
            _configManager = new ConfigManager();
        }

        private void ResetRatingsButton_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show(
                "Are you sure you want to reset all ratings? This action cannot be undone.",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes) {
                _mainWindow.ResetAllRatingsInDatabase();

                MessageBox.Show("All ratings have been reset successfully.", "Reset Complete", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else {
                MessageBox.Show("Reset operation canceled.", "Cancel", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void TransferRatingsButton_OnClick(object sender, RoutedEventArgs e) {
            try {
                var decoder = new RatingsDecoder();
                decoder.InitializeRatingsDataFile();
                decoder.InitializeUserRatings();
                decoder.ExportDataFile();

                MessageBox.Show("Ratings exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) {
                MessageBox.Show($"An error occurred during export: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            _isCanceling = true; // Indicate Cancel button was clicked
            Close(); // Trigger Window_Closing logic
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            if (Directory.Exists(RootFolderPath.Text)) {
                var rootPath = RootFolderPath.Text;
                var carsPath = Path.Combine(rootPath, "content", "cars");

                if (!Directory.Exists(carsPath)) {
                    MessageBox.Show(
                        "The selected Assetto Corsa root folder does not meet the required folder structure.\n" +
                        "Ensure that the root folder contains a 'content' subfolder with a 'cars' directory inside it.\n",
                        "Invalid Folder Structure", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedPath = carsPath;
                DialogResult = true;
            }
            else {
                MessageBox.Show("The provided path does not exist. Please enter a valid path.", "Invalid path", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ConfirmExit() {
            var result = MessageBox.Show(
                "Exiting this window without selecting a valid root folder will close the application. Are you sure?",
                "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            string existingPath = _configManager.AcRootFolder;
            if (!string.IsNullOrWhiteSpace(existingPath) && Directory.Exists(existingPath)) {
                return;
            }

            if (_isCanceling) {
                if (!ConfirmExit()) {
                    e.Cancel = true;
                    _isCanceling = false;
                    return;
                }

                _isCanceling = false;
            }
            else if (!DialogResult.HasValue) {
                if (!ConfirmExit()) {
                    e.Cancel = true;
                    return;
                }
            }

            if (!DialogResult.HasValue) {
                Environment.Exit(0);
            }
        }

        private void RestoreBackupButton_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                Title = "Select Backup File",
                Filter = "JSON Files (*.json)|*.json",
                InitialDirectory = _configManager.BackupFolder
            };

            if (openFileDialog.ShowDialog() == true) {
                var backupFilePath = openFileDialog.FileName;
                _mainWindow.RestoreCarDbFromBackup(backupFilePath);
            }
        }
    }
}
