using System.IO;
using System.Text;
using System.Text.Json;
using Ac.Ratings.Model;
using Ac.Ratings.Services.Acd;

namespace Ac.Ratings.Services {
    public class CarFactory {
        private readonly string _acRootFolder;
        private readonly string _carsRootFolder;

        public CarFactory(string acRootFolder, string carsRootFolder) {
            _acRootFolder = acRootFolder ?? throw new ArgumentNullException(nameof(acRootFolder));
            _carsRootFolder = carsRootFolder ?? throw new ArgumentNullException(nameof(carsRootFolder));
        }

        public List<Car> InitializeCars(bool forceUpdate = false) {
            var cars = new List<Car>();
            var carFolders = Directory.GetDirectories(_acRootFolder).Select(Path.GetFileName).ToList();

            EnsureCarFoldersExist(carFolders);

            DateTime lastUpdate = GetLastUpdateTime();
            bool shouldUpdate = forceUpdate || (DateTime.UtcNow - lastUpdate).TotalDays > 7;

            foreach (var folder in carFolders) {
                var car = ProcessCarFolder(folder, shouldUpdate);
                if (car != null) {
                    cars.Add(car);
                }
            }

            File.WriteAllText(ConfigManager.LastUpdatedFilepath, DateTime.UtcNow.ToString("o"));
            return cars.OrderBy(c => c.Name).ToList();
        }

        private void EnsureCarFoldersExist(IEnumerable<string?> carFolders) {
            foreach (var folder in carFolders.Where(f => f != null)) {
                // Assert folder is non-null since it's filtered before
                var newFolder = Path.Combine(_carsRootFolder, folder!);
                Directory.CreateDirectory(newFolder);
                Directory.CreateDirectory(Path.Combine(newFolder, "RatingsApp"));
                Directory.CreateDirectory(Path.Combine(ConfigManager.BackupFolder, "cm", folder!));
            }
        }

        private Car? ProcessCarFolder(string? carFolder, bool shouldUpdate) {
            if (string.IsNullOrEmpty(carFolder)) return null;

            try {
                var originalPath = Path.Combine(_acRootFolder, carFolder);
                var ratingsPath = Path.Combine(_carsRootFolder, carFolder, "RatingsApp");
                var uiJsonPath = Path.Combine(ratingsPath, "ui.json");
                var uiCarJsonPath = Path.Combine(originalPath, "ui", "ui_car.json");

                // Backup if needed
                if (shouldUpdate && File.Exists(uiCarJsonPath)) {
                    BackupUiCar(carFolder, uiCarJsonPath);
                }

                // Load or create car
                var car = LoadOrCreateCar(uiJsonPath, uiCarJsonPath);
                car.FolderPath = originalPath;
                car.FolderName = carFolder;

                // Update car data
                UpdateCarData(car, originalPath);

                // Save updated car
                SaveCar(uiJsonPath, car);

                // Load display properties and specs
                car.LoadDisplayProperties();
                car.Specs = new CarSpecs(car.FolderPath);

                return car;
            }
            catch (Exception ex) {
                ErrorLogger.LogError("CarProcessing", ex);
                return null;
            }
        }

        private void BackupUiCar(string carFolder, string uiCarJsonPath) {
            var backupPath = Path.Combine(ConfigManager.BackupFolder, "cm", carFolder, "ui_car.json");
            File.Copy(uiCarJsonPath, backupPath, true);
        }

        private Car LoadOrCreateCar(string uiJsonPath, string uiCarJsonPath) {
            if (File.Exists(uiJsonPath)) {
                using var stream = new FileStream(uiJsonPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return JsonSerializer.Deserialize<Car>(stream, ConfigManager.JsonOptions) ?? new Car();
            }

            if (File.Exists(uiCarJsonPath)) {
                using var stream = new FileStream(uiCarJsonPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return JsonSerializer.Deserialize<Car>(stream, ConfigManager.JsonOptions) ?? new Car();
            }

            return new Car();
        }

        private void UpdateCarData(Car car, string originalPath) {
            var iniData = GetCarDataFromIniFiles(originalPath);
            var acdData = ProcessAcdFile(originalPath);

            foreach (var data in new[] { iniData, acdData }.Where(d => d != null)) {
                car.Data.TractionType ??= data.TractionType;
                car.Data.GearsCount = car.Data.GearsCount == 0 ? data.GearsCount : car.Data.GearsCount;
                car.Data.TurboCount = car.Data.TurboCount == 0 ? data.TurboCount : car.Data.TurboCount;
                car.Data.SupportsShifter = car.Data.SupportsShifter || data.SupportsShifter;
            }

            if (string.IsNullOrEmpty(car.Data.TractionType) || car.Data.GearsCount == 0) {
                ErrorLogger.LogError("MissingData", new Exception($"Critical data missing for car: {originalPath}"));
            }
        }

        private void SaveCar(string uiJsonPath, Car car) {
            using var stream = new FileStream(uiJsonPath, FileMode.Create, FileAccess.Write, FileShare.None);
            JsonSerializer.Serialize(stream, car, ConfigManager.JsonOptions);
        }

        private CarData ProcessAcdFile(string folderPath) {
            var carData = new CarData();
            var acdFilePath = Path.Combine(folderPath, "data.acd");
            AcdEncryption.Factory = new AcdFactory();

            if (!File.Exists(acdFilePath)) {
                ErrorLogger.LogError("MissingData", new Exception($".acd file not found for car: {folderPath}"));
                return carData;
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
                carData.TurboCount = lines.Count(line => line.StartsWith("[TURBO_"));
            }

            return carData;
        }

        private CarData GetCarDataFromIniFiles(string folderPath) {
            var drivetrainFilePath = Path.Combine(folderPath, "data", "drivetrain.ini");
            var engineFilePath = Path.Combine(folderPath, "data", "engine.ini");
            var carData = new CarData();

            if (File.Exists(drivetrainFilePath)) {
                var lines = File.ReadAllLines(drivetrainFilePath);
                string? currentSection = null;
                foreach (var line in lines) {
                    if (line.StartsWith("[")) {
                        currentSection = line.Trim();
                    }
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

        private DateTime GetLastUpdateTime() {
            if (File.Exists(ConfigManager.LastUpdatedFilepath) &&
                DateTime.TryParse(File.ReadAllText(ConfigManager.LastUpdatedFilepath), out DateTime lastUpdate)) {
                return lastUpdate;
            }

            return DateTime.MinValue;
        }

        private static string ExtractIniValue(string line) {
            var parts = line.Split(['='], 2);
            return parts.Length > 1 ? parts[1].Split(';')[0].Trim() : string.Empty;
        }
    }
}
