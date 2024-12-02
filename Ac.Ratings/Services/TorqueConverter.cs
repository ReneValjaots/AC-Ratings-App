using System.Text.RegularExpressions;

namespace Ac.Ratings.Services {
    //public class TorqueConverter {
    //    public string ConvertedTorque { get; private set; }

    //    public TorqueConverter(string torque) {
    //        var torqueValue = torque.Replace(" ", "").ToLower();
    //        ConvertedTorque = ConvertTorqueString(torqueValue);
    //    }

    //    private string ConvertTorqueString(string torque) {
    //        torque = Regex.Replace(torque, "[^0-9nm+]", "");
    //        var match = Regex.Match(torque, @"(\d+)(\+?)nm");
    //        if (match.Success) {
    //            string value = match.Groups[1].Value;
    //            string hasPlusSymbol = match.Groups[2].Value;
    //            return $"{value}{hasPlusSymbol} Nm";
    //        }
    //        return "-";
    //    }
    //}
}
