namespace ShortDash.Core.Interfaces
{
    public interface IEncryptedChannelService
    {
        void CloseChannel(string channelId);

        string Encrypt(string channelId, object data);

        string Encrypt(string channelId, string data);

        string EncryptSigned(string channelId, object data);

        string EncryptSigned(string channelId, string data);

        string ExportEncryptedKey(string channelId);

        string ExportPublicKey();

        string ExportPublicKey(string channelId);

        string GenerateChallenge(string publicKey, out string rawChallenge);

        string GenerateChallengeResponse(string challenge, string publicKey);

        string GetChannelId(string alias);

        void ImportPrivateKey(string privateKeyXml);

        string LocalEncryptForPublicKey(string publicKey, object data);

        string LocalEncryptSigned(string data);

        string LocalEncryptSigned(object parameters);

        string OpenChannel(string receiverPublicKeyXml);

        string OpenChannel(string receiverPublicKeyXml, string encryptedKey);

        string ReceiverId(string channelId);

        void RegisterChannelAlias(string channelId, string alias);

        string SenderId();

        bool TryDecrypt(string channelId, string encryptedPacket, out string data);

        bool TryDecryptVerify(string channelId, string encryptedPacket, out string data);

        bool TryDecryptVerify<TParameterType>(string channelId, string encryptedPacket, out TParameterType parameters);

        bool TryLocalDecrypt<TParameterType>(string encryptedPacket, out TParameterType parameters);

        bool TryLocalDecryptVerify<TParameterType>(string encryptedPacket, out TParameterType parameters);

        bool TryLocalDecryptVerify(string encryptedPacket, out string data);

        void UnregisterChannelAlias(string alias);

        bool VerifyChallengeResponse(string rawChallenge, string challengeResponse);
    }
}
