using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ac.Ratings.Model;
using Ac.Ratings.Services;
using System.Text.Json;
using Ac.Ratings.Services.MainView;

namespace Ac.Ratings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly DataInitializer _initializer;
        private string _longestCarName = string.Empty;
        private List<Car> _carDb = new();
        private CancellationTokenSource? _cancellationTokenSource;

        public MainWindow() {
            InitializeComponent();
            _initializer = new DataInitializer();
            LoadCarDatabase();

            AuthorFilter.ItemsSource = GetDistinctAuthors();
            AuthorFilter.SelectedIndex = -1;
            ClassFilter.ItemsSource = GetDistinctClasses();
            ClassFilter.SelectedIndex = -1;

            CarList.ItemsSource = _carDb;
            if (_carDb.Count > 0) {
                CarList.SelectedIndex = 0;
            }
        }

        private void LoadCarDatabase() {
            if (ConfigManager.AcRootFolder == null) return;
            var carFolders = _initializer.GetAllCarFolderNames(ConfigManager.AcRootFolder);
            foreach (var carFolder in carFolders) {
                if (carFolder == null) continue;
                var uiJsonPath = Path.Combine(ConfigManager.CarsRootFolder, carFolder, "RatingsApp", "ui.json");
                if (File.Exists(uiJsonPath)) {
                    var carData = CarDataService.LoadCarData(uiJsonPath);
                    if (carData != null) {
                        _carDb.Add(carData);
                        if (carData.Name == null) continue;
                        if (carData.Name.Length > _longestCarName.Length) {
                            _longestCarName = carData.Name;
                        }
                    }
                }
            }

            _carDb = _carDb.OrderBy(x => x.Name).ToList();
        }

        private async void CarList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            try {
                var selectedCar = (Car)CarList.SelectedItem;
                if (selectedCar != null) {
                    await LoadCarImage(selectedCar);
                    DisplayCarStats(selectedCar);
                    UpdateAverageRating();

                    DataContext = selectedCar;
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void DisplayCarStats(Car selectedCar) {
            Engine.Text = CarDisplayService.ShowCarEngineStats(selectedCar);
            Drivetrain.Text = CarDisplayService.ShowCarDriveTrain(selectedCar);
            Gearbox.Text = CarDisplayService.ShowCarGearbox(selectedCar);

            var className = selectedCar.Class;
            if (!string.IsNullOrEmpty(className)) {
                if (!className.All(char.IsUpper)) {
                    className = className.ToLower();
                    className = char.ToUpper(className[0]) + className[1..];
                }
            }
        }

        private async Task LoadCarImage(Car car) {
            try {
                if (string.IsNullOrEmpty(car.FolderPath)) {
                    MessageBox.Show("Car folder path is null or empty.");
                    return;
                }

                var skinsDirectory = Path.Combine(car.FolderPath, "skins");

                if (!Directory.Exists(skinsDirectory)) {
                    MessageBox.Show($"Preview image not found for {car.Name}");
                    return;
                }

                var skinDirectories = Directory.GetDirectories(skinsDirectory);
                await PopulateSkinGrid(skinDirectories);

                var previewFilePath = Path.Combine(skinDirectories[0], "preview.jpg");
                if (File.Exists(previewFilePath)) {
                    CarImage.Source = new BitmapImage(new Uri(previewFilePath, UriKind.Absolute));
                }
            }

            catch (Exception ex) {
                MessageBox.Show($"Failed to load image: {ex.Message}");
            }
        }

        private async Task PopulateSkinGrid(string[] skinDirectories) {
            if (_cancellationTokenSource != null) {
                await _cancellationTokenSource.CancelAsync();
            }
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            SkinGrid.Children.Clear();
            SkinGrid.RowDefinitions.Clear();
            SkinGrid.ColumnDefinitions.Clear();

            const int boxSize = 35;
            const int boxesPerRow = 31;
            const int marginSize = 1;

            for (int i = 0; i < boxesPerRow; i++) {
                SkinGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(boxSize + marginSize) });
            }

            int totalRows = (int)Math.Ceiling((double)skinDirectories.Length / boxesPerRow);
            for (int i = 0; i < totalRows; i++) {
                SkinGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(boxSize + marginSize) });
            }

            for (int i = 0; i < skinDirectories.Length; i++) {
                if (token.IsCancellationRequested) {
                    return;
                }

                var previewFilePath = Path.Combine(skinDirectories[i], "preview.jpg");
                var liveryFilePath = Path.Combine(skinDirectories[i], "livery.png");

                if (File.Exists(previewFilePath) && File.Exists(liveryFilePath)) {
                    var liveryUri = new Uri(liveryFilePath, UriKind.Absolute);
                    var rowIndex = i / boxesPerRow;
                    var columnIndex = i % boxesPerRow;

                    try {
                        var bitmapImage = await Task.Run(() => {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = liveryUri;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            image.Freeze(); // Make it thread-safe
                            return image;
                        }, token); // Pass token to allow cancellation

                        if (token.IsCancellationRequested) {
                            return; // Stop processing if canceled
                        }

                        var imageControl = new Image {
                            Width = boxSize,
                            Height = boxSize,
                            Source = bitmapImage,
                            Stretch = Stretch.UniformToFill,
                        };

                        imageControl.MouseLeftButtonDown += (s, e) => SkinBox_Clicked(previewFilePath);

                        Dispatcher.Invoke(() => {
                            Grid.SetRow(imageControl, rowIndex);
                            Grid.SetColumn(imageControl, columnIndex);
                            SkinGrid.Children.Add(imageControl);
                        });
                    }
                    catch (TaskCanceledException) { }
                }
            }
        }

        private void SkinBox_Clicked(string previewFilePath) {
            CarImage.Source = new BitmapImage(new Uri(previewFilePath, UriKind.Absolute));
        }

        private void SaveRatings() {
            var selectedCar = (Car)CarList.SelectedItem;
            if (selectedCar != null) {
                SetRatingsFromSliders(selectedCar);

                UpdateAverageRating();
                CarDataService.SaveCarToFile(selectedCar);
            }
        }

        private void SaveAllRatings() {
            try {
                foreach (var car in _carDb) {
                    if (car.FolderName != null) {
                        var carFolder = Path.Combine(ConfigManager.CarsRootFolder, car.FolderName);
                        var carRatingsAppFolder = Path.Combine(carFolder, "RatingsApp");
                        var carJsonPath = Path.Combine(carRatingsAppFolder, "ui.json");

                        var jsonContent = JsonSerializer.Serialize(car, ConfigManager.JsonOptions);
                        File.WriteAllText(carJsonPath, jsonContent);
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"Error saving all ratings: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetRatingsFromSliders(Car car) {
            car.Ratings.CornerHandling = CornerHandlingSlider.Value;
            car.Ratings.Brakes = BrakesSlider.Value;
            car.Ratings.Realism = RealismSlider.Value;
            car.Ratings.Sound = SoundSlider.Value;
            car.Ratings.ExteriorQuality = ExteriorQualitySlider.Value;
            car.Ratings.InteriorQuality = InteriorQualitySlider.Value;
            car.Ratings.ForceFeedbackQuality = ForceFeedbackQualitySlider.Value;
            car.Ratings.FunFactor = FunFactorSlider.Value;
        }

        private void ClearRatings() {
            var selectedCar = (Car)CarList.SelectedItem;
            if (selectedCar != null) {
                CarRatingService.ResetRatingValues(selectedCar);
                ResetRatingSliderValues();
                UpdateAverageRating();
                CarDataService.SaveCarToFile(selectedCar);
            }
        }

        private void ResetRatingSliderValues() {
            CornerHandlingSlider.Value = 0;
            BrakesSlider.Value = 0;
            RealismSlider.Value = 0;
            SoundSlider.Value = 0;
            ExteriorQualitySlider.Value = 0;
            InteriorQualitySlider.Value = 0;
            ForceFeedbackQualitySlider.Value = 0;
            FunFactorSlider.Value = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) => SaveRatings();

        private void UpdateAverageRating() {
            var selectedCar = (Car)CarList.SelectedItem;
            if (selectedCar != null) {
                var ratings = new List<double> {
                    selectedCar.Ratings.CornerHandling,
                    selectedCar.Ratings.Brakes,
                    selectedCar.Ratings.Realism,
                    selectedCar.Ratings.Sound,
                    selectedCar.Ratings.ExteriorQuality,
                    selectedCar.Ratings.InteriorQuality,
                    selectedCar.Ratings.DashboardQuality,
                    selectedCar.Ratings.ForceFeedbackQuality,
                    selectedCar.Ratings.FunFactor,
                };
                var averageRating = ratings.Average();
                AverageRatingTextBlock.Text = $"Average Rating: {averageRating:F2}";
                selectedCar.Ratings.AverageRating = averageRating;
            }
        }

        private void ClearRatingsButton_Click(object sender, RoutedEventArgs e) => ClearRatings();

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateCarListFilter();
        }

        private bool CombinedFilter(object obj) {
            if (obj is not Car car) return false;

            var selectedAuthor = AuthorFilter.SelectedItem?.ToString();
            var selectedClass = ClassFilter.SelectedItem?.ToString();
            var searchText = SearchBox.Text.Trim();

            var conditions = new List<Func<bool>> {
                () => IsMatch(car.Author, selectedAuthor),
                () => IsMatch(car.Class, selectedClass),
                () => car.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false,
                () => car.Ratings.CornerHandling >= CornerHandlingFilter.Value,
                () => car.Ratings.Brakes >= BrakingFilter.Value,
                () => car.Ratings.Realism >= RealismFilter.Value,
                () => car.Ratings.Sound >= SoundFilter.Value,
                () => car.Ratings.ExteriorQuality >= ExteriorQualityFilter.Value,
                () => car.Ratings.InteriorQuality >= InteriorQualityFilter.Value,
                () => car.Ratings.ForceFeedbackQuality >= ForceFeedbackQualityFilter.Value,
                () => car.Ratings.FunFactor >= FunFactorFilter.Value,
                () => car.Ratings.AverageRating >= AverageRatingFilter.Value
            };

            return conditions.All(condition => condition());
        }

        private static bool IsMatch(string? value, string? filter) {
            return string.IsNullOrEmpty(filter) ||
                   string.Equals(value, filter, StringComparison.OrdinalIgnoreCase);
        }

        private List<string?> GetDistinctAuthors() {
            var authors = _carDb
                .Select(x => x.Author)
                .Where(author => !string.IsNullOrEmpty(author))
                .Distinct()
                .OrderBy(author => author)
                .ToList();

            authors.Insert(0, "-- Reset --");
            return authors;
        }

        private List<string> GetDistinctClasses() {
            var classes = _carDb
                .Select(x => x.Class?.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .GroupBy(x => x?.ToLower())
                .Select(CarDisplayService.NormalizeClassName)
                .OrderBy(x => x)
                .ToList();

            classes.Insert(0, "-- Reset --");
            return classes;
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e) {
            AuthorFilter.SelectedItem = null;
            AuthorFilter.Text = "Filter by author";
            ClassFilter.SelectedItem = null;
            SearchBox.Text = string.Empty;
            ResetFilterSliderValues();
            CarList.Items.Filter = null;
        }

        private void ResetFilterSliderValues() {
            CornerHandlingFilter.Value = 0;
            BrakingFilter.Value = 0;
            RealismFilter.Value = 0;
            SoundFilter.Value = 0;
            ExteriorQualityFilter.Value = 0;
            InteriorQualityFilter.Value = 0;
            ForceFeedbackQualityFilter.Value = 0;
            FunFactorFilter.Value = 0;
            AverageRatingFilter.Value = 0;
        }

        private void AuthorFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (AuthorFilter.SelectedItem?.ToString() == "-- Reset --") {
                AuthorFilter.SelectedIndex = -1;
                UpdateCarListFilter();
                return;
            }

            UpdateCarListFilter();
        }

        private void ClassFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ClassFilter.SelectedItem?.ToString() == "-- Reset --") {
                ClassFilter.SelectedIndex = -1;
                UpdateCarListFilter();
                return;
            }

            UpdateCarListFilter();
        }

        private void FilterSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            UpdateCarListFilter();
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e) {
            var settingsWindow = new SettingsWindow(this);
            settingsWindow.ShowDialog();
        }

        public void ResetAllRatingsInDatabase() {
            CreateBackupOfCarDb();

            foreach (var car in _carDb) {
                CarRatingService.ResetRatingValues(car);
                if (car.FolderName != null) {
                    var carFolder = Path.Combine(ConfigManager.CarsRootFolder, car.FolderName);
                    var carRatingsAppFolder = Path.Combine(carFolder, "RatingsApp");
                    var carJsonPath = Path.Combine(carRatingsAppFolder, "ui.json");

                    if (Directory.Exists(carRatingsAppFolder)) {
                        var jsonContent = JsonSerializer.Serialize(car, ConfigManager.JsonOptions);
                        File.WriteAllText(carJsonPath, jsonContent);
                    }
                }
            }

            CarList.Items.Refresh();

            var previouslySelectedCar = CarList.SelectedItem;
            CarList.SelectedItem = null;
            CarList.Items.Refresh();

            CarList.SelectedItem = previouslySelectedCar;
            CarList.Items.Refresh();
        }

        private void CreateBackupOfCarDb() {
            try {
                string backupFolder = ConfigManager.BackupFolder;

                if (!Directory.Exists(backupFolder)) {
                    Directory.CreateDirectory(backupFolder);
                }

                string backupFileName = $"CarDb_backup_{DateTime.Now:dd_MM_yyyy_HH_mm}.json";
                string backupFilePath = Path.Combine(backupFolder, backupFileName);

                var jsonContent = JsonSerializer.Serialize(_carDb, ConfigManager.JsonOptions);
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
            catch (Exception ex) {
                MessageBox.Show($"Error creating backup: {ex.Message}", "Backup Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void RestoreCarDbFromBackup(string backupFilePath) {
            try {
                if (!File.Exists(backupFilePath)) {
                    MessageBox.Show("Selected backup file not found.", "Restore Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var jsonContent = File.ReadAllText(backupFilePath);
                var restoredCarDb = JsonSerializer.Deserialize<List<Car>>(jsonContent, ConfigManager.JsonOptions);

                if (restoredCarDb != null) {
                    _carDb = restoredCarDb.OrderBy(x => x.Name).ToList();
                    CarList.ItemsSource = _carDb;
                    CarList.Items.Refresh();
                    SaveAllRatings();
                    MessageBox.Show("Car database restored successfully.", "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to restore CarDb from backup: {ex.Message}", "Restore Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                SaveRatings();
                e.Handled = true;
            }
        }

        private void RatingsFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (RatingsFilter.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Content.ToString() == "-- Reset --") {
                ResetFilterSliderValues();
                RatingsFilter.SelectedIndex = -1;
            }

            UpdateCarListFilter();
            RatingsFilter.SelectedIndex = -1;
        }

        private void UpdateCarListFilter() {
            CarList.Items.Filter = CombinedFilter;
        }

        private void ClearFeaturesButton_Click(object sender, RoutedEventArgs e) {
            foreach (var child in ExtraFeatures.Children) {
                if (child is Grid grid) {
                    foreach (var gridChild in grid.Children) {
                        if (gridChild is CheckBox checkBox) {
                            checkBox.IsChecked = false;
                        }
                    }
                }
            }

            SaveRatings();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            CreateBackupOfCarDb();
        }

        public void ResetAllExtraFeaturesInDatabase() {
            CreateBackupOfCarDb();

            foreach (var car in _carDb) {
                CarRatingService.ResetExtraFeatureValues(car);
                if (car.FolderName != null) {
                    var carFolder = Path.Combine(ConfigManager.CarsRootFolder, car.FolderName);
                    var carRatingsAppFolder = Path.Combine(carFolder, "RatingsApp");
                    var carJsonPath = Path.Combine(carRatingsAppFolder, "ui.json");

                    if (Directory.Exists(carRatingsAppFolder)) {
                        var jsonContent = JsonSerializer.Serialize(car, ConfigManager.JsonOptions);
                        File.WriteAllText(carJsonPath, jsonContent);
                    }
                }
            }

            CarList.Items.Refresh();

            var previouslySelectedCar = CarList.SelectedItem;
            CarList.SelectedItem = null;
            CarList.Items.Refresh();

            CarList.SelectedItem = previouslySelectedCar;
            CarList.Items.Refresh();
        }

        private void CarList_Loaded(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(_longestCarName)) return;

            var typeface = new Typeface(CarList.FontFamily, CarList.FontStyle, CarList.FontWeight, CarList.FontStretch);
            var fontSize = CarList.FontSize;

            var formattedText = new FormattedText(
                _longestCarName,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black, // Brush color doesn't matter
                new NumberSubstitution(),
                1);

            double carListWidth = formattedText.WidthIncludingTrailingWhitespace + 25;
            CarList.Width = carListWidth;
            SearchBox.Width = carListWidth - 10;
        }
    }
}