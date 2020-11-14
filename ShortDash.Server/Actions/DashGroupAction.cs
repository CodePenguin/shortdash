using ShortDash.Core.Plugins;
using System.ComponentModel.DataAnnotations;

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
        public ShortDashActionResult Execute(object parametersObject, bool toggleState)
        {
            // Intentionally left blank as this type of action is handled in the DashboardActionService
            return new ShortDashActionResult { Success = true, ToggleState = toggleState };
        }
    }

    public class DashGroupParameters
    {
        [Display(Name = "Group Type")]
        public DashGroupType DashGroupType { get; set; } = DashGroupType.Folder;
    }
}