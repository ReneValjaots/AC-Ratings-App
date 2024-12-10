using System.IO;
using System.Text;

namespace Ac.Ratings.Services.Acd;

internal class AcdWriter : BinaryWriter {
    private readonly IAcdEncryption _enc;

    public AcdWriter(string filename) : this(filename, File.Open(filename, FileMode.Create, FileAccess.Write)) { }

    public AcdWriter(string filename, Stream output) : base(output) {
        _enc = AcdEncryption.FromAcdFilename(filename);
    }

    public override void Write(string value) {
        if (value == null) throw new ArgumentNullException(nameof(value));
        Write(value.Length);
        Write(Encoding.ASCII.GetBytes(value));
    }

    public void Write(AcdEntry entry) {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        Write(entry.Name);
        Write(entry.Data.Length);

        var result = new byte[entry.Data.Length * 4];
        _enc.Encrypt(entry.Data, result);
        Write(result);
    }
}