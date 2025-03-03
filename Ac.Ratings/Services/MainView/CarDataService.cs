using Ac.Ratings.Model;

namespace Ac.Ratings.Services.MainView {
    public static class CarDataService {
        public static List<Car> LoadCarDatabase() {
            if (string.IsNullOrEmpty(ConfigManager.AcRootFolder))
                return new List<Car>();

            var factory = new CarFactory(ConfigManager.AcRootFolder, ConfigManager.CarsRootFolder);
            return factory.InitializeCars();
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
