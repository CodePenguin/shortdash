namespace ShortDash.Core.Services
{
    public interface IKeyStoreService
    {
        bool HasKey(string purpose);

        void RemoveKey(string purpose);

        string RetrieveKey(string purpose, bool autoGenerate = true);

        void StoreKey(string purpose, string key);
    }
}
