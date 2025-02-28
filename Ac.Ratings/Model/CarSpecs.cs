using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ac.Ratings.Core;
using Ac.Ratings.Services.Converters;

namespace Ac.Ratings.Model;

public class CarSpecs : ObservableObject{
    private readonly string? _folderPath;

    public CarSpecs(string? folderPath) {
        _folderPath = folderPath;
    }

    public string? Bhp => GetConvertedValue("bhp", new PowerConverter());
    public string? Torque => GetConvertedValue("torque", new TorqueConverter());
    public string? Weight => GetConvertedValue("weight", new WeightConverter());
    public string? TopSpeed => GetConvertedValue("topspeed", new TopSpeedConverter());
    public string? Acceleration => GetConvertedValue("acceleration", new AccelerationConverter());
    public string? PowerToWeightRatio => GetRawValue("pwratio"); // No conversion needed

    private string? GetConvertedValue(string key, JsonConverter<string?> converter) {
        var rawValue = GetRawValue(key);
        return rawValue != null ? ((dynamic)converter).TransformValue(rawValue) : "-";
    }

    private string? GetRawValue(string key) {
        if (string.IsNullOrEmpty(_folderPath)) return "-";

        var uiCarPath = Path.Combine(_folderPath, "ui", "ui_car.json");
        if (!File.Exists(uiCarPath)) return "-";

        var json = File.ReadAllText(uiCarPath);
        var carData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        if (carData != null && carData.TryGetValue("specs", out var specsObj) &&
            specsObj is JsonElement specsElement && specsElement.TryGetProperty(key, out var valueElement)) {
            return valueElement.GetString();
        }

        return "-";
    }
}