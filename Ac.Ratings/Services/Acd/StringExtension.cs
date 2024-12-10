using System.Text;

namespace Ac.Ratings.Services.Acd;

public static class StringExtension {
    public static string ToUtf8String(this byte[] bytes) {
        return GetEncoding(bytes).GetString(bytes);
    }

    public static Encoding GetEncoding(this byte[] bytes) {
        if (bytes.StartsWith(Encoding.UTF8.GetPreamble()) || Utf8Checker.IsUtf8(bytes, 200)) {
            return Encoding.UTF8;
        }

        if (bytes.StartsWith(Encoding.Unicode.GetPreamble())) return Encoding.Unicode;
        if (bytes.StartsWith(Encoding.BigEndianUnicode.GetPreamble())) return Encoding.BigEndianUnicode;
        if (bytes.StartsWith(Encoding.UTF32.GetPreamble())) return Encoding.UTF32;
        if (bytes.StartsWith(Encoding.UTF7.GetPreamble())) return Encoding.UTF7;
        return Encoding.Default;
    }

    public static bool StartsWith(this byte[] b1, params byte[] b2) {
        if (b1.Length < b2.Length) return false;
        for (var i = 0; i < b2.Length; i++) {
            if (b1[i] != b2[i]) return false;
        }

        return true;
    }
}