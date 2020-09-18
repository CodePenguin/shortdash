namespace ShortDash.Core.Services
{
    public struct EncryptedCommand
    {
        public EncryptedCommand(string data, string signature)
        {
            Data = data;
            Signature = signature;
        }

        public string Data { get; }
        public string Signature { get; }
    }
}
