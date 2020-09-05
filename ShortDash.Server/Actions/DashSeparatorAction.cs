using ShortDash.Core.Plugins;

namespace ShortDash.Server.Actions
{
    [ShortDashAction(
        Title = "Separator",
        Description = "Put some space between your actions.")]
    public class DashSeparatorAction : IShortDashAction
    {
        public bool Execute(object parametersObject, ref bool toggleState)
        {
            return true;
        }
    }
}