using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        private CollectionViewSource _carCollectionView;

        public MainWindow() {
            InitializeComponent();
            _data = new InitializeData();
            CarList.ItemsSource = _data.CarDb;
        }

        private void CarList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selectedCar = (CarData)CarList.SelectedItem;
            if (selectedCar != null) {
                LoadCarImage(selectedCar);
                DisplayCarStats(selectedCar);
                DisplayCarRatings(selectedCar);
                UpdateAverageRating();
            }
        }

        private void DisplayCarRatings(CarData selectedCar) {
            HandlingSlider.Value = selectedCar.Ratings.Handling;
            PhysicsSlider.Value = selectedCar.Ratings.Physics;
            RealismSlider.Value = selectedCar.Ratings.Realism;
            SoundSlider.Value = selectedCar.Ratings.Sound;
            VisualsSlider.Value = selectedCar.Ratings.Visuals;
            FunFactorSlider.Value = selectedCar.Ratings.FunFactor;
            ExtraFeaturesSlider.Value = selectedCar.Ratings.ExtraFeatures;
        }

        private void DisplayCarStats(CarData selectedCar) {
            Name.Text = selectedCar.Name;
            PowerFigures.Text = selectedCar.Specs.ConvertedPower ?? string.Empty;
            TorqueFigures.Text = selectedCar.Specs.Torque ?? string.Empty;
            AccelerationFigures.Text = selectedCar.Specs.Acceleration ?? string.Empty;
            TopSpeedFigures.Text = selectedCar.Specs.Topspeed ?? string.Empty;

            Brand.Text = selectedCar.Brand ?? string.Empty;
            Power.Text = selectedCar.Specs.Bhp ?? string.Empty;
            Torque.Text = selectedCar.Specs.Torque ?? string.Empty;
            Weight.Text = selectedCar.Specs.Weight ?? string.Empty;
            Topspeed.Text = selectedCar.Specs.Topspeed ?? string.Empty;
            Acceleration.Text = selectedCar.Specs.Acceleration ?? string.Empty;
            Pwratio.Text = selectedCar.Specs.Pwratio ?? string.Empty;
        }

        private void LoadCarImage(CarData car) {
            try {
                if (!string.IsNullOrEmpty(car.PreviewFolder) && File.Exists(car.PreviewFolder)) {
                    CarImage.Source = new BitmapImage(new Uri(car.PreviewFolder, UriKind.Absolute));
                }
                else {
                    MessageBox.Show($"Preview image not found for {car.Name}");
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to load image: {ex.Message}");
            }
        }

        private void SaveRatings() {
            var selectedCar = (CarData)CarList.SelectedItem;
            if (selectedCar != null) {
                selectedCar.Ratings.Handling = HandlingSlider.Value;
                selectedCar.Ratings.Physics = PhysicsSlider.Value;
                selectedCar.Ratings.Realism = RealismSlider.Value;
                selectedCar.Ratings.Sound = SoundSlider.Value;
                selectedCar.Ratings.Visuals = VisualsSlider.Value;
                selectedCar.Ratings.FunFactor = FunFactorSlider.Value;
                selectedCar.Ratings.ExtraFeatures = ExtraFeaturesSlider.Value;

                var jsonContent = JsonConvert.SerializeObject(_data.CarDb, Formatting.Indented);
                File.WriteAllText(_data.carDbFilePath, jsonContent);
            }
        }

        private void ClearRatings() {
            var selectedCar = (CarData)CarList.SelectedItem;
            if (selectedCar != null) {
                selectedCar.Ratings.Handling = 0;
                selectedCar.Ratings.Physics = 0;
                selectedCar.Ratings.Realism = 0;
                selectedCar.Ratings.Sound = 0;
                selectedCar.Ratings.Visuals = 0;
                selectedCar.Ratings.FunFactor = 0;  
                selectedCar.Ratings.ExtraFeatures = 0;

                HandlingSlider.Value = 0;
                PhysicsSlider.Value = 0;
                RealismSlider.Value = 0;
                SoundSlider.Value = 0;
                VisualsSlider.Value = 0;
                FunFactorSlider.Value = 0;
                ExtraFeaturesSlider.Value = 0;

                var jsonContent = JsonConvert.SerializeObject(_data.CarDb, Formatting.Indented);
                File.WriteAllText(_data.carDbFilePath, jsonContent);

                UpdateAverageRating();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) => SaveRatings();

        private void UpdateAverageRating() {
            var selectedCar = (CarData)CarList.SelectedItem;
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
            var car = (CarData)obj;
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
    }
}