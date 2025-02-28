using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ac.Ratings.Services.Converters;

public class WeightConverter : JsonConverter<string?>
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

        var weightValue = value.Replace(" ", "").ToLower();
        return ConvertWeightString(weightValue);
    }

    private string ConvertWeightString(string weight)
    {
        if (string.IsNullOrWhiteSpace(weight))
        {
            return "-";
        }

        weight = Regex.Replace(weight, "[^0-9kg]", "");
        var match = Regex.Match(weight, @"(\d+)kg");
        if (match.Success)
        {
            string value = match.Groups[1].Value;
            return $"{value} kg";
        }

        return "-";
    }
}