using System.IO;
using System.Windows;
using Ac.Ratings.Model;
using Newtonsoft.Json;

namespace Ac.Ratings.Services {
    public class InitializeData {
        public List<Car> CarDb { get; private set; }
        private const string _acRootFolder = @"D:\Steam\steamapps\common\assettocorsa\content\cars";
        public string CarDbFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Data\CarDb.json";
        public string CarsRootFolder = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\cars\";
        private string _missingDataLogFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Data\MissingDataLog.txt";

        public InitializeData() {
            if (File.Exists(_missingDataLogFilePath))
                File.WriteAllText(_missingDataLogFilePath, string.Empty);

            CarDb = ReadDataFromFiles(_acRootFolder);
            OrganizeCarDb();
            SaveCarData(CarDbFilePath);
            //CreateCarDirectories();
        }

        private void LogMissingData(string message) {
            var directoryPath = Path.GetDirectoryName(_missingDataLogFilePath);

            if (directoryPath != null)
                Directory.CreateDirectory(directoryPath);

            if (!File.Exists(_missingDataLogFilePath)) 
                File.Create(_missingDataLogFilePath).Dispose();

            File.AppendAllText(_missingDataLogFilePath, $"{DateTime.Now}: {message}\n");
        }

        private void CreateCarDirectories() {
            foreach (var car in CarDb) {
                if (string.IsNullOrEmpty(car.FolderName)) {
                    MessageBox.Show($"Folder name for car {car.Name} is null or empty.");
                    continue;
                }

                if (string.IsNullOrEmpty(car.FolderPath)) {
                    MessageBox.Show($"Folder path for car {car.Name} is null or empty.");
                    continue;
                }

                string carFolder = Path.Combine(CarsRootFolder, car.FolderName);
                Directory.CreateDirectory(carFolder);

                string uiFolder = Path.Combine(carFolder, "ui");
                Directory.CreateDirectory(uiFolder);

                string ratingsAppFolder = Path.Combine(carFolder, "RatingsApp");
                Directory.CreateDirectory(ratingsAppFolder);

                var sourceUiFilePath = Path.Combine(car.FolderPath, "ui", "ui_car.json");

                if (File.Exists(sourceUiFilePath)) {
                    var destinationUiFilePath = Path.Combine(uiFolder, "ui_car.json");
                    File.Copy(sourceUiFilePath, destinationUiFilePath, overwrite: true);
                }
                else {
                    LogMissingData($"ui_car.json not found for car: {car.Name}");
                }

                string physicsDataPath = Path.Combine(ratingsAppFolder, "data.json");
                string uiJsonPath = Path.Combine(ratingsAppFolder, "ui.json");
                string ratingsDataPath = Path.Combine(ratingsAppFolder, "ratings.json");

                if (!File.Exists(ratingsDataPath)) {
                    File.WriteAllText(ratingsDataPath, JsonConvert.SerializeObject(car.Ratings, Formatting.Indented));
                }

                File.WriteAllText(physicsDataPath, JsonConvert.SerializeObject(car.Data, Formatting.Indented));
                File.WriteAllText(uiJsonPath, JsonConvert.SerializeObject(car, Formatting.Indented));
            }
        }

        private List<Car> ReadDataFromFiles(string acRootFolder) {
            var result = new List<Car>();
            var carDirectories = Directory.GetDirectories(acRootFolder);
            foreach (var directory in carDirectories) {
                if (Path.GetFileName(directory).Contains("traffic")) {
                    continue;
                }
                var jsonFilePath = Path.Combine(directory, "ui", "ui_car.json");

                var dataFolders = Directory.GetDirectories(directory, "data*", SearchOption.TopDirectoryOnly);

                if (dataFolders.Length == 0) {
                    LogMissingData($"No data folder found for car in directory: {directory}");
                }
                else if (dataFolders.Length > 1) {
                    LogMissingData($"Multiple data folders found for car in directory: {directory}");
                }

                var drivetrainFilePath = Path.Combine(directory, "data", "drivetrain.ini");
                var engineFilePath = Path.Combine(directory, "data", "engine.ini");
                if (File.Exists(jsonFilePath)) {
                    var car = ReadDataFromJson(jsonFilePath, directory);
                    if (File.Exists(drivetrainFilePath)) {
                        var carData = ReadDataFromIni(drivetrainFilePath);
                        car.Data = carData;
                    }

                    if (File.Exists(engineFilePath)) {
                        var turboCount = ReadTurboCountFromIni(engineFilePath);
                        car.Data.TurboCount = turboCount;
                    }

                    result.Add(car);
                }
                else {
                    Console.WriteLine($"JSON file not found for car in directory: {directory}"); //Fix to something better later
                }
            }
            return result;
        }

        private int ReadTurboCountFromIni(string filePath) {
            var lines = File.ReadAllLines(filePath);
            return lines.Count(line => line.StartsWith("[TURBO_"));
        }

        private CarData ReadDataFromIni(string filePath) {
            var carData = new CarData();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines) {
                if (line.StartsWith("[TRACTION]")) {
                    carData.TractionType = ParseValue(lines, "TYPE");
                }
                else if (line.StartsWith("[GEARS]")) {
                    carData.GearsCount = int.Parse(ParseValue(lines, "COUNT"));
                }
                else if (line.StartsWith("[GEARBOX]")) {
                    carData.SupportsShifter = ParseValue(lines, "SUPPORTS_SHIFTER") == "1";
                }
            }
            return carData;
        }

        private string ParseValue(string[] lines, string key) {
            foreach (var line in lines) {
                if (line.StartsWith(key)) {
                    return line.Split('=')[1].Split(';')[0].Trim();
                }                
            }
            return string.Empty;
        }

        private Car ReadDataFromJson(string fileName, string carDirectory) {
            var existingCar = LoadCarFromFolder(carDirectory);

            using (var reader = new StreamReader(fileName)) {
                var jsonContent = reader.ReadToEnd();
                var car = JsonConvert.DeserializeObject<Car>(jsonContent) ?? throw new InvalidOperationException();
                if (existingCar != null) {
                    car.Ratings = existingCar.Ratings;
                }
                car.FolderName = Path.GetFileName(carDirectory);
                car.FolderPath = carDirectory;
                return car;
            }
        }

        private Car? LoadCarFromFolder(string carDirectory) {
            var carFilePath = Path.Combine(CarsRootFolder, Path.GetFileName(carDirectory), "RatingsApp", "ui.json");

            if (File.Exists(carFilePath)) {
                var jsonContent = File.ReadAllText(carFilePath);
                return JsonConvert.DeserializeObject<Car>(jsonContent);
            }
            return null;
        }

        private void SaveCarData(string filePath) {
            var jsonContent = JsonConvert.SerializeObject(CarDb, Formatting.Indented);
            File.WriteAllText(filePath, jsonContent);
        }

        private void OrganizeCarDb() {
            CarDb = CarDb.OrderBy(car => car.Name).ToList();
        }
    }
}
