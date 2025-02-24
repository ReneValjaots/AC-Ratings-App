using System.Text;

namespace Ac.Ratings.Services.Acd;

public static class StringExtension {
    public static string ToUtf8String(byte[] bytes) => Encoding.UTF8.GetString(bytes);
}