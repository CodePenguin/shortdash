using Microsoft.Extensions.DependencyInjection;
using ShortDash.Core.Extensions;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Linq;
using System.Security.Cryptography;

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
            return !string.IsNullOrWhiteSpace(RetrieveKey(purpose, false));
        }

        public string RetrieveKey(string purpose, bool autoGenerate = true)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var configurationService = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
            var key = configurationService.GetSecureSection(ConfigurationSections.Key(purpose));
            if (!string.IsNullOrWhiteSpace(key))
            {
                return key;
            }
            if (autoGenerate)
            {
                return GenerateNewKey(purpose);
            }
            return null;
        }

        public void StoreKey(string purpose, string key)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var configurationService = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
            configurationService.SetSecureSection(ConfigurationSections.Key(purpose), key);
        }

        private string GenerateNewKey(string purpose)
        {
            var rsa = RSA.Create();
            var key = rsa.ExportPrivateKey();
            StoreKey(purpose, key);
            return key;
        }
    }
}
