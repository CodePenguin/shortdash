using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ShortDash.Core.Interfaces;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ShortDash.Server.Pages
{
    public sealed partial class Devices_Link : ComponentBase, IDisposable
    {
        private const int DeviceLinkCodeLength = 6;
        public DeviceClaims DeviceClaims { get; private set; } = new DeviceClaims();

        [CascadingParameter]
        public ISecureContext SecureContext { get; set; }

        protected string DeviceLinkCode { get; set; }

        protected string DeviceLinkSecureUrl { get; set; }

        [Inject]
        protected DeviceLinkService DeviceLinkService { get; set; }

        protected string DeviceLinkUrl { get; set; }

        [Inject]
        protected IEncryptedChannelService EncryptedChannelService { get; set; }

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

        protected void DeviceLinked(string deviceLinkCode, string deviceId)
        {
            if (!deviceLinkCode.Equals(DeviceLinkCode))
            {
                return;
            }
            StopLinking();
            NavigationManager.NavigateTo("/devices/" + HttpUtility.UrlEncode(deviceId) + "?linked=1");
        }

        protected void DeviceLinkedEvent(object sender, DeviceLinkedEventArgs eventArgs)
        {
            InvokeAsync(() => DeviceLinked(eventArgs.DeviceLinkCode, eventArgs.DeviceId));
        }

        protected override Task OnParametersSetAsync()
        {
            DeviceLinkUrl = NavigationManager.ToAbsoluteUri("/").ToString();
            Linking = false;
            return base.OnParametersSetAsync();
        }

        protected async void StartLinking()
        {
            if (!await SecureContext.ValidateUser())
            {
                return;
            }
            var baseCode = Math.Abs(Guid.NewGuid().ToString().GetHashCode() % Math.Pow(10, DeviceLinkCodeLength));
            DeviceLinkCode = baseCode.ToString().PadLeft(DeviceLinkCodeLength, '1');
            DeviceLinkSecureUrl = NavigationManager.ToAbsoluteUri("/?c=" + HttpUtility.UrlEncode(DeviceLinkCode)).ToString();

            DeviceLinkService.OnDeviceLinked += DeviceLinkedEvent;

            var request = new LinkDeviceRequest { DeviceLinkCode = DeviceLinkCode };
            request.DeviceClaims.AddRange(DeviceClaims);
            DeviceLinkService.AddRequest(request);

            Linking = true;
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
