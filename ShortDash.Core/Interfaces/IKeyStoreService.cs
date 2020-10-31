namespace ShortDash.Core.Interfaces
{
    public interface IKeyStoreService
    {
        bool HasKey(string purpose);

        void RemoveKey(string purpose);

        string RetrieveKey(string purpose);

        void StoreKey(string purpose, string key);
    }
}
