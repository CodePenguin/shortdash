namespace ShortDash.Server.Data
{
    public static class ConfigurationSections
    {
        public const string AdminAccessCode = "AdminAccessCode";
        public const string DataProtectionSalt = "DataProtectionSalt";
        public const string DefaultSettings = "DefaultSettings";

        public static string Key(string purpose)
        {
            return "Key." + purpose;
        }
    }
}
