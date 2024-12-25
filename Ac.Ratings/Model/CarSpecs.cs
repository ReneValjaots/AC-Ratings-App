using System.Text.Json.Serialization;
using Ac.Ratings.Services;

namespace Ac.Ratings.Model;

public class CarSpecs {
    [JsonPropertyName("bhp")] [JsonConverter(typeof(PowerConverter))]
    public string? Bhp { get; set; }

    [JsonPropertyName("torque")] [JsonConverter(typeof(TorqueConverter))]
    public string? Torque { get; set; }

    [JsonPropertyName("weight")] [JsonConverter(typeof(WeightConverter))]
    public string? Weight { get; set; }

    [JsonPropertyName("topspeed")] [JsonConverter(typeof(TopSpeedConverter))]
    public string? TopSpeed { get; set; }

    [JsonPropertyName("acceleration")] [JsonConverter(typeof(AccelerationConverter))]
    public string? Acceleration { get; set; }

    [JsonPropertyName("pwratio")] public string? PowerToWeightRatio { get; set; }
}