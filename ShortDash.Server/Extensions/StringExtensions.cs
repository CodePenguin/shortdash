using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShortDash.Server.Extensions
{
    public static class StringExtensions
    {
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
