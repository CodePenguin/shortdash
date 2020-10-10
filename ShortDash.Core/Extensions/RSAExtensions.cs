using System;
using System.Security.Cryptography;

namespace ShortDash.Core.Extensions
{
    public static class RSAExtensions
    {
        private const string PrivateKeyPrefix = "-----BEGIN RSA PRIVATE KEY-----\n";
        private const string PrivateKeySuffix = "\n-----END RSA PRIVATE KEY-----";
        private const string PublicKeyPrefix = "-----BEGIN PUBLIC KEY-----\n";
        private const string PublicKeySuffix = "\n-----END PUBLIC KEY-----";

        public static string ExportPrivateKey(this RSA rsa)
        {
            var key = rsa.ExportRSAPrivateKey();
            return PrivateKeyPrefix + Convert.ToBase64String(key) + PrivateKeySuffix;
        }

        public static string ExportPublicKey(this RSA rsa)
        {
            var key = rsa.ExportSubjectPublicKeyInfo();
            return PublicKeyPrefix + Convert.ToBase64String(key) + PublicKeySuffix;
        }

        public static string FingerPrint(this RSA rsa)
        {
            using var sha = SHA1.Create();
            var hash = sha.ComputeHash(rsa.ExportSubjectPublicKeyInfo());
            return Convert.ToBase64String(hash);
        }

        public static void ImportPrivateKey(this RSA rsa, string privateKey)
        {
            var keyBytes = GetKeyBytes(privateKey, PrivateKeyPrefix, PrivateKeySuffix);
            rsa.ImportRSAPrivateKey(keyBytes, out _);
        }

        public static void ImportPublicKey(this RSA rsa, string publicKey)
        {
            var keyBytes = GetKeyBytes(publicKey, PublicKeyPrefix, PublicKeySuffix);
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
        }

        private static byte[] GetKeyBytes(string key, string prefix, string suffix)
        {
            key = key.Replace("\r\n", "\n").Trim('\n');
            if (!key.StartsWith(prefix) || !key.EndsWith(suffix))
            {
                throw new CryptographicException("Invalid key format.");
            }
            var startingIndex = key.IndexOf(prefix) + prefix.Length;
            var endingIndex = key.IndexOf(suffix, startingIndex);
            key = key[startingIndex..endingIndex].Replace("\n", "");
            return Convert.FromBase64String(key);
        }
    }
}
