using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using UAParser;

namespace ShortDash.Server.Components
{
    public sealed partial class DeviceLinkPanel : ComponentBase, IDisposable
    {
        [Inject]
        private DeviceLinkService DeviceLinkService { get; set; }

        [Inject]
        private IHttpContextAccessor HttpContextAccessor { get; set; }

        private bool Linking { get; set; }

        private DeviceLinkModel Model { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [CascadingParameter]
        private ISecureContext SecureContext { get; set; }

        private bool ShowRetryMessage { get; set; }

        public void Dispose()
        {
            StopLinking();
        }

        protected override Task OnParametersSetAsync()
        {
            Linking = false;
            Model = new DeviceLinkModel();

            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            QueryHelpers.ParseQuery(uri.Query).TryGetValue("c", out var deviceLinkCode);
            if (!string.IsNullOrWhiteSpace(deviceLinkCode) && Regex.IsMatch(deviceLinkCode, @"\d+"))
            {
                Model.DeviceLinkCode = deviceLinkCode;
                StartLinking();
            }

            return base.OnParametersSetAsync();
        }

        private void Cancel()
        {
            ShowRetryMessage = false;
            if (Linking)
            {
                StopLinking();
            }
            else
            {
                Model.DeviceLinkCode = "";
                StateHasChanged();
            }
        }

        private string GenerateDefaultDeviceName()
        {
            var userAgent = HttpContextAccessor.HttpContext.Request.Headers["User-Agent"];
            var parser = Parser.GetDefault();
            var client = parser.Parse(userAgent);
            return (client.Device.ToString().Equals("Other") ? "Device" : client.Device.ToString()) + " " + client.UA.Family;
        }

        private async void StartLinking()
        {
            ShowRetryMessage = false;
            if (Linking)
            {
                return;
            }
            Linking = true;

            var deviceLinkCode = Model.DeviceLinkCode.Replace(" ", "").Trim();
            var deviceId = SecureContext.DeviceId;
            var deviceName = GenerateDefaultDeviceName();
            var accessToken = await DeviceLinkService.LinkDevice(deviceLinkCode, deviceName, deviceId);

            if (accessToken == null)
            {
                ShowRetryMessage = Linking;
                StopLinking();
                StateHasChanged();
                return;
            }

            NavigationManager.NavigateTo("/login?accessToken=" + HttpUtility.UrlEncode(accessToken), true);
        }

        private void StopLinking()
        {
            if (!Linking)
            {
                return;
            }
            Linking = false;
        }

        private class DeviceLinkModel
        {
            [Required]
            [Display(Name = "Device Code")]
            public string DeviceLinkCode { get; set; }
        }
    }
}
