using ShortDash.Core.Interfaces;
using ShortDash.Server.Data;

namespace ShortDash.Server.Extensions
{
    public static class ConfigurationExtensions
    {
        public static DefaultSettings DefaultSettings(this IConfigurationService configurationService)
        {
            return configurationService.GetSection<DefaultSettings>(ConfigurationSections.DefaultSettings);
        }
    }
}
