namespace ShortDash.Core.Plugins
{
    public interface IShortDashPluginLogger
    {
        public void LogDebug(string message, params object[] args);

        public void LogError(string message, params object[] args);

        public void LogInformation(string message, params object[] args);

        public void LogWarning(string message, params object[] args);
    }

    public interface IShortDashPluginLogger<T> : IShortDashPluginLogger
    {
    }
}