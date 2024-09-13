using System.Text.RegularExpressions;

namespace Ac.Ratings.Services {
    public class PowerConverter {
        public string ConvertedPower { get; private set; }
        public bool IsManufacturerData { get; private set; }
        public int Hp { get; private set; }

        public PowerConverter(string power) {
            var powerValue = power.Replace(" ", "").ToLower();
            ConvertedPower = ConvertPowerString(powerValue);
            Hp = GetHorsePower(powerValue);
        }

        private string ConvertPowerString(string powerValue) {
            var matchHp = Regex.Match(powerValue, @"^(\d+)\+?(bhp|hp)$");
            var matchKw = Regex.Match(powerValue, @"^(\d+)\+?kw");
            var matchCv = Regex.Match(powerValue, @"^(\d+)\+?cv$");
            var matchPs = Regex.Match(powerValue, @"^(\d+)\+?ps$");

            if (matchHp.Success) {
                var hp = int.Parse(matchHp.Groups[1].Value);
                var kw = (int)Math.Round(hp / 1.36);
                return $"{kw}kW/{hp}hp";
            }

            if (matchKw.Success) {
                var kw = int.Parse(matchKw.Groups[1].Value);
                var hp = (int)Math.Round(kw * 1.36);
                return $"{kw}kW/{hp}hp";
            }

            if (matchCv.Success) {
                var cv = int.Parse(matchCv.Groups[1].Value);
                var hp = (int)Math.Round(cv * 0.98592);
                var kw = (int)Math.Round(hp / 1.36);
                return $"{kw}kW/{hp}hp";
            }

            if (matchPs.Success) {
                var ps = int.Parse(matchPs.Groups[1].Value);
                var hp = (int)Math.Round(ps / 1.0135);
                var kw = (int)Math.Round(hp / 1.36);
                return $"{kw}kW/{hp}hp";
            }

            return "-";
        }

        private int GetHorsePower(string power) {
            var matchKw = Regex.Match(power, @"^(\d+)\+?(kw)");
            var matchHp = Regex.Match(power, @"^(\d+)\+?(hp|bhp)");
            var matchCv = Regex.Match(power, @"^(\d+)\+?(cv)");
            var matchPs = Regex.Match(power, @"^(\d+)\+?(ps)");

            if (matchKw.Success) {
                var kwValue = int.Parse(matchKw.Groups[1].Value);
                return (int)Math.Round(kwValue * 1.36);
            }

            if (matchHp.Success) {
                return int.Parse(matchHp.Groups[1].Value);
            }

            if (matchCv.Success) {
                var cvValue = int.Parse(matchCv.Groups[1].Value);
                return (int)Math.Round(cvValue * 0.98592);
            }

            if (matchPs.Success) {
                var psValue = int.Parse(matchPs.Groups[1].Value);
                return (int)Math.Round(psValue * 0.986);
            }

            return 0;
        } 
    }
}
