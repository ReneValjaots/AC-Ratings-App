using System.IO;
using Ac.Ratings.Model;
using Newtonsoft.Json;

namespace Ac.Ratings.Services {
    public class InitializeData {
        public Dictionary<string, CarData> CarDb;
        private const string _acRootFolder = @"D:\\SteamLibrary\\steamapps\\common\\assettocorsa\\content\\cars";
        private const string _carDbFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Data\CarDb.txt";
        private const string _carRatingsFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Ratings\CarRatings.txt";

        public InitializeData() {
            CarDb = ReadDataFromFiles(_acRootFolder);
            OrganizeCarDb();
            SetUpDbFile(_carDbFilePath);
            SetUpRatingsFile(_carRatingsFilePath);
        }

        private Dictionary<string, CarData> ReadDataFromFiles(string acRootFolder) {
            var result = new Dictionary<string, CarData>();
            var carDirectories = Directory.GetDirectories(acRootFolder);
            foreach (var directory in carDirectories) {
                var jsonFilePath = Path.Combine(directory, "ui", "ui_car.json");
                if (File.Exists(jsonFilePath)) {
                    var carData = ReadDataFromJson(jsonFilePath);
                    var folderName = Path.GetFileName(directory);
                    result[folderName] = carData;
                }
                else {
                    //throw new Exception($"File with path {jsonFilePath} does not exist."); FIX!
                }
            }
            return result;
        }

        private CarData ReadDataFromJson(string fileName) {
            using (var reader = new StreamReader(fileName)) {
                var jsonContent = reader.ReadToEnd();
                var carData = JsonConvert.DeserializeObject<CarData>(jsonContent) ?? throw new InvalidOperationException();
                return carData;
            }
        }

        private void SetUpDbFile(string filePath) {
            if (CarDb == null) {
                Console.WriteLine("No data to write. Exiting program"); // Rework error logic 
                return;
            }

            using (StreamWriter writer = new StreamWriter(filePath)) {
                foreach (var kvp in CarDb) {
                    writer.WriteLine($"{kvp.Key}, {kvp.Value.Name} {kvp.Value.Brand}, {kvp.Value.Specs.Bhp}, {kvp.Value.Specs.Torque}, {kvp.Value.Specs.Weight}, " + 
                                     $"{kvp.Value.Specs.Topspeed}, {kvp.Value.Specs.Acceleration}, {kvp.Value.Specs.Pwratio}");
                }
            }
        }

        private void SetUpRatingsFile(string filePath) {
            if (CarDb == null) {
                Console.WriteLine("No data to write. Exiting program"); // Rework error logic 
                return;
            }

            using (StreamWriter writer = new StreamWriter(filePath)) {
                foreach (var kvp in CarDb) {
                    writer.WriteLine($"{kvp.Key}, {kvp.Value.Ratings.Handling}, {kvp.Value.Ratings.Physics}, {kvp.Value.Ratings.Realism}, " + 
                                     $"{kvp.Value.Ratings.Sound}, {kvp.Value.Ratings.Visuals}, {kvp.Value.Ratings.FunFactor}, {kvp.Value.Ratings.ExtraFeatures}");
                }
            }
        }

        private void OrganizeCarDb() {
            var orderedDb = CarDb.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            CarDb = orderedDb;
        }
    }
}
