using System.IO;
using System.Text.RegularExpressions;

namespace Ac.Ratings.Services {
    public class AccelerationConverter {
        public string ConvertedAcceleration { get; private set; }

        public AccelerationConverter(string acceleration) {
            var accelerationValue = acceleration.Replace(" ", "").ToLower();
            ConvertedAcceleration = ConvertAccelerationString(accelerationValue);
        }

        private string ConvertAccelerationString(string acceleration) {
            if (string.IsNullOrWhiteSpace(acceleration) || !acceleration.Contains("s")) {
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

            if (string.IsNullOrWhiteSpace(acceleration) || acceleration == "s") {
                return "-";
            }

            var match = Regex.Match(acceleration, @"(<*)(\d*\.?\d*)s");

            if (match.Success) {
                string timeValue = match.Groups[2].Value;
                bool hasLessThanSymbol = match.Groups[1].Value.Contains("<");

                if (double.TryParse(timeValue, out double time)) {
                    string formattedTime = time.ToString("0.0");
                    return hasLessThanSymbol ? $"<{formattedTime}s" : $"{formattedTime}s";
                }
            }

            return acceleration;
        }
    }
}
