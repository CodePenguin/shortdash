using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ShortDash.Server.Data;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class DeviceLinkService
    {
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<string, LinkDeviceRequest> requests = new ConcurrentDictionary<string, LinkDeviceRequest>();

        public DeviceLinkService(ILogger<DeviceLinkService> logger)
        {
            this.logger = logger;
        }

        public event EventHandler<DeviceLinkedEventArgs> OnDeviceLinked;

        public void AddRequest(LinkDeviceRequest request)
        {
            requests[request.DeviceLinkCode] = request;
        }

        public void CancelRequest(LinkDeviceRequest request)
        {
            requests.TryRemove(request.DeviceLinkCode, out _);
        }

        public Task<string> LinkDevice(string deviceLinkCode, string deviceId)
        {
            string[] claims;
            string requesterConnectionId = null;
            logger.LogDebug("Received LinkDevice message - {0} - {1}", deviceLinkCode, deviceId);
            if (requests.TryRemove(deviceLinkCode, out var request))
            {
                claims = request.Claims;
                requesterConnectionId = request.RequesterConnectionId;
            }
            else if (IsValidAdminDeviceLinkCode(deviceLinkCode))
            {
                claims = new[] { "ADMINISTRATOR" };
            }
            else
            {
                logger.LogDebug("Received unexpected device link code - {0}", deviceLinkCode);
                return null;
            }

            // Store here

            OnDeviceLinked?.Invoke(this, new DeviceLinkedEventArgs() { DeviceId = deviceId, DeviceLinkCode = deviceLinkCode });

            return Task.FromResult(deviceId); // TODO: Generate access token with storage date and deviceID using HMACSHA256?
        }

        private bool IsValidAdminDeviceLinkCode(string deviceLinkCode)
        {
            return deviceLinkCode == "123456";
        }
    }
}
