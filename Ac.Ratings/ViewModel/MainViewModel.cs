using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Ac.Ratings.Core;
using Ac.Ratings.Model;
using Ac.Ratings.Services.MainView;

namespace Ac.Ratings.ViewModel {
    public class MainViewModel : ObservableObject {
        private ObservableCollection<Car> _carDb;
        private Car _selectedCar;
        private string _engineStats;
        private string _drivetrainStats;
        private string _gearboxStats;
        private string _classNameFormatted;
        private BitmapImage _carImageSource;
        private ObservableCollection<SkinPreview> _skinPreviews;

        public ObservableCollection<Car> CarDb {
            get => _carDb;
            set {
                _carDb = value;
                OnPropertyChanged();
            }
        }

        public Car SelectedCar {
            get => _selectedCar;
            set {
                _selectedCar = value;
                OnPropertyChanged();
                UpdateCarData();
            }
        }

        public string EngineStats {
            get => _engineStats;
            set {
                _engineStats = value;
                OnPropertyChanged();
            }
        }

        public string DrivetrainStats {
            get => _drivetrainStats;
            set {
                _drivetrainStats = value;
                OnPropertyChanged();
            }
        }

        public string GearboxStats {
            get => _gearboxStats;
            set {
                _gearboxStats = value;
                OnPropertyChanged();
            }
        }

        public string ClassNameFormatted {
            get => _classNameFormatted;
            set {
                _classNameFormatted = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage CarImageSource {
            get => _carImageSource;
            set {
                _carImageSource = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SkinPreview> SkinPreviews {
            get => _skinPreviews;
            set {
                _skinPreviews = value;
                OnPropertyChanged();
            }
        }

        public ICommand ClearRatingsCommand { get; }
        public ICommand ClearExtraFeaturesCommand { get; }
        public ICommand SelectSkinCommand { get; }

        public MainViewModel() {
            try {
                CarDb = new ObservableCollection<Car>(CarDataService.LoadCarDatabase());
                ClearRatingsCommand = new RelayCommand(ClearRatings);
                ClearExtraFeaturesCommand = new RelayCommand(ClearExtraFeatures);
                SelectSkinCommand = new RelayCommand<string>(SelectSkin);
                _skinPreviews = new ObservableCollection<SkinPreview>();
                if (CarDb.Count > 0) {
                    SelectedCar = CarDb[0];
                }
            }
            // ReSharper disable once RedundantCatchClause
            catch (Exception ex) {
                throw; // Re-throw to let the View handle UI notification
            }
        }

        private void ClearRatings() {
            if (SelectedCar != null) {
                CarRatingService.ResetRatingValues(SelectedCar);
            }
        }

        private void ClearExtraFeatures() {
            if (SelectedCar != null) {
                CarRatingService.ResetExtraFeatureValues(SelectedCar);
            }
        }

        private void UpdateCarData() {
            if (SelectedCar != null) {
                EngineStats = CarDisplayService.ShowCarEngineStats(SelectedCar);
                DrivetrainStats = CarDisplayService.ShowCarDriveTrain(SelectedCar);
                GearboxStats = CarDisplayService.ShowCarGearbox(SelectedCar);

                // Format ClassName
                var className = SelectedCar.Class;
                if (!string.IsNullOrEmpty(className)) {
                    if (!className.All(char.IsUpper)) {
                        className = className.ToLower();
                        ClassNameFormatted = char.ToUpper(className[0]) + className[1..];
                    }
                    else {
                        ClassNameFormatted = className;
                    }
                }
                else {
                    ClassNameFormatted = string.Empty;
                }

                LoadSkinPreviews(); 
            }
            else {
                EngineStats = string.Empty;
                DrivetrainStats = string.Empty;
                GearboxStats = string.Empty;
                ClassNameFormatted = string.Empty;
                CarImageSource = null;
                SkinPreviews?.Clear();
            }
        }

        private void LoadSkinPreviews() {
            if (string.IsNullOrEmpty(SelectedCar?.FolderPath)) return;

            var skinsDirectory = Path.Combine(SelectedCar.FolderPath, "skins");
            if (!Directory.Exists(skinsDirectory)) return;

            var skinDirectories = Directory.GetDirectories(skinsDirectory);
            SkinPreviews.Clear();

            foreach (var dir in skinDirectories) {
                var previewFilePath = Path.Combine(dir, "preview.jpg");
                var liveryFilePath = Path.Combine(dir, "livery.png");

                if (File.Exists(previewFilePath) && File.Exists(liveryFilePath)) {
                    try {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(liveryFilePath, UriKind.Absolute);
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        SkinPreviews.Add(new SkinPreview {
                            LiveryImage = bitmapImage,
                            PreviewPath = previewFilePath
                        });
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Failed to load skin: {ex.Message}");
                    }
                }
            }

            if (SkinPreviews.Any()) {
                SelectSkin(SkinPreviews[0].PreviewPath);
            }
            else {
                CarImageSource = null;
            }
        }

        private void SelectSkin(string previewPath) {
            if (File.Exists(previewPath)) {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(previewPath, UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                CarImageSource = image;
            }
        }

        public class SkinPreview {
            public BitmapImage LiveryImage { get; set; }
            public string PreviewPath { get; set; }
        }
    }
}
