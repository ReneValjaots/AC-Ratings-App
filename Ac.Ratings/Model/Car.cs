using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Ac.Ratings.Model {
    public class Car : INotifyPropertyChanged {
        private string? _name;
        private string? _brand;
        private string? _previewFolder;
        private CarSpecs _specs = new();
        private CarRatings _ratings = new();
        private CarData _carData = new();
        private List<string>? _tags;
        private string? _year;
        private string? _author;

        [JsonProperty("name")] public string? Name {
            get => _name;
            set => SetField(ref _name, value);
        }

        [JsonProperty("brand")] public string? Brand {
            get => _brand;
            set => SetField(ref _brand, value);
        }

        [JsonProperty("year")] public string? Year {
            get => _year;
            set => SetField(ref _year, value);
        }

        [JsonProperty("author")] public string? Author {
            get => _author;
            set => SetField(ref _author, value);
        }

        [JsonProperty("previewFolder")] public string? PreviewFolder {
            get => _previewFolder;
            set => SetField(ref _previewFolder, value);
        }

        [JsonProperty("specs")] public CarSpecs Specs {
            get => _specs;
            set => SetField(ref _specs, value);
        }

        [JsonProperty("ratings")] public CarRatings Ratings {
            get => _ratings;
            set => SetField(ref _ratings, value);
        }

        [JsonProperty("data")] public CarData CarData {
            get => _carData;
            set => SetField(ref _carData, value);
        }

        [JsonProperty("tags")] public List<string>? Tags {
            get => _tags;
            set => SetField(ref _tags, value);
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
