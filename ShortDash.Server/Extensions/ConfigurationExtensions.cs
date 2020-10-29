using ShortDash.Server.Data;
using ShortDash.Server.Services;

namespace ShortDash.Server.Extensions
{
    public static class ConfigurationExtensions
    {
        public static DefaultSettings DefaultSettings(this ConfigurationService configurationService)
        {
            return configurationService.GetSection<DefaultSettings>(ConfigurationSections.DefaultSettings);
        }
    }
}
