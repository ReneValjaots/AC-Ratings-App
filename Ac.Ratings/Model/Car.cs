using System.ComponentModel;
using System.Text.Json.Serialization;
using Ac.Ratings.Core;
using Ac.Ratings.Services.MainView;

namespace Ac.Ratings.Model;

public class Car : ObservableObject {
    private CarRatings _ratings = new();

    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("brand")] public string? Brand { get; set; }
    [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
    [JsonPropertyName("class")] public string? Class { get; set; }
    [JsonPropertyName("specs")] public CarSpecs Specs { get; set; } = new();
    [JsonPropertyName("country")] public string? Country { get; set; }
    [JsonPropertyName("year")] public int? Year { get; set; }
    [JsonPropertyName("author")] public string? Author { get; set; }

    [JsonPropertyName("ratings")] public CarRatings Ratings {
        get => _ratings;
        set {
            if (SetField(ref _ratings, value)) {
                _ratings.PropertyChanged += Ratings_PropertyChanged;
            }
        }
    }
    [JsonPropertyName("data")] public CarData Data { get; set; } = new();
    [JsonPropertyName("folderPath")] public string? FolderPath { get; set; }
    [JsonPropertyName("folderName")] public string? FolderName { get; set; }

    private void Ratings_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        CarDataManager.MarkCarAsModified(this);
    }
}