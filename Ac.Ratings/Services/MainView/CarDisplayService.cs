using System.Text.RegularExpressions;
using Ac.Ratings.Model;

namespace Ac.Ratings.Services.MainView {
    public class CarDisplayService {
        private static readonly List<string> _gearboxTags = ["manual", "automatic", "semiautomatic", "sequential"];
        private static readonly List<string> _drivetrainTags = ["rwd", "awd", "fwd"];

        public static string ShowCarEngineStats(Car selectedCar) {
            var data = GetCarEngineData(selectedCar);
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            var result = string.Empty;
            var parts = data.Split('&');

            if (parts.Length > 0) result = GetDisplacement(result, parts[0]);
            result = AppendInductionSystemToEngineStats(result, selectedCar);
            if (parts.Length > 1) result = GetLayout(result, parts[1]);

            return result.Trim();
        }

        public static string ShowCarDriveTrain(Car selectedCar) {
            var tags = selectedCar.Tags;
            var data = selectedCar.Data.TractionType;

            if (data != null) {
                if (data.Contains("rwd", StringComparison.OrdinalIgnoreCase))
                    return "Rear-wheel drive";
                if (data.Contains("awd", StringComparison.OrdinalIgnoreCase))
                    return "All-wheel drive";
                if (data.Contains("fwd", StringComparison.OrdinalIgnoreCase))
                    return "Front-wheel drive";
            }

            var drivetrainFromSpecificTag = tags?.FirstOrDefault(x => x.Contains("#+"))?.Replace(" ", "").ToLower().Remove(0, 2);
            var drivetrainFromRegularTags = tags?.FirstOrDefault(tag => _drivetrainTags.Contains(tag.ToLower()));
            return drivetrainFromSpecificTag?.ToUpper() ?? drivetrainFromRegularTags?.ToUpper() ?? string.Empty;
        }

        public static string ShowCarGearbox(Car selectedCar) {
            var gearsCount = selectedCar.Data.GearsCount;
            var isManual = selectedCar.Data.SupportsShifter;
            var tags = selectedCar.Tags;
            var gearboxFromSpecificTag = tags?.FirstOrDefault(x => x.Contains("#-"))?.Replace(" ", "").Remove(0, 2);
            var gearboxFromRegularTags = tags?.FirstOrDefault(tag => _gearboxTags.Contains(tag.ToLower()));

            if (gearsCount == 0)
                return gearboxFromSpecificTag ?? gearboxFromRegularTags ?? string.Empty;

            return isManual switch {
                true => $"{gearsCount}-speed manual transmission",
                false => $"{gearsCount}-speed automatic transmission",
            };
        }

        public static string NormalizeClassName(IGrouping<string?, string?> group) {
            var uppercaseName = group.FirstOrDefault(name => name != null && name.All(c => !char.IsLetter(c) || char.IsUpper(c)));

            if (uppercaseName != null)
                return uppercaseName;

            var name = group.FirstOrDefault();

            return name == null ? string.Empty : char.ToUpper(name[0]) + name[1..].ToLower();
        }

        private static string? GetCarEngineData(Car selectedCar) {
            var tags = selectedCar.Tags;
            var engineTag = tags?.FirstOrDefault(x => x.Contains("#!"))?.Replace(" ", "").Remove(0, 2);
            return engineTag;
        }

        private static string GetLayout(string result, string data) {
            if (data.StartsWith("I", StringComparison.OrdinalIgnoreCase))
                result += "inline-" + Regex.Match(data, @"\d+").Value + " engine";

            if (data.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                result += data.ToUpper() + " engine";

            if (data.StartsWith("F", StringComparison.OrdinalIgnoreCase))
                result += "flat-" + Regex.Match(data, @"\d+").Value + " engine";

            if (data.StartsWith("B", StringComparison.OrdinalIgnoreCase))
                result += "boxer-" + Regex.Match(data, @"\d+").Value + " engine";

            if (data.StartsWith("R", StringComparison.OrdinalIgnoreCase))
                result += "rotary engine";

            return result;
        }

        private static string GetDisplacement(string result, string data) {
            if (char.IsDigit(data[0])) {
                var displacementValue = data.Replace("L", "", StringComparison.OrdinalIgnoreCase);
                result += $"{displacementValue}l ";
            }

            return result;
        }

        private static string AppendInductionSystemToEngineStats(string result, Car car) {
            string inductionSystem = ShowInductionSystemForEngineStats(car);
            result += inductionSystem + " ";
            return result;
        }

        private static string ShowInductionSystemForEngineStats(Car car) {
            return car.Data.TurboCount switch {
                1 => "turbocharged",
                2 => "twin turbo",
                _ => "naturally aspirated"
            };
        }
    }
}
