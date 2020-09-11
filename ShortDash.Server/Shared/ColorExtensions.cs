using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ShortDash.Server.Shared
{
    public static class ColorExtensions
    {
        private static Regex _regex = new Regex("^#([0-9a-f]{2}){3}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Color? FromHtmlString(string value)
        {
            if (TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        public static string TextClass(this Color color)
        {
            const float minimumContrast = 0.5f;
            var darkTextColor = Color.FromArgb(52, 58, 64);
            return Math.Abs(color.GetBrightness() - darkTextColor.GetBrightness()) >= minimumContrast ? "dark" : "light";
        }

        public static string ToHtmlString(this Color color)
        {
            return $"#{color.R:x2}{color.G:x2}{color.B:x2}";
        }

        public static bool TryParse(string value, out Color output)
        {
            output = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            Match match = _regex.Match(value);
            if (!match.Success)
            {
                return false;
            }

            var r = HexStringToByte(match.Groups[1].Captures[0].Value);
            var g = HexStringToByte(match.Groups[1].Captures[1].Value);
            var b = HexStringToByte(match.Groups[1].Captures[2].Value);

            output = Color.FromArgb(r, g, b);
            return true;
        }

        private static byte HexStringToByte(string hex)
        {
            if (hex.Length != 2)
            {
                return 0;
            }
            const string HexChars = "0123456789abcdef";
            hex = hex.ToLowerInvariant();
            var result = (HexChars.IndexOf(hex[0]) * 16) + HexChars.IndexOf(hex[1]);
            return (byte)result;
        }
    }
}