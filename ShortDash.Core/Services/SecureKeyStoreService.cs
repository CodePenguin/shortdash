using ShortDash.Core.Interfaces;

namespace ShortDash.Core.Services
{
    public class SecureKeyStoreService : ISecureKeyStoreService
    {
        private readonly IDataProtectionService dataProtectionService;
        private readonly IKeyStoreService keyStoreService;

        public SecureKeyStoreService(IDataProtectionService dataProtectionService, IKeyStoreService keyStoreService)
        {
            this.dataProtectionService = dataProtectionService;
            this.keyStoreService = keyStoreService;
        }

        public bool HasKey(string purpose)
        {
            return keyStoreService.HasKey(purpose);
        }

        public void RemoveKey(string purpose)
        {
            keyStoreService.RemoveKey(purpose);
        }

        public string RetrieveSecureKey(string purpose)
        {
            var protectedKey = keyStoreService.RetrieveKey(purpose);
            return dataProtectionService.Unprotect(protectedKey);
        }

        public void StoreSecureKey(string purpose, string key)
        {
            var protectedKey = dataProtectionService.Protect(key);
            keyStoreService.StoreKey(purpose, protectedKey);
        }
    }
}
