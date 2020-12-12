using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShortDash.Target.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ShortDash.Target.Services
{
    public sealed class TargetHubHostService : BackgroundService
    {
        private readonly TargetHubClient targetHubClient;

        public TargetHubHostService(IServiceScopeFactory serviceScopeFactory)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TargetApplicationDbContext>();
            dbContext.Database.Migrate();
            targetHubClient = scope.ServiceProvider.GetRequiredService<TargetHubClient>();
        }

        public override void Dispose()
        {
            targetHubClient.Dispose();
            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return targetHubClient.ConnectAsync(stoppingToken);
        }
    }
}
