using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ac.Ratings.Services.Converters;

public class AccelerationConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return TransformValue(value);
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }

    protected string? TransformValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var accelerationValue = value.Replace(" ", "").ToLower();
        return ConvertAccelerationString(accelerationValue);
    }

    private string ConvertAccelerationString(string acceleration)
    {
        if (string.IsNullOrWhiteSpace(acceleration) || !acceleration.Contains("s"))
        {
            return "-";
        }

        acceleration = acceleration.Replace("0-100", "")
            .Replace("/", "")
            .Replace("--", "")
            .Replace("kph", "")
            .Replace("kmh", "")
            .Replace("in", "")
            .Replace("-", "")
            .Trim();

        if (string.IsNullOrWhiteSpace(acceleration) || acceleration == "s")
        {
            return "-";
        }

        var match = Regex.Match(acceleration, @"(<*)(\d*\.?\d*)s");

        if (match.Success)
        {
            string timeValue = match.Groups[2].Value;
            bool hasLessThanSymbol = match.Groups[1].Value.Contains("<");

            if (double.TryParse(timeValue, out double time))
            {
                string formattedTime = time.ToString("0.0");
                return hasLessThanSymbol ? $"<{formattedTime}s" : $"{formattedTime}s";
            }
        }

        return acceleration;
    }
}