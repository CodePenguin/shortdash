using ShortDash.Core.Plugins;

namespace ShortDash.Server.Actions
{
    [ShortDashAction(
        Title = "Separator",
        Description = "Put some space between your actions.")]
    [ShortDashActionDefaultSettings(
        Label = "Separator")]
    public class DashSeparatorAction : IShortDashAction
    {
        public ShortDashActionResult Execute(object parametersObject, bool toggleState)
        {
            return new ShortDashActionResult { Success = true, ToggleState = toggleState };
        }
    }
}