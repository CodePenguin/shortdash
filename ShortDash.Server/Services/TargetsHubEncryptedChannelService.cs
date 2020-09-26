using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortDash.Core.Services;

namespace ShortDash.Server.Services
{
    public class TargetsHubEncryptedChannelService : EncryptedChannelService
    {
        public TargetsHubEncryptedChannelService(IKeyStoreService keyStore) : base(keyStore)
        {
        }

        protected override string KeyPurpose => typeof(TargetsHubEncryptedChannelService).FullName;
    }
}
