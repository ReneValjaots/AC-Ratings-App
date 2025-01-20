using System.IO;

namespace Ac.Ratings.Services.Acd;

public static class AcdEncryption {
    internal static IAcdEncryptionFactory? Factory;

    public static IAcdEncryption FromAcdFilename(string acdFilename) {
        if (Factory == null) throw new NotSupportedException();
        var name = Path.GetFileName(acdFilename) ?? "";
        var directoryName = Path.GetDirectoryName(acdFilename);
        var id = name.StartsWith("data", StringComparison.OrdinalIgnoreCase) && directoryName != null
            ? Path.GetFileName(directoryName)
            : name;
        if (id == null) throw new ArgumentException("Invalid file path.", nameof(acdFilename));
        return Factory.Create(id);
    }
}