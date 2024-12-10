namespace Ac.Ratings.Services.Acd;

public class AcdEntry {
    public string Name;
    public byte[] Data;

    public override string ToString() {
        return Data.ToUtf8String();
    }
}