using System.Windows;
using Ac.Ratings.Services.MainView;
using Ac.Ratings.ViewModel;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MainViewModel _viewModel;

        public MainWindow() {
            InitializeComponent();
            try {
                _viewModel = new MainViewModel();
                DataContext = _viewModel;
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to load cars: {ex.Message}");
            }
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e) {
            var settingsWindow = new SettingsWindow(_viewModel.CarDb);
            settingsWindow.ShowDialog();
        }

        private void CreateBackupOfCarDb() {
            try {
                CarDataManager.CreateBackupOfCarDb(_viewModel.CarDb);
            }
            catch (Exception ex) {
                MessageBox.Show($"Error creating backup: {ex.Message}", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            CarDataManager.SaveModifiedCars();
            CreateBackupOfCarDb();
        }
    }
}