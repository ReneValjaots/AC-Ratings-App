using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ac.Ratings.Services.Converters;

public class PowerConverter : JsonConverter<string?>
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

        var powerValue = value.Replace(" ", "").ToLower();
        return ConvertPowerString(powerValue);
    }

    private string ConvertPowerString(string powerValue)
    {
        var matchHp = Regex.Match(powerValue, @"^(\d+)\+?(bhp|hp|whp)$");
        var matchKw = Regex.Match(powerValue, @"^(\d+)\+?kw");
        var matchCv = Regex.Match(powerValue, @"^(\d+)\+?cv$");
        var matchPs = Regex.Match(powerValue, @"^(\d+)\+?ps$");

        if (matchHp.Success)
        {
            var hp = int.Parse(matchHp.Groups[1].Value);
            var kw = (int)Math.Round(hp / 1.36);
            return $"{kw}kW/{hp}hp";
        }

        if (matchKw.Success)
        {
            var kw = int.Parse(matchKw.Groups[1].Value);
            var hp = (int)Math.Round(kw * 1.36);
            return $"{kw}kW/{hp}hp";
        }

        if (matchCv.Success)
        {
            var cv = int.Parse(matchCv.Groups[1].Value);
            var hp = (int)Math.Round(cv * 0.98592);
            var kw = (int)Math.Round(hp / 1.36);
            return $"{kw}kW/{hp}hp";
        }

        if (matchPs.Success)
        {
            var ps = int.Parse(matchPs.Groups[1].Value);
            var hp = (int)Math.Round(ps / 1.0135);
            var kw = (int)Math.Round(hp / 1.36);
            return $"{kw}kW/{hp}hp";
        }

        return "-";
    }
}