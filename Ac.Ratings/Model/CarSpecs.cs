using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ac.Ratings.Model {
    public class CarSpecs : INotifyPropertyChanged {
        private string? bhp;
        private string? torque;
        private string? weight;
        private string? topspeed;
        private string? acceleration;
        private string? pwratio;

        [JsonProperty("bhp")]
        public string? Bhp {
            get => bhp;
            set => SetField(ref bhp, value);
        }

        [JsonProperty("torque")]
        public string? Torque {
            get => torque;
            set => SetField(ref torque, value);
        }

        [JsonProperty("weight")]
        public string? Weight {
            get => weight;
            set => SetField(ref weight, value);
        }

        [JsonProperty("topspeed")]
        public string? Topspeed {
            get => topspeed;
            set => SetField(ref topspeed, value);
        }

        [JsonProperty("acceleration")]
        public string? Acceleration {
            get => acceleration;
            set => SetField(ref acceleration, value);
        }

        [JsonProperty("pwratio")]
        public string? Pwratio {
            get => pwratio;
            set => SetField(ref pwratio, value);
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
