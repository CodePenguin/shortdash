using Microsoft.AspNetCore.Components;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System.Threading.Tasks;

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

        private bool IsDataSignatureValid { get; set; }
        private bool IsLoading { get; set; }

        protected async override Task OnParametersSetAsync()
        {
            IsLoading = true;
            DashboardDevice = null;
            DeviceClaims = null;
            IsDataSignatureValid = true;
            await LoadDashboardDevice();
            IsLoading = false;
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
            ToastService.ShowInfo("The device has been unlinked.", "UNLINKED");
            NavigationManager.NavigateTo("/devices");
        }

        private async Task LoadDashboardDevice()
        {
            DashboardDevice = await DashboardService.GetDashboardDeviceAsync(DashboardDeviceId);
            if (DashboardDevice == null)
            {
                return;
            }
            DeviceClaims = DashboardDevice.GetDeviceClaimsList();
            IsDataSignatureValid = DashboardService.VerifySignature(DashboardDevice);
        }

        private async void SaveChanges()
        {
            if (!await SecureContext.ValidateUserAsync() || !IsDataSignatureValid)
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
