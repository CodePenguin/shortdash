namespace ShortDash.Core.Interfaces
{
    public interface IEncryptedChannelService
    {
        void CloseChannel(string channelId);

        string EncryptCommand(string channelId, string data);

        string ExportEncryptedKey(string channelId);

        string ExportPublicKey();

        public string GenerateChallenge(string publicKey, out byte[] rawChallenge);

        public string GenerateChallengeResponse(string challenge, string publicKey);

        void ImportPrivateKey(string privateKeyXml);

        void OpenChannel(string channelId, string receiverPublicKeyXml);

        void OpenChannel(string channelId, string receiverPublicKeyXml, string encryptedKey);

        bool TryDecryptCommand(string channelId, string encryptedCommand, out string command);

        public bool VerifyChallengeResponse(byte[] challenge, string challengeResponse);
    }

    public interface IEncryptedChannelService<T> : IEncryptedChannelService
    {
    }
}
