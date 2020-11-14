using ShortDash.Core.Interfaces;
using ShortDash.Server.Data;
using System.Linq;
using System.Text.Json;

namespace ShortDash.Server.Services
{
    public class ConfigurationService
    {
        private readonly IDataProtectionService dataProtectionService;
        private readonly ApplicationDbContextFactory dbContextFactory;

        public ConfigurationService(ApplicationDbContextFactory dbContextFactory, IDataProtectionService dataProtectionService)
        {
            this.dbContextFactory = dbContextFactory;
            this.dataProtectionService = dataProtectionService;
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

        public void RemoveSection(string sectionId)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var configurationSection = dbContext.ConfigurationSections
                .Where(s => s.ConfigurationSectionId == sectionId)
                .FirstOrDefault();
            if (configurationSection != null)
            {
                dbContext.Remove(configurationSection);
                dbContext.SaveChanges();
            }
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

        private string GetSection(string sectionId, bool secure)
        {
            var data = GetConfigurationSectionData(sectionId);
            return secure ? dataProtectionService.Unprotect(data) : data;
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

        private void SetSection(string sectionId, string sectionData, bool secure)
        {
            var data = secure ? dataProtectionService.Protect(sectionData) : sectionData;
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
    }
}
