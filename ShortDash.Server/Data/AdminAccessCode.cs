namespace ShortDash.Server.Data
{
    public enum AdminAccessCodeType
    {
        Static = 0,
        DynamicTotp = 1
    }

    public class AdminAccessCode
    {
        public AdminAccessCodeType AccessCodeType { get; set; }
        public string Data { get; set; }
    }
}
