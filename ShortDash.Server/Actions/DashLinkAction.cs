﻿using Microsoft.AspNetCore.Components;
using ShortDash.Core.Plugins;
using System;
using System.Text.Json;

namespace ShortDash.Server.Actions
{
    [ShortDashAction(
        Title = "Go to Dashboard",
        Description = "Navigates to a specific dashboard.",
        ParametersType = typeof(DashLinkProcessParameters))]
    public class DashLinkAction : IShortDashAction
    {
        private readonly NavigationManager navigationManager;

        public DashLinkAction(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }

        public bool Execute(object parametersObject, ref bool toggleState)
        {
            var parameters = parametersObject as DashLinkProcessParameters;
            navigationManager.NavigateTo($"/dashboard/{parameters.DashboardId}");
            return true;
        }

        private class DashLinkProcessParameters
        {
            public int DashboardId { get; set; }
        }
    }
}