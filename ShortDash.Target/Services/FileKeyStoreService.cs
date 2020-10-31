using ShortDash.Core.Interfaces;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ShortDash.Target.Services
{
    public class FileKeyStoreService : IKeyStoreService
    {
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

        public string RetrieveKey(string purpose)
        {
            if (!HasKey(purpose))
            {
                return null;
            }
            return File.ReadAllText(PurposeToFileName(purpose));
        }

        public void StoreKey(string purpose, string key)
        {
            File.WriteAllText(PurposeToFileName(purpose), key);
        }

        private string PurposeToFileName(string purpose)
        {
            var normalizedPurpose = Regex.Replace(purpose, @"[^\w.-]", "");
            return Path.Combine(AppContext.BaseDirectory, "ShortDash.Target." + normalizedPurpose + ".key");
        }
    }
}
