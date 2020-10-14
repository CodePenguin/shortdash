using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;

namespace ShortDash.Server.Pages
{
    public partial class Devices_Edit : PageBase
    {
        [Parameter]
        public string DashboardDeviceId { get; set; }

        private DashboardDevice DashboardDevice { get; set; }

        private DeviceClaims DeviceClaims { get; set; }

        [Inject]
        private DeviceLinkService DeviceLinkService { get; set; }

        private bool IsLoading => DashboardDevice == null;

        private bool SuccessfullyLinked { get; set; }

        protected async override Task OnParametersSetAsync()
        {
            DashboardDevice = null;
            DeviceClaims = null;
            await LoadDashboardDevice();

            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            QueryHelpers.ParseQuery(uri.Query).TryGetValue("linked", out var linkedValue);
            SuccessfullyLinked = !string.IsNullOrWhiteSpace(linkedValue);
        }

        private void CancelChanges()
        {
            NavigationManager.NavigateTo("/devices");
        }

        private async void ConfirmUnlink()
        {
            var confirmed = await ConfirmDialog.ShowAsync(ModalService,
                title: "Unlink Device",
                message: "Are you sure you want to unlink this device?",
                confirmLabel: "Unlink",
                confirmClass: "btn-danger");
            if (!confirmed || !await SecureContext.ValidateUserAsync())
            {
                return;
            }
            await DeviceLinkService.UnlinkDevice(DashboardDevice.DashboardDeviceId);
            NavigationManager.NavigateTo("/devices");
        }

        private async Task LoadDashboardDevice()
        {
            DashboardDevice = await DashboardService.GetDashboardDeviceAsync(DashboardDeviceId);
            DeviceClaims = DashboardDevice.GetDeviceClaimsList();
        }

        private async void SaveChanges()
        {
            if (!await SecureContext.ValidateUserAsync())
            {
                return;
            }
            var refreshClaims = !DeviceClaims.Equals(DashboardDevice.GetDeviceClaimsList());
            DashboardDevice.SetDeviceClaimsList(DeviceClaims);
            await DashboardService.UpdateDashboardDeviceAsync(DashboardDevice);
            if (refreshClaims)
            {
                DeviceLinkService.UpdateDeviceClaims(DashboardDevice.DashboardDeviceId, DeviceClaims);
            }
            NavigationManager.NavigateTo("/devices");
        }
    }
}
