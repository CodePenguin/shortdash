using System.Collections.Generic;

namespace ShortDash.Server.Data
{
    public class LinkDeviceRequest
    {
        public string[] Claims { get; set; }
        public string DeviceLinkCode { get; set; }
        public string RequesterConnectionId { get; set; }
    }
}
