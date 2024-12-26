using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ac.Ratings.Model;
using Ac.Ratings.Services.Acd;

namespace Ac.Ratings.Services {
    public class DataInitializer {
        public string ConfigFilePath;
        public string CarsRootFolder;
        public string CarDbFilePath;
        private string _missingDataLogFilePath;
        private string _backupFolder;
        //private string _unpackFolderPath;
        private string _acRootFolder;

        public readonly JsonSerializerOptions JsonOptions = new() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        public DataInitializer() {
            var resourceFolder = LoadConfigValue("ResourceFolder")
                                 ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Resources"));

            ConfigFilePath = Path.Combine(resourceFolder, "config", "config.json");
            EnsureConfigFileExists();

            if (LoadConfigValue("ResourceFolder") == null) {
                SaveConfigValue("ResourceFolder", resourceFolder);
            }

            CarsRootFolder = Path.Combine(resourceFolder, "cars");
            CarDbFilePath = Path.Combine(resourceFolder, "data", "CarDb.json");
            _missingDataLogFilePath = Path.Combine(resourceFolder, "data", "MissingDataLog.txt");
            _backupFolder = Path.Combine(resourceFolder, "backup");
            //_unpackFolderPath = Path.Combine(resourceFolder, "unpackData");

            _acRootFolder = InitializeAcRootFolder();

            if (File.Exists(_missingDataLogFilePath))
                File.WriteAllText(_missingDataLogFilePath, string.Empty);

            CreateCarFolders(_acRootFolder);
            InitializeCarData();
 

        }

        private string InitializeAcRootFolder() {
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

        private string? AskUserForAcRootFolder() {
            var window = new AcRootFolderWindow();
            if (window.ShowDialog() == true) {
                return window.SelectedPath;
            }
            return null;
        }

        public List<string?> GetAllCarFolderNames(string rootFolder) {
            return Directory.GetDirectories(rootFolder)
                .Select(Path.GetFileName)
                .ToList();
        }

        public void CreateCarFolders(string rootFolder) {
            var carFolders = GetAllCarFolderNames(rootFolder);
            foreach (var carFolder in carFolders) {
                if (carFolder is null) continue;
                var newFolder = Path.Combine(CarsRootFolder, carFolder);
                if (!Directory.Exists(newFolder)) {
                    Directory.CreateDirectory(newFolder);
                }
            }
        }

        public void InitializeCarData() {
            var carFolders = GetAllCarFolderNames(_acRootFolder);
            foreach (var carFolder in carFolders) {
                if (carFolder is null) continue;
                try {
                    ProcessCarFolder(carFolder);
                }
                catch (Exception ex) {
                    LogMissingData($"Error processing car folder {carFolder}: {ex.Message}");
                }
            }
        }

        public CarData? ProcessAcdFile(string carFolder) {
            var carData = new CarData();
            var acdFilePath = Path.Combine(_acRootFolder, carFolder, "data.acd");
            AcdEncryption.Factory = new AcdFactory();

            if (!File.Exists(acdFilePath)) {
                LogMissingData($".acd file not found for car: {carFolder}");
                return null;
            }

            var acd = Acd.Acd.FromFile(acdFilePath);

            var drivetrainEntry = acd.GetEntry("drivetrain.ini");
            var engineEntry = acd.GetEntry("engine.ini");

            if (drivetrainEntry != null) {
                var content = Encoding.UTF8.GetString(drivetrainEntry.Data);
                var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                string? currentSection = null;

                foreach (var line in lines) {
                    if (line.StartsWith("[")) currentSection = line.Trim();

                    else if (currentSection == "[TRACTION]" && line.Contains("TYPE=")) {
                        carData.TractionType = ExtractIniValue(line);
                    }
                    else if (currentSection == "[GEARS]" && line.Contains("COUNT=")) {
                        carData.GearsCount = int.Parse(ExtractIniValue(line));
                    }
                    else if (currentSection == "[GEARBOX]" && line.Contains("SUPPORTS_SHIFTER=")) {
                        carData.SupportsShifter = ExtractIniValue(line) == "1";
                    }
                }
            }

            if (engineEntry != null) {
                var content = Encoding.UTF8.GetString(engineEntry.Data);
                var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                int turboCount = 0;

                foreach (var line in lines) {
                    if (line.StartsWith("[TURBO_")) {
                        turboCount++;
                    }
                }

                carData.TurboCount = turboCount;
            }

            return carData;
        }

        private void ProcessCarFolder(string carFolder) {
            var ratingsAppFolder = Path.Combine(CarsRootFolder, carFolder, "RatingsApp");
            var uiJsonPath = Path.Combine(ratingsAppFolder, "ui.json");
            var originalCarFolder = Path.Combine(_acRootFolder, carFolder);
            var uiJsonPathInOriginalFolder = Path.Combine(originalCarFolder, "ui", "ui_car.json");


            var cmBackupFolder = Path.Combine(_backupFolder, "cm", carFolder);
            var appBackupFolder = Path.Combine(_backupFolder, "ratings", carFolder);
            var backupCmUiPath = Path.Combine(cmBackupFolder, "ui_car.json");
            var backupRatingsUiPath = Path.Combine(appBackupFolder, "ui.json");

            Directory.CreateDirectory(ratingsAppFolder);
            Directory.CreateDirectory(cmBackupFolder);
            Directory.CreateDirectory(appBackupFolder);

            if (File.Exists(uiJsonPathInOriginalFolder)) {
                File.Copy(uiJsonPathInOriginalFolder, backupCmUiPath, overwrite: true);
            }
            else {
                LogMissingData($"ui_car.json not found for car: {carFolder}");
            }

            var newCar = LoadCarDataFromJson(uiJsonPathInOriginalFolder) ?? new Car();
            var fetchedData = GetCarDataFromOriginalFolder(originalCarFolder);
            var acdData = ProcessAcdFile(carFolder);

            if (fetchedData != null) {
                newCar.Data.TractionType ??= fetchedData.TractionType;
                newCar.Data.GearsCount = newCar.Data.GearsCount == 0 ? fetchedData.GearsCount : newCar.Data.GearsCount;
                newCar.Data.TurboCount = newCar.Data.TurboCount == 0 ? fetchedData.TurboCount : newCar.Data.TurboCount;
            }

            if (acdData != null)
            {
                newCar.Data.TractionType ??= acdData.TractionType;
                newCar.Data.GearsCount = newCar.Data.GearsCount == 0 ? acdData.GearsCount : newCar.Data.GearsCount;
                newCar.Data.TurboCount = newCar.Data.TurboCount == 0 ? acdData.TurboCount : newCar.Data.TurboCount;
                newCar.Data.SupportsShifter = acdData.SupportsShifter;
            }

            if (File.Exists(uiJsonPath)) {
                var existingCarData = LoadCarDataFromJson(uiJsonPath);
                if (existingCarData != null) {
                    newCar.Ratings = existingCarData.Ratings;
                    newCar.Data.TractionType ??= existingCarData.Data.TractionType;
                    newCar.Data.GearsCount = newCar.Data.GearsCount == 0 ? existingCarData.Data.GearsCount : newCar.Data.GearsCount;
                    newCar.Data.TurboCount = newCar.Data.TurboCount == 0 ? existingCarData.Data.TurboCount : newCar.Data.TurboCount;
                }
            }

            // Check for missing critical data
            if (string.IsNullOrEmpty(newCar.Data.TractionType) || newCar.Data.GearsCount == 0) {
                LogMissingData($"Critical data missing for car: {carFolder}");
            }

            // Finalize newCar object
            newCar.FolderPath = originalCarFolder;
            newCar.FolderName = carFolder;

            // Save newCar to ui.json
            using (var createStream = new FileStream(uiJsonPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.None)) {
                JsonSerializer.Serialize(createStream, newCar, JsonOptions);
            }

            // Backup the updated ui.json
            File.Copy(uiJsonPath, backupRatingsUiPath, overwrite: true);
        }


        private Car? LoadCarDataFromJson(string filePath) {
            if (!File.Exists(filePath)) return null;
            using (var openStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.None)) {
                return JsonSerializer.Deserialize<Car>(openStream);
            }
        }

        private CarData GetCarDataFromOriginalFolder(string originalCarFolder) {
            var drivetrainFilePath = Path.Combine(originalCarFolder, "data", "drivetrain.ini");
            var engineFilePath = Path.Combine(originalCarFolder, "data", "engine.ini");
            var carData = new CarData();

            if (File.Exists(drivetrainFilePath)) {
                var lines = File.ReadAllLines(drivetrainFilePath);
                foreach (var line in lines) {
                    if (line.StartsWith("[TRACTION]")) {
                        carData.TractionType = ParseIniValue(lines, "TYPE");
                    }
                    else if (line.StartsWith("[GEARS]")) {
                        carData.GearsCount = int.Parse(ParseIniValue(lines, "COUNT"));
                    }
                }
            }

            if (File.Exists(engineFilePath)) {
                var lines = File.ReadAllLines(engineFilePath);
                carData.TurboCount = lines.Count(line => line.StartsWith("[TURBO_"));
            }

            return carData;
        }

        private string ParseIniValue(string[] lines, string key) {
            foreach (var line in lines) {
                if (line.StartsWith(key)) {
                    return line.Split('=')[1].Split(';')[0].Trim();
                }
            }

            return string.Empty;
        }

        private string ExtractIniValue(string line) {
            var parts = line.Split(new[] { '=' }, 2); 
            return parts.Length > 1 ? parts[1].Split(';')[0].Trim() : string.Empty;
        }

        private void LogMissingData(string message) {
            var directoryPath = Path.GetDirectoryName(_missingDataLogFilePath);
            if (directoryPath != null && !Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            try {
                File.AppendAllText(_missingDataLogFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (IOException ex) {
                Console.WriteLine($"Failed to log missing data: {ex.Message}");
            }
        }
    }
}
