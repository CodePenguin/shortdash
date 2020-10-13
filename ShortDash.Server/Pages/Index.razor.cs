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
    public partial class Index : ComponentBase
    {
        [CascadingParameter]
        public Task<AuthenticationState> AuthenticationStateTask { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        [CascadingParameter]
        public ISecureContext SecureContext { get; set; }

        [Inject]
        private AdminAccessCodeService AdministratorAccessCodeService { get; set; }

        private List<Dashboard> Dashboards { get; set; } = new List<Dashboard>();

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private DeviceLinkService DeviceLinkService { get; set; }

        private bool IsFirstRun { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private bool ShowAdminDeviceLinkMessage { get; set; }
        private ClaimsPrincipal User { get; set; }

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
            await DeviceLinkService.UnlinkDevice(User.Identity.Name);
            NavigationManager.NavigateTo("/logout", true);
        }

        protected void OnCompletedEvent(object sender, EventArgs eventArgs)
        {
            IsFirstRun = false;
            ShowAdminDeviceLinkMessage = true;
            StateHasChanged();
        }

        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            ShowAdminDeviceLinkMessage = false;
            IsFirstRun = !await AdministratorAccessCodeService.IsInitialized();
            User = (await AuthenticationStateTask).User;

            if (User.Identity.IsAuthenticated)
            {
                Dashboards = await DashboardService.GetDashboardsAsync();
            }
        }
    }
}
