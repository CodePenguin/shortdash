using System.Collections.Generic;
using System.Security.Claims;

namespace ShortDash.Server.Data
{
    public class LinkDeviceRequest
    {
        public Claim[] Claims { get; set; }
        public string DeviceLinkCode { get; set; }
        public string RequesterConnectionId { get; set; }
    }
}
