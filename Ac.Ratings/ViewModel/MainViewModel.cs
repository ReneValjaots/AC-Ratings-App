using System.Collections.ObjectModel;
using System.Windows.Input;
using Ac.Ratings.Core;
using Ac.Ratings.Model;
using Ac.Ratings.Services.MainView;

namespace Ac.Ratings.ViewModel {
    public class MainViewModel : ObservableObject {
        private ObservableCollection<Car> _carDb;
        private Car _selectedCar;

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
                //LoadCarImageAsync(value); // Move async logic here
            }
        }

        public ICommand ClearRatingsCommand { get; }
        public ICommand ClearExtraFeaturesCommand { get; }

        public MainViewModel() {
            try {
                CarDb = new ObservableCollection<Car>(CarDataService.LoadCarDatabase());
                if (CarDb.Count > 0) {
                    SelectedCar = CarDb[0];
                }
            }
            // ReSharper disable once RedundantCatchClause
            catch (Exception ex) {
                throw; // Re-throw to let the View handle UI notification
            }
            ClearRatingsCommand = new RelayCommand(ClearRatings);
            ClearExtraFeaturesCommand = new RelayCommand(ClearExtraFeatures);

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
    }
}
