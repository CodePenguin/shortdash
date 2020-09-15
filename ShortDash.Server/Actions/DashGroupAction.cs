using Microsoft.AspNetCore.Components;
using ShortDash.Core.Plugins;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ShortDash.Server.Actions
{
    [ShortDashAction(
        Title = "Action Group",
        Description = "Execute a group of actions.",
        ParametersType = typeof(DashGroupParameters))]
    [ShortDashActionDefaultSettings(
        Icon = "oi-project")]
    public class DashGroupAction : IShortDashAction
    {
        public bool Execute(object parametersObject, ref bool toggleState)
        {
            return true;
        }

        private class DashGroupParameters
        {
        }
    }
}