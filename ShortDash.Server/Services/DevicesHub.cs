using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShortDash.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class DevicesHub : Hub<IDevicesHub>
    {
        public const string HubUrl = "/deviceshub";

        private readonly ILogger<DevicesHub> logger;

        public DevicesHub(ILogger<DevicesHub> logger)
        {
            this.logger = logger;
        }

        public Task DeviceLinked(string encryptedParameters)
        {
            logger.LogDebug("Received DeviceLinked message - {0}", encryptedParameters);
            var decryptedParameters = encryptedParameters; // TODO: DECRYPT THIS!!!
            var parameters = JsonSerializer.Deserialize<LinkDeviceParameters>(decryptedParameters);
            return Clients.Group(parameters.DeviceId).DeviceLinked(encryptedParameters);
        }

        public Task LinkDevice(string encryptedParameters)
        {
            var decryptedParameters = encryptedParameters; // TODO: DECRYPT THIS!!!
            var parameters = JsonSerializer.Deserialize<LinkDeviceParameters>(decryptedParameters);
            Groups.AddToGroupAsync(Context.ConnectionId, parameters.DeviceId);
            logger.LogDebug("Received LinkDevice message - {0}", encryptedParameters);
            return Clients.Others.LinkDevice(encryptedParameters);
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            logger.LogDebug("Device connected");
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            logger.LogDebug("Device disconnected");
            await base.OnDisconnectedAsync(exception);
        }

        public Task RegisterDevice(string encryptedDeviceCode, string devicePublicKey)
        {
            logger.LogDebug("Device attempting registration: {0} - {1}", encryptedDeviceCode, devicePublicKey);
            return Task.CompletedTask;
        }
    }
}
