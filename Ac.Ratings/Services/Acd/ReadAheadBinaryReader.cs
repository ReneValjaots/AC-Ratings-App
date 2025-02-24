using System.IO;
using System.Text;

namespace Ac.Ratings.Services.Acd;

public class ReadAheadBinaryReader : BinaryReader {
    public ReadAheadBinaryReader(Stream input) : base(input, Encoding.UTF8, leaveOpen: true) { }

    public override string ReadString() => Encoding.UTF8.GetString(ReadBytes(ReadInt32()));

    public void Skip(int count) => BaseStream.Seek(count, SeekOrigin.Current);
}