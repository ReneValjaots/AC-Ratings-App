using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Ac.Ratings.Core;
using Ac.Ratings.Model;
using Ac.Ratings.Services;
using Ac.Ratings.Services.MainView;

namespace Ac.Ratings.ViewModel {
    public class SettingsViewModel : ObservableObject {
        private string _selectedPrimaryUnit;
        private string _selectedSecondaryUnit;
        private ObservableCollection<Car> _carDb;

        public event EventHandler<string> Notification;

        public ICommand ResetRatingsCommand { get; }
        public ICommand ResetExtraFeaturesCommand { get; }
        public ICommand RestoreBackupCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand ResetRootFolderCommand { get; }

        public SettingsViewModel() {
            ResetRatingsCommand = new RelayCommand(ResetAllRatings);
            ResetExtraFeaturesCommand = new RelayCommand(ResetAllExtraFeatures);
            RestoreBackupCommand = new RelayCommand(RestoreBackup);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            ResetRootFolderCommand = new RelayCommand(ResetRootFolder);
        }
        public string SelectedPrimaryUnit {
            get => _selectedPrimaryUnit;
            set => SetField(ref _selectedPrimaryUnit, value);
        }

        public string SelectedSecondaryUnit {
            get => _selectedSecondaryUnit;
            set => SetField(ref _selectedSecondaryUnit, value);
        }

        public void SetCarDb(ObservableCollection<Car> carDb) {
            _carDb = carDb;
        }


        public void LoadSettings(string configPath) {
            if (File.Exists(configPath)) {
                var json = File.ReadAllText(configPath);
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

        public void SaveSettings(string configPath) {
            if (!File.Exists(configPath)) {
                throw new FileNotFoundException("Config file not found. Cannot save settings without a valid config file.", configPath);
            }

            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (config == null) {
                throw new InvalidOperationException(
                    "The configuration file could not be parsed. It must be in a valid dictionary format, where each setting consists of a name and a value. For example: \"PrimaryPowerUnit\": \"ps\". Both the name and the value must be enclosed in double quotes and separated by a colon (:).");
            }

            config["PrimaryPowerUnit"] = SelectedPrimaryUnit;
            config["SecondaryPowerUnit"] = SelectedSecondaryUnit;

            File.WriteAllText(configPath, JsonSerializer.Serialize(config, ConfigManager.JsonOptions));
        }

        private void SaveSettings() {
            try {
                SaveSettings(ConfigManager.ConfigFilePath);
                Notification?.Invoke(this, "Settings saved successfully.");
            }
            catch (FileNotFoundException ex) {
                throw new FileNotFoundException("Config file not found. Cannot save settings without a valid config file.", ex);
            }
            catch (InvalidOperationException ex) {
                throw new InvalidOperationException(
                    "The configuration file could not be parsed. It must be in a valid dictionary format, where each setting consists of a name and a value. For example: \"PrimaryPowerUnit\": \"ps\". Both the name and the value must be enclosed in double quotes and separated by a colon (:).",
                    ex);
            }
            catch (Exception ex) {
                throw new Exception($"An unexpected error occurred while saving power formats: {ex.Message}", ex);
            }
        }

        private void ResetAllRatings() {
            if (_carDb == null) {
                throw new InvalidOperationException("Car database is not initialized.");
            }

            CarDataManager.ResetAllRatingsInDatabase(_carDb);
            Notification?.Invoke(this, "All ratings have been reset successfully.");
        }

        private void ResetAllExtraFeatures() {
            if (_carDb == null) {
                throw new InvalidOperationException("Car database is not initialized.");
            }

            CarDataManager.ResetAllExtraFeaturesInDatabase(_carDb);
            Notification?.Invoke(this, "All extra features have been reset successfully.");
        }

        private void RestoreBackup() {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                Title = "Select Backup File",
                Filter = "JSON Files (*.json)|*.json",
                InitialDirectory = ConfigManager.BackupFolder
            };

            if (openFileDialog.ShowDialog() == true && _carDb != null) {
                var backupFilePath = openFileDialog.FileName;
                try {
                    var restoredCarDb = CarDataManager.RestoreCarDbFromBackup(backupFilePath);
                    if (restoredCarDb != null) {
                        _carDb.Clear();
                        foreach (var car in restoredCarDb) {
                            _carDb.Add(car);
                            CarDataManager.SaveCarToFile(car);
                        }
                        Notification?.Invoke(this, "Car database restored successfully.");
                    }
                }
                catch (Exception ex) {
                    throw new Exception($"Failed to restore CarDb from backup: {ex.Message}", ex);
                }
            }
            else {
                throw new InvalidOperationException("No backup file selected or car database is not initialized.");
            }
        }

        private void ResetRootFolder() {
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
                        throw new FileNotFoundException("No configuration file was found to reset.");
                    }
                }
                catch (Exception ex) {
                    throw new Exception($"An error occurred while resetting: {ex.Message}", ex);
                }
            }
            else {
                Notification?.Invoke(this, "Reset operation canceled.");
            }
        }
    }
}
