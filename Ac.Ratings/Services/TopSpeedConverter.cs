using System.Text.RegularExpressions;

namespace Ac.Ratings.Services {
    //public class TopSpeedConverter {
    //    public string ConvertedTopSpeed { get; private set; }

    //    public TopSpeedConverter(string topSpeed) {
    //        var topSpeedValue = topSpeed.Replace(" ", "").ToLower();
    //        ConvertedTopSpeed = ConvertTopSpeedString(topSpeedValue);
    //    }

    //    private string ConvertTopSpeedString(string topSpeed) {
    //        if (string.IsNullOrWhiteSpace(topSpeed)) {
    //            return "-";
    //        }

    //        topSpeed = Regex.Replace(topSpeed, "[^0-9kphm+]", "");
    //        var match = Regex.Match(topSpeed, @"(\d+)(\+?)(kmh|kph|mph)");
    //        if (match.Success) {
    //            string value = match.Groups[1].Value;
    //            string hasPlusSymbol = match.Groups[2].Value;
    //            var unit = match.Groups[3].Value;

    //            if (unit == "mph") {
    //                double mphValue = double.Parse(value);
    //                double kmhValue = mphValue * 1.60934;
    //                value = kmhValue.ToString("0");
    //                unit = "km/h";
    //            }
    //            else {
    //                unit = "km/h"; 
    //            }
    //            return $"{value}{hasPlusSymbol} {unit}";
    //        }

    //        return "-";
    //    }
    //}
}
