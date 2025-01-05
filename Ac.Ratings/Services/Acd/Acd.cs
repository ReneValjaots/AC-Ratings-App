using System.IO;

namespace Ac.Ratings.Services.Acd;

public class Acd {
    public static IAcdEncryptionFactory Factory {
        get => AcdEncryption.Factory;
        set => AcdEncryption.Factory = value;
    }

    private readonly string _packedFile;

    private readonly string _unpackedDirectory;

    private byte[] _packedBytes;

    private Acd(string packedFile, string unpackedDirectory) {
        _packedBytes = packedFile == null ? null : File.ReadAllBytes(packedFile);
        _packedFile = packedFile;
        _unpackedDirectory = unpackedDirectory;
        _entries = new Dictionary<string, AcdEntry>(10);
    }

    private readonly Dictionary<string, AcdEntry> _entries;

    public AcdEntry GetEntry(string entryName) {
        AcdEntry entry;

        if (!_entries.TryGetValue(entryName, out entry)) {
            if (_unpackedDirectory != null) {
                var filename = Path.Combine(_unpackedDirectory, entryName);
                entry = File.Exists(filename)
                    ? new AcdEntry {
                        Name = entryName,
                        Data = File.ReadAllBytes(filename)
                    }
                    : null;
            }
            else {
                var data = ReadPacked(entryName);
                entry = data != null
                    ? new AcdEntry {
                        Name = entryName,
                        Data = data
                    }
                    : null;
            }

            _entries[entryName] = entry;
        }

        return entry;
    }

    private byte[] ReadPacked(string entryName) {
        if (_packedBytes == null) {
            if (_packedFile == null) return null;
            _packedBytes = File.ReadAllBytes(_packedFile);
        }

        using (var stream = new MemoryStream(_packedBytes))
            using (var reader = new AcdReader(_packedFile, stream)) {
                return reader.ReadEntryData(entryName);
            }
    }

    public static Acd FromFile(string filename) {
        if (!File.Exists(filename)) throw new FileNotFoundException(filename);
        return new Acd(filename, null);
    }
}