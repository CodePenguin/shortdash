using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Interfaces;
using ShortDash.Server.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class DeviceLinkService
    {
        private static readonly ConcurrentDictionary<string, LinkDeviceRequest> Requests = new ConcurrentDictionary<string, LinkDeviceRequest>();
        private readonly DashboardService dashboardService;
        private readonly IEncryptedChannelService encryptedChannelService;
        private readonly ILogger logger;

        public DeviceLinkService(ILogger<DeviceLinkService> logger, DashboardService dashboardService, IEncryptedChannelService encryptedChannelService)
        {
            this.dashboardService = dashboardService;
            this.logger = logger;
            this.encryptedChannelService = encryptedChannelService;
        }

        public event EventHandler<DeviceLinkedEventArgs> OnDeviceLinked;

        public void AddRequest(LinkDeviceRequest request)
        {
            Requests[request.DeviceLinkCode] = request;
        }

        public void CancelRequest(LinkDeviceRequest request)
        {
            Requests.TryRemove(request.DeviceLinkCode, out _);
        }

        public async Task<string> LinkDevice(string deviceLinkCode, string deviceId)
        {
            var claims = new List<Claim>();
            logger.LogDebug("Received LinkDevice message - {0} - {1}", deviceLinkCode, deviceId);
            if (Requests.TryRemove(deviceLinkCode, out var request))
            {
                claims.AddRange(request.Claims ?? Array.Empty<Claim>());
            }
            else if (IsValidAdminDeviceLinkCode(deviceLinkCode))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
            }
            else
            {
                logger.LogDebug("Received unexpected device link code - {0}", deviceLinkCode);
                // Intentionally wait 5 seconds before returning for unknown codes
                await Task.Delay(5000);
                return null;
            }

            var isNewDevice = false;
            var dashboardDevice = await dashboardService.GetDashboardDeviceAsync(deviceId);
            if (dashboardDevice == null)
            {
                dashboardDevice = new DashboardDevice() { DashboardDeviceId = deviceId };
                isNewDevice = true;
            }

            dashboardDevice.DashboardDeviceId = deviceId;
            dashboardDevice.SetClaimsArray(claims);
            dashboardDevice.LastSeenDateTime = DateTime.Now;

            if (isNewDevice)
            {
                dashboardDevice = await dashboardService.AddDashboardDeviceAsync(dashboardDevice);
            }
            else
            {
                dashboardDevice = await dashboardService.UpdateDashboardDeviceAsync(dashboardDevice);
            }
            return GenerateAccessToken(dashboardDevice);
        }

        public void SendDeviceLinkedNotification(string deviceLinkCode, string deviceId)
        {
            OnDeviceLinked?.Invoke(this, new DeviceLinkedEventArgs()
            {
                DeviceId = deviceId,
                DeviceLinkCode = deviceLinkCode
            });
        }

        private string GenerateAccessToken(DashboardDevice dashboardDevice)
        {
            return encryptedChannelService.LocalEncryptSigned(dashboardDevice);
        }

        private bool IsValidAdminDeviceLinkCode(string deviceLinkCode)
        {
            // TODO: Add better mechnism
            return deviceLinkCode == "123456";
        }
    }
}
