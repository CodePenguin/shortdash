using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;

namespace ShortDash.Target.Services
{
    public class TargetHubRetryPolicy : IRetryPolicy
    {
        private ILogger<TargetHubRetryPolicy> logger;

        public TargetHubRetryPolicy(ILogger<TargetHubRetryPolicy> logger)
        {
            this.logger = logger;
        }

        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            var retryDelay = new[] { 0, 2000, 10000, 30000 };
            var retryIndex = retryContext.PreviousRetryCount % retryDelay.Length;
            var retryMilliseconds = retryDelay[retryIndex];
            logger.LogDebug($"Will attempt to reconnect in {retryMilliseconds} ms...");
            return new TimeSpan(0, 0, 0, 0, retryDelay[retryIndex]);
        }
    }
}
