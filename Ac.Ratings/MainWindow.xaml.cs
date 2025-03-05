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
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to load cars: {ex.Message}");
            }
        

            AuthorFilter.ItemsSource = CarDataService.GetDistinctAuthors(_viewModel.CarDb);
            AuthorFilter.SelectedIndex = -1;
            ClassFilter.ItemsSource = CarDataService.GetDistinctClasses(_viewModel.CarDb);
            ClassFilter.SelectedIndex = -1;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateCarListFilter();
        }

        private bool CombinedFilter(object obj) {
            if (obj is not Car car) return false;

            var selectedAuthor = AuthorFilter.SelectedItem?.ToString();
            var selectedClass = ClassFilter.SelectedItem?.ToString();
            var searchText = SearchBox.Text.Trim();

            return CarDataManager.CombinedFilter(
                car,
                selectedAuthor,
                selectedClass,
                searchText,
                CornerHandlingFilter.Value,
                BrakingFilter.Value,
                RealismFilter.Value,
                SoundFilter.Value,
                ExteriorQualityFilter.Value,
                InteriorQualityFilter.Value,
                ForceFeedbackQualityFilter.Value,
                FunFactorFilter.Value,
                AverageRatingFilter.Value
            );
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            CarDataManager.SaveModifiedCars();
            CreateBackupOfCarDb();
        }
    }
}