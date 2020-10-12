using ShortDash.Server.Data;
using System;

namespace ShortDash.Server.Services
{
    public class DeviceClaimsUpdatedEventArgs : EventArgs
    {
        public DeviceClaims DeviceClaims { get; set; }
        public string DeviceId { get; set; }
    }
}
