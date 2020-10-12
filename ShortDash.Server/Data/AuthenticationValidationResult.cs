using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class AuthenticationValidationResult
    {
        public AuthenticationValidationResult()
        {
            DeviceClaims = null;
            DeviceId = null;
            IsValid = false;
            RequiresUpdate = false;
        }

        public AuthenticationValidationResult(string deviceId) : base()
        {
            DeviceClaims = null;
            DeviceId = deviceId;
            IsValid = true;
            RequiresUpdate = false;
        }

        public AuthenticationValidationResult(string deviceId, DeviceClaims deviceClaims)
        {
            DeviceId = deviceId;
            DeviceClaims = deviceClaims;
            IsValid = true;
            RequiresUpdate = true;
        }

        public DeviceClaims DeviceClaims { get; set; }
        public string DeviceId { get; set; }
        public bool IsValid { get; set; }
        public bool RequiresUpdate { get; set; }
    }
}
