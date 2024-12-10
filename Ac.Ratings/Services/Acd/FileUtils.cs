using System.IO;

namespace Ac.Ratings.Services.Acd;

public partial class FileUtils {
    public static void EnsureDirectoryExists(string directory) {
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }
    }

    public static string EnsureFilenameIsValid(string fileName, bool allowSquareBrackets) {
        var input = Path.GetInvalidFileNameChars().ApartFrom('/', '\\', ':');
        if (!allowSquareBrackets) input = input.Union("[]");
        return input.Aggregate(fileName, (current, c) => current.Replace(c, '-'));
    }
}