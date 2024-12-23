using System.Text.Json.Serialization;

namespace Ac.Ratings.Model;

public class CarRatings
{
    [JsonPropertyName("cornerHandling")] public double CornerHandling { get; set; } = 0.0;
    [JsonPropertyName("brakes")] public double Brakes { get; set; } = 0.0;
    [JsonPropertyName("realism")] public double Realism { get; set; } = 0.0;
    [JsonPropertyName("sound")] public double Sound { get; set; } = 0.0;
    [JsonPropertyName("exteriorQuality")] public double ExteriorQuality { get; set; } = 0.0;
    [JsonPropertyName("interiorQuality")] public double InteriorQuality { get; set; } = 0.0;
    [JsonPropertyName("dashboardQuality")] public double DashboardQuality { get; set; } = 0.0;
    [JsonPropertyName("funFactor")] public double FunFactor { get; set; } = 0.0;
    [JsonPropertyName("forceFeedbackQuality")] public double ForceFeedbackQuality { get; set; } = 0.0;
    [JsonPropertyName("averageRating")] public double AverageRating { get; set; } = 0.0;
}