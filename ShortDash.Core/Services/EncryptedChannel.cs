using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ShortDash.Core.Services
{
    public class EncryptedChannel
    {
        private readonly Aes aes;
        private readonly RSA rsa;

        public EncryptedChannel(string receiverPublicKeyXml)
        {
            aes = Aes.Create();
            rsa = RSA.Create();
            rsa.FromXmlString(receiverPublicKeyXml);

            // TODO: REMOVE ME!!!!

            aes.Key = Convert.FromBase64String("A2BbC83qlh9550ZOdCFOaOJaBeaR4rb+xGyIBhzq/Lk=");
            aes.IV = Convert.FromBase64String("ZcpYERcwAuGhevN3qgWQHg==");

            Console.WriteLine("KeyHex: " + BitConverter.ToString(aes.Key).Replace("-", ""));
            Console.WriteLine("IVHex: " + BitConverter.ToString(aes.IV).Replace("-", ""));
            var data = Encrypt("This is a test!");
            Console.WriteLine("Data64: " + Convert.ToBase64String(data));

            /*
            using var aesX = Aes.Create();
            aesX.Key = Convert.FromBase64String("A2BbC83qlh9550ZOdCFOaOJaBeaR4rb+xGyIBhzq/Lk=");
            aesX.IV = Convert.FromBase64String("ZcpYERcwAuGhevN3qgWQHg==");

            Console.WriteLine("KeyHex: " + BitConverter.ToString(aesX.Key).Replace("-", ""));
            Console.WriteLine("IVHex: " + BitConverter.ToString(aesX.IV).Replace("-", ""));

            //var data = Encrypt("This is a test!");
            var plaintext = "This is a test!";
            using var encryptor = aesX.CreateEncryptor(aesX.Key, aesX.IV);
            using var memoryStream = new MemoryStream();
            // memoryStream.Write(aesX.IV);
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plaintext);
            }
            var encryptedData = memoryStream.ToArray();
            Console.WriteLine("EncDataHex: " + BitConverter.ToString(encryptedData).Replace("-", ""));
            */
            // TODO: REMOVE ME!!!!
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
            // TODO: REMOVE ME!!!!
            // TODO: REMOVE ME!!!!aes.GenerateIV();
            // TODO: REMOVE ME!!!!
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
