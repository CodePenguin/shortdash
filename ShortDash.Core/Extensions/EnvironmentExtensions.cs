using System.Runtime.InteropServices;

namespace ShortDash.Core.Extensions
{
    public static class EnvironmentExtensions
    {
        public static string Platform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                return nameof(OSPlatform.FreeBSD);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return nameof(OSPlatform.Linux);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return nameof(OSPlatform.OSX);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return nameof(OSPlatform.Windows);
            }
            return "Unknown";
        }
    }
}
