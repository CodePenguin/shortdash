namespace ShortDash.Server.Data
{
    public class DeviceClaim
    {
        public DeviceClaim()
        {
        }

        public DeviceClaim(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }
        public string Value { get; set; }
    }
}
