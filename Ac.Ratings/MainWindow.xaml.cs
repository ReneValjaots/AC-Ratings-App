using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ac.Ratings.Model;
using Ac.Ratings.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Ac.Ratings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        //private InitializeData _data;
        private DataInitializer _initializer;
        private List<Car> _carDb = new();
        private static readonly List<string> _gearboxTags = ["manual", "automatic", "semiautomatic", "sequential"];
        private static readonly List<string> _drivetrainTags = ["rwd", "awd", "fwd"];
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
            var carFolders = _initializer.GetAllCarFolderNames(_initializer.CarsRootFolder);
            foreach (var carFolder in carFolders) {
                var uiJsonPath = Path.Combine(_initializer.CarsRootFolder, carFolder, "RatingsApp", "ui.json");
                if (File.Exists(uiJsonPath)) {
                    var carData = LoadCarData(uiJsonPath);
                    if (carData != null) {
                        _carDb.Add(carData);
                    }
                }
            }
            _carDb = _carDb.OrderBy(x => x.Name).ToList();
        }

        private Car? LoadCarData(string filePath) {
            try {
                var jsonContent = File.ReadAllText(filePath);
                var car = JsonSerializer.Deserialize<Car>(jsonContent, _initializer.JsonOptions);
                return car;
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to load car data from {filePath}: {ex.Message}");
                return null;
            }
        }

        private void CarList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selectedCar = (Car)CarList.SelectedItem;
            if (selectedCar != null) {
                LoadCarImage(selectedCar);
                DisplayCarStats(selectedCar);
                UpdateAverageRating();

                DataContext = selectedCar;
            }
        }

        private void DisplayCarStats(Car selectedCar) {
            Name.Text = selectedCar.Name;

            Engine.Text = ShowCarEngineStats(selectedCar);
            Drivetrain.Text = ShowCarDriveTrain(selectedCar);
            Gearbox.Text = ShowCarGearbox(selectedCar);

            var className = selectedCar.Class;
            if (!string.IsNullOrEmpty(className)) {
                if (!className.All(char.IsUpper)) {
                    className = className.ToLower();
                    className = char.ToUpper(className[0]) + className[1..];
                }
            }

            Brand.Text = selectedCar.Brand ?? string.Empty;
            Year.Text = (selectedCar.Year).ToString() ?? string.Empty;
            Class.Text = className ?? string.Empty;
            Author.Text = selectedCar.Author ?? string.Empty;
            Weight.Text = selectedCar.Specs.Weight ?? string.Empty;
            Pwratio.Text = selectedCar.Specs.PowerToWeightRatio ?? string.Empty;
        }

        private void LoadCarImage(Car car) {
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
                PopulateSkinGrid(skinDirectories);

                var previewFilePath = Path.Combine(skinDirectories[0], "preview.jpg");
                if (File.Exists(previewFilePath)) {
                    CarImage.Source = new BitmapImage(new Uri(previewFilePath, UriKind.Absolute));
                }
            }

            catch (Exception ex) {
                MessageBox.Show($"Failed to load image: {ex.Message}");
            }
        }

        private async void PopulateSkinGrid(string[] skinDirectories) {
            _cancellationTokenSource?.Cancel();
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
                            Grid.SetRow(imageControl, i / boxesPerRow);
                            Grid.SetColumn(imageControl, i % boxesPerRow);
                            SkinGrid.Children.Add(imageControl);
                        });
                    }
                    catch (TaskCanceledException) {
                        
                    }
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

                var jsonContent = JsonSerializer.Serialize(_carDb, _initializer.JsonOptions);
                File.WriteAllText(_initializer.CarDbFilePath, jsonContent);
                SaveCarToFile(selectedCar);
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

        private void SaveCarToFile(Car car) {
            try {
                if (string.IsNullOrEmpty(_initializer.CarsRootFolder)) {
                    MessageBox.Show("Cars root folder path is null or empty.");
                    return;
                }

                if (string.IsNullOrEmpty(car.FolderName)) {
                    MessageBox.Show($"Folder name for car {car.Name} is null or empty.");
                    return;
                }

                var carFolderPath = Path.Combine(_initializer.CarsRootFolder, car.FolderName);
                var carJsonFilePath = Path.Combine(carFolderPath, "RatingsApp", "ui.json");
                var jsonContent = JsonSerializer.Serialize(car, _initializer.JsonOptions);
                File.WriteAllText(carJsonFilePath, jsonContent);
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to save car ratings to file: {ex.Message}");
            }
        }

        private void ClearRatings() {
            var selectedCar = (Car)CarList.SelectedItem;
            if (selectedCar != null) {
                ResetRatingValues(selectedCar);

                ResetRatingSliderValues();

                var jsonContent = JsonSerializer.Serialize(_carDb, _initializer.JsonOptions);
                File.WriteAllText(_initializer.CarDbFilePath, jsonContent);

                UpdateAverageRating();

                SaveCarToFile(selectedCar);
            }
        }

        private static void ResetRatingValues(Car selectedCar) {
            selectedCar.Ratings.CornerHandling = 0;
            selectedCar.Ratings.Brakes = 0;
            selectedCar.Ratings.Realism = 0;
            selectedCar.Ratings.Sound = 0;
            selectedCar.Ratings.ExteriorQuality = 0;
            selectedCar.Ratings.InteriorQuality = 0;
            selectedCar.Ratings.DashboardQuality = 0;
            selectedCar.Ratings.ForceFeedbackQuality = 0;
            selectedCar.Ratings.FunFactor = 0;
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

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearRatings();

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateCarListFilter();
        }

        private string ShowCarDriveTrain(Car selectedCar) {
            var tags = selectedCar.Tags;
            var data = selectedCar.Data.TractionType;

            if (data != null) {
                if (data.Contains("rwd", StringComparison.OrdinalIgnoreCase))
                    return "Rear-wheel drive";
                if (data.Contains("awd", StringComparison.OrdinalIgnoreCase))
                    return "All-wheel drive";
                if (data.Contains("fwd", StringComparison.OrdinalIgnoreCase))
                    return "Front-wheel drive";
            }

            var drivetrainFromSpecificTag = tags?.FirstOrDefault(x => x.Contains("#+"))?.Replace(" ", "").ToLower().Remove(0, 2);
            var drivetrainFromRegularTags = tags?.FirstOrDefault(tag => _drivetrainTags.Contains(tag.ToLower()));
            return drivetrainFromSpecificTag?.ToUpper() ?? drivetrainFromRegularTags?.ToUpper() ?? string.Empty;
        }

        private string ShowCarGearbox(Car selectedCar) {
            var gearsCount = selectedCar.Data.GearsCount;
            var isManual = selectedCar.Data.SupportsShifter;
            var tags = selectedCar.Tags;
            var gearboxFromSpecificTag = tags?.FirstOrDefault(x => x.Contains("#-"))?.Replace(" ", "").Remove(0, 2);
            var gearboxFromRegularTags = tags?.FirstOrDefault(tag => _gearboxTags.Contains(tag.ToLower()));

            if (gearsCount == 0)
                return gearboxFromSpecificTag ?? gearboxFromRegularTags ?? string.Empty;

            return isManual switch {
                true => $"{gearsCount}-speed manual transmission",
                false => $"{gearsCount}-speed automatic transmission",
            };
        }

        private string? GetCarEngineData(Car selectedCar) {
            var tags = selectedCar.Tags;
            var engineTag = tags?.FirstOrDefault(x => x.Contains("#!"))?.Replace(" ", "").Remove(0, 2);
            return engineTag;
        }

        private string ShowCarEngineStats(Car selectedCar) {
            var data = GetCarEngineData(selectedCar);
            if (string.IsNullOrEmpty(data)) 
                return string.Empty;

            var result = string.Empty;
            var parts = data.Split('&');

            if (parts.Length > 0) result = GetDisplacement(result, parts[0]);
            result = AppendInductionSystemToEngineStats(result, selectedCar);
            if (parts.Length > 1) result = GetLayout(result, parts[1]);

            return result.Trim();
        }

        private string GetLayout(string result, string data) {
            if (data.StartsWith("I", StringComparison.OrdinalIgnoreCase)) 
                result += "inline-" + Regex.Match(data, @"\d+").Value + " engine";

            if (data.StartsWith("V", StringComparison.OrdinalIgnoreCase)) 
                result += data.ToUpper() + " engine";
            
            if (data.StartsWith("F", StringComparison.OrdinalIgnoreCase)) 
                result += "flat-" + Regex.Match(data, @"\d+").Value + " engine";

            if (data.StartsWith("B", StringComparison.OrdinalIgnoreCase))
                result += "boxer-" + Regex.Match(data, @"\d+").Value + " engine";

            if (data.StartsWith("R", StringComparison.OrdinalIgnoreCase)) 
                result += "rotary engine";

            return result;
        }


        private string GetDisplacement(string result, string data) {
            if (char.IsDigit(data[0])) {
                var displacementValue = data.Replace("L", "", StringComparison.OrdinalIgnoreCase);
                result += $"{displacementValue}l ";
            }
            return result;
        }

        private string AppendInductionSystemToEngineStats(string result, Car car) {
            string inductionSystem = ShowInductionSystemForEngineStats(car);
            result += inductionSystem + " ";
            return result;
        }

        private string ShowInductionSystemForEngineStats(Car car) {
            return car.Data.TurboCount switch {
                1 => "turbocharged",
                2 => "twin turbo",
                _ => "naturally aspirated"
            };
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

        private bool IsMatch(string? value, string? filter) {
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
                .Select(NormalizeClassName) 
                .OrderBy(x => x)
                .ToList();

            classes.Insert(0, "-- Reset --");
            return classes;
        }

        private string NormalizeClassName(IGrouping<string?, string?> group) {
            var uppercaseName = group.FirstOrDefault(name => name != null && name.All(c => !char.IsLetter(c) || char.IsUpper(c)));

            if (uppercaseName != null)
                return uppercaseName;

            var name = group.FirstOrDefault();

            return name == null ? string.Empty : char.ToUpper(name[0]) + name[1..].ToLower();
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
                ResetRatingValues(car);
                if (car.FolderName != null) {
                    var carFolder = Path.Combine(_initializer.CarsRootFolder, car.FolderName);
                    var carRatingsAppFolder = Path.Combine(carFolder, "RatingsApp");
                    var carJsonPath = Path.Combine(carRatingsAppFolder, "ui.json");

                    if (Directory.Exists(carRatingsAppFolder)) {
                        var jsonContent = JsonSerializer.Serialize(car, _initializer.JsonOptions);
                        File.WriteAllText(carJsonPath, jsonContent);
                    }
                }
            }

            var dbJsonContent = JsonSerializer.Serialize(_carDb, _initializer.JsonOptions);
            File.WriteAllText(_initializer.CarDbFilePath, dbJsonContent);
            CarList.Items.Refresh();

            var previouslySelectedCar = CarList.SelectedItem;
            CarList.SelectedItem = null;
            CarList.Items.Refresh();

            CarList.SelectedItem = previouslySelectedCar;
            CarList.Items.Refresh();
        }

        private void CreateBackupOfCarDb() {
            try {
                string resourcesFolder = Directory.GetParent(Directory.GetParent(_initializer.CarDbFilePath).FullName).FullName;
                string backupFolder = Path.Combine(resourcesFolder, "backup");

                if (!Directory.Exists(backupFolder)) {
                    Directory.CreateDirectory(backupFolder);
                }

                string backupFileName = $"CarDb_backup_{DateTime.Now:dd_mm_yyyy_HHmm}.json";
                string backupFilePath = Path.Combine(backupFolder, backupFileName);

                File.Copy(_initializer.CarDbFilePath, backupFilePath, overwrite: true);

                MessageBox.Show("Backup of CarDb.json created successfully.", "Backup Complete", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex) {
                MessageBox.Show($"Error creating backup: {ex.Message}", "Backup Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
        }

        private void UpdateCarListFilter() {
            CarList.Items.Filter = CombinedFilter;
        }
    }
}