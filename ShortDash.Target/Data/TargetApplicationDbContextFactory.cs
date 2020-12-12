using Microsoft.Extensions.DependencyInjection;
using ShortDash.Core.Data;
using System;

namespace ShortDash.Target.Data
{
    public class TargetApplicationDbContextFactory : IApplicationDbContextFactory
    {
        private readonly IServiceProvider provider;

        public TargetApplicationDbContextFactory(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public TargetApplicationDbContext CreateDbContext()
        {
            return ActivatorUtilities.CreateInstance<TargetApplicationDbContext>(provider);
        }

        ApplicationDbContext IApplicationDbContextFactory.CreateDbContext()
        {
            return ActivatorUtilities.CreateInstance<TargetApplicationDbContext>(provider);
        }
    }
}
