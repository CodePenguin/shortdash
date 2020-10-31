namespace ShortDash.Core.Interfaces
{
    public interface ISecureKeyStoreService
    {
        bool HasKey(string purpose);

        void RemoveKey(string purpose);

        string RetrieveSecureKey(string purpose);

        void StoreSecureKey(string purpose, string key);
    }
}
