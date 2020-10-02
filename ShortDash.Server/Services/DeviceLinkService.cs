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

        public static event EventHandler<DeviceLinkedEventArgs> OnDeviceLinked;

        public void AddRequest(LinkDeviceRequest request)
        {
            Requests[request.DeviceLinkCode] = request;
        }

        public void CancelRequest(LinkDeviceRequest request)
        {
            Requests.TryRemove(request.DeviceLinkCode, out _);
        }

        public async void DeviceLinked(LinkDeviceResponse response)
        {
            var dashboardDevice = await dashboardService.GetDashboardDeviceAsync(response.DeviceId);
            if (dashboardDevice == null)
            {
                return;
            }
            dashboardDevice.LinkedDateTime = DateTime.Now;
            await dashboardService.UpdateDashboardDeviceAsync(dashboardDevice);

            OnDeviceLinked?.Invoke(this, new DeviceLinkedEventArgs()
            {
                DeviceId = response.DeviceId,
                DeviceLinkCode = response.DeviceLinkCode
            });
        }

        public async Task<string> LinkDevice(string deviceLinkCode, string deviceName, string deviceId)
        {
            var claims = new List<DeviceClaim>();
            logger.LogDebug("Received LinkDevice message - {0} - {1}", deviceLinkCode, deviceId);
            if (Requests.TryRemove(deviceLinkCode, out var request))
            {
                claims.AddRange(request.Claims);
            }
            else if (IsValidAdminDeviceLinkCode(deviceLinkCode))
            {
                claims.Add(new DeviceClaim(ClaimTypes.Role, "Administrator"));
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
            dashboardDevice.Name = deviceName;
            dashboardDevice.SetClaimsList(claims);
            dashboardDevice.LastSeenDateTime = DateTime.Now;

            if (isNewDevice)
            {
                dashboardDevice = await dashboardService.AddDashboardDeviceAsync(dashboardDevice);
            }
            else
            {
                dashboardDevice = await dashboardService.UpdateDashboardDeviceAsync(dashboardDevice);
            }
            return GenerateAccessToken(deviceLinkCode, dashboardDevice);
        }

        public async Task<LinkDeviceResponse> ValidateAccessToken(string accessToken)
        {
            logger.LogDebug("Validating access token...");
            var validToken = encryptedChannelService.TryLocalDecryptVerify<LinkDeviceResponse>(accessToken, out var response);
            if (!validToken)
            {
                logger.LogDebug("Access token could be decrypted and verified.");
                return null;
            }

            var dashboardDevice = await dashboardService.GetDashboardDeviceAsync(response.DeviceId);
            if (dashboardDevice == null)
            {
                logger.LogDebug($"Device not found: {response.DeviceId}");
                return null;
            }

            var responseClaimText = JsonSerializer.Serialize(response.Claims);
            if (!dashboardDevice.Claims.Equals(responseClaimText))
            {
                logger.LogDebug("Claims in access token did not match.");
                return null;
            }

            logger.LogDebug("Access token was valid.");
            return response;
        }

        private string GenerateAccessToken(string deviceLinkCode, DashboardDevice dashboardDevice)
        {
            var response = new LinkDeviceResponse
            {
                Claims = dashboardDevice.GetClaimsList(),
                DeviceId = dashboardDevice.DashboardDeviceId,
                DeviceLinkCode = deviceLinkCode
            };
            return encryptedChannelService.LocalEncryptSigned(response);
        }

        private bool IsValidAdminDeviceLinkCode(string deviceLinkCode)
        {
            // TODO: Add better mechnism
            return deviceLinkCode == "123456";
        }
    }
}
