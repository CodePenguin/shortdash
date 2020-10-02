using System;
using System.Collections.Generic;

namespace ShortDash.Server.Data
{
    public class LinkDeviceRequest
    {
        public List<DeviceClaim> Claims { get; } = new List<DeviceClaim>();
        public string DeviceLinkCode { get; set; }

        public void AddClaim(string type, string value)
        {
            Claims.Add(new DeviceClaim { Type = type, Value = value });
        }
    }
}
