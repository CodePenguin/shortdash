namespace ShortDash.Core.Data
{
    public static class ConfigurationSections
    {
        public static string Key(string purpose)
        {
            return "Key." + purpose;
        }
    }
}
