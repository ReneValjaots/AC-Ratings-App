using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ac.Ratings.Model {
    public class CarRatings : INotifyPropertyChanged {
        private double _cornerHandling;
        private double _brakes;
        private double _realism;
        private double _sound;
        private double _exteriorQuality;
        private double _interiorQuality;
        private double _dashboardQuality;
        private double _funFactor;
        private double _forceFeedbackQuality;
        private double _extraFeatures;
        private double _averageRating;

        [JsonProperty("cornerHandling")] public double CornerHandling {
            get => _cornerHandling;
            set => SetField(ref _cornerHandling, value);
        }

        [JsonProperty("brakes")] public double Brakes {
            get => _brakes;
            set => SetField(ref _brakes, value);
        }

        [JsonProperty("realism")] public double Realism {
            get => _realism;
            set => SetField(ref _realism, value);
        }

        [JsonProperty("sound")] public double Sound {
            get => _sound;
            set => SetField(ref _sound, value);
        }

        [JsonProperty("exteriorQuality")] public double ExteriorQuality {
            get => _exteriorQuality;
            set => SetField(ref _exteriorQuality, value);
        }

        [JsonProperty("interiorQuality")] public double InteriorQuality {
            get => _interiorQuality;
            set => SetField(ref _interiorQuality, value);
        }

        [JsonProperty("dashboardQuality")] public double DashboardQuality {
            get => _dashboardQuality;
            set => SetField(ref _dashboardQuality, value);
        }

        [JsonProperty("forceFeedbackQuality")] public double ForceFeedbackQuality {
            get => _forceFeedbackQuality;
            set => SetField(ref _forceFeedbackQuality, value);
        }

        [JsonProperty("funFactor")] public double FunFactor {
            get => _funFactor;
            set => SetField(ref _funFactor, value);
        }

        [JsonProperty("extraFeatures")] public double ExtraFeatures {
            get => _extraFeatures;
            set => SetField(ref _extraFeatures, value);
        }

        [JsonProperty("averageRating")] public double AverageRating {
            get => _averageRating;
            set => SetField(ref _averageRating, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string? propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
