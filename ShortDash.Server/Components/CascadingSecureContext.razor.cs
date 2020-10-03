using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ShortDash.Core.Extensions;
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
        private string channelId;

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        public string ReceiverId { get; private set; }

        [Inject]
        protected IEncryptedChannelService EncryptedChannelService { get; set; }

        protected bool IsInitialized { get; private set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected ILogger<CascadingSecureContext> Logger { get; set; }

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

        public string GenerateChallenge(out string rawChallenge)
        {
            return EncryptedChannelService.GenerateChallenge(EncryptedChannelService.ExportPublicKey(channelId), out rawChallenge);
        }

        public bool VerifyChallengeResponse(string rawChallenge, string challengeResponse)
        {
            return EncryptedChannelService.VerifyChallengeResponse(rawChallenge, challengeResponse);
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
        }

        private async Task GetClientPublicKey()
        {
            var publicKey = await JSRuntime.InvokeAsync<string>("secureContext.exportPublicKey");
            channelId = EncryptedChannelService.OpenChannel(publicKey);
            ReceiverId = EncryptedChannelService.ReceiverId(channelId);
        }

        private string GetServerPublicKey()
        {
            return EncryptedChannelService.ExportPublicKey();
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
