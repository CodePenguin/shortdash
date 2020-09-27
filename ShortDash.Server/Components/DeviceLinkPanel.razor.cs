using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        protected HubConnection connection;

        protected EditContext DeviceLinkEditContext { get; set; }
        protected bool Linking { get; set; }

        [Inject]
        protected ILogger<DeviceLinkPanel> Logger { get; set; }

        protected DeviceLinkModel Model { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [CascadingParameter(Name = "SecureContext")]
        protected ISecureContext SecureContext { get; set; }

        public async void Dispose()
        {
            await StopLinking();
        }

        protected async void Cancel()
        {
            if (Linking)
            {
                await StopLinking();
            }
            else
            {
                Model.DeviceLinkCode = "";
            }
        }

        protected void DeviceLinked(string encryptedParameters)
        {
            var decryptedParameters = encryptedParameters; // TODO: DECRYPT THIS
            var parameters = JsonSerializer.Deserialize<LinkDeviceParameters>(decryptedParameters);
            Console.WriteLine($"DeviceLinked: {parameters.DeviceId} - {parameters.DeviceLinkCode}");
            var accessToken = parameters.DeviceId;
            NavigationManager.NavigateTo("/login?AccessToken=" + HttpUtility.UrlEncode(accessToken), true);
        }

        protected override Task OnParametersSetAsync()
        {
            Linking = false;
            connection = null;
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
            Model.DeviceLinkCode = Model.DeviceLinkCode.Replace(" ", "").Trim();

            Linking = true;

            connection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri(DevicesHub.HubUrl))
                .Build();

            connection.On<string>("DeviceLinked", DeviceLinked);

            await connection.StartAsync();

            Logger.LogDebug("Linking Device: {0}", Model.DeviceLinkCode);
            var parameters = new LinkDeviceParameters
            {
                DeviceId = SecureContext.ReceiverId,
                DeviceLinkCode = Model.DeviceLinkCode
            };
            var serialized = JsonSerializer.Serialize(parameters);
            var encryptedParameters = serialized; // TODO: ENCRYPT THIS!!!
            await connection.SendAsync("LinkDevice", encryptedParameters);
        }

        protected async Task StopLinking()
        {
            if (!Linking || connection == null)
            {
                return;
            }
            Linking = false;
            await connection.StopAsync();
            await connection.DisposeAsync();
            connection = null;
        }

        protected class DeviceLinkModel
        {
            [Required]
            public string DeviceLinkCode { get; set; }
        }
    }
}
