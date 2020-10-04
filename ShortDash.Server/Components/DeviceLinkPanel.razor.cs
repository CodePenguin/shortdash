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
        protected EditContext DeviceLinkEditContext { get; set; }

        [Inject]
        protected DeviceLinkService DeviceLinkService { get; set; }

        [Inject]
        protected IHttpContextAccessor HttpContextAccessor { get; set; }

        protected bool Linking { get; set; }

        protected DeviceLinkModel Model { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [CascadingParameter(Name = "SecureContext")]
        protected ISecureContext SecureContext { get; set; }

        protected bool ShowRetryMessage { get; set; }

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

        protected string GenerateDefaultDeviceName()
        {
            var userAgent = HttpContextAccessor.HttpContext.Request.Headers["User-Agent"];
            var parser = Parser.GetDefault();
            var client = parser.Parse(userAgent);
            return (client.Device.ToString().Equals("Other") ? "Device" : client.Device.ToString()) + " " + client.UA.Family;
        }

        protected override Task OnParametersSetAsync()
        {
            Linking = false;
            Model = new DeviceLinkModel();
            DeviceLinkEditContext = new EditContext(Model);

            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            QueryHelpers.ParseQuery(uri.Query).TryGetValue("c", out var deviceLinkCode);
            if (!string.IsNullOrWhiteSpace(deviceLinkCode) && Regex.IsMatch(deviceLinkCode, @"\d+"))
            {
                Model.DeviceLinkCode = deviceLinkCode;
                StartLinking();
            }

            return base.OnParametersSetAsync();
        }

        protected async void StartLinking()
        {
            ShowRetryMessage = false;
            if (Linking || !DeviceLinkEditContext.Validate())
            {
                return;
            }
            Linking = true;

            var deviceLinkCode = Model.DeviceLinkCode.Replace(" ", "").Trim();
            var deviceId = SecureContext.ReceiverId;
            var deviceName = GenerateDefaultDeviceName();
            var accessToken = await DeviceLinkService.LinkDevice(deviceLinkCode, deviceName, deviceId);

            if (accessToken == null)
            {
                ShowRetryMessage = true;
                StopLinking();
                StateHasChanged();
                return;
            }

            NavigationManager.NavigateTo("/login?accessToken=" + HttpUtility.UrlEncode(accessToken), true);
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
            [Display(Name = "Device Code")]
            public string DeviceLinkCode { get; set; }
        }
    }
}
