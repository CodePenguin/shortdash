using Microsoft.Extensions.DependencyInjection;
using ShortDash.Core.Interfaces;
using ShortDash.Server.Data;
using ShortDash.Server.Services;

namespace ShortDash.Core.Services
{
    public class ConfigurationKeyStoreService : IKeyStoreService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        public ConfigurationKeyStoreService(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public bool HasKey(string purpose)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var configurationService = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
            return !string.IsNullOrWhiteSpace(RetrieveKey(purpose));
        }

        public void RemoveKey(string purpose)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var configurationService = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
            configurationService.RemoveSection(ConfigurationSections.Key(purpose));
        }

        public string RetrieveKey(string purpose)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var configurationService = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
            var key = configurationService.GetSection(ConfigurationSections.Key(purpose));
            return !string.IsNullOrWhiteSpace(key) ? key : null;
        }

        public void StoreKey(string purpose, string key)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var configurationService = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
            configurationService.SetSection(ConfigurationSections.Key(purpose), key);
        }
    }
}
