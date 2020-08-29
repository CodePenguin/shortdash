namespace ShortDash.Core.Plugins
{
    public interface IShortDashAction
    {
        static string Description { get; }
        static string Title { get; }

        bool Execute(string parameters, ref bool toggleState);
    }
}