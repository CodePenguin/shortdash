using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ShortDash.Target.Services
{
    public sealed class TargetHubHostService : BackgroundService
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly TargetHubClient targetHubClient;

        public TargetHubHostService(TargetHubClient targetHubClient)
        {
            this.targetHubClient = targetHubClient;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public override void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            targetHubClient.Dispose();
            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return targetHubClient.ConnectAsync(cancellationTokenSource.Token);
        }
    }
}
