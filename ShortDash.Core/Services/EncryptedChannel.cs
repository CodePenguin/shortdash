using ShortDash.Core.Extensions;
using System;
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
            return aes.Decrypt(data);
        }

        public byte[] Encrypt(string data)
        {
            return aes.Encrypt(data);
        }

        public string ExportEncryptedKey()
        {
            var keyData = Encoding.UTF8.GetBytes(Convert.ToBase64String(aes.Key));
            return Convert.ToBase64String(rsa.Encrypt(keyData, RSAEncryptionPadding.Pkcs1));
        }

        public string ExportPublicKey()
        {
            return rsa.ExportPublicKey();
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
