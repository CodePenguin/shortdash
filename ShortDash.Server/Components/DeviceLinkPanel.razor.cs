using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace ShortDash.Server.Components
{
    public sealed partial class DeviceLinkPanel : ComponentBase, IDisposable
    {
        protected EditContext DeviceLinkEditContext { get; set; }

        [Inject]
        protected DeviceLinkService DeviceLinkService { get; set; }

        protected bool Linking { get; set; }

        [Inject]
        protected ILogger<DeviceLinkPanel> Logger { get; set; }

        protected DeviceLinkModel Model { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [CascadingParameter(Name = "SecureContext")]
        protected ISecureContext SecureContext { get; set; }

        public void Dispose()
        {
            StopLinking();
        }

        protected void Cancel()
        {
            if (Linking)
            {
                StopLinking();
            }
            else
            {
                Model.DeviceLinkCode = "";
            }
        }

        protected override Task OnParametersSetAsync()
        {
            Linking = false;
            Model = new DeviceLinkModel();
            DeviceLinkEditContext = new EditContext(Model);
            return base.OnParametersSetAsync();
        }

        protected async void StartLinking()
        {
            if (Linking || !DeviceLinkEditContext.Validate())
            {
                return;
            }
            var deviceLinkCode = Model.DeviceLinkCode.Replace(" ", "").Trim();
            var deviceId = SecureContext.ReceiverId;

            Linking = true;

            Logger.LogDebug("Linking Device: {0}", Model.DeviceLinkCode);
            var accessToken = await DeviceLinkService.LinkDevice(Model.DeviceLinkCode, SecureContext.ReceiverId);

            if (accessToken == null)
            {
                // TODO: Show error
                StopLinking();
            }

            Logger.LogDebug($"Device Linked: {deviceLinkCode} - {deviceId}");
            NavigationManager.NavigateTo("/login?accessToken=" + accessToken, true);
        }

        protected void StopLinking()
        {
            if (!Linking)
            {
                return;
            }
            Linking = false;
        }

        protected class DeviceLinkModel
        {
            [Required]
            public string DeviceLinkCode { get; set; }
        }
    }
}
