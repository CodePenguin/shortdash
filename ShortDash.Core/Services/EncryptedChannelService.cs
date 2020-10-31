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
    public abstract class EncryptedChannelService : IEncryptedChannelService, IDisposable
    {
        private const char CommandDelimiter = ':';
        private const string RSAChallengeType = "RSA";
        private static readonly IDictionary<string, string> ChannelAliases = new ConcurrentDictionary<string, string>();
        private static readonly IDictionary<string, EncryptedChannel> Channels = new ConcurrentDictionary<string, EncryptedChannel>();
        private readonly ISecureKeyStoreService keyStore;
        private RSA _rsa = null;

        protected EncryptedChannelService(ISecureKeyStoreService keyStore)
        {
            this.keyStore = keyStore;
        }

        protected bool IsDisposed { get; private set; }
        protected abstract string KeyPurpose { get; }

        protected RSA LocalRsa
        {
            get
            {
                if (_rsa == null)
                {
                    _rsa = RSA.Create();
                    _rsa.ImportPrivateKey(GetPrivateKey(keyStore));
                }
                return _rsa;
            }
        }

        public static string LocalEncryptForPublicKey(string publicKey, string data)
        {
            using var publicRsa = RSA.Create();
            publicRsa.ImportPublicKey(publicKey);
            using var aes = Aes.Create();
            var encryptedKey = publicRsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);
            var encryptedData = aes.Encrypt(data);
            return Convert.ToBase64String(encryptedKey) + CommandDelimiter + Convert.ToBase64String(encryptedData);
        }

        public void CloseChannel(string channelId)
        {
            if (!string.IsNullOrWhiteSpace(channelId))
            {
                Channels.Remove(channelId);
                var alias = ChannelAliases.FirstOrDefault(a => a.Value.Equals(channelId));
                UnregisterChannelAlias(alias.Key);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            return LocalRsa.ExportPublicKey();
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
            using var challengeRsa = RSA.Create();
            challengeRsa.ImportPublicKey(publicKey);
            challenge = challengeRsa.Encrypt(Encoding.UTF8.GetBytes(rawChallenge), RSAEncryptionPadding.Pkcs1);
            return RSAChallengeType + CommandDelimiter + Convert.ToBase64String(challenge);
        }

        public string GenerateChallengeResponse(string challenge, string publicKey)
        {
            try
            {
                var challengeType = GetChallengeType(challenge, out var data);
                if (challengeType != RSAChallengeType)
                {
                    throw new NotSupportedException($"Unsupported challenge type: {challengeType}");
                }
                var decryptedBytes = LocalRsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
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

        public string GetChannelId(string alias)
        {
            return !string.IsNullOrWhiteSpace(alias) && ChannelAliases.TryGetValue(alias, out var channelId) ? channelId : null;
        }

        public string LocalEncryptForPublicKey(string publicKey, object parameters)
        {
            var data = JsonSerializer.Serialize(parameters);
            return LocalEncryptForPublicKey(publicKey, data);
        }

        public string LocalEncryptSigned(string data)
        {
            using var aes = Aes.Create();
            var encryptedKey = LocalRsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);
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
            var decryptedBytes = LocalRsa.Decrypt(encryptedKeyBytes, RSAEncryptionPadding.Pkcs1);
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

        public string SenderId()
        {
            return LocalRsa.Fingerprint();
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

        public bool TryDecryptVerify<TParameterType>(string channelId, string encryptedParameters, out TParameterType parameters)
        {
            if (!TryDecryptVerify(channelId, encryptedParameters, out var decryptedParameters))
            {
                parameters = default;
                return false;
            }
            parameters = JsonSerializer.Deserialize<TParameterType>(decryptedParameters);
            return true;
        }

        public bool TryLocalDecrypt(string encryptedPacket, out string data)
        {
            data = null;
            try
            {
                if (encryptedPacket == null)
                {
                    return false;
                }
                var packetParts = encryptedPacket.Split(CommandDelimiter);
                if (packetParts.Length != 2)
                {
                    return false;
                }
                var encryptedKey = Convert.FromBase64String(packetParts[0]);
                var encryptedData = Convert.FromBase64String(packetParts[1]);
                var decryptedKey = LocalRsa.Decrypt(encryptedKey, RSAEncryptionPadding.Pkcs1);
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

        public bool TryLocalDecrypt<TParameterType>(string encryptedPacket, out TParameterType parameters)
        {
            if (!TryLocalDecrypt(encryptedPacket, out var decryptedData))
            {
                parameters = default;
                return false;
            }
            parameters = JsonSerializer.Deserialize<TParameterType>(decryptedData);
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
                if (!LocalRsa.VerifyData(encryptedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                {
                    return false;
                }
                var decryptedKey = LocalRsa.Decrypt(encryptedKey, RSAEncryptionPadding.Pkcs1);
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

        public bool TryLocalDecryptVerify<TParameterType>(string encryptedPacket, out TParameterType parameters)
        {
            if (!TryLocalDecryptVerify(encryptedPacket, out var decryptedData))
            {
                parameters = default;
                return false;
            }
            parameters = JsonSerializer.Deserialize<TParameterType>(decryptedData);
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
            var responseBytes = LocalRsa.Decrypt(responseData, RSAEncryptionPadding.Pkcs1);
            var compareChallenge = Encoding.UTF8.GetString(responseBytes);
            return rawChallenge.Equals(compareChallenge);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                _rsa?.Dispose();
                Channels.Clear();
            }

            IsDisposed = true;
        }

        private static string GenerateUniqueChannelId()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string GetChallengeType(string challenge, out byte[] data)
        {
            var challengeTypeIndex = challenge.IndexOf(CommandDelimiter);
            if (challengeTypeIndex == -1)
            {
                data = Array.Empty<byte>();
                return "N/A";
            }
            var dataIndex = challengeTypeIndex + 1;
            data = Convert.FromBase64String(challenge[dataIndex..]);
            return challenge[..challengeTypeIndex];
        }

        private string GetPrivateKey(ISecureKeyStoreService keyStore)
        {
            var key = keyStore.RetrieveSecureKey(KeyPurpose);
            if (key != null)
            {
                return key;
            }
            var rsa = RSA.Create();
            key = rsa.ExportPrivateKey();
            keyStore.StoreSecureKey(KeyPurpose, key);
            return key;
        }

        private byte[] RsaSign(byte[] data)
        {
            return LocalRsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
