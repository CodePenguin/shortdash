using Microsoft.AspNetCore.Components;
using ShortDash.Core.Plugins;
using System.Text.Json;

namespace ShortDash.Server.Actions
{
    public class DashLinkAction : IShortDashAction
    {
        private readonly NavigationManager navigationManager;

        public DashLinkAction(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }

        public bool Execute(string parameters, ref bool toggleState)
        {
            var dashLinkParameters = JsonSerializer.Deserialize<DashLinkProcessParameters>(parameters);
            navigationManager.NavigateTo($"/dashboard/{dashLinkParameters.DashboardId}");
            return true;
        }

        private class DashLinkProcessParameters
        {
            public int DashboardId { get; set; }
        }
    }
}