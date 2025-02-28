using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Ac.Ratings.Core;
using Ac.Ratings.Services;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged {
        private readonly MainWindow _mainWindow;
        private readonly string _configPath = ConfigManager.ConfigFilePath;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<string> PrimaryPowerUnits { get; } = new() { "kW", "hp", "ps", "cv" };
        public List<string> SecondaryPowerUnits { get; } = new() { "kW", "hp", "ps", "cv", "None" };

        private string _selectedPrimaryUnit;
        private string _selectedSecondaryUnit;

        public string SelectedPrimaryUnit {
            get => _selectedPrimaryUnit;
            set {
                _selectedPrimaryUnit = value;
                OnPropertyChanged(nameof(SelectedPrimaryUnit));
            }
        }

        public string SelectedSecondaryUnit {
            get => _selectedSecondaryUnit;
            set {
                _selectedSecondaryUnit = value;
                OnPropertyChanged(nameof(SelectedSecondaryUnit));
            }
        }

        public SettingsWindow(MainWindow mainWindow) {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadSettings();
        }

        private void LoadSettings() {
            if (File.Exists(_configPath)) {
                var json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                if (config != null) {
                    SelectedPrimaryUnit = config.GetValueOrDefault("PrimaryPowerUnit", "kW");
                    SelectedSecondaryUnit = config.GetValueOrDefault("SecondaryPowerUnit", "hp");
                }
            }
            else {
                SelectedPrimaryUnit = "kW";
                SelectedSecondaryUnit = "hp";
            }
        }

        private void SaveSettings() {
            Dictionary<string, string> config;

            if (File.Exists(_configPath)) {
                var json = File.ReadAllText(_configPath);
                config = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }

            else {
                config = new Dictionary<string, string>();
            }

            config["PrimaryPowerUnit"] = SelectedPrimaryUnit;
            config["SecondaryPowerUnit"] = SelectedSecondaryUnit;

            File.WriteAllText(_configPath, JsonSerializer.Serialize(config, ConfigManager.JsonOptions));
        }

        private void OnSaveClick(object sender, RoutedEventArgs e) {
            if (PrimaryUnitComboBox.SelectedItem is string primaryUnit)
                SelectedPrimaryUnit = primaryUnit;
            else if (PrimaryUnitComboBox.SelectedItem is ComboBoxItem primaryItem)
                SelectedPrimaryUnit = primaryItem.Content.ToString();

            if (SecondaryUnitComboBox.SelectedItem is string secondaryUnit)
                SelectedSecondaryUnit = secondaryUnit;
            else if (SecondaryUnitComboBox.SelectedItem is ComboBoxItem secondaryItem)
                SelectedSecondaryUnit = secondaryItem.Content.ToString();

            SaveSettings();
            MessageBox.Show($"Primary Unit: {SelectedPrimaryUnit}\nSecondary Unit: {SelectedSecondaryUnit}",
                "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
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
