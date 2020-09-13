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

        public virtual void LogDebug(string message, params object[] args)
        {
            logger.LogDebug(message, args);
        }

        public virtual void LogError(string message, params object[] args)
        {
            logger.LogError(message, args);
        }

        public virtual void LogInformation(string message, params object[] args)
        {
            logger.LogInformation(message, args);
        }

        public virtual void LogWarning(string message, params object[] args)
        {
            logger.LogWarning(message, args);
        }
    }
}