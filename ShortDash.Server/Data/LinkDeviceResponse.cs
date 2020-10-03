using System;
using System.Collections.Generic;

namespace ShortDash.Server.Data
{
    public class LinkDeviceResponse
    {
        public bool AllowSync { get; set; }
        public List<DeviceClaim> Claims { get; set; }
        public string DeviceId { get; set; }
        public string DeviceLinkCode { get; set; }
    }
}
