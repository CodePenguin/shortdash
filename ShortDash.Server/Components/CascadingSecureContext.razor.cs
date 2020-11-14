using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ShortDash.Core.Interfaces;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
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

        public bool InitializedDataProtection { get; private set; }

        [Inject]
        private AuthenticationEvents AuthenticationEvents { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        [Inject]
        private IAuthorizationService AuthorizationService { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; }

        [Inject]
        private ConfigurationService ConfigurationService { get; set; }

        [Inject]
        private IDataProtectionService DataProtectionService { get; set; }

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
            if (!InitializedDataProtection)
            {
                return;
            }
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
            InitializedDataProtection = DataProtectionService.Initialized();

            if (!InitializedDataProtection)
            {
                Logger.LogWarning("Data protection initialization failed.");
                CheckRecoveryMode();
            }
        }

        private void CheckRecoveryMode()
        {
            var recoveryCode = Configuration.GetValue<string>("recovery");
            if (string.IsNullOrWhiteSpace(recoveryCode))
            {
                return;
            }
            Logger.LogInformation("Importing provided recovery code...");
            try
            {
                recoveryCode = recoveryCode.Replace(" ", "");
                var encodedSalt = ConfigurationService.GetSection(ConfigurationSections.DataProtectionSalt);
                using var aes = Aes.Create();
                using var derivedBytes = new Rfc2898DeriveBytes(recoveryCode, salt: Convert.FromBase64String(encodedSalt), iterations: 100000, HashAlgorithmName.SHA256);
                DataProtectionService.SetKey(derivedBytes.GetBytes(aes.Key.Length));
                InitializedDataProtection = DataProtectionService.Initialized();
                Logger.LogInformation("Recovery code imported successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Recovery code import failed.");
            }
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
