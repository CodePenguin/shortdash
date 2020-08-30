using Microsoft.Extensions.Logging;
using ShortDash.Core.Plugins;

namespace ShortDash.Core.Services
{
    public class ShortDashPluginLogger<T> : IShortDashPluginLogger<T>
    {
        private readonly ILogger logger;

        public ShortDashPluginLogger(ILogger<T> logger)
        {
            this.logger = logger;
        }

        void IShortDashPluginLogger<T>.LogDebug(string message, params object[] args)
        {
            logger.LogDebug(message, args);
        }

        void IShortDashPluginLogger<T>.LogError(string message, params object[] args)
        {
            logger.LogError(message, args);
        }

        void IShortDashPluginLogger<T>.LogInformation(string message, params object[] args)
        {
            logger.LogInformation(message, args);
        }

        void IShortDashPluginLogger<T>.LogWarning(string message, params object[] args)
        {
            logger.LogWarning(message, args);
        }
    }
}