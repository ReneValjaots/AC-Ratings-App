namespace Ac.Ratings.Services.Acd {
    public interface IAcdEncryptionFactory {
        IAcdEncryption Create(string keySource);
    }
}
