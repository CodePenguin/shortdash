using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Shared
{
    public partial class NavMenu : ComponentBase
    {
        private bool collapseNavMenu = true;

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        protected List<Dashboard> Dashboards { get; set; } = new List<Dashboard>();

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

        protected async void LoadDashboards()
        {
            Dashboards = await DashboardService.GetDashboardsAsync();
        }

        protected override void OnParametersSet()
        {
            LoadDashboards();
        }

        protected async void ShowAddDashboardDialog()
        {
            var result = await AddDashboardDialog.ShowAsync(ModalService);
            if (result.Cancelled) { return; }
            var dashboard = new Dashboard { Title = result.Data.ToString() };
            await DashboardService.AddDashboardAsync(dashboard);
            LoadDashboards();
            StateHasChanged();
            NavigationManager.NavigateTo($"/dashboard/{dashboard.DashboardId}");
        }

        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }
    }
}