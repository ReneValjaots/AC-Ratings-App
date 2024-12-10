using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ac.Ratings.Model;
using Ac.Ratings.Services.Acd;

namespace Ac.Ratings.Services
{
    public class DataInitializer {
        private const string _acRootFolder = @"D:\Steam\steamapps\common\assettocorsa\content\cars";
        public string CarsRootFolder = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\cars\";
        public string CarDbFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\data\CarDb.json";
        private const string _missingDataLogFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\data\MissingDataLog.txt";
        private const string _backupFolder = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\backup\";

        public readonly JsonSerializerOptions JsonOptions = new() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        public DataInitializer() {
            if (File.Exists(_missingDataLogFilePath))
                File.WriteAllText(_missingDataLogFilePath, string.Empty);
            CreateCarFolders(_acRootFolder);
            InitializeCarData();
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
            string unpackFolderPath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\unpackData\";

            if (!File.Exists(acdFilePath)) {
                LogMissingData($".acd file not found for car: {carFolder}");
                return null;
            }

            FileUtils.EnsureDirectoryExists(unpackFolderPath);

            var acd = Acd.Acd.FromFile(acdFilePath);

            acd.ExportDirectory(unpackFolderPath);

            string drivetrainFilePath = Path.Combine(unpackFolderPath, "drivetrain.ini");

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

            string engineFilePath = Path.Combine(unpackFolderPath, "engine.ini");

            if (File.Exists(engineFilePath)) {
                var lines = File.ReadAllLines(engineFilePath);
                carData.TurboCount = lines.Count(line => line.StartsWith("[TURBO_"));
            }

            try {
                var directoryInfo = new DirectoryInfo(unpackFolderPath);

                foreach (var file in directoryInfo.GetFiles()) {
                    file.Delete();
                }
            }
            catch (Exception ex) {
                LogMissingData($"Failed to clean up unpack folder: {ex.Message}");
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

            var newCar = LoadCarDataFromJson(uiJsonPathInOriginalFolder) ?? new();
            var fetchedData = GetCarDataFromOriginalFolder(originalCarFolder);
            var acdData = ProcessAcdFile(carFolder);

            if (fetchedData != null) {
                newCar.Data.TractionType ??= fetchedData.TractionType;
                newCar.Data.GearsCount = newCar.Data.GearsCount == 0 ? fetchedData.GearsCount : newCar.Data.GearsCount;
                newCar.Data.TurboCount = newCar.Data.TurboCount == 0 ? fetchedData.TurboCount : newCar.Data.TurboCount;
            }

            if (acdData != null) {
                newCar.Data.TractionType ??= acdData.TractionType;
                newCar.Data.GearsCount = newCar.Data.GearsCount == 0 ? acdData.GearsCount : newCar.Data.GearsCount;
                newCar.Data.TurboCount = newCar.Data.TurboCount == 0 ? acdData.TurboCount : newCar.Data.TurboCount;
                newCar.Data.SupportsShifter = acdData.SupportsShifter;
            }

            if (File.Exists(uiJsonPath)) {
                var existingCarData = LoadCarDataFromJson(uiJsonPath);
                if (existingCarData != null) {
                    newCar.Ratings ??= existingCarData.Ratings;
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
