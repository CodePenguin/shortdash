using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShortDash.Server.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class DevicesHub : Hub<IDevicesHub>
    {
        public const string HubUrl = "/deviceshub";

        private static readonly ConcurrentDictionary<string, LinkDeviceRequest> LinkDeviceRequests = new ConcurrentDictionary<string, LinkDeviceRequest>();
        private readonly ILogger<DevicesHub> logger;

        public DevicesHub(ILogger<DevicesHub> logger)
        {
            this.logger = logger;
        }

        public Task LinkDevice(string encryptedData)
        {
            var decryptedData = encryptedData; // TODO: DECRYPT THIS!!!
            var parameters = JsonSerializer.Deserialize<LinkDeviceParameters>(decryptedData);
            string[] claims;
            string requesterConnectionId = null;
            logger.LogDebug("Received LinkDevice message - {0}", encryptedData);
            if (LinkDeviceRequests.TryRemove(parameters.DeviceLinkCode, out var request))
            {
                claims = request.Claims;
                requesterConnectionId = request.RequesterConnectionId;
            }
            else if (IsValidAdminDeviceLinkCode(parameters.DeviceLinkCode))
            {
                claims = new[] { "ADMINISTRATOR" };
            }
            else
            {
                logger.LogDebug("Received unexpected link device request - {0}", parameters.DeviceLinkCode);
                return Task.CompletedTask;
            }

            // STORE THE CLAIM DATA HERE

            var deviceCallback = Clients.Caller.DeviceLinked(encryptedData);
            var requesterCallback = (requesterConnectionId != null) ? Clients.Client(request.RequesterConnectionId).DeviceLinked(encryptedData) : null;
            return Task.WhenAll(deviceCallback, requesterCallback);
        }

        public Task LinkDeviceInfo(string encryptedData)
        {
            logger.LogDebug("Received LinkDeviceInfo message - {0}", encryptedData);
            var decryptedData = encryptedData; // TODO: DECRYPT THIS!!!
            var request = JsonSerializer.Deserialize<LinkDeviceRequest>(decryptedData);
            request.RequesterConnectionId = Context.ConnectionId;
            LinkDeviceRequests[request.DeviceLinkCode] = request;
            return Task.CompletedTask;
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            logger.LogDebug("Device connected");
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            logger.LogDebug("Device disconnected");

            // Remove any requests for the current connection
            var requests = LinkDeviceRequests.Values.Where(r => r.RequesterConnectionId == Context.ConnectionId).ToList();
            requests.ForEach(r => LinkDeviceRequests.TryRemove(r.DeviceLinkCode, out _));

            await base.OnDisconnectedAsync(exception);
        }

        private bool IsValidAdminDeviceLinkCode(string deviceLinkCode)
        {
            return (deviceLinkCode == "123456");
        }
    }
}
