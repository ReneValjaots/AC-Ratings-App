using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Ac.Ratings.Model;
using Ac.Ratings.Services;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private InitializeData _data;
        private CreatePreviewFolders _previews;
        public CarData SelectedCar { get; set; }
        public MainWindow() {
            InitializeComponent();
            _data = new InitializeData();
            CarList.ItemsSource = _data.CarDb.Values;
            //_previews = new CreatePreviewFolders();
        }

        private void CarList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selectedCar = (CarData)CarList.SelectedItem;
            if (selectedCar != null) {
                //MessageBox.Show($"You selected: {selectedCar.Name}");
                LoadCarImage(selectedCar);
                DisplayCarStats(selectedCar);
            }
        }

        private void DisplayCarStats(CarData selectedCar) {
            Name.Text = selectedCar.Name;
            Brand.Text = selectedCar.Brand;
            Power.Text = selectedCar.Specs.Bhp;
            Torque.Text = selectedCar.Specs.Torque;
            Weight.Text = selectedCar.Specs.Weight;
            Topspeed.Text = selectedCar.Specs.Topspeed;
            Acceleration.Text = selectedCar.Specs.Acceleration;
            Pwratio.Text = selectedCar.Specs.Pwratio;
        }

        private void LoadCarImage(CarData car) {
            try {
                var key = _data.CarDb.FirstOrDefault(x => x.Value == car).Key;
                string imagePath = $"C:\\Users\\ReneVa\\source\\repos\\Ac.Ratings\\Ac.Ratings\\Resources\\Previews\\{key}-preview.jpg";
                CarImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to load image: {ex.Message}");
            }
        }
    }
}