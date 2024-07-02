using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Ac.Ratings.Model {
    public class CarData : INotifyPropertyChanged {
        private string? _tractionType;
        private int _gearsCount;
        private bool _supportsShifter;

        [JsonProperty("tractionType")] public string? TractionType {
            get => _tractionType;
            set => SetField(ref _tractionType, value);
        }

        [JsonProperty("gearsCount")] public int GearsCount {
            get => _gearsCount;
            set => SetField(ref _gearsCount, value);
        }

        [JsonProperty("supportsShifter")] public bool SupportsShifter {
            get => _supportsShifter;
            set => SetField(ref _supportsShifter, value);
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
