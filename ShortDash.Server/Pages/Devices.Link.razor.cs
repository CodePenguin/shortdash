using Microsoft.AspNetCore.Components;
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
        private const int DeviceLinkCodeLength = 9;
        protected string DeviceLinkCode { get; set; }

        [Inject]
        protected DeviceLinkService DeviceLinkService { get; set; }

        protected string DeviceLinkUrl { get; set; }
        protected bool Linked { get; set; }
        protected bool Linking { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        public void Dispose()
        {
            StopLinking();
        }

        protected void Cancel()
        {
            if (Linking)
            {
                StopLinking();
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

        protected void DeviceLinked(string deviceLinkCode, string deviceId)
        {
            Console.WriteLine($"DeviceLinked: {deviceLinkCode} - {deviceId}");
            Linked = true;
            StopLinking();
            StateHasChanged();
        }

        protected void DeviceLinkedEvent(object sender, DeviceLinkedEventArgs eventArgs)
        {
            InvokeAsync(() => DeviceLinked(eventArgs.DeviceId, eventArgs.DeviceLinkCode));
        }

        protected override Task OnParametersSetAsync()
        {
            DeviceLinkUrl = NavigationManager.ToAbsoluteUri("/").ToString();
            Linking = false;
            return base.OnParametersSetAsync();
        }

        protected void StartLinking()
        {
            var baseCode = Math.Abs(Guid.NewGuid().ToString().GetHashCode() % Math.Pow(10, DeviceLinkCodeLength));
            DeviceLinkCode = baseCode.ToString().PadLeft(DeviceLinkCodeLength, '1');
            Linking = true;

            DeviceLinkService.OnDeviceLinked += DeviceLinkedEvent;

            var request = new LinkDeviceRequest
            {
                DeviceLinkCode = DeviceLinkCode
            };
            DeviceLinkService.AddRequest(request);
        }

        protected void StopLinking()
        {
            if (!Linking)
            {
                return;
            }
            Linking = false;
            DeviceLinkService.OnDeviceLinked -= DeviceLinkedEvent;
        }
    }
}
