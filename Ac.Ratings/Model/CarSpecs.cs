using Ac.Ratings.Services;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ac.Ratings.Model {
    public class CarSpecs : INotifyPropertyChanged {
        private string? _bhp;
        private string? _torque;
        private string? _weight;
        private string? _topspeed;
        private string? _acceleration;
        private string? _pwratio;
        private string? _convertedPower;
        private bool _isManufacturerData;
        private string? _normalizedAcceleration;

        [JsonProperty("convertedPower")] public string? ConvertedPower {
            get => _convertedPower;
            internal set => SetField(ref _convertedPower, value);
        }
        [JsonProperty("isManufacturerData")] public bool IsManufacturerData {
            get => _isManufacturerData;
            internal set => SetField(ref _isManufacturerData, value);
        }

        [JsonProperty("normalizedAcceleration")] public string? NormalizedAcceleration {
            get => _normalizedAcceleration;
            private set => SetField(ref _normalizedAcceleration, value);
        }

        [JsonProperty("bhp")] public string? Bhp {
            get => _bhp;
            set {
                if (SetField(ref _bhp, value)) {
                    if (value != null) {
                        var converter = new PowerConverter(value);
                        ConvertedPower = converter.ConvertedPower;
                        IsManufacturerData = converter.IsManufacturerData;
                    }
                }
            }
        }

        [JsonProperty("torque")] public string? Torque {
            get => _torque;
            set => SetField(ref _torque, value);
        }

        [JsonProperty("weight")] public string? Weight {
            get => _weight;
            set => SetField(ref _weight, value);
        }

        [JsonProperty("topspeed")] public string? Topspeed {
            get => _topspeed;
            set => SetField(ref _topspeed, value);
        }

        [JsonProperty("acceleration")] public string? Acceleration {
            get => _acceleration;
            set {
                if (SetField(ref _acceleration, value)) {
                    if (value != null) {
                        var converter = new AccelerationConverter(value, GetCarData());
                        NormalizedAcceleration = converter.ConvertedAcceleration;
                    }
                }
            }
        }

        [JsonProperty("pwratio")] public string? Pwratio {
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

        private string GetCarData() {
            var sb = new StringBuilder();
            sb.AppendLine($"\"bhp\": \"{Bhp}\",");
            sb.AppendLine($"\"torque\": \"{Torque}\",");
            sb.AppendLine($"\"weight\": \"{Weight}\",");
            sb.AppendLine($"\"topspeed\": \"{Topspeed}\",");
            sb.AppendLine($"\"acceleration\": \"{Acceleration}\",");
            sb.AppendLine($"\"pwratio\": \"{Pwratio}\"");
            return sb.ToString();
        }
    }
}
