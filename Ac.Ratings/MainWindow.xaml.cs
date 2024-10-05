using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ac.Ratings.Model;
using Ac.Ratings.Services;
using Newtonsoft.Json;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private InitializeData _data;
        private static readonly List<string> _gearboxTags = ["manual", "automatic", "semiautomatic", "sequential"];
        private static readonly List<string> _drivetrainTags = ["rwd", "awd", "fwd"];
        private CancellationTokenSource? _cancellationTokenSource;

        public MainWindow() {
            InitializeComponent();
            _data = new InitializeData();
            AuthorFilter.ItemsSource = GetDistinctAuthors();
            AuthorFilter.SelectedIndex = -1;
            ClassFilter.ItemsSource = GetDistinctClasses();
            ClassFilter.SelectedIndex = -1;
            CarList.ItemsSource = _data.CarDb;
        }

        private void CarList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selectedCar = (Car)CarList.SelectedItem;
            if (selectedCar != null) {
                LoadCarImage(selectedCar);
                DisplayCarStats(selectedCar);
                DisplayCarRatings(selectedCar);
                UpdateAverageRating();
            }
        }

        private void DisplayCarRatings(Car selectedCar) {
            HandlingSlider.Value = selectedCar.Ratings.Handling;
            PhysicsSlider.Value = selectedCar.Ratings.Physics;
            RealismSlider.Value = selectedCar.Ratings.Realism;
            SoundSlider.Value = selectedCar.Ratings.Sound;
            VisualsSlider.Value = selectedCar.Ratings.Visuals;
            FunFactorSlider.Value = selectedCar.Ratings.FunFactor;
            ExtraFeaturesSlider.Value = selectedCar.Ratings.ExtraFeatures;
        }

        private void DisplayCarStats(Car selectedCar) {
            Name.Text = selectedCar.Name;
            PowerFigures.Text = selectedCar.Specs.ConvertedPower ?? string.Empty;
            TorqueFigures.Text = selectedCar.Specs.NormalizedTorque ?? string.Empty;
            AccelerationFigures.Text = selectedCar.Specs.NormalizedAcceleration ?? string.Empty;
            TopSpeedFigures.Text = selectedCar.Specs.NormalizedTopSpeed ?? string.Empty;

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
            Year.Text = selectedCar.Year ?? string.Empty;
            Class.Text = className ?? string.Empty;
            Author.Text = selectedCar.Author ?? string.Empty;
            Weight.Text = selectedCar.Specs.NormalizedWeight ?? string.Empty;
            Pwratio.Text = selectedCar.Specs.NormalizedPwRatio ?? string.Empty;
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

                // Load the file paths on a background thread
                if (File.Exists(previewFilePath) && File.Exists(liveryFilePath)) {
                    var liveryUri = new Uri(liveryFilePath, UriKind.Absolute);

                    try {
                        // Load the image in the background with cancellation support
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

                        // Back to the UI thread to add the image control
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

                var jsonContent = JsonConvert.SerializeObject(_data.CarDb, Formatting.Indented);
                File.WriteAllText(_data.CarDbFilePath, jsonContent);
                SaveCarToFile(selectedCar);
            }
        }

        private void SetRatingsFromSliders(Car car) {
            car.Ratings.Handling = HandlingSlider.Value;
            car.Ratings.Physics = PhysicsSlider.Value;
            car.Ratings.Realism = RealismSlider.Value;
            car.Ratings.Sound = SoundSlider.Value;
            car.Ratings.Visuals = VisualsSlider.Value;
            car.Ratings.FunFactor = FunFactorSlider.Value;
            car.Ratings.ExtraFeatures = ExtraFeaturesSlider.Value;
        }

        private void SaveCarToFile(Car car) {
            try {
                if (string.IsNullOrEmpty(_data.CarsRootFolder)) {
                    MessageBox.Show("Cars root folder path is null or empty.");
                    return;
                }

                if (string.IsNullOrEmpty(car.FolderName)) {
                    MessageBox.Show($"Folder name for car {car.Name} is null or empty.");
                    return;
                }

                var carFolderPath = Path.Combine(_data.CarsRootFolder, car.FolderName);
                var carJsonFilePath = Path.Combine(carFolderPath, "RatingsApp", "ui.json");
                var jsonContent = JsonConvert.SerializeObject(car, Formatting.Indented);
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

                var jsonContent = JsonConvert.SerializeObject(_data.CarDb, Formatting.Indented);
                File.WriteAllText(_data.CarDbFilePath, jsonContent);

                UpdateAverageRating();

                SaveCarToFile(selectedCar);
            }
        }

        private static void ResetRatingValues(Car selectedCar) {
            selectedCar.Ratings.Handling = 0;
            selectedCar.Ratings.Physics = 0;
            selectedCar.Ratings.Realism = 0;
            selectedCar.Ratings.Sound = 0;
            selectedCar.Ratings.Visuals = 0;
            selectedCar.Ratings.FunFactor = 0;
            selectedCar.Ratings.ExtraFeatures = 0;
        }

        private void ResetRatingSliderValues() {
            HandlingSlider.Value = 0;
            PhysicsSlider.Value = 0;
            RealismSlider.Value = 0;
            SoundSlider.Value = 0;
            VisualsSlider.Value = 0;
            FunFactorSlider.Value = 0;
            ExtraFeaturesSlider.Value = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) => SaveRatings();

        private void UpdateAverageRating() {
            var selectedCar = (Car)CarList.SelectedItem;
            if (selectedCar != null) {
                var ratings = new List<double> {
                    HandlingSlider.Value,
                    PhysicsSlider.Value,
                    RealismSlider.Value,
                    SoundSlider.Value,
                    VisualsSlider.Value,
                    FunFactorSlider.Value,
                    ExtraFeaturesSlider.Value
                };
                var averageRating = ratings.Average();
                AverageRating.Text = $"Average Rating: {averageRating:F2}";
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdateAverageRating();

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearRatings();

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) {
            CarList.Items.Filter = CombinedFilter;
        }

        private void MenuButton_OnClick(object sender, RoutedEventArgs e) {
            if (CarList.Visibility == Visibility.Visible) {
                CarList.Visibility = Visibility.Collapsed;
                SearchBox.Visibility = Visibility.Collapsed;
            }
            else {
                CarList.Visibility = Visibility.Visible;
                SearchBox.Visibility = Visibility.Visible;
            }
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
                () => car.Ratings.Physics >= PhysicsFilter.Value,
                () => car.Ratings.Handling >= HandlingFilter.Value,
                () => car.Ratings.Realism >= RealismFilter.Value,
                () => car.Ratings.Sound >= SoundFilter.Value,
                () => car.Ratings.Visuals >= VisualsFilter.Value,
                () => car.Ratings.FunFactor >= FunFactorFilter.Value,
                () => car.Ratings.ExtraFeatures >= ExtraFeaturesFilter.Value
            };

            return conditions.All(condition => condition());
        }

        private bool IsMatch(string? value, string? filter) {
            return string.IsNullOrEmpty(filter) ||
                   string.Equals(value, filter, StringComparison.OrdinalIgnoreCase);
        }

        private List<string?> GetDistinctAuthors() {
            var authors = _data.CarDb
                .Select(x => x.Author)
                .Where(author => !string.IsNullOrEmpty(author))
                .Distinct()
                .OrderBy(author => author)
                .ToList();

            authors.Insert(0, "-- Reset --");
            return authors;
        }

        private List<string> GetDistinctClasses() {
            var classes = _data.CarDb
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
            PhysicsFilter.Value = 0;
            HandlingFilter.Value = 0;
            RealismFilter.Value = 0;
            SoundFilter.Value = 0;
            VisualsFilter.Value = 0;
            FunFactorFilter.Value = 0;
            ExtraFeaturesFilter.Value = 0;
        }

        private void AuthorFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (AuthorFilter.SelectedItem?.ToString() == "-- Reset --") {
                AuthorFilter.SelectedIndex = -1; 
                CarList.Items.Filter = CombinedFilter; 
                return;
            }

            CarList.Items.Filter = CombinedFilter;
        }

        private void ClassFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ClassFilter.SelectedItem?.ToString() == "-- Reset --") {
                ClassFilter.SelectedIndex = -1;
                CarList.Items.Filter = CombinedFilter;
                return;
            }

            CarList.Items.Filter = CombinedFilter;
        }

        private void PhysicsFilter_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CarList.Items.Filter = CombinedFilter;
        }

        private void HandlingFilter_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CarList.Items.Filter = CombinedFilter;
        }

        private void RealismFilter_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CarList.Items.Filter = CombinedFilter;
        }

        private void SoundFilter_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CarList.Items.Filter = CombinedFilter;
        }

        private void VisualsFilter_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CarList.Items.Filter = CombinedFilter;
        }

        private void FunFactorFilter_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CarList.Items.Filter = CombinedFilter;
        }

        private void ExtraFeaturesFilter_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CarList.Items.Filter = CombinedFilter;
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e) {
            var settingsWindow = new SettingsWindow(this);
            settingsWindow.ShowDialog();
        }

        public void ResetAllRatingsInDatabase() {
            CreateBackupOfCarDb();

            foreach (var car in _data.CarDb) {
                ResetRatingValues(car);
                if (car.FolderName != null) {
                    var carFolder = Path.Combine(_data.CarsRootFolder, car.FolderName);
                    var carRatingsAppFolder = Path.Combine(carFolder, "RatingsApp");
                    var carJsonPath = Path.Combine(carRatingsAppFolder, "ui.json");

                    if (Directory.Exists(carRatingsAppFolder)) {
                        var jsonContent = JsonConvert.SerializeObject(car, Formatting.Indented);
                        File.WriteAllText(carJsonPath, jsonContent);
                    }
                }
            }

            var dbJsonContent = JsonConvert.SerializeObject(_data.CarDb, Formatting.Indented);
            File.WriteAllText(_data.CarDbFilePath, dbJsonContent);
            CarList.Items.Refresh();

            var previouslySelectedCar = CarList.SelectedItem;
            CarList.SelectedItem = null;
            CarList.Items.Refresh();

            CarList.SelectedItem = previouslySelectedCar;
            CarList.Items.Refresh();
        }

        private void CreateBackupOfCarDb() {
            try {
                string resourcesFolder = Directory.GetParent(Directory.GetParent(_data.CarDbFilePath).FullName).FullName;
                string backupFolder = Path.Combine(resourcesFolder, "Backup");

                if (!Directory.Exists(backupFolder)) {
                    Directory.CreateDirectory(backupFolder);
                }

                string backupFileName = $"CarDb_backup_{DateTime.Now:dd_mm_yyyy_HHmm}.json";
                string backupFilePath = Path.Combine(backupFolder, backupFileName);

                File.Copy(_data.CarDbFilePath, backupFilePath, overwrite: true);

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

            CarList.Items.Filter = CombinedFilter;
        }
    }
}