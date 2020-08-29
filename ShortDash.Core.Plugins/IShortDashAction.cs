namespace ShortDash.Core.Plugins
{
    public interface IShortDashAction
    {
        static string Title { get; }
        static string Description { get; }

        bool Execute(string parameters);
    }
}