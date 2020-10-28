using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Index : PageBase
    {
        [Inject]
        private AdminAccessCodeService AdministratorAccessCodeService { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        private List<Dashboard> Dashboards { get; set; } = new List<Dashboard>();

        [Inject]
        private DeviceLinkService DeviceLinkService { get; set; }

        private bool HasActions { get; set; }
        private bool IsFirstRun { get; set; }

        private bool ShowAdminDeviceLinkMessage { get; set; }
        private ClaimsPrincipal User { get; set; }

        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            Dashboards = null;
            ShowAdminDeviceLinkMessage = false;
            IsFirstRun = !AdministratorAccessCodeService.IsInitialized();
            User = (await AuthenticationStateTask).User;
            HasActions = DashboardService.HasDashboardActions();

            if (User.Identity.IsAuthenticated)
            {
                Dashboards = await DashboardService.GetDashboardsAsync();
            }
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
            await DeviceLinkService.UnlinkDevice(User.Identity.Name);
            NavigationManager.NavigateTo("/logout", true);
        }

        private void OnCompletedEvent(object sender, EventArgs eventArgs)
        {
            IsFirstRun = false;
            ShowAdminDeviceLinkMessage = true;
            StateHasChanged();
        }

        private async void ShowAddDashboardDialog()
        {
            var result = await AddDashboardDialog.ShowAsync(ModalService);
            if (result.Cancelled)
            {
                return;
            }
            var dashboard = new Dashboard { Name = result.Data.ToString() };
            await DashboardService.AddDashboardAsync(dashboard);
            StateHasChanged();
            NavigationManager.NavigateTo($"/dashboard/{dashboard.DashboardId}");
        }
    }
}
