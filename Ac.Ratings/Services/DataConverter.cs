using System.Text.RegularExpressions;

namespace Ac.Ratings.Services {
    public class DataConverter {
        private readonly string _powerValue;
        public string ConvertedPower { get; private set; }
        public bool IsManufacturerData { get; private set; }

        public DataConverter(string powerValue) {
            _powerValue = powerValue.Replace(" ", "").ToLower();
            IsManufacturerData = !RequiresConversion(_powerValue);
            ConvertedPower = IsManufacturerData ? _powerValue : ConvertPowerString(_powerValue);
        }

        private bool RequiresConversion(string powerValue) {
            return !Regex.IsMatch(powerValue, @"^\d+kw/\d+hp$");
        }

        private string ConvertPowerString(string powerValue) {
            var match = Regex.Match(powerValue, @"^(\d+)\+?bhp$");
            if (match.Success) {
                var hp = int.Parse(match.Groups[1].Value);
                var kw = (int)Math.Round(hp / 1.36);
                return $"{kw}kW/{hp}hp";
            }
            else {
                return powerValue;
            }
        }
    }
}
