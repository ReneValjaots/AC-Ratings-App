using System.IO;

namespace Ac.Ratings.Services.Acd;

public class Acd {
    private readonly string? _packedFile;
    private byte[]? _packedBytes;
    private readonly Dictionary<string, AcdEntry?> _entries = new(10);


    private Acd(string? packedFile, byte[]? packedBytes) {
        _packedFile = packedFile;
        _packedBytes =packedBytes;
    }

    public AcdEntry? GetEntry(string entryName) {
        if (!_entries.TryGetValue(entryName, out var entry)) {
            var data = ReadPacked(entryName);
            entry = data != null ? new AcdEntry(entryName, data) : null;
            _entries[entryName] = entry;
        }

        return entry;
    }

    private byte[]? ReadPacked(string entryName) {
        if (_packedBytes == null && _packedFile != null) {
            _packedBytes = File.ReadAllBytes(_packedFile);
        }

        if (_packedFile == null) return null;

        using var stream = new MemoryStream(_packedBytes);
        using var reader = new AcdReader(_packedFile, stream);
        return reader.ReadEntryData(entryName);
    }

    public static Acd FromFile(string filename) {
        if (!File.Exists(filename)) throw new FileNotFoundException(filename);
        var bytes = File.ReadAllBytes(filename);
        return new Acd(filename, bytes);
    }
}