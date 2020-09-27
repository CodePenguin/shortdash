using ShortDash.Core.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ShortDash.Core.Services
{
    public class EncryptedChannel
    {
        public readonly string ReceiverId;
        private readonly Aes aes;
        private readonly RSA rsa;

        public EncryptedChannel(string receiverPublicKey)
        {
            aes = Aes.Create();
            rsa = RSA.Create();
            rsa.ImportPublicKey(receiverPublicKey);
            ReceiverId = rsa.FingerPrint();
        }

        ~EncryptedChannel()
        {
            aes.Dispose();
            rsa.Dispose();
        }

        public string Decrypt(byte[] data)
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

        public byte[] Encrypt(string data)
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

        public string ExportEncryptedKey()
        {
            var keyData = Encoding.UTF8.GetBytes(Convert.ToBase64String(aes.Key));
            return Convert.ToBase64String(rsa.Encrypt(keyData, RSAEncryptionPadding.Pkcs1));
        }

        public void ImportKey(byte[] key)
        {
            aes.Key = key;
        }

        public bool Verify(byte[] data, byte[] signature)
        {
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
