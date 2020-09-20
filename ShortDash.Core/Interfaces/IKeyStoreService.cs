namespace ShortDash.Core.Services
{
    public interface IKeyStoreService
    {
        bool HasKey();

        string RetrieveKey(bool autoGenerate = true);

        void StoreKey(string key);
    }

    public interface IKeyStoreService<out TKeyName> : IKeyStoreService
    {
    }
}
