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
    public enum DashGroupType
    {
        Folder = 1,
        List = 2
    }

    [ShortDashAction(
        Title = "Action Group",
        Description = "Execute a group of actions.",
        ParametersType = typeof(DashGroupParameters))]
    [ShortDashActionDefaultSettings(
        Icon = "fas fa-project-diagram")]
    public class DashGroupAction : IShortDashAction
    {
        public bool Execute(object parametersObject, ref bool toggleState)
        {
            // Intentionally left blank as this type of action is handled in the DashboardActionService
            return true;
        }
    }

    public class DashGroupParameters
    {
        [Display(Name = "Group Type")]
        public DashGroupType DashGroupType { get; set; } = DashGroupType.Folder;
    }
}