using System.Text.RegularExpressions;

namespace Ac.Ratings.Services {
    public class PowerConverter {
        private readonly string _powerValue;
        public string ConvertedPower { get; private set; }
        public bool IsManufacturerData { get; private set; }

        public PowerConverter(string powerValue) {
            _powerValue = powerValue.Replace(" ", "").ToLower();
            IsManufacturerData = IsExpectedPowerFormat(_powerValue);
            ConvertedPower = IsManufacturerData ? powerValue : ConvertPowerString(_powerValue);
        }

        private bool IsExpectedPowerFormat(string powerValue) {
            return Regex.IsMatch(powerValue, @"^\d+kw/\d+hp$") || Regex.IsMatch(powerValue, @"^\d+kw$");
        }

        private string ConvertPowerString(string powerValue) {
            var match = Regex.Match(powerValue, @"^(\d+)\+?bhp$");
            if (match.Success) {
                var hp = int.Parse(match.Groups[1].Value);
                var kw = (int)Math.Round(hp / 1.36);
                return $"{kw}kW/{hp}hp";
            }
            return powerValue;
        }
    }
}
