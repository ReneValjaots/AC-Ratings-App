using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ac.Ratings.Services.Converters;

public class PowerConverter : JsonConverter<string?> {
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var value = reader.GetString();
        return TransformValue(value);
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) {
        writer.WriteStringValue(value);
    }

    public string? TransformValue(string? value) {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var powerValue = value.Replace(" ", "").ToLower();
        return ConvertPowerString(powerValue);
    }

    public string? Convert(string? value) {
        if (string.IsNullOrWhiteSpace(value)) return "-";
        var powerValue = value.Replace(" ", "").ToLower();
        return ConvertPowerString(powerValue);
    }

    private Dictionary<string, int> CalculatePower(string powerValue) {
        var matchHp = Regex.Match(powerValue, @"^(\d+)\+?(bhp|hp|whp)$");
        var matchKw = Regex.Match(powerValue, @"^(\d+)\+?kw");
        var matchCv = Regex.Match(powerValue, @"^(\d+)\+?cv$");
        var matchPs = Regex.Match(powerValue, @"^(\d+)\+?ps$");

        var result = new Dictionary<string, int>();

        if (matchHp.Success) {
            var hp = int.Parse(matchHp.Groups[1].Value);
            var kw = (int)Math.Round(hp / 1.359375);
            var ps = (int)Math.Round(hp * 1.013);
            var cv = ps;

            result.Add("hp", hp);
            result.Add("kW", kw);
            result.Add("ps", ps);
            result.Add("cv", cv);
        }

        if (matchKw.Success) {
            var kw = int.Parse(matchKw.Groups[1].Value);
            var hp = (int)Math.Round(kw * 1.359375);
            var ps = (int)Math.Round(kw * 1.36);
            var cv = ps;

            result.Add("hp", hp);
            result.Add("kW", kw);
            result.Add("ps", ps);
            result.Add("cv", cv);
        }

        if (matchCv.Success) {
            var cv = int.Parse(matchCv.Groups[1].Value);
            var hp = (int)Math.Round(cv / 1.013);
            var kw = (int)Math.Round(hp / 1.36);
            var ps = cv;


            result.Add("hp", hp);
            result.Add("kW", kw);
            result.Add("ps", ps);
            result.Add("cv", cv);
        }

        if (matchPs.Success) {
            var ps = int.Parse(matchPs.Groups[1].Value);
            var hp = (int)Math.Round(ps / 1.013);
            var kw = (int)Math.Round(hp / 1.36);
            var cv = ps;

            result.Add("hp", hp);
            result.Add("kW", kw);
            result.Add("ps", ps);
            result.Add("cv", cv);
        }

        return result;
    }

    private string ConvertPowerString(string powerValue) {
        var powerValues = CalculatePower(powerValue);
        if (powerValues.Count == 0)
            return "-";

        string primaryUnit = LoadPowerFormat("PrimaryPowerUnit");
        string secondaryUnit = LoadPowerFormat("SecondaryPowerUnit");

        if (!powerValues.ContainsKey(primaryUnit))
            return "-";

        string formattedPower = $"{powerValues[primaryUnit]}{primaryUnit}";

        if (secondaryUnit != "None" && powerValues.ContainsKey(secondaryUnit)) {
            formattedPower += $"/{powerValues[secondaryUnit]}{secondaryUnit}";
        }

        return formattedPower;
    }


    private static string LoadPowerFormat(string key) {
        var configPath = ConfigManager.ConfigFilePath;
        if (File.Exists(configPath)) {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (config != null && config.TryGetValue(key, out var value)) {
                return value;
            }
        }

        return key == "PrimaryPowerUnit" ? "kW" : "hp"; // Defaults
    }
}