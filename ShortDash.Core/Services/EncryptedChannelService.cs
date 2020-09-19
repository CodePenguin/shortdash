using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ShortDash.Core.Services
{
    public class EncryptedChannelService

    {
        private const char CommandDelimiter = ':';
        private readonly Dictionary<int, EncryptedChannel> channels = new Dictionary<int, EncryptedChannel>();
        private readonly RSA rsa;

        public EncryptedChannelService()
        {
            rsa = RSA.Create();
        }

        ~EncryptedChannelService()
        {
            rsa.Dispose();
            channels.Clear();
        }

        public void CloseChannel(int channelId)
        {
            channels.Remove(channelId);
        }

        public string EncryptCommand(int channelId, string data)
        {
            var channel = channels[channelId];
            var encryptedData = channel.Encrypt(data);
            var signature = RsaSign(encryptedData);
            return Convert.ToBase64String(encryptedData) + CommandDelimiter + Convert.ToBase64String(signature);
        }

        public string ExportEncryptedKey(int channelId)
        {
            var channel = channels[channelId];
            return channel.ExportEncryptedKey();
        }

        public string ExportPublicKey()
        {
            return rsa.ToXmlString(false);
        }

        public void ImportPrivateKey(string privateKeyXml)
        {
            rsa.FromXmlString(privateKeyXml);
        }

        public void OpenChannel(int channelId, string receiverPublicKeyXml)
        {
            channels.Add(channelId, new EncryptedChannel(receiverPublicKeyXml));
        }

        public void OpenChannel(int channelId, string receiverPublicKeyXml, string encryptedKey)
        {
            var encryptedKeyBytes = Convert.FromBase64String(encryptedKey);
            var decryptedKey = rsa.Decrypt(encryptedKeyBytes, RSAEncryptionPadding.Pkcs1);
            var channel = new EncryptedChannel(receiverPublicKeyXml);
            channel.ImportKey(decryptedKey);
            channels.Add(channelId, channel);
        }

        public bool TryDecryptCommand(int channelId, string encryptedCommand, out string command)
        {
            command = null;
            try
            {
                var channel = channels[channelId];
                var commandParts = encryptedCommand.Split(CommandDelimiter);
                if (commandParts.Length != 2)
                {
                    return false;
                }
                var encryptedData = Convert.FromBase64String(commandParts[0]);
                var signature = Convert.FromBase64String(commandParts[1]);
                if (!channel.Verify(encryptedData, signature))
                {
                    return false;
                }
                var data = channel.Decrypt(encryptedData);
                command = data;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private byte[] RsaSign(byte[] data)
        {
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
