namespace Ac.Ratings.Services.Acd;

public class AcdFactory : IAcdEncryption, IAcdEncryptionFactory {
    private readonly string _key;

    public AcdFactory() { }

    public IAcdEncryption Create(string keySource) => new AcdFactory(keySource);

    private AcdFactory(string id) => _key = CreateKey(id);

    void IAcdEncryption.Decrypt(byte[] data) {
        int num1 = _key.Length - 1;
        if (num1 < 0)
            return;
        int index1 = 0;
        int index2 = 0;
        for (int length = data.Length; index1 < length; ++index1) {
            int num2 = data[index1] - _key[index2];
            data[index1] = num2 < 0 ? (byte)(num2 + 256) : (byte)num2;
            if (index2 == num1)
                index2 = 0;
            else
                ++index2;
        }
    }

    private static byte IntToByte(int value) => (byte)((value % 256 + 256) % 256);

    private static string CreateKey(string s) {
        s = s.ToLower();
        byte num1 = IntToByte(s.Aggregate(0, (current, t) => current + t));
        int num2 = 0;
        for (int index = 0; index < s.Length - 1; index += 2)
            num2 = num2 * s[index] - s[index + 1];
        byte num3 = IntToByte(num2);
        int num4 = 0;
        for (int index = 1; index < s.Length - 3; index += 3)
            num4 = num4 * s[index] / (s[index + 1] + 27) + (-27 - s[index - 1]);
        byte num5 = IntToByte(num4);
        int num6 = 5763;
        for (int index = 1; index < s.Length; ++index)
            num6 -= s[index];
        byte num7 = IntToByte(num6);
        int num8 = 66;
        for (int index = 1; index < s.Length - 4; index += 4)
            num8 = (s[index] + 15) * num8 * (s[index - 1] + 15) + 22;
        byte num9 = IntToByte(num8);
        int num10 = 101;
        for (int index = 0; index < s.Length - 2; index += 2)
            num10 -= s[index];
        byte num11 = IntToByte(num10);
        int num12 = 171;
        for (int index = 0; index < s.Length - 2; index += 2)
            num12 %= s[index];
        byte num13 = IntToByte(num12);
        int num14 = 171;
        for (int index = 0; index < s.Length - 1; ++index)
            num14 = num14 / s[index] + s[index + 1];
        byte num15 = IntToByte(num14);
        return string.Join("-", num1, num3, num5, num7, num9, num11, num13, num15);
    }
}