using System.Text.Json.Serialization;

namespace Ac.Ratings.Model;

public class Car
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("brand")] public string? Brand { get; set; }
    [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
    [JsonPropertyName("class")] public string? Class { get; set; }
    [JsonPropertyName("specs")] public CarSpecs Specs { get; set; } = new();
    [JsonPropertyName("torqueCurve")] public List<List<double>>? TorqueCurve { get; set; }
    [JsonPropertyName("powerCurve")] public List<List<double>>? PowerCurve { get; set; }
    [JsonPropertyName("country")] public string? Country { get; set; }
    [JsonPropertyName("year")] public int? Year { get; set; }
    [JsonPropertyName("author")] public string? Author { get; set; }
    [JsonPropertyName("ratings")] public CarRatings Ratings { get; set; } = new();
    [JsonPropertyName("data")] public CarData Data { get; set; } = new();

    [JsonPropertyName("folderPath")]
    public string? FolderPath { get; set; }

    [JsonPropertyName("folderName")]
    public string? FolderName { get; set; }
}