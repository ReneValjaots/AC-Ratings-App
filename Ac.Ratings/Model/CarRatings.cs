using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ac.Ratings.Model {
    public class CarRatings : INotifyPropertyChanged {
        private double handling;
        private double physics;
        private double realism;
        private double sound;
        private double visuals;
        private double funFactor;
        private double extraFeatures;

        [JsonProperty("handling")]
        public double Handling {
            get => handling;
            set => SetField(ref handling, value);
        }

        [JsonProperty("physics")]
        public double Physics {
            get => physics;
            set => SetField(ref physics, value);
        }

        [JsonProperty("realism")]
        public double Realism {
            get => realism;
            set => SetField(ref realism, value);
        }

        [JsonProperty("sound")]
        public double Sound {
            get => sound;
            set => SetField(ref sound, value);
        }

        [JsonProperty("visuals")]
        public double Visuals {
            get => visuals;
            set => SetField(ref visuals, value);
        }

        [JsonProperty("funFactor")]
        public double FunFactor {
            get => funFactor;
            set => SetField(ref funFactor, value);
        }

        [JsonProperty("extraFeatures")]
        public double ExtraFeatures {
            get => extraFeatures;
            set => SetField(ref extraFeatures, value);
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
