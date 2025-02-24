namespace Ac.Ratings.Services.Acd;

public class AcdEntry {
    public string Name { get; }
    public byte[] Data { get; }

    public AcdEntry(string name, byte[] data) {
        Name = name;
        Data = data;
    }

    public override string ToString() => StringExtension.ToUtf8String(Data);
}