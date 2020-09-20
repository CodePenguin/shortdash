using ShortDash.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ShortDash.Core.Services
{
    public abstract class EncryptedChannelService<T> : IEncryptedChannelService<T>
    {
        private const char CommandDelimiter = ':';
        private const string EncryptedChallengePrefix = "RSA:";
        private readonly Dictionary<string, EncryptedChannel> channels = new Dictionary<string, EncryptedChannel>();
        private readonly RSA rsa;

        protected EncryptedChannelService(IKeyStoreService keyStore)
        {
            rsa = RSA.Create();
            rsa.FromXmlString(keyStore.RetrieveKey());
        }

        ~EncryptedChannelService()
        {
            rsa.Dispose();
            channels.Clear();
        }

        public void CloseChannel(string channelId)
        {
            channels.Remove(channelId);
        }

        public string EncryptCommand(string channelId, string data)
        {
            var channel = channels[channelId];
            var encryptedData = channel.Encrypt(data);
            var signature = RsaSign(encryptedData);
            return Convert.ToBase64String(encryptedData) + CommandDelimiter + Convert.ToBase64String(signature);
        }

        public string ExportEncryptedKey(string channelId)
        {
            var channel = channels[channelId];
            return channel.ExportEncryptedKey();
        }

        public string ExportPublicKey()
        {
            return rsa.ToXmlString(false);
        }

        public string GenerateChallenge(string publicKey, out byte[] rawChallenge)
        {
            using var aes = Aes.Create();
            rawChallenge = aes.IV.Concat(aes.Key).ToArray();
            byte[] challenge;
            var isEncryptedChallenge = !string.IsNullOrEmpty(publicKey);
            if (isEncryptedChallenge)
            {
                // If the public key is known, generate a specific challenge for that key
                using var challengeRsa = RSA.Create();
                challengeRsa.FromXmlString(publicKey);
                challenge = challengeRsa.Encrypt(rawChallenge, RSAEncryptionPadding.Pkcs1);
            }
            else
            {
                // If the challenge is for a new registration, send a unencrypted challenge
                challenge = rawChallenge;
            }
            // Add the encrypted challenge prefix if applicable
            return (isEncryptedChallenge ? EncryptedChallengePrefix : string.Empty) + Convert.ToBase64String(challenge);
        }

        public string GenerateChallengeResponse(string challenge, string publicKey)
        {
            try
            {
                var isEncryptedChallenge = IsEncryptedChallenge(challenge, out var data);
                var decryptedChallenge = isEncryptedChallenge ? rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1) : data;
                using var challengeRsa = RSA.Create();
                challengeRsa.FromXmlString(publicKey);
                var challengeResponse = challengeRsa.Encrypt(decryptedChallenge, RSAEncryptionPadding.Pkcs1);
                return Convert.ToBase64String(challengeResponse);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        public void ImportPrivateKey(string privateKeyXml)
        {
            rsa.FromXmlString(privateKeyXml);
        }

        public void OpenChannel(string channelId, string receiverPublicKeyXml)
        {
            var channel = new EncryptedChannel(receiverPublicKeyXml);
            channels.Add(channelId, channel);
        }

        public void OpenChannel(string channelId, string receiverPublicKeyXml, string encryptedKey)
        {
            var encryptedKeyBytes = Convert.FromBase64String(encryptedKey);
            var decryptedKey = rsa.Decrypt(encryptedKeyBytes, RSAEncryptionPadding.Pkcs1);
            var channel = new EncryptedChannel(receiverPublicKeyXml);
            channel.ImportKey(decryptedKey);
            channels.Add(channelId, channel);
        }

        public bool TryDecryptCommand(string channelId, string encryptedCommand, out string command)
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
            catch (CryptographicException)
            {
                return false;
            }
        }

        public bool VerifyChallengeResponse(byte[] challenge, string challengeResponse)
        {
            var responseData = Convert.FromBase64String(challengeResponse);
            var compareChallenge = rsa.Decrypt(responseData, RSAEncryptionPadding.Pkcs1);
            return challenge.SequenceEqual(compareChallenge);
        }

        private bool IsEncryptedChallenge(string challenge, out byte[] data)
        {
            var isEncryptedChallenge = challenge.StartsWith(EncryptedChallengePrefix);
            if (isEncryptedChallenge)
            {
                challenge = challenge.Substring(EncryptedChallengePrefix.Length, challenge.Length - EncryptedChallengePrefix.Length);
            }
            data = Convert.FromBase64String(challenge);
            return isEncryptedChallenge;
        }

        private byte[] RsaSign(byte[] data)
        {
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
