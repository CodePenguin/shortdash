using Microsoft.AspNetCore.DataProtection;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ShortDash.Core.Services
{
    public class FileKeyStoreService : IKeyStoreService
    {
        private readonly IDataProtectionProvider dataProtectionProvider;

        public FileKeyStoreService(IDataProtectionProvider dataProtectionProvider)
        {
            this.dataProtectionProvider = dataProtectionProvider;
        }

        public bool HasKey(string purpose)
        {
            return File.Exists(PurposeToFileName(purpose));
        }

        public string RetrieveKey(string purpose, bool autoGenerate = true)
        {
            if (!HasKey(purpose))
            {
                if (autoGenerate)
                {
                    return GenerateNewKey(purpose);
                }
                return null;
            }
            return Unprotect(purpose, File.ReadAllText(PurposeToFileName(purpose)));
        }

        public void StoreKey(string purpose, string key)
        {
            File.WriteAllText(PurposeToFileName(purpose), Protect(purpose, key));
        }

        private string GenerateNewKey(string purpose)
        {
            var rsa = RSA.Create();
            var key = rsa.ToXmlString(true);
            StoreKey(purpose, key);
            return key;
        }

        private IDataProtector GetDataProtector(string purpose)
        {
            return dataProtectionProvider.CreateProtector(purpose);
        }

        private string Protect(string purpose, string value)
        {
            var protector = GetDataProtector(purpose);
#if DEBUG
            return value;
#else
            return protector.Protect(value);
#endif
        }

        private string PurposeToFileName(string purpose)
        {
            var normalizedPurpose = Regex.Replace(purpose, @"[^\w.-]", "");
            return Path.Combine(AppContext.BaseDirectory, normalizedPurpose + ".key");
        }

        private string Unprotect(string purpose, string value)
        {
            var protector = GetDataProtector(purpose);
#if DEBUG
            return value;
#else
            return protector.Unprotect(value);
#endif
        }
    }
}
