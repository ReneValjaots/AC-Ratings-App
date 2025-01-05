namespace Ac.Ratings.Services.Acd;

public class AcdFactory : IAcdEncryption, IAcdEncryptionFactory {
    private readonly string _key;

    public AcdFactory() { }

    IAcdEncryption IAcdEncryptionFactory.Create(string keySource) {
        return (IAcdEncryption)new AcdFactory(keySource);
    }

    private AcdFactory(string id) => _key = CreateKey(id);

    void IAcdEncryption.Decrypt(byte[] data) {
        int num1 = _key.Length - 1;
        if (num1 < 0)
            return;
        int index1 = 0;
        int index2 = 0;
        for (int length = data.Length; index1 < length; ++index1) {
            int num2 = (int)data[index1] - (int)_key[index2];
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
        byte num1 = IntToByte(s.Aggregate<char, int>(0, (Func<int, char, int>)((current, t) => current + (int)t)));
        int num2 = 0;
        for (int index = 0; index < s.Length - 1; index += 2)
            num2 = num2 * (int)s[index] - (int)s[index + 1];
        byte num3 = IntToByte(num2);
        int num4 = 0;
        for (int index = 1; index < s.Length - 3; index += 3)
            num4 = num4 * (int)s[index] / ((int)s[index + 1] + 27) + (-27 - (int)s[index - 1]);
        byte num5 = IntToByte(num4);
        int num6 = 5763;
        for (int index = 1; index < s.Length; ++index)
            num6 -= (int)s[index];
        byte num7 = IntToByte(num6);
        int num8 = 66;
        for (int index = 1; index < s.Length - 4; index += 4)
            num8 = ((int)s[index] + 15) * num8 * ((int)s[index - 1] + 15) + 22;
        byte num9 = IntToByte(num8);
        int num10 = 101;
        for (int index = 0; index < s.Length - 2; index += 2)
            num10 -= (int)s[index];
        byte num11 = IntToByte(num10);
        int num12 = 171;
        for (int index = 0; index < s.Length - 2; index += 2)
            num12 %= (int)s[index];
        byte num13 = IntToByte(num12);
        int num14 = 171;
        for (int index = 0; index < s.Length - 1; ++index)
            num14 = num14 / (int)s[index] + (int)s[index + 1];
        byte num15 = IntToByte(num14);
        return string.Join("-", (object)num1, (object)num3, (object)num5, (object)num7, (object)num9, (object)num11, (object)num13, (object)num15);
    }
}