using Microsoft.AspNetCore.Components;
using ShortDash.Core.Plugins;
using ShortDash.Server.Components;
using System.ComponentModel.DataAnnotations;

namespace ShortDash.Server.Actions
{
    [ShortDashAction(
        Title = "Switch to Dashboard",
        Description = "Navigates to a specific dashboard.",
        ParametersType = typeof(DashLinkParameters))]
    [ShortDashActionDefaultSettings(
        Icon = "fas fa-grip-horizontal")]
    public class DashLinkAction : IShortDashAction
    {
        private readonly NavigationManager navigationManager;

        public DashLinkAction(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }

        public ShortDashActionResult Execute(object parametersObject, bool toggleState)
        {
            var parameters = parametersObject as DashLinkParameters;
            navigationManager.NavigateTo($"/dashboard/{parameters.DashboardId}");
            return new ShortDashActionResult { Success = true, ToggleState = toggleState };
        }

        private class DashLinkParameters
        {
            [Display(Name = "Dashboard")]
            [FormInput(TypeName = nameof(DashboardInputSelect))]
            public int DashboardId { get; set; }
        }
    }
}