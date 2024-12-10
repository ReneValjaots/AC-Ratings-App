namespace Ac.Ratings.Services.Acd;

public interface IAcdEncryption {
    void Decrypt(byte[] data);
    void Encrypt(byte[] data, byte[] result);
}