using System.IO;

namespace Ac.Ratings.Services.Acd;

public static class AcdEncryption {
    internal static IAcdEncryptionFactory Factory;

    public static IAcdEncryption FromAcdFilename(string acdFilename) {
        if (Factory == null) throw new NotSupportedException();
        var name = Path.GetFileName(acdFilename) ?? "";
        var id = name.StartsWith("data", StringComparison.OrdinalIgnoreCase) ? Path.GetFileName(Path.GetDirectoryName(acdFilename)) : name;
        return Factory.Create(id);
    }
}