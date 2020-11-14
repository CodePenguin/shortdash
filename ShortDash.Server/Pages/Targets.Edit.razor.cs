using Microsoft.AspNetCore.Components;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Targets_Edit : PageBase
    {
        [Parameter]
        public string DashboardActionTargetId { get; set; }

        private DashboardActionTarget DashboardActionTarget { get; set; }
        private bool IsDataSignatureValid { get; set; }
        private bool IsLoading { get; set; }

        [Inject]
        private TargetLinkService TargetLinkService { get; set; }

        protected async override Task OnParametersSetAsync()
        {
            IsLoading = true;
            DashboardActionTarget = null;
            IsDataSignatureValid = true;
            if (DashboardActionTargetId == DashboardActionTarget.ServerTargetId)
            {
                NavigationManager.NavigateTo("/targets");
                return;
            }
            await LoadDashboardActionTarget();
            IsLoading = false;
        }

        private void CancelChanges()
        {
            NavigationManager.NavigateTo("/targets");
        }

        private async void ConfirmUnlink()
        {
            var confirmed = await ConfirmDialog.ShowAsync(ModalService,
                title: "Unlink Target",
                message: "Are you sure you want to unlink this target?",
                confirmLabel: "Unlink",
                confirmClass: "btn-danger");
            if (!confirmed || !await SecureContext.ValidateUserAsync())
            {
                return;
            }
            await TargetLinkService.UnlinkTarget(DashboardActionTarget.DashboardActionTargetId);
            ToastService.ShowInfo("The target has been unlinked.", "UNLINKED");
            NavigationManager.NavigateTo("/targets");
        }

        private async Task LoadDashboardActionTarget()
        {
            DashboardActionTarget = await DashboardService.GetDashboardActionTargetAsync(DashboardActionTargetId);
            if (DashboardActionTarget == null)
            {
                return;
            }
            IsDataSignatureValid = DashboardService.VerifySignature(DashboardActionTarget);
        }

        private async void SaveChanges()
        {
            if (!await SecureContext.ValidateUserAsync())
            {
                return;
            }
            await DashboardService.UpdateDashboardActionTargetAsync(DashboardActionTarget);
            NavigationManager.NavigateTo("/targets");
        }
    }
}
