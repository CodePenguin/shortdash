using System.IO;
using System.Security.Cryptography;

namespace ShortDash.Core.Extensions
{
    public static class AesExtensions
    {
        public static string Decrypt(this Aes aes, byte[] data)
        {
            using var memoryStream = new MemoryStream(data);
            var iv = new byte[16];
            memoryStream.Read(iv, 0, aes.IV.Length);
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }

        public static byte[] Encrypt(this Aes aes, string data)
        {
            aes.GenerateIV();
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream();
            memoryStream.Write(aes.IV);
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(data);
            }
            return memoryStream.ToArray();
        }
    }
}
