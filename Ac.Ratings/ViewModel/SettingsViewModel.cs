using System.IO;
using System.Text.Json;
using Ac.Ratings.Core;
using Ac.Ratings.Services;

namespace Ac.Ratings.ViewModel {
    public class SettingsViewModel : ObservableObject {
        private string _selectedPrimaryUnit;
        private string _selectedSecondaryUnit;

        public string SelectedPrimaryUnit {
            get => _selectedPrimaryUnit;
            set => SetField(ref _selectedPrimaryUnit, value);
        }

        public string SelectedSecondaryUnit {
            get => _selectedSecondaryUnit;
            set => SetField(ref _selectedSecondaryUnit, value);
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
    }
}
