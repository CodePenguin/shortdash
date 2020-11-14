using Microsoft.Extensions.DependencyInjection;
using System;

namespace ShortDash.Server.Data
{
    public class ApplicationDbContextFactory
    {
        private readonly IServiceProvider provider;

        public ApplicationDbContextFactory(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public ApplicationDbContext CreateDbContext()
        {
            return ActivatorUtilities.CreateInstance<ApplicationDbContext>(provider);
        }
    }
}
