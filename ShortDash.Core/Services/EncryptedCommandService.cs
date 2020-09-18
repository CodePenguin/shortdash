using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ShortDash.Core.Services
{
    public class EncryptedCommandService
    {
        private const char KeyDelimiter = ':';
        private readonly RSA privateKey;
        private readonly RSA publicKey;

        public EncryptedCommandService()
        {
            // TODO: USE DATA STORE
            var privateXml = File.ReadAllText("d:\\temp\\rsaprivate.xml");
            privateKey = RSA.Create();
            privateKey.FromXmlString(privateXml);

            var publicXml = File.ReadAllText("d:\\temp\\rsapublic.xml");
            publicKey = RSA.Create();
            publicKey.FromXmlString(publicXml);
        }

        ~EncryptedCommandService()
        {
            privateKey?.Dispose();
            publicKey?.Dispose();
        }

        private string KeyContainerName => typeof(EncryptedCommandService).FullName;

        public string AesDecrypt(Aes aes, byte[] data)
        {
            using var memoryStream = new MemoryStream(data);
            byte[] iv = new byte[16];
            memoryStream.Read(iv, 0, aes.IV.Length);
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }

        public byte[] AesEncrypt(Aes aes, string data)
        {
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

        public EncryptedCommand EncryptCommand(string data, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(key);
            var encryptedData = Convert.ToBase64String(AesEncrypt(aes, data));
            var signature = Convert.ToBase64String(RsaSign(data));
            return new EncryptedCommand(encryptedData, signature);
        }

        public string ExportPublicKey()
        {
            return Convert.ToBase64String(privateKey.ExportRSAPublicKey());
        }

        public byte[] RsaSign(string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            return privateKey.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public bool RsaVerify(string data, string signature)
        {
            var signatureBytes = Encoding.UTF8.GetBytes(signature);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            return publicKey.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public bool TryDecryptCommand(EncryptedCommand encryptedCommand, string key, out string command)
        {
            // TODO: PARSE THE IV OFF THE DATA
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(key);
            var encryptedData = Convert.FromBase64String(encryptedCommand.Data);
            var data = AesDecrypt(aes, encryptedData);

            command = data;
            // TODO: NEED TO VALIDATE SIGNATURE AND TIMESTAMP
            return true;
        }
    }
}
