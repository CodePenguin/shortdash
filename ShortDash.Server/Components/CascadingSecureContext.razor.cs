﻿using Microsoft.AspNetCore.Authorization;
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

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        public string DeviceId { get; private set; }

        [Inject]
        private AuthenticationEvents AuthenticationEvents { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        [Inject]
        private IAuthorizationService AuthorizationService { get; set; }

        [Inject]
        private IEncryptedChannelService EncryptedChannelService { get; set; }

        private bool IsInitialized { get; set; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [Inject]
        private ILogger<CascadingSecureContext> Logger { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        public async Task<bool> AuthorizeAsync(string policy)
        {
            var user = (await AuthenticationStateTask).User;
            return await AuthorizeAsync(user, policy);
        }

        public async Task<bool> AuthorizeAsync(ClaimsPrincipal user, string policy)
        {
            var result = await AuthorizationService.AuthorizeAsync(user, policy);
            return result.Succeeded;
        }

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

        public async Task GetClientPublicKey()
        {
            var publicKey = await JSRuntime.InvokeAsync<string>("secureContext.exportPublicKey");
            channelId = EncryptedChannelService.OpenChannel(publicKey);
        }

        public async Task<bool> ValidateUserAsync()
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
            await ValidateUserAsync();
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
            InvokeAsync(async () => await ValidateUserAsync());
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
            await ValidateUserAsync();
        }
    }
}
