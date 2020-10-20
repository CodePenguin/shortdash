using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShortDash.Server.Extensions
{
    public static class StringExtensions
    {
        public static string PlatformToIconClass(this string value)
        {
            return value switch
            {
                nameof(OSPlatform.FreeBSD) => "fab fa-freebsd",
                nameof(OSPlatform.Linux) => "fab fa-linux",
                nameof(OSPlatform.OSX) => "fab fa-apple",
                nameof(OSPlatform.Windows) => "fab fa-windows",
                _ => "fas fa-desktop"
            };
        }

        public static string Separate(this string value, int groupLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            return string.Join(" ", from Match m in Regex.Matches(value, @"\w{1," + groupLength.ToString() + "}") select m.Value);
        }
    }
}
