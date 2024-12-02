using System.Text.RegularExpressions;

namespace Ac.Ratings.Services {
    //public class WeightConverter {
    //    public string ConvertedWeight { get; private set; }
    //    public int Weight { get; private set; }

    //    public WeightConverter(string weight) {
    //        var weightValue = weight.Replace(" ", "").ToLower();
    //        ConvertedWeight = ConvertWeightString(weightValue);
    //    }

    //    private string ConvertWeightString(string weight) {
    //        if (string.IsNullOrWhiteSpace(weight)) {
    //            return "-";
    //        }

    //        weight = Regex.Replace(weight, "[^0-9kg]", "");
    //        var match = Regex.Match(weight, @"(\d+)kg");
    //        if (match.Success) {
    //            string value = match.Groups[1].Value;
    //            var weightValue = int.Parse(match.Groups[1].Value);
    //            Weight = weightValue;
    //            return $"{value} kg";
    //        }

    //        return "-";
    //    }
    //}
}
