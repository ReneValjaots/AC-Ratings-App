using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Ac.Ratings.Model {
    public class CarData : INotifyPropertyChanged {
        private string? _name;
        private string? _brand;
        private string? _previewFolder;
        private CarSpecs _specs = new();
        private CarRatings _ratings = new();

        [JsonProperty("name")]
        public string? Name {
            get => _name;
            set => SetField(ref _name, value);
        }

        [JsonProperty("brand")]
        public string? Brand {
            get => _brand;
            set => SetField(ref _brand, value);
        }

        [JsonProperty("previewFolder")]
        public string? PreviewFolder {
            get => _previewFolder;
            set => SetField(ref _previewFolder, value);
        }

        [JsonProperty("specs")]
        public CarSpecs Specs {
            get => _specs;
            set => SetField(ref _specs, value);
        }

        [JsonProperty("ratings")]
        public CarRatings Ratings {
            get => _ratings;
            set => SetField(ref _ratings, value);
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

        public static CarData FromJson(string json) {
            return JsonConvert.DeserializeObject<CarData>(json);
        }

        public string ToJson() {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
