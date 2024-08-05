using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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

        public MainWindow() {
            InitializeComponent();
            _data = new InitializeData();
            CarList.ItemsSource = _data.CarDb;
        }

        private void CarList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selectedCar = (Car)CarList.SelectedItem;
            if (selectedCar != null) {
                LoadCarImage(selectedCar);
                DisplayCarStats(selectedCar);
                DisplayCarRatings(selectedCar);
                UpdateAverageRating();
                DisplayWarningIcon(selectedCar.Specs.IsManufacturerData);
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
            InductionSystem.Text = ShowInductionSystem(selectedCar);
            Drivetrain.Text = ShowCarDriveTrain(selectedCar);
            Gearbox.Text = ShowCarGearbox(selectedCar);

            var className = selectedCar.Class;
            if (!string.IsNullOrEmpty(className)) {
                className = className.ToLower();
                className = char.ToUpper(className[0]) + className.Substring(1);
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

        private void PopulateSkinGrid(string[] skinDirectories) {
            SkinGrid.Children.Clear();
            SkinGrid.RowDefinitions.Clear();
            SkinGrid.ColumnDefinitions.Clear();

            const int boxSize = 35;
            const int boxesPerRow = 25;
            const int marginSize = 1;

            for (int i = 0; i < boxesPerRow; i++) {
                SkinGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(boxSize + marginSize) });
            }

            for (int i = 0; i < skinDirectories.Length; i++) {
                if (i % boxesPerRow == 0) {
                    SkinGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(boxSize + marginSize) });
                }

                var previewFilePath = Path.Combine(skinDirectories[i], "preview.jpg");
                var liveryFilePath = Path.Combine(skinDirectories[i], "livery.png");

                if (File.Exists(previewFilePath) && File.Exists(liveryFilePath)) {
                    var image = new Image {
                        Width = boxSize,
                        Height = boxSize,
                        Source = new BitmapImage(new Uri(liveryFilePath, UriKind.Absolute)),
                        Stretch = System.Windows.Media.Stretch.UniformToFill,
                    };

                    image.MouseLeftButtonDown += (s, e) => SkinBox_Clicked(previewFilePath);

                    Grid.SetRow(image, i / boxesPerRow);
                    Grid.SetColumn(image, i % boxesPerRow);
                    SkinGrid.Children.Add(image);
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
                File.WriteAllText(_data.carDbFilePath, jsonContent);
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
                string carFolderPath = Path.Combine(_data.carsRootFolder, car.FolderName);
                string carJsonFilePath = Path.Combine(carFolderPath, "car.json");

                // Serialize and save the car object to its car.json file
                var jsonContent = JsonConvert.SerializeObject(car, Formatting.Indented);
                File.WriteAllText(carJsonFilePath, jsonContent);
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to save car ratings to file: {ex.Message}");
            }
        }

        private void ClearRatings() {
            var selectedCar = (Car)CarList.SelectedItem;
            if (selectedCar != null)
            {
                ResetRatingValues(selectedCar);

                ResetSliderValues();

                var jsonContent = JsonConvert.SerializeObject(_data.CarDb, Formatting.Indented);
                File.WriteAllText(_data.carDbFilePath, jsonContent);

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

        private void ResetSliderValues() {
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
            CarList.Items.Filter = FilterCarList;
        }

        private bool FilterCarList(object obj) {
            var car = (Car)obj;
            var searchText = SearchBox.Text.Trim();

            if (searchText.StartsWith("author:", StringComparison.OrdinalIgnoreCase)) {
                var authorSearch = searchText.Substring("author:".Length).Trim();
                return car.Author != null && car.Author.Contains(authorSearch, StringComparison.OrdinalIgnoreCase);
            }

            return car.Name.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase);
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

        private void DisplayWarningIcon(bool isManufacturerData) {
            WarningIcon.Visibility = isManufacturerData ? Visibility.Collapsed : Visibility.Visible;
        }

        private string ShowCarDriveTrain(Car selectedCar) {
            var tags = selectedCar.Tags;
            var data = selectedCar.Data.TractionType;

            if (data != null) {
                if (data.Contains("rwd", StringComparison.OrdinalIgnoreCase))
                    return "RWD";
                if (data.Contains("awd", StringComparison.OrdinalIgnoreCase))
                    return "AWD";
                if (data.Contains("fwd", StringComparison.OrdinalIgnoreCase))
                    return "FWD";
            }

            var drivetrainTag = tags?.FirstOrDefault(x => x.Contains("#+"))?.Replace(" ", "").ToLower().Remove(0, 2);
            return drivetrainTag == null ? string.Empty : drivetrainTag.ToUpper();
        }

        private string ShowCarGearbox(Car selectedCar) {
            var gearsCount = selectedCar.Data.GearsCount;
            var isManual = selectedCar.Data.SupportsShifter;
            var tags = selectedCar.Tags;
            var gearboxFromTags = tags?.FirstOrDefault(x => x.Contains("#-"))?.Replace(" ", "").Remove(0, 2);

            if (gearsCount == 0)
                return gearboxFromTags ?? string.Empty;

            if (isManual)
                return $"{gearsCount}-speed manual";

            if (!isManual)
                return $"{gearsCount}-speed automatic";

            return string.Empty;
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
            
            if (data.StartsWith("R", StringComparison.OrdinalIgnoreCase)) 
                result += "rotary engine";

            return result;
        }


        private string GetDisplacement(string result, string data) {
            if (char.IsDigit(data[0])) {
                var displacementValue = data.Replace("L", "");
                result += $"{displacementValue}-litre ";
            }
            return result;
        }

        private string ShowInductionSystem(Car car) {
            return car.Data.TurboCount switch {
                1 => "Single turbo",
                2 => "Twin turbo",
                _ => "Naturally aspirated"
            };
        }

        //private void AuthorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    CarList.Items.Filter = FilterByAuthor;
        //}

        //private bool FilterByAuthor(object obj) {
        //    var car = (Car)obj;
        //    var authorFilter = AuthorFilter.SelectedItem?.ToString();
        //    return car.Author.Contains(authorFilter, StringComparison.OrdinalIgnoreCase);
        //}
    }
}