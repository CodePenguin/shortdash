using ShortDash.Core.Interfaces;
using ShortDash.Core.Services;

namespace ShortDash.Target.Services
{
    public class TargetEncryptedChannelService : EncryptedChannelService
    {
        public TargetEncryptedChannelService(ISecureKeyStoreService keyStore) : base(keyStore)
        {
        }

        protected override string KeyPurpose => "TargetEncryptedChannelService";
    }
}
