using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ShortDash.Server.Components;
using ShortDash.Server.Services;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;

namespace ShortDash.Server.Pages
{
    public partial class DeviceSync : ComponentBase
    {
        [CascadingParameter]
        protected Task<AuthenticationState> AuthenticationStateTask { get; set; }

        [Inject]
        protected DeviceLinkService DeviceLinkService { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected ILogger<DeviceSync> Logger { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [CascadingParameter(Name = "SecureContext")]
        protected ISecureContext SecureContext { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            Logger.LogDebug("Sending device challenge...");
            var challenge = SecureContext.GenerateChallenge(out var rawChallenge);
            var challengeResponse = await JSRuntime.InvokeAsync<string>("secureContext.challenge", challenge);
            Logger.LogDebug("Verifying device challenge response...");

            Logger.LogDebug("Challenge: {0} - {1}", challenge, challengeResponse);

            if (SecureContext.VerifyChallengeResponse(rawChallenge, challengeResponse))
            {
                var user = (await AuthenticationStateTask).User;
                var accessToken = await DeviceLinkService.GenerateSyncToken(user.Identity.Name);
                NavigationManager.NavigateTo("/login?accessToken=" + HttpUtility.UrlEncode(accessToken), true);
            }
            else
            {
                NavigationManager.NavigateTo("/logout", true);
            }
        }
    }
}
