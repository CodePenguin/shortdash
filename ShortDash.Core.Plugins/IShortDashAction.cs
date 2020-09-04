namespace ShortDash.Core.Plugins
{
    public interface IShortDashAction
    {
        bool Execute(object parameters, ref bool toggleState);
    }
}