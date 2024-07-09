using System.IO;
using Ac.Ratings.Model;
using Newtonsoft.Json;

namespace Ac.Ratings.Services {
    public class InitializeData {
        public List<Car> CarDb { get; private set; }
        public List<CarSpecs> CarSpecs { get; private set; }
        public List<CarData> CarData { get; private set; }
        public List<CarRatings> CarRatings { get; private set; }
        private const string _acRootFolder = @"D:\Steam\steamapps\common\assettocorsa\content\cars";
        public string carDbFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Data\CarDb.json";
        public string carDbTestFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Data\CarDbTest.json";
        private string carsRootFolder = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\cars\";

        public InitializeData() {
            //if (File.Exists(carDbFilePath)) {
            //    var jsonContent = File.ReadAllText(carDbFilePath);
            //    CarDb = JsonConvert.DeserializeObject<List<Car>>(jsonContent);
            //}
            //else {
            CarDb = ReadDataFromFiles(_acRootFolder);
            OrganizeCarDb();
            SaveCarData(carDbFilePath);
            CreateCarDirectories();
            //CopyUiFiles();
            //}
        }

        private void CopyUiFiles() {
            foreach (var car in CarDb) {
                string sourceFilePath = Path.Combine(car.FolderPath, "ui", "ui_car.json");
                if (File.Exists(sourceFilePath)) {
                    string destinationFolderPath = Path.Combine(carsRootFolder, car.FolderName, "RatingsApp");
                    string destinationFilePath = Path.Combine(destinationFolderPath, "ui_car.json");
                    Directory.CreateDirectory(destinationFolderPath); // Ensure the directory exists
                    File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
                }
            }
        }

        private void CreateCarDirectories() {
            foreach (var car in CarDb) {
                string carFolder = Path.Combine(carsRootFolder, car.FolderName);
                string ratingsAppFolder = Path.Combine(carFolder, "RatingsApp");

                // Create directories if they do not exist
                Directory.CreateDirectory(carFolder);
                Directory.CreateDirectory(ratingsAppFolder);

                // Define file paths
                string physicsDataPath = Path.Combine(ratingsAppFolder, "data.json");
                string uiDataPath = Path.Combine(ratingsAppFolder, "ui.json");
                string ratingsDataPath = Path.Combine(ratingsAppFolder, "ratings.json");

                // Create JSON files
                File.WriteAllText(physicsDataPath, JsonConvert.SerializeObject(car.CarData, Formatting.Indented));
                File.WriteAllText(uiDataPath, JsonConvert.SerializeObject(car, Formatting.Indented));
                File.WriteAllText(ratingsDataPath, JsonConvert.SerializeObject(car.Ratings, Formatting.Indented));
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
                var drivetrainFilePath = Path.Combine(directory, "data", "drivetrain.ini");
                if (File.Exists(jsonFilePath)) {
                    var car = ReadDataFromJson(jsonFilePath, directory);
                    if (File.Exists(drivetrainFilePath)) {
                        var carData = ReadDataFromIni(drivetrainFilePath);
                        car.CarData = carData;
                    }
                    result.Add(car);
                }
                else {
                    Console.WriteLine($"JSON file not found for car in directory: {directory}"); //Fix to something better later
                }
            }
            return result;
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
            using (var reader = new StreamReader(fileName)) {
                var jsonContent = reader.ReadToEnd();
                var carData = JsonConvert.DeserializeObject<Car>(jsonContent) ?? throw new InvalidOperationException();
                //carData.PreviewFolder = FindPreviewImagePath(carDirectory);
                carData.FolderName = Path.GetFileName(carDirectory);
                carData.FolderPath = carDirectory;
                return carData;
            }
        }

        //private string FindPreviewImagePath(string carDirectory) {
        //    var skinsDirectory = Path.Combine(carDirectory, "skins");
        //    if (!Directory.Exists(skinsDirectory)) {
        //        return null; // Return null if the skins directory doesn't exist
        //    }
        //    var skinDirectories = Directory.GetDirectories(skinsDirectory);
        //    foreach (var skinDir in skinDirectories) {
        //        var previewFilePath = Path.Combine(skinDir, "preview.jpg");
        //        if (File.Exists(previewFilePath)) {
        //            return previewFilePath; // Return the path if the preview.jpg file is found
        //        }
        //    }
        //    return null;
        //}

        private void SaveCarData(string filePath) {
            var jsonContent = JsonConvert.SerializeObject(CarDb, Formatting.Indented);
            File.WriteAllText(filePath, jsonContent);
        }

        private void OrganizeCarDb() {
            CarDb = CarDb.OrderBy(car => car.Name).ToList();
        }
    }
}
