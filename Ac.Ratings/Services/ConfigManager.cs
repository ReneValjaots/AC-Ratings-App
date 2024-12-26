using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ac.Ratings.Services {
    public class ConfigManager {
        public string ResourceFolder { get; private set; }
        public string ConfigFilePath { get; private set; }
        public string CarsRootFolder { get; private set; }
        public string MissingDataLogFilePath { get; private set; }
        public string BackupFolder { get; private set; }
        public string UnpackFolderPath { get; private set; }
        public string ModifiedRatingsPath { get; private set; }
        public string? OriginalRatingsPath { get; private set; }
        public string AcRootFolder { get; private set; }


        public readonly JsonSerializerOptions JsonOptions = new() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        public ConfigManager() {
            ResourceFolder = LoadConfigValue("ResourceFolder")
                             ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Resources"));

            ConfigFilePath = Path.Combine(ResourceFolder, "config", "config.json");

            CarsRootFolder = Path.Combine(ResourceFolder, "cars");
            MissingDataLogFilePath = Path.Combine(ResourceFolder, "data", "MissingDataLog.txt");
            BackupFolder = Path.Combine(ResourceFolder, "backup");
            UnpackFolderPath = Path.Combine(ResourceFolder, "unpackData");
            ModifiedRatingsPath = Path.Combine(UnpackFolderPath, "Ratings.txt");

            OriginalRatingsPath = LoadOriginalRatingsPath();
            EnsureConfigFileExists();
            if (LoadConfigValue("ResourceFolder") == null) {
                SaveConfigValue("ResourceFolder", ResourceFolder);
            }

            if (LoadConfigValue("OriginalRatingsDatafilePath") == null && OriginalRatingsPath != null) {
                SaveConfigValue("OriginalRatingsDatafilePath", OriginalRatingsPath);
            }

            AcRootFolder = LoadAcRootFolder();

            if (LoadConfigValue("AcRootFolder") == null && string.IsNullOrEmpty(AcRootFolder)) {
                SaveConfigValue("AcRootFolder", AcRootFolder);
            }
        }

        private string LoadAcRootFolder() {
            var rootFolder = LoadConfigValue("AcRootFolder");

            if (string.IsNullOrEmpty(rootFolder) || !Directory.Exists(rootFolder)) {
                rootFolder = AskUserForAcRootFolder();
                if (!string.IsNullOrEmpty(rootFolder)) {
                    SaveConfigValue("AcRootFolder", rootFolder);
                }
                else {
                    throw new InvalidOperationException("Assetto Corsa root folder is required.");
                }
            }

            return rootFolder;
        }

        private string? AskUserForAcRootFolder() {
            var settingsWindow = new SettingsWindow(null);
            if (settingsWindow.ShowDialog() == true) {
                return settingsWindow.SelectedPath;
            }

            return null;
        }

        private string? LoadConfigValue(string key) {
            if (File.Exists(ConfigFilePath)) {
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(ConfigFilePath));
                return config?.GetValueOrDefault(key);
            }

            return null;
        }

        private void SaveConfigValue(string key, string value) {
            Dictionary<string, string> config;

            if (File.Exists(ConfigFilePath)) {
                config = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(ConfigFilePath)) ?? new Dictionary<string, string>();
            }
            else {
                config = new Dictionary<string, string>();
            }

            config[key] = value;
            File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(config, JsonOptions));
        }

        private string? LoadOriginalRatingsPath() {
            if (LoadConfigValue("OriginalRatingsDatafilePath") != null) {
                return LoadConfigValue("OriginalRatingsDatafilePath");
            }

            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcTools Content Manager", "Progress");
            string ratingsPath = Path.Combine(appDataPath, "Ratings.data");

            return File.Exists(ratingsPath) ? ratingsPath : null;
        }

        private void EnsureConfigFileExists() {
            var directoryPath = Path.GetDirectoryName(ConfigFilePath);
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath!);
            }

            if (!File.Exists(ConfigFilePath)) {
                var defaultConfig = new Dictionary<string, string>();
                File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(defaultConfig, JsonOptions));
            }
        }
    }
}
