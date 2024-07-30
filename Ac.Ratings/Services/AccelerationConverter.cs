using System.IO;
using System.Text.RegularExpressions;

namespace Ac.Ratings.Services {
    public class AccelerationConverter {
        public string ConvertedAcceleration { get; private set; }

        public AccelerationConverter(string acceleration, string carData) {
            var accelerationValue = acceleration.Replace(" ", "").ToLower();
            ConvertedAcceleration = ConvertAccelerationString(accelerationValue, carData);
        }

        private string ConvertAccelerationString(string acceleration, string carData) {
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

            LogUnconvertedData(carData);
            return acceleration;
        }

        private void LogUnconvertedData(string carData) {
            string logFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Data\AccelerationConverterErrorLog.txt";
            string logEntry = $"{DateTime.Now}: {carData}\n";
            File.AppendAllText(logFilePath, logEntry);
        }

        public static void InitializeLogFile() {
            string logFilePath =
                @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Data\AccelerationConverterErrorLog.txt";
            if (File.Exists(logFilePath)) {
                File.WriteAllText(logFilePath, string.Empty);
            }
        }
    }
}
