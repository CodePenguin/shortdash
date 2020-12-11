using Microsoft.Extensions.DependencyInjection;
using ShortDash.Core.Data;
using System;

namespace ShortDash.Server.Data
{
    public class ServerApplicationDbContextFactory : IApplicationDbContextFactory
    {
        private readonly IServiceProvider provider;

        public ServerApplicationDbContextFactory(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public ServerApplicationDbContext CreateDbContext()
        {
            return ActivatorUtilities.CreateInstance<ServerApplicationDbContext>(provider);
        }

        ApplicationDbContext IApplicationDbContextFactory.CreateDbContext()
        {
            return ActivatorUtilities.CreateInstance<ServerApplicationDbContext>(provider);
        }
    }
}
