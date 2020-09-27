using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public interface IDevicesHub
    {
        public Task DeviceLinked(string encryptedParameters);

        public Task LinkDevice(string encryptedParameters);
    }
}
