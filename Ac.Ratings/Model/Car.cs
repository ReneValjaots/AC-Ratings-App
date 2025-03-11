using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ac.Ratings.Core;
using Ac.Ratings.Services.MainView;

namespace Ac.Ratings.Model;

public class Car : ObservableObject {
    private CarRatings _ratings = new();
    private CarSpecs _specs;

    [JsonIgnore] public string? Name { get; set; }
    [JsonIgnore] public string? Brand { get; set; }
    [JsonIgnore] public List<string>? Tags { get; set; }
    [JsonIgnore] public string? Class { get; set; }
    [JsonIgnore] public CarSpecs Specs {
        get => _specs;
        set => SetField(ref _specs, value);
    }
    [JsonIgnore] public string? Country { get; set; }
    [JsonIgnore] public int? Year { get; set; }
    [JsonIgnore] public string? Author { get; set; }

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

    public void LoadDisplayProperties() {
        if (string.IsNullOrEmpty(FolderPath)) return;

        var uiCarPath = Path.Combine(FolderPath, "ui", "ui_car.json");
        if (!File.Exists(uiCarPath)) return;

        try {
            var jsonContent = File.ReadAllText(uiCarPath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);

            if (jsonData == null) return;

            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            Name = GetJsonString(root, "name");
            Brand = GetJsonString(root, "brand");
            Tags = GetJsonArray(root, "tags");
            Class = GetJsonString(root, "class");
            Country = GetJsonString(root, "country");
            Year = GetJsonInt(root, "year");
            Author = GetJsonString(root, "author");
        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to read display properties for {FolderPath}: {ex.Message}");
        }
    }

    private static string? GetJsonString(JsonElement root, string key) {
        return root.TryGetProperty(key, out var element) && element.ValueKind == JsonValueKind.String
            ? element.GetString()
            : null;
    }

    private static int? GetJsonInt(JsonElement root, string key) {
        return root.TryGetProperty(key, out var element) && element.ValueKind == JsonValueKind.Number
            ? element.GetInt32()
            : (int?)null;
    }

    private static List<string> GetJsonArray(JsonElement root, string key) {
        if (root.TryGetProperty(key, out var element) && element.ValueKind == JsonValueKind.Array) {
            return element.EnumerateArray().Select(e => e.GetString() ?? "").ToList();
        }

        return new List<string>();
    }
}