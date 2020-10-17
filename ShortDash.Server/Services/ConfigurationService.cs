using Microsoft.AspNetCore.DataProtection;
using ShortDash.Server.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class ConfigurationService
    {
        private readonly IDataProtectionProvider dataProtectionProvider;
        private readonly ApplicationDbContextFactory dbContextFactory;

        public ConfigurationService(ApplicationDbContextFactory dbContextFactory, IDataProtectionProvider dataProtectionProvider)
        {
            this.dbContextFactory = dbContextFactory;
            this.dataProtectionProvider = dataProtectionProvider;
        }

        public string GetSection(string sectionId)
        {
            return GetSection(sectionId, false);
        }

        public T GetSection<T>(string sectionId) where T : new()
        {
            return GetSection<T>(sectionId, false);
        }

        public string GetSecureSection(string sectionId)
        {
            return GetSection(sectionId, true);
        }

        public T GetSecureSection<T>(string sectionId) where T : new()
        {
            return GetSection<T>(sectionId, true);
        }

        public void SetSection(string sectionId, string sectionData)
        {
            SetSection(sectionId, sectionData, false);
        }

        public void SetSection(string sectionId, object data)
        {
            SetSection(sectionId, data, false);
        }

        public void SetSecureSection(string sectionId, string sectionData)
        {
            SetSection(sectionId, sectionData, true);
        }

        public void SetSecureSection(string sectionId, object data)
        {
            SetSection(sectionId, data, true);
        }

        public void SetSecureSectionAsync(string sectionId, string sectionData)
        {
            SetSection(sectionId, sectionData, true);
        }

        private string GetConfigurationSectionData(string configurationSectionId)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var configurationSection = dbContext.ConfigurationSections
                .Where(s => s.ConfigurationSectionId == configurationSectionId)
                .FirstOrDefault();
            return configurationSection?.Data;
        }

        private IDataProtector GetDataProtector(string purpose)
        {
            return dataProtectionProvider.CreateProtector("ConfigurationService." + purpose);
        }

        private string GetSection(string sectionId, bool secure)
        {
            var data = GetConfigurationSectionData(sectionId);
            return secure ? Unprotect(sectionId, data) : data;
        }

        private T GetSection<T>(string sectionId, bool secure) where T : new()
        {
            var data = GetSection(sectionId, secure);
            if (string.IsNullOrWhiteSpace(data))
            {
                return new T();
            }
            return JsonSerializer.Deserialize<T>(data);
        }

        private string Protect(string sectionId, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            var dataProtector = GetDataProtector(sectionId);
            return dataProtector.Protect(value);
        }

        private void SetSection(string sectionId, string sectionData, bool secure)
        {
            var data = secure ? Protect(sectionId, sectionData) : sectionData;
            StoreConfigurationSectionData(sectionId, data);
        }

        private void SetSection(string sectionId, object data, bool secure)
        {
            var sectionData = JsonSerializer.Serialize(data);
            SetSection(sectionId, sectionData, secure);
        }

        private void StoreConfigurationSectionData(string configurationSectionId, string data)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var configurationSection = dbContext.ConfigurationSections
                .Where(s => s.ConfigurationSectionId == configurationSectionId)
                .FirstOrDefault();
            if (configurationSection == null)
            {
                configurationSection = new ConfigurationSection
                {
                    ConfigurationSectionId = configurationSectionId,
                    Data = data
                };
                dbContext.Add(configurationSection);
            }
            else
            {
                configurationSection.Data = data;
                dbContext.Update(configurationSection);
            }
            dbContext.SaveChanges();
        }

        private string Unprotect(string sectionId, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            var dataProtector = GetDataProtector(sectionId);
            return dataProtector.Unprotect(value);
        }
    }
}
