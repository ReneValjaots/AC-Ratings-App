using System.IO;
using System.Text.Json;
using Ac.Ratings.Model;

namespace Ac.Ratings.Services.MainView {
    public static class CarDataService {
        public static List<Car> LoadCarDatabase() {
            var carDb = new List<Car>();
            if (ConfigManager.AcRootFolder == null) return carDb;
            var carFolders = new DataInitializer().GetAllCarFolderNames(ConfigManager.AcRootFolder);
            foreach (var carFolder in carFolders) {
                if (carFolder == null) continue;
                var uiJsonPath = Path.Combine(ConfigManager.CarsRootFolder, carFolder, "RatingsApp", "ui.json");
                if (File.Exists(uiJsonPath)) {
                    try {
                        var carData = LoadCarData(uiJsonPath);
                        if (carData != null) {
                            carData.Specs = new CarSpecs(carData.FolderPath);
                            carDb.Add(carData);
                        }
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Failed to load car data from {uiJsonPath}: {ex.Message}");
                    }
                }
            }

            return carDb.OrderBy(x => x.Name).ToList();
        }

        private static Car? LoadCarData(string filePath) {
            var jsonContent = File.ReadAllText(filePath);
            var car = JsonSerializer.Deserialize<Car>(jsonContent, ConfigManager.JsonOptions);
            return car;
            
        }

        public static List<string> GetDistinctClasses(List<Car> carDb) {
            var classes = carDb
                .Select(x => x.Class?.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .GroupBy(x => x?.ToLower())
                .Select(CarDisplayService.NormalizeClassName)
                .OrderBy(x => x)
                .ToList();

            classes.Insert(0, "-- Reset --");
            return classes;
        }


        public static List<string?> GetDistinctAuthors(List<Car> carDb) {
            var authors = carDb
                .Select(x => x.Author)
                .Where(author => !string.IsNullOrEmpty(author))
                .Distinct()
                .OrderBy(author => author)
                .ToList();

            authors.Insert(0, "-- Reset --");
            return authors;
        }

        public static string GetLongestCarName(List<Car> carDb) {
            return carDb
                .Where(c => c.Name != null)
                .OrderByDescending(c => c.Name!.Length)
                .Select(c => c.Name)
                .FirstOrDefault() ?? string.Empty;
        }
    }
}
