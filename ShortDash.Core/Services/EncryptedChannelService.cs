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
        private static readonly IDictionary<string, string> ChannelAliases = new ConcurrentDictionary<string, string>();
        private static readonly IDictionary<string, EncryptedChannel> Channels = new ConcurrentDictionary<string, EncryptedChannel>();
        private readonly RSA rsa;

        protected EncryptedChannelService(IKeyStoreService keyStore)
        {
            rsa = RSA.Create();
            rsa.ImportPrivateKey(keyStore.RetrieveKey(KeyPurpose));
        }

        ~EncryptedChannelService()
        {
            rsa.Dispose();
            Channels.Clear();
        }

        protected abstract string KeyPurpose { get; }

        public void CloseChannel(string channelId)
        {
            if (!string.IsNullOrWhiteSpace(channelId))
            {
                Channels.Remove(channelId);
                var alias = ChannelAliases.FirstOrDefault(a => a.Value.Equals(channelId));
                UnregisterChannelAlias(alias.Key);
            }
        }

        public string Encrypt(string channelId, object parameters)
        {
            var data = JsonSerializer.Serialize(parameters);
            return Encrypt(channelId, data);
        }

        public string Encrypt(string channelId, string data)
        {
            var channel = Channels[channelId];
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
            var channel = Channels[channelId];
            var encryptedData = channel.Encrypt(data);
            var signature = RsaSign(encryptedData);
            return Convert.ToBase64String(encryptedData) + CommandDelimiter + Convert.ToBase64String(signature);
        }

        public string ExportEncryptedKey(string channelId)
        {
            var channel = Channels[channelId];
            return channel.ExportEncryptedKey();
        }

        public string ExportPublicKey()
        {
            return rsa.ExportPublicKey();
        }

        public string ExportPublicKey(string channelId)
        {
            var channel = Channels[channelId];
            return channel.ExportPublicKey();
        }

        public string GenerateChallenge(string publicKey, out string rawChallenge)
        {
            // Use Aes class to generate random cryptographic data for the challenge data
            using var aes = Aes.Create();
            rawChallenge = Convert.ToBase64String(aes.IV.Concat(aes.Key).ToArray());
            byte[] challenge;
            var isEncryptedChallenge = !string.IsNullOrEmpty(publicKey);
            if (isEncryptedChallenge)
            {
                // If the public key is known, generate a specific challenge for that key
                using var challengeRsa = RSA.Create();
                challengeRsa.ImportPublicKey(publicKey);
                challenge = challengeRsa.Encrypt(Encoding.UTF8.GetBytes(rawChallenge), RSAEncryptionPadding.Pkcs1);
            }
            else
            {
                // If the challenge is for a new registration, send an unencrypted challenge
                challenge = Encoding.UTF8.GetBytes(rawChallenge);
            }
            // Add the encrypted challenge prefix if applicable
            return (isEncryptedChallenge ? EncryptedChallengePrefix : string.Empty) + Convert.ToBase64String(challenge);
        }

        public string GenerateChallengeResponse(string challenge, string publicKey)
        {
            try
            {
                var isEncryptedChallenge = IsEncryptedChallenge(challenge, out var data);
                var decryptedBytes = isEncryptedChallenge ? rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1) : data;
                var decryptedChallenge = Encoding.UTF8.GetString(decryptedBytes);
                using var challengeRsa = RSA.Create();
                challengeRsa.ImportPublicKey(publicKey);
                var challengeResponse = challengeRsa.Encrypt(Encoding.UTF8.GetBytes(decryptedChallenge), RSAEncryptionPadding.Pkcs1);
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

        public string GetChannelId(string alias)
        {
            return !string.IsNullOrWhiteSpace(alias) && ChannelAliases.TryGetValue(alias, out var channelId) ? channelId : null;
        }

        public void ImportPrivateKey(string privateKey)
        {
            rsa.ImportPrivateKey(privateKey);
        }

        public string LocalEncryptSigned(string data)
        {
            using var aes = Aes.Create();
            var encryptedKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);
            var encryptedData = aes.Encrypt(data);
            var signature = RsaSign(encryptedData);
            return Convert.ToBase64String(encryptedKey) + CommandDelimiter + Convert.ToBase64String(encryptedData) + CommandDelimiter + Convert.ToBase64String(signature);
        }

        public string LocalEncryptSigned(object parameters)
        {
            var data = JsonSerializer.Serialize(parameters);
            return LocalEncryptSigned(data);
        }

        public string OpenChannel(string receiverPublicKey)
        {
            var channel = new EncryptedChannel(receiverPublicKey);
            var channelId = GenerateUniqueChannelId();
            Channels.Add(channelId, channel);
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
            Channels.Add(channelId, channel);
            return channelId;
        }

        public string ReceiverId(string channelId)
        {
            var channel = Channels[channelId];
            return channel.ReceiverId;
        }

        public void RegisterChannelAlias(string channelId, string alias)
        {
            ChannelAliases.Add(alias, channelId);
        }

        public bool TryDecrypt(string channelId, string encryptedPacket, out string data)
        {
            data = null;
            try
            {
                var channel = Channels[channelId];
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
                if (encryptedPacket == null)
                {
                    return false;
                }
                var channel = Channels[channelId];
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

        public bool TryLocalDecryptVerify(string encryptedPacket, out string data)
        {
            data = null;
            try
            {
                if (encryptedPacket == null)
                {
                    return false;
                }
                var packetParts = encryptedPacket.Split(CommandDelimiter);
                if (packetParts.Length != 3)
                {
                    return false;
                }
                var encryptedKey = Convert.FromBase64String(packetParts[0]);
                var encryptedData = Convert.FromBase64String(packetParts[1]);
                var signature = Convert.FromBase64String(packetParts[2]);
                if (!rsa.VerifyData(encryptedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                {
                    return false;
                }
                var decryptedKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.Pkcs1);
                using var aes = Aes.Create();
                aes.Key = decryptedKey;
                data = aes.Decrypt(encryptedData);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        public bool TryLocalDecryptVerify<TParameterType>(string encryptedParameters, out TParameterType data)
        {
            if (!TryLocalDecryptVerify(encryptedParameters, out var decryptedParameters))
            {
                data = default;
                return false;
            }
            data = JsonSerializer.Deserialize<TParameterType>(decryptedParameters);
            return true;
        }

        public void UnregisterChannelAlias(string alias)
        {
            if (!string.IsNullOrWhiteSpace(alias))
            {
                ChannelAliases.Remove(alias);
            }
        }

        public bool VerifyChallengeResponse(string rawChallenge, string challengeResponse)
        {
            var responseData = Convert.FromBase64String(challengeResponse);
            var responseBytes = rsa.Decrypt(responseData, RSAEncryptionPadding.Pkcs1);
            var compareChallenge = Encoding.UTF8.GetString(responseBytes);
            return rawChallenge.Equals(compareChallenge);
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
