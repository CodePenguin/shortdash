using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public sealed partial class CascadingSecureContext : ComponentBase, ISecureContext, IDisposable
    {
        private string channelId;

        [CascadingParameter]
        public Task<AuthenticationState> AuthenticationStateTask { get; set; }

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

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

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
            Logger.LogDebug("Establishing secure context...");
            await GetClientPublicKey();
            var user = (await AuthenticationStateTask).User;
            if (user.Identity.IsAuthenticated && user.Identity.Name != ReceiverId)
            {
                Logger.LogError("Device was already linked but the identity did not match.");
                NavigationManager.NavigateTo("/logout", true);
                return;
            }
            Logger.LogDebug("Sending initialization request...");
            var challenge = GenerateChallenge(out var rawChallenge);
            var challengeResponse = await JSRuntime.InvokeAsync<string>("secureContext.initChannel", GetServerPublicKey(), challenge);
            Logger.LogDebug("Verifying initialization response...");
            if (!VerifyChallengeResponse(rawChallenge, challengeResponse))
            {
                Logger.LogError("Initialization challenge could not be verified.");
                NavigationManager.NavigateTo("/logout", true);
                return;
            }
            Logger.LogDebug("Sending session key...");
            var encryptedKey = EncryptedChannelService.ExportEncryptedKey(channelId);
            await JSRuntime.InvokeVoidAsync("secureContext.openChannel", encryptedKey);
            Logger.LogDebug("Secure context established successfully.");
            IsInitialized = true;
        }
    }
}
