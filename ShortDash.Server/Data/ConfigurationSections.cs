using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public static class ConfigurationSections
    {
        public const string AdminAccessCode = "AdminAccessCode";
        public const string DefaultSettings = "DefaultSettings";

        public static string Key(string purpose)
        {
            return "Key." + purpose;
        }
    }
}
