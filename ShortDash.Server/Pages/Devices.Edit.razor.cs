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
    public partial class Devices_Edit : ComponentBase
    {
        [Parameter]
        public string DashboardDeviceId { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        [CascadingParameter]
        public ISecureContext SecureContext { get; set; }

        protected DashboardDevice DashboardDevice { get; set; }
        protected DeviceClaims DeviceClaims { get; set; }

        protected bool IsLoading => DashboardDevice == null;

        protected bool SuccessfullyLinked { get; set; }

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private DeviceLinkService DeviceLinkService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        protected void CancelChanges()
        {
            NavigationManager.NavigateTo("/devices");
        }

        protected async void ConfirmUnlink()
        {
            var confirmed = await ConfirmDialog.ShowAsync(ModalService,
                title: "Unlink Device",
                message: "Are you sure you want to unlink this device?",
                confirmLabel: "Unlink",
                confirmClass: "btn-danger");
            if (!confirmed || !await SecureContext.ValidateUser())
            {
                return;
            }
            await DeviceLinkService.UnlinkDevice(DashboardDevice.DashboardDeviceId);
            NavigationManager.NavigateTo("/devices");
        }

        protected async override Task OnParametersSetAsync()
        {
            DashboardDevice = null;
            DeviceClaims = null;
            await LoadDashboardDevice();

            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            QueryHelpers.ParseQuery(uri.Query).TryGetValue("linked", out var linkedValue);
            SuccessfullyLinked = !string.IsNullOrWhiteSpace(linkedValue);
        }

        protected async void SaveChanges()
        {
            if (!await SecureContext.ValidateUser())
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

        private async Task LoadDashboardDevice()
        {
            DashboardDevice = await DashboardService.GetDashboardDeviceAsync(DashboardDeviceId);
            DeviceClaims = DashboardDevice.GetDeviceClaimsList();
        }
    }
}
