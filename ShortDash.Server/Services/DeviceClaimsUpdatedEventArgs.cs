using ShortDash.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class DeviceClaimsUpdatedEventArgs
    {
        public DeviceClaims Claims { get; set; }
        public string DeviceId { get; set; }
    }
}
