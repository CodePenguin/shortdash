using System;
using System.Collections.Generic;

namespace ShortDash.Server.Data
{
    public class LinkDeviceRequest
    {
        public DeviceClaims Claims { get; } = new DeviceClaims();
        public string DeviceLinkCode { get; set; }
    }
}
