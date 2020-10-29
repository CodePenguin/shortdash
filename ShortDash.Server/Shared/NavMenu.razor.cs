using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Extensions;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShortDash.Server.Shared
{
    public sealed partial class NavMenu : ComponentBase, IDisposable
    {
        private bool showNavMenu = false;

        [CascadingParameter]
        public Task<AuthenticationState> AuthenticationStateTask { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        [CascadingParameter]
        public ISecureContext SecureContext { get; set; }

        [Inject]
        private ConfigurationService ConfigurationService { get; set; }

        private List<Dashboard> Dashboards { get; set; } = new List<Dashboard>();

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private string NavMenuCssClass => showNavMenu ? "show" : "";

        [Inject]
        private NavMenuManager NavMenuManager { get; set; }

        private ClaimsPrincipal User { get; set; }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= LocationChanged;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            NavigationManager.LocationChanged += LocationChanged;
        }

        protected async override Task OnParametersSetAsync()
        {
            User = (await AuthenticationStateTask).User;
            await LoadDashboards();
        }

        private void HideNavMenu()
        {
            showNavMenu = false;
        }

        private async Task LoadDashboards()
        {
            Dashboards = await DashboardService.GetDashboardsAsync();
        }

        private void LocationChanged(object sender, LocationChangedEventArgs e)
        {
            HideNavMenu();
            StateHasChanged();
        }

        private async void ShowAddDashboardDialog()
        {
            HideNavMenu();
            var result = await AddDashboardDialog.ShowAsync(ModalService);
            if (result.Cancelled)
            {
                return;
            }
            var defaultSettings = ConfigurationService.DefaultSettings();
            var dashboard = new Dashboard
            {
                BackgroundColor = defaultSettings.DashboardBackgroundColor,
                Name = result.Data.ToString(),
            };
            await DashboardService.AddDashboardAsync(dashboard);
            await LoadDashboards();
            StateHasChanged();
            NavigationManager.NavigateTo($"/dashboard/{dashboard.DashboardId}");
        }

        private void ToggleNavMenu()
        {
            showNavMenu = !showNavMenu;
        }
    }
}