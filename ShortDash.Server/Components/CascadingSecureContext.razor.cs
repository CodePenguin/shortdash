using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ShortDash.Core.Extensions;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Services;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
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

        public string DeviceId { get; private set; }

        [Inject]
        protected AuthenticationEvents AuthenticationEvents { get; set; }

        [Inject]
        protected DeviceLinkService DeviceLinkService { get; set; }

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
            DeviceLinkService.OnDeviceClaimsUpdated -= DeviceClaimsUpdatedEvent;
            DeviceLinkService.OnDeviceUnlinked -= DeviceUnlinkedEvent;
            NavigationManager.LocationChanged -= LocationChanged;
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

        public async Task<bool> ValidateUser()
        {
            var user = (await AuthenticationStateTask).User;
            if (!user.Identity.IsAuthenticated)
            {
                return true;
            }
            var authenticationResult = await AuthenticationEvents.ValidatePrincipal(user);
            if (authenticationResult.IsValid && authenticationResult.DeviceId == DeviceId)
            {
                if (authenticationResult.RequiresUpdate)
                {
                    // Force reload to refresh the device claims
                    Logger.LogError("Device's access has changed and will be updated.");
                    NavigationManager.NavigateTo(NavigationManager.Uri, true);
                }
                return true;
            }
            Logger.LogError("Device's identity could not be verified.");
            NavigationManager.NavigateTo("/logout", true);
            return false;
        }

        public bool VerifyChallengeResponse(string rawChallenge, string challengeResponse)
        {
            return EncryptedChannelService.VerifyChallengeResponse(rawChallenge, challengeResponse);
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await InitializeEncryptedChannel();
                StateHasChanged();
            }
            await ValidateUser();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            channelId = Guid.NewGuid().ToString();
            NavigationManager.LocationChanged += LocationChanged;
            DeviceLinkService.OnDeviceClaimsUpdated += DeviceClaimsUpdatedEvent;
            DeviceLinkService.OnDeviceUnlinked += DeviceUnlinkedEvent;
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            DeviceId = null;
        }

        private void DeviceClaimsUpdatedEvent(object sender, DeviceClaimsUpdatedEventArgs e)
        {
            // Refresh the page so that the device claims are refreshed for the active device
            if (e.DeviceId == DeviceId)
            {
                NavigationManager.NavigateTo(NavigationManager.Uri, true);
            }
        }

        private void DeviceUnlinkedEvent(object sender, DeviceUnlinkedEventArgs e)
        {
            InvokeAsync(async () => await ValidateUser());
        }

        private async Task GetClientPublicKey()
        {
            var publicKey = await JSRuntime.InvokeAsync<string>("secureContext.exportPublicKey");
            channelId = EncryptedChannelService.OpenChannel(publicKey);
        }

        private string GetServerPublicKey()
        {
            return EncryptedChannelService.ExportPublicKey();
        }

        private async Task InitializeEncryptedChannel()
        {
            Logger.LogDebug("Establishing secure context...");
            await GetClientPublicKey();
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
            DeviceId = EncryptedChannelService.ReceiverId(channelId);
            Logger.LogDebug("Secure context established successfully.");
            IsInitialized = true;
        }

        private async void LocationChanged(object sender, LocationChangedEventArgs e)
        {
            await ValidateUser();
        }
    }
}
