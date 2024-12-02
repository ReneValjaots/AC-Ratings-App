using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ac.Ratings.Services {
    public class NewInitializer {
        private const string _acRootFolder = @"D:\Steam\steamapps\common\assettocorsa\content\cars";
        public string CarsRootFolder = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\cars\";
        public string CarDbFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\data\CarDb.json";
        private const string _missingDataLogFilePath = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\data\MissingDataLog.txt";
        private const string _backupFolder = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\backup\";

        public readonly JsonSerializerOptions JsonOptions = new() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        public NewInitializer() {
            if (File.Exists(_missingDataLogFilePath))
                File.WriteAllText(_missingDataLogFilePath, string.Empty);
            CreateCarFolders(_acRootFolder);
            InitializeCarData();
        }

        public List<string?> GetAllCarFolderNames(string rootFolder) {
            return Directory.GetDirectories(rootFolder)
                .Select(Path.GetFileName)
                .ToList();
        }

        public void CreateCarFolders(string rootFolder) {
            var carFolders = GetAllCarFolderNames(rootFolder);
            foreach (var carFolder in carFolders) {
                if (carFolder is null) continue;
                var newFolder = Path.Combine(CarsRootFolder, carFolder);
                if (!Directory.Exists(newFolder)) {
                    Directory.CreateDirectory(newFolder);
                }
            }
        }

        public void InitializeCarData() {
            var carFolders = GetAllCarFolderNames(_acRootFolder);
            foreach (var carFolder in carFolders) {
                if (carFolder is null) continue;
                try {
                    ProcessCarFolder(carFolder);
                }
                catch (Exception ex) {
                    LogMissingData($"Error processing car folder {carFolder}: {ex.Message}");
                }
            }
        }

        private void ProcessCarFolder(string carFolder) {
            var ratingsAppFolder = Path.Combine(CarsRootFolder, carFolder, "RatingsApp");
            var uiJsonPath = Path.Combine(ratingsAppFolder, "ui.json");
            var originalCarFolder = Path.Combine(_acRootFolder, carFolder);
            var uiJsonPathInOriginalFolder = Path.Combine(originalCarFolder, "ui", "ui_car.json");

            var backupFolder = Path.Combine(_backupFolder, carFolder);
            var backupUiPath = Path.Combine(backupFolder, "ui_car.json");

            if (!Directory.Exists(ratingsAppFolder)) {
                Directory.CreateDirectory(ratingsAppFolder);
            }

            if (File.Exists(uiJsonPathInOriginalFolder)) {
                if (!Directory.Exists(backupFolder)) {
                    Directory.CreateDirectory(backupFolder);
                }

                File.Copy(uiJsonPathInOriginalFolder, backupUiPath, overwrite: true);
            }
            else {
                LogMissingData($"ui_car.json not found for car: {carFolder}");
            }

            var newCarData = LoadCarDataFromJson(uiJsonPathInOriginalFolder);

            if (newCarData == null) {
                LogMissingData($"Failed to load valid data from {uiJsonPathInOriginalFolder} for car: {carFolder}");
                return;
            }

            if (File.Exists(uiJsonPath)) {
                var existingCarData = LoadCarDataFromJson(uiJsonPath);
                if (existingCarData != null) {
                    newCarData.Ratings ??= new CarRatings();
                    if (existingCarData.Ratings != null) {
                        newCarData.Ratings = existingCarData.Ratings;
                    }

                    if (existingCarData.Data.GearsCount != 0 || !string.IsNullOrEmpty(existingCarData.Data.TractionType)) {
                        newCarData.Data = existingCarData.Data;
                    }
                }
            }

            var fetchedData = GetCarDataFromOriginalFolder(originalCarFolder);
            if (fetchedData != null) {
                newCarData.Data ??= fetchedData;
            }
            else {
                LogMissingData($"Valid data not found for car: {carFolder}");
            }

            newCarData.FolderPath = originalCarFolder;
            newCarData.FolderName = carFolder;

            using (var createStream = new FileStream(uiJsonPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.None)) {
                JsonSerializer.Serialize(createStream, newCarData, JsonOptions);
            }
        }

        private Car? LoadCarDataFromJson(string filePath) {
            if (!File.Exists(filePath)) return null;
            using (var openStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.None)) {
                return JsonSerializer.Deserialize<Car>(openStream);
            }
        }

        private CarData GetCarDataFromOriginalFolder(string originalCarFolder) {
            var drivetrainFilePath = Path.Combine(originalCarFolder, "data", "drivetrain.ini");
            var engineFilePath = Path.Combine(originalCarFolder, "data", "engine.ini");
            var carData = new CarData();

            if (File.Exists(drivetrainFilePath)) {
                var lines = File.ReadAllLines(drivetrainFilePath);
                foreach (var line in lines) {
                    if (line.StartsWith("[TRACTION]")) {
                        carData.TractionType = ParseIniValue(lines, "TYPE");
                    }
                    else if (line.StartsWith("[GEARS]")) {
                        carData.GearsCount = int.Parse(ParseIniValue(lines, "COUNT"));
                    }
                }
            }

            if (File.Exists(engineFilePath)) {
                var lines = File.ReadAllLines(engineFilePath);
                carData.TurboCount = lines.Count(line => line.StartsWith("[TURBO_"));
            }

            return carData;
        }

        private string ParseIniValue(string[] lines, string key) {
            foreach (var line in lines) {
                if (line.StartsWith(key)) {
                    return line.Split('=')[1].Split(';')[0].Trim();
                }
            }

            return string.Empty;
        }

        private void LogMissingData(string message) {
            var directoryPath = Path.GetDirectoryName(_missingDataLogFilePath);
            if (directoryPath != null && !Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            try {
                File.AppendAllText(_missingDataLogFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (IOException ex) {
                Console.WriteLine($"Failed to log missing data: {ex.Message}");
            }
        }
    }

    public class Car {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("brand")] public string? Brand { get; set; }
        [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
        [JsonPropertyName("class")] public string? Class { get; set; }
        [JsonPropertyName("specs")] public CarSpecs Specs { get; set; } = new();
        [JsonPropertyName("torqueCurve")] public List<List<double>>? TorqueCurve { get; set; }
        [JsonPropertyName("powerCurve")] public List<List<double>>? PowerCurve { get; set; }
        [JsonPropertyName("country")] public string? Country { get; set; }
        [JsonPropertyName("year")] public int? Year { get; set; }
        [JsonPropertyName("author")] public string? Author { get; set; }
        [JsonPropertyName("ratings")] public CarRatings Ratings { get; set; } = new();
        [JsonPropertyName("data")] public CarData Data { get; set; } = new();

        [JsonPropertyName("folderPath")]
        public string? FolderPath { get; set; }

        [JsonPropertyName("folderName")]
        public string? FolderName { get; set; }
    }

    public class CarSpecs {
        [JsonPropertyName("bhp")] [JsonConverter(typeof(PowerConverter))]
        public string? Bhp { get; set; }

        [JsonPropertyName("torque")] [JsonConverter(typeof(TorqueConverter))]
        public string? Torque { get; set; }

        [JsonPropertyName("weight")] [JsonConverter(typeof(WeightConverter))]
        public string? Weight { get; set; }

        [JsonPropertyName("topspeed")] [JsonConverter(typeof(TopSpeedConverter))]
        public string? TopSpeed { get; set; }

        [JsonPropertyName("acceleration")] [JsonConverter(typeof(AccelerationConverter))]
        public string? Acceleration { get; set; }

        [JsonPropertyName("pwratio")] public string? PowerToWeightRatio { get; set; }
    }

    public class CarData {
        [JsonPropertyName("tractionType")] public string? TractionType { get; set; }
        [JsonPropertyName("gearsCount")] public int GearsCount { get; set; }
        [JsonPropertyName("supportsShifter")] public bool SupportsShifter { get; set; }
        [JsonPropertyName("turboCount")] public int TurboCount { get; set; }
    }

    public class CarRatings {
        [JsonPropertyName("cornerHandling")] public double CornerHandling { get; set; } = 0.0;
        [JsonPropertyName("brakes")] public double Brakes { get; set; } = 0.0;
        [JsonPropertyName("realism")] public double Realism { get; set; } = 0.0;
        [JsonPropertyName("sound")] public double Sound { get; set; } = 0.0;
        [JsonPropertyName("exteriorQuality")] public double ExteriorQuality { get; set; } = 0.0;
        [JsonPropertyName("interiorQuality")] public double InteriorQuality { get; set; } = 0.0;
        [JsonPropertyName("dashboardQuality")] public double DashboardQuality { get; set; } = 0.0;
        [JsonPropertyName("funFactor")] public double FunFactor { get; set; } = 0.0;

        [JsonPropertyName("forceFeedbackQuality")]
        public double ForceFeedbackQuality { get; set; } = 0.0;

        [JsonPropertyName("extraFeatures")] public double ExtraFeatures { get; set; } = 0.0;
        [JsonPropertyName("averageRating")] public double AverageRating { get; set; } = 0.0;
    }

    public class PowerConverter : JsonConverter<string?> {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var value = reader.GetString();
            return TransformValue(value);
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) {
            writer.WriteStringValue(value);
        }

        protected string? TransformValue(string? value) {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var powerValue = value.Replace(" ", "").ToLower();
            return ConvertPowerString(powerValue);
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
    }

    public class TopSpeedConverter : JsonConverter<string?> {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var value = reader.GetString();
            return TransformValue(value);
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) {
            writer.WriteStringValue(value);
        }

        protected string? TransformValue(string? value) {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var topSpeedValue = value.Replace(" ", "").ToLower();
            return ConvertTopSpeedString(topSpeedValue);
        }

        private string ConvertTopSpeedString(string topSpeed) {
            if (string.IsNullOrWhiteSpace(topSpeed)) {
                return "-";
            }

            topSpeed = Regex.Replace(topSpeed, "[^0-9kphm+]", "");
            var match = Regex.Match(topSpeed, @"(\d+)(\+?)(kmh|kph|mph)");
            if (match.Success) {
                string value = match.Groups[1].Value;
                string hasPlusSymbol = match.Groups[2].Value;
                var unit = match.Groups[3].Value;

                if (unit == "mph") {
                    double mphValue = double.Parse(value);
                    double kmhValue = mphValue * 1.60934;
                    value = kmhValue.ToString("0");
                    unit = "km/h";
                }
                else {
                    unit = "km/h";
                }

                return $"{value}{hasPlusSymbol} {unit}";
            }

            return "-";
        }
    }

    public class TorqueConverter : JsonConverter<string?> {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var value = reader.GetString();
            return TransformValue(value);
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) {
            writer.WriteStringValue(value);
        }

        protected string? TransformValue(string? value) {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var torqueValue = value.Replace(" ", "").ToLower();
            return ConvertTorqueString(torqueValue);
        }

        private string ConvertTorqueString(string torque) {
            torque = Regex.Replace(torque, "[^0-9nm+]", "");
            var match = Regex.Match(torque, @"(\d+)(\+?)nm");
            if (match.Success) {
                string value = match.Groups[1].Value;
                string hasPlusSymbol = match.Groups[2].Value;
                return $"{value}{hasPlusSymbol} Nm";
            }

            return "-";
        }
    }

    public class WeightConverter : JsonConverter<string?> {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var value = reader.GetString();
            return TransformValue(value);
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) {
            writer.WriteStringValue(value);
        }

        protected string? TransformValue(string? value) {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var weightValue = value.Replace(" ", "").ToLower();
            return ConvertWeightString(weightValue);
        }

        private string ConvertWeightString(string weight) {
            if (string.IsNullOrWhiteSpace(weight)) {
                return "-";
            }

            weight = Regex.Replace(weight, "[^0-9kg]", "");
            var match = Regex.Match(weight, @"(\d+)kg");
            if (match.Success) {
                string value = match.Groups[1].Value;
                return $"{value} kg";
            }

            return "-";
        }
    }

    public class AccelerationConverter : JsonConverter<string?> {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var value = reader.GetString();
            return TransformValue(value);
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) {
            writer.WriteStringValue(value);
        }

        protected string? TransformValue(string? value) {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var accelerationValue = value.Replace(" ", "").ToLower();
            return ConvertAccelerationString(accelerationValue);
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
