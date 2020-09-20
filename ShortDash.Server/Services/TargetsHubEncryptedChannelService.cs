using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortDash.Core.Services;

namespace ShortDash.Server.Services
{
    public class TargetsHubEncryptedChannelService : EncryptedChannelService<TargetsHub>
    {
        public TargetsHubEncryptedChannelService(IKeyStoreService<TargetsHubEncryptedChannelService> keyStore) : base(keyStore)
        {
        }
    }
}
