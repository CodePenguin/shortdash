using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
