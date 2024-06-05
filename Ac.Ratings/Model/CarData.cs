using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Ac.Ratings.Model {
    public class CarData : INotifyPropertyChanged {
        private string? id;
        private string? name;
        private string? brand;
        private string? previewFolder;
        private CarSpecs specs = new();
        private CarRatings ratings = new();

        [JsonProperty("name")]
        public string? Name {
            get => name;
            set => SetField(ref name, value);
        }

        [JsonProperty("brand")]
        public string? Brand {
            get => brand;
            set => SetField(ref brand, value);
        }

        [JsonProperty("previewFolder")]
        public string? PreviewFolder {
            get => previewFolder;
            set => SetField(ref previewFolder, value);
        }

        [JsonProperty("specs")]
        public CarSpecs Specs {
            get => specs;
            set => SetField(ref specs, value);
        }

        [JsonProperty("ratings")]
        public CarRatings Ratings {
            get => ratings;
            set => SetField(ref ratings, value);
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
