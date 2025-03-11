using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Ac.Ratings.Model;

namespace Ac.Ratings.Services.MainView {
    public static class CarDataManager {
        private static HashSet<Car> _modifiedCars = new();

        public static void MarkCarAsModified(Car car) {
            _modifiedCars.Add(car);
        }

        public static void SaveModifiedCars() {
            foreach (var car in _modifiedCars) {
                SaveCarToFile(car);
            }

            _modifiedCars.Clear();
        }

        public static void SaveCarToFile(Car car) {
            if (string.IsNullOrEmpty(ConfigManager.CarsRootFolder)) {
                throw new ArgumentException("Cars root folder path is null or empty.");
            }

            if (string.IsNullOrEmpty(car.FolderName)) {
                throw new ArgumentException($"Folder name for car {car.Name} is null or empty.");
            }

            var carFolderPath = Path.Combine(ConfigManager.CarsRootFolder, car.FolderName);
            var carJsonFilePath = Path.Combine(carFolderPath, "RatingsApp", "ui.json");
            var jsonContent = JsonSerializer.Serialize(car, ConfigManager.JsonOptions);
            File.WriteAllText(carJsonFilePath, jsonContent);
        }

        public static void CreateBackupOfCarDb(ObservableCollection<Car> cars) {
            string backupFolder = Path.Combine(ConfigManager.BackupFolder, "backups");

            if (!Directory.Exists(backupFolder)) {
                Directory.CreateDirectory(backupFolder);
            }

            string backupFileName = $"CarDb_backup_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}.json";
            string backupFilePath = Path.Combine(backupFolder, backupFileName);

            var jsonContent = JsonSerializer.Serialize(cars, ConfigManager.JsonOptions);
            File.WriteAllText(backupFilePath, jsonContent);

            var backupFiles = Directory.GetFiles(backupFolder, "CarDb_backup_*.json")
                .OrderByDescending(File.GetCreationTime)
                .ToList();

            if (backupFiles.Count > 10) {
                foreach (var oldBackup in backupFiles.Skip(10)) {
                    File.Delete(oldBackup);
                }
            }
        }

        public static void ResetAllRatingsInDatabase(ObservableCollection<Car> cars) {
            try {
                CreateBackupOfCarDb(cars);

                foreach (var car in cars) {
                    CarRatingService.ResetRatingValues(car);
                    SaveCarToFile(car);
                }
            }
            catch (Exception ex) {
                ErrorLogger.LogError("ResetAllRatings", ex);
            }
        }

        public static void ResetAllExtraFeaturesInDatabase(ObservableCollection<Car> cars) {
            try {
                CreateBackupOfCarDb(cars);

                foreach (var car in cars) {
                    CarRatingService.ResetExtraFeatureValues(car);
                    SaveCarToFile(car);
                }
            }
            catch (Exception ex) {
                ErrorLogger.LogError("ResetAllExtraFeatures", ex);
            }
        }

        public static List<Car> RestoreCarDbFromBackup(string backupFilePath) {
            if (!File.Exists(backupFilePath)) {
                throw new FileNotFoundException("Selected backup file not found.");
            }

            var jsonContent = File.ReadAllText(backupFilePath);
            var restoredCarDb = JsonSerializer.Deserialize<List<Car>>(jsonContent, ConfigManager.JsonOptions);

            if (restoredCarDb != null) {
                return restoredCarDb.OrderBy(x => x.Name).ToList();
            }

            throw new InvalidOperationException("Failed to deserialize the backup file.");
        }

        public static bool CombinedFilter(Car car, string? selectedAuthor, string? selectedClass, string searchText, double cornerHandlingFilter, double brakingFilter, double realismFilter,
            double soundFilter, double exteriorQualityFilter, double interiorQualityFilter, double forceFeedbackQualityFilter, double funFactorFilter, double averageRatingFilter) {
            var conditions = new List<Func<bool>> {
                () => IsMatch(car.Author, selectedAuthor),
                () => IsMatch(car.Class, selectedClass),
                () => car.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false,
                () => car.Ratings.CornerHandling >= cornerHandlingFilter,
                () => car.Ratings.Brakes >= brakingFilter,
                () => car.Ratings.Realism >= realismFilter,
                () => car.Ratings.Sound >= soundFilter,
                () => car.Ratings.ExteriorQuality >= exteriorQualityFilter,
                () => car.Ratings.InteriorQuality >= interiorQualityFilter,
                () => car.Ratings.ForceFeedbackQuality >= forceFeedbackQualityFilter,
                () => car.Ratings.FunFactor >= funFactorFilter,
                () => car.Ratings.AverageRating >= averageRatingFilter
            };

            return conditions.All(condition => condition());
        }

        private static bool IsMatch(string? value, string? filter) {
            return string.IsNullOrEmpty(filter) ||
                   string.Equals(value, filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
