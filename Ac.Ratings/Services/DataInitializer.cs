using System.IO;
using System.Text;
using System.Text.Json;
using Ac.Ratings.Model;
using Ac.Ratings.Services.Acd;

namespace Ac.Ratings.Services {
    public class DataInitializer {
        private readonly string _acRootFolder;

        public DataInitializer() {
            var acRootFolder = ConfigManager.AcRootFolder;
            if (string.IsNullOrEmpty(acRootFolder)) {
                LogMissingData("Critical Error: acRootFolder is null or empty. The application cannot continue.");
                Environment.Exit(1);
            }
            _acRootFolder = acRootFolder;

            if (File.Exists(ConfigManager.ErrorLogFilepath))
                File.WriteAllText(ConfigManager.ErrorLogFilepath, string.Empty);

            CreateCarFolders();
            InitializeCarData();
        }

        public List<string?> GetAllCarFolderNames(string rootFolder) {
            return Directory.GetDirectories(rootFolder)
                .Select(Path.GetFileName)
                .ToList();
        }

        public void CreateCarFolders() {
            var carFolders = GetAllCarFolderNames(_acRootFolder);
            foreach (var carFolder in carFolders) {
                if (carFolder is null) continue;
                var newFolder = Path.Combine(ConfigManager.CarsRootFolder, carFolder);
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
                    if (line.StartsWith('[')) {
                        currentSection = line.Trim();
                        continue;
                    }

                    if (currentSection == "[TRACTION]" && line.Contains("TYPE=")) {
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

                int turboCount = lines.Count(line => line.StartsWith("[TURBO_"));

                carData.TurboCount = turboCount;
            }

            return carData;
        }

        private void ProcessCarFolder(string carFolder) {
            var ratingsAppFolder = Path.Combine(ConfigManager.CarsRootFolder, carFolder, "RatingsApp");
            var uiJsonPath = Path.Combine(ratingsAppFolder, "ui.json");
            var originalCarFolder = Path.Combine(_acRootFolder, carFolder);
            var uiJsonPathInOriginalFolder = Path.Combine(originalCarFolder, "ui", "ui_car.json");


            var cmBackupFolder = Path.Combine(ConfigManager.BackupFolder, "cm", carFolder);
            var appBackupFolder = Path.Combine(ConfigManager.BackupFolder, "ratings", carFolder);
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

            if (string.IsNullOrEmpty(newCar.Data.TractionType) || newCar.Data.GearsCount == 0) {
                LogMissingData($"Critical data missing for car: {carFolder}");
            }

            newCar.FolderPath = originalCarFolder;
            newCar.FolderName = carFolder;

            using (var createStream = new FileStream(uiJsonPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.None)) {
                JsonSerializer.Serialize(createStream, newCar, ConfigManager.JsonOptions);
            }

            File.Copy(uiJsonPath, backupRatingsUiPath, overwrite: true);
        }


        private static Car? LoadCarDataFromJson(string filePath) {
            if (!File.Exists(filePath)) return null;
            using (var openStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.None)) {
                return JsonSerializer.Deserialize<Car>(openStream);
            }
        }

        private static CarData GetCarDataFromOriginalFolder(string originalCarFolder) {
            var drivetrainFilePath = Path.Combine(originalCarFolder, "data", "drivetrain.ini");
            var engineFilePath = Path.Combine(originalCarFolder, "data", "engine.ini");
            var carData = new CarData();

            if (File.Exists(drivetrainFilePath)) {
                var lines = File.ReadAllLines(drivetrainFilePath);
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

            if (File.Exists(engineFilePath)) {
                var lines = File.ReadAllLines(engineFilePath);
                carData.TurboCount = lines.Count(line => line.StartsWith("[TURBO_"));
            }

            return carData;
        }

        private static string ExtractIniValue(string line) {
            var parts = line.Split(new[] { '=' }, 2); 
            return parts.Length > 1 ? parts[1].Split(';')[0].Trim() : string.Empty;
        }

        private static void LogMissingData(string message) {
            try {
                ErrorLogger.LogError("MissingData", new Exception(message));
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to log missing data: {ex.Message}");
            }
        }
    }
}
