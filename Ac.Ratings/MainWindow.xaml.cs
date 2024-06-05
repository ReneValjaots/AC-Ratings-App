using System.IO;
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
            var selectedCar = (CarData)CarList.SelectedItem;
            if (selectedCar != null) {
                LoadCarImage(selectedCar);
                DisplayCarStats(selectedCar);
                DisplayCarRatings(selectedCar);
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

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            SaveRatings();
        }
    }
}