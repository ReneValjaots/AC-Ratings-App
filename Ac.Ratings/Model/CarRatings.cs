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

    // Extra Features
    [JsonPropertyName("turnSignalsDashboard")] public bool TurnSignalsDashboard { get; set; } = false;
    [JsonPropertyName("absOnFlashing")] public bool ABSOnFlashing { get; set; } = false;
    [JsonPropertyName("tcOnFlashing")] public bool TCOnFlashing { get; set; } = false;
    [JsonPropertyName("absOff")] public bool ABSOff { get; set; } = false;
    [JsonPropertyName("tcOff")] public bool TCOff { get; set; } = false;
    [JsonPropertyName("handbrake")] public bool Handbrake { get; set; } = false;
    [JsonPropertyName("lightsDashboard")] public bool LightsDashboard { get; set; } = false;
    [JsonPropertyName("otherDashboard")] public bool OtherDashboard { get; set; } = false;
    [JsonPropertyName("turnSignalsExterior")] public bool TurnSignalsExterior { get; set; } = false;
    [JsonPropertyName("goodQualityLights")] public bool GoodQualityLights { get; set; } = false;
    [JsonPropertyName("emergencyBrakeLights")] public bool EmergencyBrakeLights { get; set; } = false;
    [JsonPropertyName("fogLights")] public bool FogLights { get; set; } = false;
    [JsonPropertyName("sequentialTurnSignals")] public bool SequentialTurnSignals { get; set; } = false;
    [JsonPropertyName("animations")] public bool Animations { get; set; } = false;
    [JsonPropertyName("extendedPhysics")] public bool ExtendedPhysics { get; set; } = false;
    [JsonPropertyName("startupSound")] public bool StartupSound { get; set; } = false;
    [JsonPropertyName("differentDisplays")] public bool DifferentDisplays { get; set; } = false;
    [JsonPropertyName("differentDrivingModes")] public bool DifferentDrivingModes { get; set; } = false;
}