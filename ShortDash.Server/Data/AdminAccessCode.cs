namespace ShortDash.Server.Data
{
    public enum AdminAccessCodeType
    {
        DynamicTotp,
        Static,
    }

    public class AdminAccessCode
    {
        public AdminAccessCodeType AccessCodeType { get; set; }
        public string Data { get; set; }
    }
}
