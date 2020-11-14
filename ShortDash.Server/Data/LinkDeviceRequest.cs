namespace ShortDash.Server.Data
{
    public class LinkDeviceRequest
    {
        public DeviceClaims DeviceClaims { get; } = new DeviceClaims();
        public string DeviceLinkCode { get; set; }
    }
}
