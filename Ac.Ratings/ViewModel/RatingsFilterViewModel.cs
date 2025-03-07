using Ac.Ratings.Core;
using System.Windows.Input;

namespace Ac.Ratings.ViewModel
{
    public class RatingsFilterViewModel : ObservableObject
    {
        private double _minCornerHandling;
        public double MinCornerHandling {
            get => _minCornerHandling;
            set => SetField(ref _minCornerHandling, value);
        }

        private double _minBraking;
        public double MinBraking {
            get => _minBraking;
            set => SetField(ref _minBraking, value);
        }

        private double _minRealism;
        public double MinRealism {
            get => _minRealism;
            set => SetField(ref _minRealism, value);
        }

        private double _minSound;
        public double MinSound {
            get => _minSound;
            set => SetField(ref _minSound, value);
        }

        private double _minExteriorQuality;
        public double MinExteriorQuality {
            get => _minExteriorQuality;
            set => SetField(ref _minExteriorQuality, value);
        }

        private double _minInteriorQuality;
        public double MinInteriorQuality {
            get => _minInteriorQuality;
            set => SetField(ref _minInteriorQuality, value);
        }

        private double _minForceFeedbackQuality;
        public double MinForceFeedbackQuality {
            get => _minForceFeedbackQuality;
            set => SetField(ref _minForceFeedbackQuality, value);
        }

        private double _minFunFactor;
        public double MinFunFactor {
            get => _minFunFactor;
            set => SetField(ref _minFunFactor, value);
        }

        private double _minAverageRating;
        public double MinAverageRating {
            get => _minAverageRating;
            set => SetField(ref _minAverageRating, value);
        }

        public ICommand ApplyCommand { get; }
        public ICommand ResetCommand { get; }

        public RatingsFilterViewModel() {
            ApplyCommand = new RelayCommand(Apply);
            ResetCommand = new RelayCommand(Reset);
        }

        private void Apply() {
        }

        public void Reset() {
            MinCornerHandling = 0;
            MinBraking = 0;
            MinRealism = 0;
            MinSound = 0;
            MinExteriorQuality = 0;
            MinInteriorQuality = 0;
            MinForceFeedbackQuality = 0;
            MinFunFactor = 0;
            MinAverageRating = 0;
        }
    }
}
