using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ac.Ratings.Services {
    public static class ConfigManager {
        public static string ResourceFolder { get; private set; }
        public static string ConfigFilePath { get; private set; }
        public static string CarsRootFolder { get; private set; }
        public static string MissingDataLogFilePath { get; private set; }
        public static string ErrorLogFilepath { get; private set; }
        public static string BackupFolder { get; private set; }
        public static string UnpackFolderPath { get; private set; }
        public static string ModifiedRatingsPath { get; private set; }
        public static string? OriginalRatingsPath { get; private set; }
        public static string? AcRootFolder { get; private set; }

        public static readonly JsonSerializerOptions JsonOptions = new() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        static ConfigManager() {
            ResourceFolder = LoadConfigValue("ResourceFolder")
                             ?? Path.Combine(AppContext.BaseDirectory, "Resources");

            EnsureResourceFolderStructure();

            ConfigFilePath = Path.Combine(ResourceFolder, "config", "config.json");

            CarsRootFolder = Path.Combine(ResourceFolder, "cars");
            MissingDataLogFilePath = Path.Combine(ResourceFolder, "data", "MissingDataLog.txt");
            ErrorLogFilepath = Path.Combine(ResourceFolder, "data", "ErrorLog.txt");
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

            if (string.IsNullOrEmpty(AcRootFolder)) {
                AcRootFolder = AskUserForAcRootFolder();
                if (string.IsNullOrEmpty(AcRootFolder)) {
                    Environment.Exit(0);
                }

                SaveConfigValue("AcRootFolder", AcRootFolder);
            }

            if (LoadConfigValue("AcRootFolder") == null && string.IsNullOrEmpty(AcRootFolder)) {
                SaveConfigValue("AcRootFolder", AcRootFolder);
            }
        }

        private static void EnsureResourceFolderStructure() {
            if (!Directory.Exists(ResourceFolder)) {
                Directory.CreateDirectory(ResourceFolder);
            }

            var subfolders = new[] {
                Path.Combine(ResourceFolder, "config"),
                Path.Combine(ResourceFolder, "cars"),
                Path.Combine(ResourceFolder, "data"),
                Path.Combine(ResourceFolder, "backup"),
                Path.Combine(ResourceFolder, "unpackData")
            };

            foreach (var folder in subfolders) {
                if (!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }
            }

            var files = new[] {
                Path.Combine(ResourceFolder, "data", "MissingDataLog.txt"),
                Path.Combine(ResourceFolder, "data", "ErrorLog.txt")
            };

            foreach (var file in files) {
                if (!File.Exists(file)) {
                    File.Create(file).Close();
                }
            }
        }

        private static string? LoadAcRootFolder() {
            var rootFolder = LoadConfigValue("AcRootFolder");

            if (string.IsNullOrEmpty(rootFolder) || !Directory.Exists(rootFolder)) {
                return null;
            }

            return rootFolder;
        }

        private static string? AskUserForAcRootFolder() {
            var acRootFolderWindow = new AcRootFolderWindow();
            if (acRootFolderWindow.ShowDialog() == true) {
                return acRootFolderWindow.SelectedPath;
            }

            return null;
        }

        private static string? LoadConfigValue(string key) {
            if (File.Exists(ConfigFilePath)) {
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(ConfigFilePath));
                return config?.GetValueOrDefault(key);
            }

            return null;
        }

        private static void SaveConfigValue(string key, string value) {
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

        private static string? LoadOriginalRatingsPath() {
            if (LoadConfigValue("OriginalRatingsDatafilePath") != null) {
                return LoadConfigValue("OriginalRatingsDatafilePath");
            }

            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcTools Content Manager", "Progress");
            string ratingsPath = Path.Combine(appDataPath, "Ratings.data");

            return File.Exists(ratingsPath) ? ratingsPath : null;
        }

        private static void EnsureConfigFileExists() {
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
