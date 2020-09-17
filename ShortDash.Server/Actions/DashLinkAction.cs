using Microsoft.AspNetCore.Components;
using ShortDash.Core.Plugins;
using ShortDash.Server.Components;
using ShortDash.Server.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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

        public bool Execute(object parametersObject, ref bool toggleState)
        {
            var parameters = parametersObject as DashLinkParameters;
            navigationManager.NavigateTo($"/dashboard/{parameters.DashboardId}");
            return true;
        }

        private class DashLinkParameters
        {
            [Display(Name = "Dashboard")]
            [FormInput(Type = typeof(DashboardInputSelect))]
            public int DashboardId { get; set; }
        }
    }
}