using System.IO;
using System.Text.Json;
using System.Windows;
using Ac.Ratings.Model;

namespace Ac.Ratings.Services.MainView {
    public static class CarDataService {
        public static Car? LoadCarData(string filePath) {
            try {
                var jsonContent = File.ReadAllText(filePath);
                var car = JsonSerializer.Deserialize<Car>(jsonContent, ConfigManager.JsonOptions);
                return car;
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to load car data from {filePath}: {ex.Message}");
                return null;
            }
        }

        public static void SaveCarToFile(Car car) {
            try {
                if (string.IsNullOrEmpty(ConfigManager.CarsRootFolder)) {
                    MessageBox.Show("Cars root folder path is null or empty.");
                    return;
                }

                if (string.IsNullOrEmpty(car.FolderName)) {
                    MessageBox.Show($"Folder name for car {car.Name} is null or empty.");
                    return;
                }

                var carFolderPath = Path.Combine(ConfigManager.CarsRootFolder, car.FolderName);
                var carJsonFilePath = Path.Combine(carFolderPath, "RatingsApp", "ui.json");
                var jsonContent = JsonSerializer.Serialize(car, ConfigManager.JsonOptions);
                File.WriteAllText(carJsonFilePath, jsonContent);
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to save car ratings to file: {ex.Message}");
            }
        }
    }
}
