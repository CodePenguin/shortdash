using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ShortDash.Target.Services
{
    public class TargetHubHostService : BackgroundService
    {
        private CancellationTokenSource cancellationTokenSource;
        private TargetHubClient targetHubClient;

        public TargetHubHostService(TargetHubClient targetHubClient)
        {
            this.targetHubClient = targetHubClient;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public override void Dispose()
        {
            cancellationTokenSource.Cancel();
            targetHubClient.Dispose();
            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return targetHubClient.ConnectAsync(cancellationTokenSource.Token);
        }
    }
}
