using System.Collections.ObjectModel;
using Ac.Ratings.Model;

namespace Ac.Ratings.Services.MainView {
    public static class CarDataService {
        public static List<Car> LoadCarDatabase() {
            if (string.IsNullOrEmpty(ConfigManager.AcRootFolder))
                return new List<Car>();

            var factory = new CarFactory(ConfigManager.AcRootFolder, ConfigManager.CarsRootFolder);
            return factory.InitializeCars();
        }

        public static List<string> GetDistinctClasses(ObservableCollection<Car> carDb) {
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


        public static List<string?> GetDistinctAuthors(ObservableCollection<Car> carDb) {
            var authors = carDb
                .Select(x => x.Author)
                .Where(author => !string.IsNullOrEmpty(author))
                .Distinct()
                .OrderBy(author => author)
                .ToList();

            authors.Insert(0, "-- Reset --");
            return authors;
        }
    }
}
