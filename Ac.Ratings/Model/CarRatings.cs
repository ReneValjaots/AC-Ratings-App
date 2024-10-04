using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ac.Ratings.Model {
    public class CarRatings : INotifyPropertyChanged {
        private double _handling;
        private double _physics;
        private double _realism;
        private double _sound;
        private double _visuals;
        private double _funFactor;
        private double _extraFeatures;
        private double _averageRating;

        [JsonProperty("handling")] public double Handling {
            get => _handling;
            set => SetField(ref _handling, value);
        }

        [JsonProperty("physics")] public double Physics {
            get => _physics;
            set => SetField(ref _physics, value);
        }

        [JsonProperty("realism")] public double Realism {
            get => _realism;
            set => SetField(ref _realism, value);
        }

        [JsonProperty("sound")] public double Sound {
            get => _sound;
            set => SetField(ref _sound, value);
        }

        [JsonProperty("visuals")] public double Visuals {
            get => _visuals;
            set => SetField(ref _visuals, value);
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
