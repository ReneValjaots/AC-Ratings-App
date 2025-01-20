using System.IO;

namespace Ac.Ratings.Services.Acd;

public class Acd {
    private readonly string? _packedFile;
    private byte[]? _packedBytes;
    private readonly Dictionary<string, AcdEntry?> _entries;


    private Acd(string? packedFile) {
        _packedFile = packedFile;
        _packedBytes = packedFile != null ? File.ReadAllBytes(packedFile) : null;
        _entries = new Dictionary<string, AcdEntry?>(10);
    }

    public AcdEntry? GetEntry(string entryName) {
        if (!_entries.TryGetValue(entryName, out var entry)) {
            var data = ReadPacked(entryName);
            entry = data != null ? new AcdEntry { Name = entryName, Data = data } : null;
            _entries[entryName] = entry;
        }

        return entry;
    }

    private byte[]? ReadPacked(string entryName) {
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
        return new Acd(filename);
    }
}