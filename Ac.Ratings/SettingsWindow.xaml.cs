using System.IO;
using System.Windows;
using Ac.Ratings.Services;
using Ac.Ratings.ViewModel;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        private readonly MainWindow _mainWindow;
        private readonly string _configPath = ConfigManager.ConfigFilePath;
        public SettingsViewModel ViewModel { get; } = new();

        public SettingsWindow(MainWindow mainWindow) {
            InitializeComponent();
            _mainWindow = mainWindow;
            ViewModel.LoadSettings(_configPath);
            DataContext = ViewModel;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e) {
            try {
                ViewModel.SaveSettings(_configPath);
                MessageBox.Show($"Primary Unit: {ViewModel.SelectedPrimaryUnit}\nSecondary Unit: {ViewModel.SelectedSecondaryUnit}",
                    "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (FileNotFoundException ex) {
                MessageBox.Show(ex.Message, "Config File Missing", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException ex) {
                MessageBox.Show(ex.Message, "Invalid Config Format", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex) {
                MessageBox.Show($"An unexpected error occurred while saving power formats: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void RestoreBackupButton_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                Title = "Select Backup File",
                Filter = "JSON Files (*.json)|*.json",
                InitialDirectory = ConfigManager.BackupFolder
            };

            if (openFileDialog.ShowDialog() == true) {
                var backupFilePath = openFileDialog.FileName;
                _mainWindow.RestoreCarDbFromBackup(backupFilePath);
            }
        }

        private void ResetExtraFeatures_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show(
                "Are you sure you want to reset all extra features? This action cannot be undone.",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes) {
                _mainWindow.ResetAllExtraFeaturesInDatabase();

                MessageBox.Show("All extra features have been reset successfully.", "Reset Complete", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else {
                MessageBox.Show("Reset operation canceled.", "Cancel", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ResetRootFolder_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show(
                "This will reset the root folder and close the application. You then need to reopen the application to change the root folder. Are you sure?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes) {
                try {
                    if (File.Exists(ConfigManager.ConfigFilePath)) {
                        File.Delete(ConfigManager.ConfigFilePath);

                        Environment.Exit(0);
                    }
                    else {
                        MessageBox.Show(
                            "No configuration file was found to reset.",
                            "Unexpected error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show($"An error occurred while resetting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
