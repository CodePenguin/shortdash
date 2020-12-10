using Microsoft.AspNetCore.Components;
using OtpNet;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace ShortDash.Server.Pages
{
    public sealed partial class Devices_Link : PageBase, IDisposable
    {
        private int urlIndex;
        private DeviceClaims DeviceClaims { get; set; } = new DeviceClaims();

        private string DeviceLinkCode { get; set; }

        private string DeviceLinkSecureUrl => Urls[urlIndex] + "/?c=" + HttpUtility.UrlEncode(DeviceLinkCode);

        private string DeviceLinkUrl => Urls[urlIndex];
        private bool Linking { get; set; }

        [Inject]
        private ServerUrlRetrieverService ServerUrlRetrieverService { get; set; }

        private string[] Urls { get; set; }

        public void Dispose()
        {
            StopLinking();
        }

        protected override Task OnParametersSetAsync()
        {
            Linking = false;
            LoadServerUrls();
            return base.OnParametersSetAsync();
        }

        private void Cancel()
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

        private void DeviceLinked(string deviceLinkCode, string deviceId)
        {
            if (!deviceLinkCode.Equals(DeviceLinkCode))
            {
                return;
            }
            StopLinking();
            ToastService.ShowSuccess("The device has been linked!", "LINKED");
            NavigationManager.NavigateTo("/devices/" + HttpUtility.UrlEncode(deviceId));
        }

        private void DeviceLinkedEvent(object sender, DeviceLinkedEventArgs eventArgs)
        {
            InvokeAsync(() => DeviceLinked(eventArgs.DeviceLinkCode, eventArgs.DeviceId));
        }

        private string GenerateDeviceLinkCode()
        {
            var otp = new Totp(KeyGeneration.GenerateRandomKey(10));
            return otp.ComputeTotp();
        }

        private void LoadServerUrls()
        {
            var serverUrls = new List<string>(ServerUrlRetrieverService.Urls);
            var baseUri = NavigationManager.BaseUri.Trim('/');
            var baseUriIndex = serverUrls.IndexOf(baseUri);
            if (baseUriIndex == -1 && !baseUri.Contains("localhost", StringComparison.InvariantCultureIgnoreCase) && !baseUri.Contains("127.0.0.1") && !baseUri.Contains("::1"))
            {
                serverUrls.Add(baseUri);
                baseUriIndex = serverUrls.Count - 1;
            }
            urlIndex = Math.Max(0, baseUriIndex);
            Urls = serverUrls.ToArray();
        }

        private void ShowNextUrl(int step)
        {
            urlIndex = Math.Min(Urls.Length - 1, Math.Max(0, urlIndex + step));
        }

        private async void StartLinking()
        {
            if (!await SecureContext.ValidateUserAsync())
            {
                return;
            }
            DeviceLinkCode = GenerateDeviceLinkCode();

            DeviceLinkService.OnDeviceLinked += DeviceLinkedEvent;

            var request = new LinkDeviceRequest { DeviceLinkCode = DeviceLinkCode };
            request.DeviceClaims.AddRange(DeviceClaims);
            DeviceLinkService.AddRequest(request);

            Linking = true;
        }

        private void StopLinking()
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
