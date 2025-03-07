using System.Windows;
using System.Windows.Controls;
using Ac.Ratings.Model;
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
                _viewModel.RatingsFilterApplied += ViewModel_RatingsFilterApplied;
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to load cars: {ex.Message}");
            }
        }

        private void ViewModel_RatingsFilterApplied(object sender, EventArgs e) {
            UpdateCarListFilter(); // Refresh filter when ratings change
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateCarListFilter();
        }

        private bool CombinedFilter(object obj) {
            if (obj is not Car car) return false;

            var selectedAuthor = _viewModel.SelectedAuthor;
            var selectedClass = _viewModel.SelectedClass;
            var searchText = SearchBox.Text.Trim();

            return CarDataManager.CombinedFilter(
                car,
                selectedAuthor,
                selectedClass,
                searchText,
                _viewModel.RatingsFilterViewModel.MinCornerHandling,
                _viewModel.RatingsFilterViewModel.MinBraking,
                _viewModel.RatingsFilterViewModel.MinRealism,
                _viewModel.RatingsFilterViewModel.MinSound,
                _viewModel.RatingsFilterViewModel.MinExteriorQuality,
                _viewModel.RatingsFilterViewModel.MinInteriorQuality,
                _viewModel.RatingsFilterViewModel.MinForceFeedbackQuality,
                _viewModel.RatingsFilterViewModel.MinFunFactor,
                _viewModel.RatingsFilterViewModel.MinAverageRating
            );
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e) {
            _viewModel.SelectedAuthor = null; 
            AuthorFilter.Text = "Filter by author";
            _viewModel.SelectedClass = null; 
            ClassFilter.SelectedItem = null;
            SearchBox.Text = string.Empty;
            _viewModel.RatingsFilterViewModel.Reset();
            CarList.Items.Filter = null;
        }

        private void AuthorFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_viewModel.SelectedAuthor == "-- Reset --") {
                _viewModel.SelectedAuthor = null;
                UpdateCarListFilter();
                return;
            }

            UpdateCarListFilter();
        }

        private void ClassFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_viewModel.SelectedClass == "-- Reset --") {
                _viewModel.SelectedClass = null;
                UpdateCarListFilter();
                return;
            }

            UpdateCarListFilter();
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

        private void UpdateCarListFilter() {
            CarList.Items.Filter = CombinedFilter;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            CarDataManager.SaveModifiedCars();
            CreateBackupOfCarDb();
        }
    }
}