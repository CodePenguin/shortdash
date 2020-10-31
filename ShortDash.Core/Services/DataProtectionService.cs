using Microsoft.AspNetCore.DataProtection;
using ShortDash.Core.Extensions;
using ShortDash.Core.Interfaces;
using System;
using System.Security.Cryptography;

namespace ShortDash.Core.Services
{
    public class DataProtectionService : IDataProtectionService
    {
        private const string KeyPurpose = "DataProtection";
        private readonly IDataProtector dataProtector;
        private readonly IKeyStoreService keyStoreService;

        public DataProtectionService(IDataProtectionProvider dataProtectionProvider, IKeyStoreService keyStoreService)
        {
            dataProtector = dataProtectionProvider.CreateProtector(KeyPurpose);
            this.keyStoreService = keyStoreService;
        }

        private static event EventHandler OnKeyChanged;

        public void AddKeyChangedEventHandler(EventHandler handler)
        {
            OnKeyChanged += handler;
        }

        public bool Initialized()
        {
            try
            {
                GetProtectionKey();
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        public string Protect(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            using var aes = Aes.Create();
            aes.Key = GetProtectionKey();
            return Convert.ToBase64String(aes.Encrypt(value));
        }

        public void RemoveKeyChangedEventHandler(EventHandler handler)
        {
            OnKeyChanged -= handler;
        }

        public void SetKey(byte[] key = null)
        {
            if (key == null)
            {
                using var aes = Aes.Create();
                key = aes.Key;
            }
            var protectedBytes = dataProtector.Protect(key);
            var protectedKey = Convert.ToBase64String(protectedBytes);
            keyStoreService.StoreKey(KeyPurpose, protectedKey);
            OnKeyChanged?.Invoke(this, new EventArgs());
        }

        public string Unprotect(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            using var aes = Aes.Create();
            aes.Key = GetProtectionKey();
            return aes.Decrypt(Convert.FromBase64String(value));
        }

        private byte[] GetProtectionKey()
        {
            if (!keyStoreService.HasKey(KeyPurpose))
            {
                SetKey();
            }
            var key = keyStoreService.RetrieveKey(KeyPurpose);
            var protectedBytes = Convert.FromBase64String(key);
            return dataProtector.Unprotect(protectedBytes);
        }
    }
}
