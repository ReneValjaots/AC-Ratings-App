using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ac.Ratings.Services.Converters;

public class TopSpeedConverter : JsonConverter<string?>
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

        var topSpeedValue = value.Replace(" ", "").ToLower();
        return ConvertTopSpeedString(topSpeedValue);
    }

    private string ConvertTopSpeedString(string topSpeed)
    {
        if (string.IsNullOrWhiteSpace(topSpeed))
        {
            return "-";
        }

        topSpeed = Regex.Replace(topSpeed, "[^0-9kphm+]", "");
        var match = Regex.Match(topSpeed, @"(\d+)(\+?)(kmh|kph|mph)");
        if (match.Success)
        {
            string value = match.Groups[1].Value;
            string hasPlusSymbol = match.Groups[2].Value;
            var unit = match.Groups[3].Value;

            if (unit == "mph")
            {
                double mphValue = double.Parse(value);
                double kmhValue = mphValue * 1.60934;
                value = kmhValue.ToString("0");
                unit = "km/h";
            }
            else
            {
                unit = "km/h";
            }

            return $"{value}{hasPlusSymbol} {unit}";
        }

        return "-";
    }
}