using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortDash.Core.Services;

namespace ShortDash.Server.Services
{
    public class ServerEncryptedChannelService : EncryptedChannelService
    {
        public ServerEncryptedChannelService(IKeyStoreService keyStore) : base(keyStore)
        {
        }

        protected override string KeyPurpose => "ServerEncryptedChannelService";
    }
}
