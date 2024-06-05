using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ac.Ratings.Model {
    public class CarSpecs : INotifyPropertyChanged {
        private string? _bhp;
        private string? _torque;
        private string? _weight;
        private string? _topspeed;
        private string? _acceleration;
        private string? _pwratio;

        [JsonProperty("bhp")]
        public string? Bhp {
            get => _bhp;
            set => SetField(ref _bhp, value);
        }

        [JsonProperty("torque")]
        public string? Torque {
            get => _torque;
            set => SetField(ref _torque, value);
        }

        [JsonProperty("weight")]
        public string? Weight {
            get => _weight;
            set => SetField(ref _weight, value);
        }

        [JsonProperty("topspeed")]
        public string? Topspeed {
            get => _topspeed;
            set => SetField(ref _topspeed, value);
        }

        [JsonProperty("acceleration")]
        public string? Acceleration {
            get => _acceleration;
            set => SetField(ref _acceleration, value);
        }

        [JsonProperty("pwratio")]
        public string? Pwratio {
            get => _pwratio;
            set => SetField(ref _pwratio, value);
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
