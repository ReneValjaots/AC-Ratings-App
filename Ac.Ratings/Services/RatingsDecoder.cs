using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Ac.Ratings.Model;
using Ac.Ratings.Services.Acd;

namespace Ac.Ratings.Services {
    public class RatingsDecoder {
        private const byte _deflateFlag = 0;
        private const byte _lzfFlag = 11;

        private Dictionary<string, double> _ratingDb = new Dictionary<string, double>();

        public void InitializeRatingsDataFile() {
            try {
                string? originalRatingsPath = ConfigManager.OriginalRatingsPath;
                if (originalRatingsPath == null) {
                    Console.WriteLine("Original ratings file not found.");
                    return;
                }

                byte[] inputData = File.ReadAllBytes(originalRatingsPath);

                byte[] decompressedData = DecodeBytes(inputData);

                ProcessDecompressedData(decompressedData);

                File.WriteAllBytes(ConfigManager.ModifiedRatingsPath, decompressedData);
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static byte[] DecodeBytes(byte[] bytes) {
            if (bytes[0] == _lzfFlag) {
                // LZF decompression
                return Lzf.Decompress(bytes, 1, bytes.Length - 1);
            }

            bool deflateMode = bytes[0] == _deflateFlag;
            if (!deflateMode && !bytes.Any(x => x < 0x20 && x != '\t' && x != '\n' && x != '\r')) {
                // Plain text or already decompressed
                return bytes;
            }

            using (var inputStream = new MemoryStream(bytes)) {
                if (deflateMode) {
                    inputStream.Seek(1, SeekOrigin.Begin); // Skip the flag byte
                }

                using (var gzip = new DeflateStream(inputStream, CompressionMode.Decompress))
                    using (var memory = new MemoryStream()) {
                        gzip.CopyTo(memory);
                        return memory.ToArray();
                    }
            }
        }

        private void ProcessDecompressedData(byte[] decompressedData) {
            string data = Encoding.UTF8.GetString(decompressedData);
            var lines = data.Split("\n");

            foreach (var line in lines) {
                if (line.Contains("CarObject")) {
                    var parts = line.Split(":");
                    var folderName = parts[1]; 
                    var rating = double.Parse(parts[2].Split("\t")[1]); 
                    _ratingDb[folderName] = rating;
                }
            }
        }

        public void InitializeUserRatings() {
            string carsRootFolder = ConfigManager.CarsRootFolder;
            if (!Directory.Exists(carsRootFolder)) {
                return;
            }

            foreach (var directory in Directory.GetDirectories(carsRootFolder)) {
                string uiPath = Path.Combine(directory, "RatingsApp", "ui.json");

                if (!File.Exists(uiPath)) {
                    continue;
                }

                try {
                    string jsonContent = File.ReadAllText(uiPath);
                    var car = JsonSerializer.Deserialize<Car>(jsonContent);

                    if (car == null || car.Ratings.AverageRating == 0) continue;
                    var folderName = car.FolderName ?? Path.GetFileName(directory);
                    var averageRating = car.Ratings.AverageRating;
                    var formattedRating = Math.Round(averageRating / 2 * 2) / 2;
                    _ratingDb[folderName] = formattedRating;
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error processing car in directory '{directory}': {ex.Message}");
                }
            }
        }

        public string CreateOutputString() {
            string outputString = string.Empty;
            outputString += "version: 2";

            foreach (var kvp in _ratingDb) {
                outputString += $"\nCarObject:{kvp.Key}:rating\t{kvp.Value}";
            }

            return outputString;
        }

        private static byte[] OutputStringToByteArray(string input) {
            return Encoding.UTF8.GetBytes(input);
        }

        private byte[] CompressData() {
            byte[] data = OutputStringToByteArray(CreateOutputString());
            byte[] compressedData = new byte[data.Length + (data.Length / 4) + 16];
            int compressedSize = Lzf.Compress(data, data.Length, compressedData, compressedData.Length);

            if (compressedSize == 0) {
                throw new InvalidOperationException("Compression failed: Output buffer too small.");
            }

            // Truncate the array to the actual compressed size
            Array.Resize(ref compressedData, compressedSize);

            // Prepend the LZF flag byte
            byte[] finalData = new byte[compressedData.Length + 1];
            finalData[0] = _lzfFlag;
            Array.Copy(compressedData, 0, finalData, 1, compressedData.Length);

            return finalData;
        }

        private void SaveCompressedFile(byte[] compressedData, string filePath) {
            File.WriteAllBytes(filePath, compressedData);
        }

        public void ExportDataFile() {
            try {
                byte[] compressedData = CompressData();

                string localPath = Path.Combine(ConfigManager.UnpackFolderPath, "Ratings.data");
                SaveCompressedFile(compressedData, localPath);

                string? originalRatingsPath = ConfigManager.OriginalRatingsPath;
                if (originalRatingsPath == null) {
                    Console.WriteLine("Original ratings path not configured.");
                    return;
                }

                File.Copy(localPath, originalRatingsPath, overwrite: true);
                Console.WriteLine($"Ratings.data copied to: {originalRatingsPath}");
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred during export: {ex.Message}");
            }
        }
    }
}
