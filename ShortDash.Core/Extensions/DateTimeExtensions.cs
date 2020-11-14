using System;

namespace ShortDash.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToDisplayString(this DateTime value)
        {
            return value.Ticks == 0 ? "N/A" : value.ToString();
        }
    }
}
