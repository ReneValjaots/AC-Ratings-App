using System.Text.RegularExpressions;

namespace Ac.Ratings.Services {
    public class PowerConverter {
        public string ConvertedPower { get; private set; }
        public bool IsManufacturerData { get; private set; }
        public int Hp { get; private set; }

        public PowerConverter(string power) {
            var powerValue = power.Replace(" ", "").ToLower();
            IsManufacturerData = IsExpectedPowerFormat(powerValue);
            ConvertedPower = IsManufacturerData ? power : ConvertPowerString(powerValue);
            Hp = GetHorsePower(powerValue);
        }

        private bool IsExpectedPowerFormat(string powerValue) {
            return Regex.IsMatch(powerValue, @"^\d+kw/\d+hp$") || Regex.IsMatch(powerValue, @"^\d+kw$");
        }

        private string ConvertPowerString(string powerValue) {
            var match = Regex.Match(powerValue, @"^(\d+)\+?(bhp|hp)$");
            if (match.Success) {
                var hp = int.Parse(match.Groups[1].Value);
                var kw = (int)Math.Round(hp / 1.36);
                return $"{kw}kW/{hp}hp";
            }
            return "-";
        }

        private int GetHorsePower(string power) {
            var matchKw = Regex.Match(power, @"^(\d+)\+?(kw)");
            var matchHp = Regex.Match(power, @"^(\d+)\+?(hp|bhp)");

            if (matchKw.Success) {
                var kwValue = int.Parse(matchKw.Groups[1].Value);
                return (int)Math.Round(kwValue * 1.36);
            }
            if (matchHp.Success) {
                return int.Parse(matchHp.Groups[1].Value);
            }

            return 0;
        } 
    }
}
