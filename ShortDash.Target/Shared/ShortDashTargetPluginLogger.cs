using Microsoft.Extensions.Logging;
using ShortDash.Core.Services;
using ShortDash.Target.Services;

namespace ShortDash.Target.Shared
{
    public class ShortDashTargetPluginLogger<T> : ShortDashPluginLogger<T>
    {
        private readonly TargetHubClient targetHubClient;

        public ShortDashTargetPluginLogger(ILogger<T> logger, TargetHubClient targetHubClient) : base(logger)
        {
            this.targetHubClient = targetHubClient;
        }

        public override void LogDebug(string message, params object[] args)
        {
            base.LogDebug(message, args);
            targetHubClient.LogDebug<T>(message, args);
        }

        public override void LogError(string message, params object[] args)
        {
            base.LogError(message, args);
            targetHubClient.LogError<T>(message, args);
        }

        public override void LogInformation(string message, params object[] args)
        {
            base.LogInformation(message, args);
            targetHubClient.LogInformation<T>(message, args);
        }

        public override void LogWarning(string message, params object[] args)
        {
            base.LogWarning(message, args);
            targetHubClient.LogWarning<T>(message, args);
        }
    }
}
