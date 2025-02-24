using System.IO;

namespace Ac.Ratings.Services.Acd;

public static class AcdEncryption {
    internal static IAcdEncryptionFactory? Factory { get; set; }

    public static IAcdEncryption FromAcdFilename(string acdFilename) {
        if (Factory == null) throw new NotSupportedException("AcdEncryption Factory is not initialized.");

        var name = Path.GetFileName(acdFilename) ?? throw new ArgumentException("Invalid file path.", nameof(acdFilename));
        var directoryName = Path.GetDirectoryName(acdFilename);
        var id = name.StartsWith("data", StringComparison.OrdinalIgnoreCase) && directoryName != null
            ? Path.GetFileName(directoryName)
            : name;

        return Factory.Create(id);
    }
}