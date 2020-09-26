using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Services;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public sealed partial class CascadingSecureContext : ComponentBase, ISecureContext, IDisposable
    {
        private const string PublicKeyPrefix = "-----BEGIN PUBLIC KEY-----\n";

        private const string PublicKeySuffix = "\n-----END PUBLIC KEY-----";

        private string channelId;

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected string ClientPublicKey { get; private set; }

        [Inject]
        protected IEncryptedChannelService EncryptedChannelService { get; set; }

        protected bool IsInitialized { get; private set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected ILogger<CascadingSecureContext> Logger { get; set; }

        protected string ServerPublicKey { get; private set; }

        public string Decrypt(string value)
        {
            if (!EncryptedChannelService.TryDecrypt(channelId, value, out var decrypted))
            {
                return "";
            }
            return decrypted;
        }

        public void Dispose()
        {
            EncryptedChannelService.CloseChannel(channelId);
        }

        public string Encrypt(string value)
        {
            return EncryptedChannelService.Encrypt(channelId, value);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await InitializeEncryptedChannel();
                StateHasChanged();
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            channelId = Guid.NewGuid().ToString();
            ServerPublicKey = GetServerPublicKey();
        }

        private async Task GetClientPublicKey()
        {
            var publicKey = await JSRuntime.InvokeAsync<string>("secureContext.exportPublicKey");
            var startingIndex = publicKey.IndexOf(PublicKeyPrefix) + PublicKeyPrefix.Length;
            var endingIndex = publicKey.IndexOf(PublicKeySuffix, startingIndex);
            publicKey = publicKey[startingIndex..endingIndex].Replace("\n", "");
            var publicKeyBytes = Convert.FromBase64String(publicKey);
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            EncryptedChannelService.OpenChannel(channelId, rsa.ToXmlString(false));
        }

        private string GetServerPublicKey()
        {
            // Convert all Keys to PEM?
            using var rsa = RSA.Create();
            rsa.FromXmlString(EncryptedChannelService.ExportPublicKey());
            var key = rsa.ExportSubjectPublicKeyInfo();
            return PublicKeyPrefix + Convert.ToBase64String(key) + PublicKeySuffix;
        }

        private async Task InitializeEncryptedChannel()
        {
            Logger.LogDebug("Retrieving client public key...");
            await GetClientPublicKey();
            Logger.LogDebug("Sending session key to client...");
            var encryptedKey = EncryptedChannelService.ExportEncryptedKey(channelId);
            await JSRuntime.InvokeVoidAsync("secureContext.openChannel", GetServerPublicKey(), encryptedKey);
            IsInitialized = true;
        }
    }
}
