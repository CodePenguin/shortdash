namespace ShortDash.Core.Plugins
{
    public interface IShortDashAction
    {
        ShortDashActionResult Execute(object parameters, bool toggleState);
    }
}