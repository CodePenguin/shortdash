using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class ConfigurationService
    {
        private readonly DashboardService dashboardService;

#pragma warning disable IDE0052 // Remove unread private members
        private readonly IDataProtector dataProtector;
#pragma warning restore IDE0052 // Remove unread private members

        public ConfigurationService(DashboardService dashboardService, IDataProtectionProvider dataProtectionProvider)
        {
            this.dashboardService = dashboardService;
            dataProtector = dataProtectionProvider.CreateProtector(typeof(ConfigurationService).FullName);
        }

        public async Task<T> GetSectionAsync<T>() where T : new()
        {
            return await GetSectionAsync<T>(false);
        }

        public async Task<T> GetSecureSectionAsync<T>() where T : new()
        {
            return await GetSectionAsync<T>(true);
        }

        public async Task SetSectionAsync<T>(T data) where T : new()
        {
            await SetSectionAsync(data, false);
        }

        public async Task SetSecureSectionAsync<T>(T data) where T : new()
        {
            await SetSectionAsync(data, true);
        }

        private async Task<T> GetSectionAsync<T>(bool secure) where T : new()
        {
            var sectionName = GetSectionName<T>();
            var configurationSection = await dashboardService.GetConfigurationSectionAsync(sectionName);
            if (configurationSection == null)
            {
                return new T();
            }
            var data = secure ? Unprotect(configurationSection.Data) : configurationSection.Data;
            return JsonSerializer.Deserialize<T>(data);
        }

        private string GetSectionName<T>()
        {
            return typeof(T).FullName;
        }

        private string Protect(string value)
        {
#if DEBUG
            return value;
#else
            return dataProtector.Protect(value);
#endif
        }

        private async Task SetSectionAsync<T>(T data, bool secure) where T : new()
        {
            var sectionName = GetSectionName<T>();
            var sectionData = JsonSerializer.Serialize(data);
            var configurationSection = await dashboardService.GetConfigurationSectionAsync(sectionName);
            if (configurationSection == null)
            {
                configurationSection = new Data.ConfigurationSection
                {
                    ConfigurationSectionId = sectionName,
                    Data = secure ? Protect(sectionData) : sectionData
                };
                await dashboardService.AddConfigurationSectionAsync(configurationSection);
            }
            else
            {
                configurationSection.Data = secure ? Protect(sectionData) : sectionData;
                await dashboardService.UpdateConfigurationSectionAsync(configurationSection);
            }
        }

        private string Unprotect(string value)
        {
#if DEBUG
            return value;
#else
            return dataProtector.Unprotect(value);
#endif
        }
    }
}
