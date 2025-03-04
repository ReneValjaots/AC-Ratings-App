using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ac.Ratings.Model;
using Ac.Ratings.Services.MainView;
using Ac.Ratings.ViewModel;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MainViewModel _viewModel;
        private CancellationTokenSource? _cancellationTokenSource;

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

        private async void CarList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            try {
                var selectedCar = (Car)CarList.SelectedItem;
                if (selectedCar != null) {
                    await LoadCarImage(selectedCar);
                    DisplayCarStats(selectedCar);
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
                        }, token);

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
            var settingsWindow = new SettingsWindow(this);
            settingsWindow.ShowDialog();
        }

        public void ResetAllRatingsInDatabase() {
            CarDataManager.ResetAllRatingsInDatabase(_viewModel.CarDb);
            UpdateCarListFilter();
        }

        private void CreateBackupOfCarDb() {
            try {
                CarDataManager.CreateBackupOfCarDb(_viewModel.CarDb);
            }
            catch (Exception ex) {
                MessageBox.Show($"Error creating backup: {ex.Message}", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RestoreCarDbFromBackup(string backupFilePath) {
            try {
                var restoredCarDb = CarDataManager.RestoreCarDbFromBackup(backupFilePath);
                if (restoredCarDb != null) {
                    _viewModel.CarDb.Clear();
                    foreach (var car in restoredCarDb) {
                        _viewModel.CarDb.Add(car);
                        CarDataManager.SaveCarToFile(car);
                    }

                    CarList.Items.Refresh();

                    if (CarList.SelectedItem is Car selectedCar) {
                        DataContext = null; 
                        DataContext = selectedCar;
                    }

                    MessageBox.Show("Car database restored successfully.", "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to restore CarDb from backup: {ex.Message}", "Restore Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void ResetAllExtraFeaturesInDatabase() {
            CarDataManager.ResetAllExtraFeaturesInDatabase(_viewModel.CarDb);
            UpdateCarListFilter();
        }
    }
}