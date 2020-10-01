using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR.Client;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public sealed partial class Devices_Link : ComponentBase, IDisposable
    {
        protected HubConnection connection;
        private const int DeviceLinkCodeLength = 9;
        protected string DeviceLinkCode { get; set; }
        protected string DeviceLinkUrl { get; set; }
        protected bool Linked { get; set; }
        protected bool Linking { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        public async void Dispose()
        {
            await StopLinking();
        }

        protected async void Cancel()
        {
            if (Linking)
            {
                await StopLinking();
                StateHasChanged();
            }
            else
            {
                NavigationManager.NavigateTo("/devices");
            }
        }

        protected string DeviceLinkCodeDisplay()
        {
            return string.Join(" ", from Match m in Regex.Matches(DeviceLinkCode, @"\d{1,3}") select m.Value);
        }

        protected async void DeviceLinked(string encryptedParameters)
        {
            var decryptedParameters = encryptedParameters; // TODO: DECRYPT THIS
            var parameters = JsonSerializer.Deserialize<LinkDeviceParameters>(decryptedParameters);
            Console.WriteLine($"DeviceLinked: {parameters.DeviceId} - {parameters.DeviceLinkCode}");
            Linked = true;
            await StopLinking();
            StateHasChanged();
        }

        protected override Task OnParametersSetAsync()
        {
            DeviceLinkUrl = NavigationManager.ToAbsoluteUri("/").ToString();
            Linking = false;
            return base.OnParametersSetAsync();
        }

        protected async void StartLinking()
        {
            var baseCode = Math.Abs(Guid.NewGuid().ToString().GetHashCode() % Math.Pow(10, DeviceLinkCodeLength));
            DeviceLinkCode = baseCode.ToString().PadLeft(DeviceLinkCodeLength, '1');
            Linking = true;

            connection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri(DevicesHub.HubUrl))
                .Build();

            connection.On<string>("DeviceLinked", DeviceLinked);

            await connection.StartAsync();

            var request = new LinkDeviceRequest
            {
                DeviceLinkCode = DeviceLinkCode
            };
            var data = JsonSerializer.Serialize(request);
            var encryptedData = data; // TODO: ENCRYPT THIS
            await connection.SendAsync("LinkDeviceInfo", encryptedData);
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
    }
}
