using System.IO;
using System.Text;

namespace Ac.Ratings.Services.Acd;

public static class StringExtension {
    public static string ToUtf8String(byte[] bytes) {
        return GetEncoding(bytes).GetString(bytes);
    }

    public static Encoding GetEncoding(byte[] bytes) {
        using (var ms = new MemoryStream(bytes)) {
            using (var reader = new StreamReader(ms, detectEncodingFromByteOrderMarks: true)) {
                return reader.CurrentEncoding;
            }
        }
    }
}