using Microsoft.AspNetCore.DataProtection;
using ShortDash.Core.Extensions;
using ShortDash.Core.Services;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ShortDash.Target.Services
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

        public void RemoveKey(string purpose)
        {
            var keyFileName = PurposeToFileName(purpose);
            if (File.Exists(keyFileName))
            {
                File.Delete(keyFileName);
            }
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
            var key = rsa.ExportPrivateKey();
            StoreKey(purpose, key);
            return key;
        }

        private IDataProtector GetDataProtector(string purpose)
        {
            return dataProtectionProvider.CreateProtector("FileKeyStoreService." + purpose);
        }

        private string Protect(string purpose, string value)
        {
            var protector = GetDataProtector(purpose);
            return protector.Protect(value);
        }

        private string PurposeToFileName(string purpose)
        {
            var normalizedPurpose = Regex.Replace(purpose, @"[^\w.-]", "");
            return Path.Combine(AppContext.BaseDirectory, "ShortDash.Target." + normalizedPurpose + ".key");
        }

        private string Unprotect(string purpose, string value)
        {
            var protector = GetDataProtector(purpose);
            return protector.Unprotect(value);
        }
    }
}
