using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Extensions;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShortDash.Server.Shared
{
    public partial class NavMenu : ComponentBase
    {
        private bool collapseNavMenu = true;

        [CascadingParameter]
        public Task<AuthenticationState> AuthenticationStateTask { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        protected List<Dashboard> Dashboards { get; set; } = new List<Dashboard>();

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

        private ClaimsPrincipal User { get; set; }

        protected bool CanAccessDashboard(int dashboardId)
        {
            return User.CanAccessDashboard(dashboardId);
        }

        protected async Task LoadDashboards()
        {
            Dashboards = await DashboardService.GetDashboardsAsync();
        }

        protected async override Task OnParametersSetAsync()
        {
            User = (await AuthenticationStateTask).User;
            await LoadDashboards();
        }

        protected async void ShowAddDashboardDialog()
        {
            var result = await AddDashboardDialog.ShowAsync(ModalService);
            if (result.Cancelled)
            {
                return;
            }
            var dashboard = new Dashboard { Name = result.Data.ToString() };
            await DashboardService.AddDashboardAsync(dashboard);
            await LoadDashboards();
            StateHasChanged();
            NavigationManager.NavigateTo($"/dashboard/{dashboard.DashboardId}");
        }

        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }
    }
}