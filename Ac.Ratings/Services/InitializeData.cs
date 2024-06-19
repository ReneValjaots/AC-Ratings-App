using System.IO;
using Ac.Ratings.Model;
using Newtonsoft.Json;

namespace Ac.Ratings.Services {
    public class InitializeData {
        public List<CarData> CarDb { get; private set; }
        private const string _acRootFolder = @"D:\Steam\steamapps\common\assettocorsa\content\cars";
        public string carDbFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Data\CarDb.json";
        public string carDbTestFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Data\CarDbTest.json";

        public InitializeData() {
            if (File.Exists(carDbFilePath)) {
                var jsonContent = File.ReadAllText(carDbFilePath);
                CarDb = JsonConvert.DeserializeObject<List<CarData>>(jsonContent);
            }
            else {
                CarDb = ReadDataFromFiles(_acRootFolder);
                OrganizeCarDb();
                SaveCarData(carDbFilePath);
            }
        }

        private List<CarData> ReadDataFromFiles(string acRootFolder) {
            var result = new List<CarData>();
            var carDirectories = Directory.GetDirectories(acRootFolder);
            foreach (var directory in carDirectories) {
                var jsonFilePath = Path.Combine(directory, "ui", "ui_car.json");
                if (File.Exists(jsonFilePath)) {
                    var carData = ReadDataFromJson(jsonFilePath, directory);
                    result.Add(carData);
                }
                else {
                    Console.WriteLine($"JSON file not found for car in directory: {directory}"); //Fix to something better later
                }
            }
            return result;
        }

        private CarData ReadDataFromJson(string fileName, string carDirectory) {
            using (var reader = new StreamReader(fileName)) {
                var jsonContent = reader.ReadToEnd();
                var carData = JsonConvert.DeserializeObject<CarData>(jsonContent) ?? throw new InvalidOperationException();
                carData.PreviewFolder = FindPreviewImagePath(carDirectory);
                return carData;
            }
        }

        private string FindPreviewImagePath(string carDirectory) {
            var skinsDirectory = Path.Combine(carDirectory, "skins");
            if (!Directory.Exists(skinsDirectory)) {
                return null; // Return null if the skins directory doesn't exist
            }
            var skinDirectories = Directory.GetDirectories(skinsDirectory);
            foreach (var skinDir in skinDirectories) {
                var previewFilePath = Path.Combine(skinDir, "preview.jpg");
                if (File.Exists(previewFilePath)) {
                    return previewFilePath; // Return the path if the preview.jpg file is found
                }
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
