using System.Runtime.InteropServices;

namespace ShortDash.Core.Plugins
{
    public static class TargetEnvironment
    {
        public static bool IsFreeBSD => RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static string Platform()
        {
            if (IsFreeBSD)
            {
                return nameof(OSPlatform.FreeBSD);
            }
            if (IsLinux)
            {
                return nameof(OSPlatform.Linux);
            }
            if (IsOSX)
            {
                return nameof(OSPlatform.OSX);
            }
            if (IsWindows)
            {
                return nameof(OSPlatform.Windows);
            }
            return "Unknown";
        }
    }
}
