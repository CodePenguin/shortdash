using Microsoft.AspNetCore.DataProtection;
using System;
using System.IO;
using System.Security.Cryptography;

namespace ShortDash.Core.Services
{
    public class KeyStoreService<T> : IKeyStoreService<T>
    {
        private readonly IDataProtectionProvider dataProtectionProvider;
        private readonly string keyFileName;

        public KeyStoreService(IDataProtectionProvider dataProtectionProvider)
        {
            this.dataProtectionProvider = dataProtectionProvider;
            keyFileName = Path.Combine(AppContext.BaseDirectory, typeof(T).FullName + ".key");
        }

        public bool HasKey()
        {
            return File.Exists(keyFileName);
        }

        public string RetrieveKey(bool autoGenerate = true)
        {
            if (!HasKey())
            {
                if (autoGenerate)
                {
                    return GenerateNewKey();
                }
                return null;
            }
            return Unprotect(File.ReadAllText(keyFileName));
        }

        public void StoreKey(string key)
        {
            File.WriteAllText(keyFileName, Protect(key));
        }

        private string GenerateNewKey()
        {
            var rsa = RSA.Create();
            var key = rsa.ToXmlString(true);
            StoreKey(key);
            return key;
        }

        private IDataProtector GetDataProtector()
        {
            var purpose = typeof(T).FullName;
            return dataProtectionProvider.CreateProtector(purpose);
        }

        private string Protect(string value)
        {
            var protector = GetDataProtector();
#if DEBUG
            return value;
#else
            return protector.Protect(value);
#endif
        }

        private string Unprotect(string value)
        {
            var protector = GetDataProtector();
#if DEBUG
            return value;
#else
            return protector.Unprotect(value);
#endif
        }
    }
}
