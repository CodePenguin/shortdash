using ShortDash.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Target.Services
{
    public class TargetEncryptedChannelService : EncryptedChannelService
    {
        public TargetEncryptedChannelService(IKeyStoreService keyStore) : base(keyStore)
        {
        }

        protected override string KeyPurpose => "TargetEncryptedChannelService";
    }
}
