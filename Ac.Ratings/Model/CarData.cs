using System.Text.Json.Serialization;

namespace Ac.Ratings.Model;

public class CarData
{
    [JsonPropertyName("tractionType")] public string? TractionType { get; set; }
    [JsonPropertyName("gearsCount")] public int GearsCount { get; set; }
    [JsonPropertyName("supportsShifter")] public bool SupportsShifter { get; set; }
    [JsonPropertyName("turboCount")] public int TurboCount { get; set; }
}