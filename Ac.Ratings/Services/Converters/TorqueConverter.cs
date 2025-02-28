using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ac.Ratings.Services.Converters;

public class TorqueConverter : JsonConverter<string?>
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

    public string? TransformValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var torqueValue = value.Replace(" ", "").ToLower();
        return ConvertTorqueString(torqueValue);
    }

    private string ConvertTorqueString(string torque)
    {
        torque = Regex.Replace(torque, "[^0-9nm+]", "");
        var match = Regex.Match(torque, @"(\d+)(\+?)nm");
        if (match.Success)
        {
            string value = match.Groups[1].Value;
            string hasPlusSymbol = match.Groups[2].Value;
            return $"{value}{hasPlusSymbol} Nm";
        }

        return "-";
    }
}