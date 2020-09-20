using ShortDash.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Target.Services
{
    public class TargetHubClientEncryptedChannelService : EncryptedChannelService<TargetHubClient>
    {
        public TargetHubClientEncryptedChannelService(IKeyStoreService<TargetHubClientEncryptedChannelService> keyStore) : base(keyStore)
        {
        }
    }
}
