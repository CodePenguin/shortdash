using ShortDash.Core.Extensions;
using ShortDash.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ShortDash.Core.Services
{
    public abstract class EncryptedChannelService : IEncryptedChannelService
    {
        private const char CommandDelimiter = ':';
        private const string EncryptedChallengePrefix = "RSA:";
        private readonly IDictionary<string, EncryptedChannel> channels = new ConcurrentDictionary<string, EncryptedChannel>();
        private readonly RSA rsa;

        protected EncryptedChannelService(IKeyStoreService keyStore)
        {
            rsa = RSA.Create();
            rsa.ImportPrivateKey(keyStore.RetrieveKey(KeyPurpose));
        }

        ~EncryptedChannelService()
        {
            rsa.Dispose();
            channels.Clear();
        }

        protected abstract string KeyPurpose { get; }

        public void CloseChannel(string channelId)
        {
            channels.Remove(channelId);
        }

        public string Encrypt(string channelId, object parameters)
        {
            var data = JsonSerializer.Serialize(parameters);
            return Encrypt(channelId, data);
        }

        public string Encrypt(string channelId, string data)
        {
            var channel = channels[channelId];
            var encryptedData = channel.Encrypt(data);
            return Convert.ToBase64String(encryptedData);
        }

        public string EncryptSigned(string channelId, object parameters)
        {
            var data = JsonSerializer.Serialize(parameters);
            return EncryptSigned(channelId, data);
        }

        public string EncryptSigned(string channelId, string data)
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
            return rsa.ExportPublicKey();
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
                challengeRsa.ImportPublicKey(publicKey);
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
                challengeRsa.ImportPublicKey(publicKey);
                var challengeResponse = challengeRsa.Encrypt(decryptedChallenge, RSAEncryptionPadding.Pkcs1);
                return Convert.ToBase64String(challengeResponse);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        public string GenerateUniqueChannelId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public void ImportPrivateKey(string privateKey)
        {
            rsa.ImportPrivateKey(privateKey);
        }

        public string OpenChannel(string receiverPublicKey)
        {
            var channel = new EncryptedChannel(receiverPublicKey);
            var channelId = GenerateUniqueChannelId();
            channels.Add(channelId, channel);
            return channelId;
        }

        public string OpenChannel(string receiverPublicKeyXml, string encryptedKey)
        {
            var encryptedKeyBytes = Convert.FromBase64String(encryptedKey);
            var decryptedBytes = rsa.Decrypt(encryptedKeyBytes, RSAEncryptionPadding.Pkcs1);
            var base64Key = Encoding.UTF8.GetString(decryptedBytes);
            var keyBytes = Convert.FromBase64String(base64Key);
            var channel = new EncryptedChannel(receiverPublicKeyXml);
            channel.ImportKey(keyBytes);
            var channelId = GenerateUniqueChannelId();
            channels.Add(channelId, channel);
            return channelId;
        }

        public string ReceiverId(string channelId)
        {
            var channel = channels[channelId];
            return channel.ReceiverId;
        }

        public bool TryDecrypt(string channelId, string encryptedPacket, out string data)
        {
            data = null;
            try
            {
                var channel = channels[channelId];
                var encryptedData = Convert.FromBase64String(encryptedPacket);
                data = channel.Decrypt(encryptedData);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        public bool TryDecryptVerify(string channelId, string encryptedPacket, out string data)
        {
            data = null;
            try
            {
                var channel = channels[channelId];
                var packetParts = encryptedPacket.Split(CommandDelimiter);
                if (packetParts.Length != 2)
                {
                    return false;
                }
                var encryptedData = Convert.FromBase64String(packetParts[0]);
                var signature = Convert.FromBase64String(packetParts[1]);
                if (!channel.Verify(encryptedData, signature))
                {
                    return false;
                }
                data = channel.Decrypt(encryptedData);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        public bool TryDecryptVerify<TParameterType>(string channelId, string encryptedParameters, out TParameterType data)
        {
            if (!TryDecryptVerify(channelId, encryptedParameters, out var decryptedParameters))
            {
                data = default;
                return false;
            }
            data = JsonSerializer.Deserialize<TParameterType>(decryptedParameters);
            return true;
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
                challenge = challenge[EncryptedChallengePrefix.Length..];
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
