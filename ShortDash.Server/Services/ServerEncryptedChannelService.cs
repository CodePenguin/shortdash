using ShortDash.Core.Extensions;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Services;
using System;

namespace ShortDash.Server.Services
{
    public class ServerEncryptedChannelService : EncryptedChannelService
    {
        private readonly IDataProtectionService dataProtectionService;
        private readonly ISecureKeyStoreService keyStore;

        public ServerEncryptedChannelService(IDataProtectionService dataProtectionService, ISecureKeyStoreService keyStore) : base(keyStore)
        {
            this.dataProtectionService = dataProtectionService;
            this.keyStore = keyStore;

            if (dataProtectionService.Initialized())
            {
                dataProtectionService.AddKeyChangedEventHandler(DataProtectionkeyChangedEvent);
            }
        }

        protected override string KeyPurpose => "ServerEncryptedChannelService";

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                dataProtectionService.RemoveKeyChangedEventHandler(DataProtectionkeyChangedEvent);
            }

            base.Dispose(disposing);
        }

        private void DataProtectionkeyChangedEvent(object sender, EventArgs args)
        {
            var key = LocalRsa.ExportPrivateKey();
            keyStore.StoreSecureKey(KeyPurpose, key);
        }
    }
}
