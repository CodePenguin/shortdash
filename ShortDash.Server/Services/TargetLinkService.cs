using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Models;
using ShortDash.Core.Plugins;
using ShortDash.Server.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public sealed class TargetLinkService
    {
        private static readonly ConcurrentDictionary<string, LinkTargetRequest> Requests = new ConcurrentDictionary<string, LinkTargetRequest>();
        private readonly DashboardService dashboardService;
        private readonly IEncryptedChannelService encryptedChannelService;
        private readonly ILogger logger;
        private readonly IHubContext<TargetsHub, ITargetsHub> targetsHubContext;

        public TargetLinkService(ILogger<TargetLinkService> logger, DashboardService dashboardService, IEncryptedChannelService encryptedChannelService,
            IHubContext<TargetsHub, ITargetsHub> targetsHubContext)
        {
            this.dashboardService = dashboardService;
            this.logger = logger;
            this.encryptedChannelService = encryptedChannelService;
            this.targetsHubContext = targetsHubContext;
        }

        public static event EventHandler<TargetLinkedEventArgs> OnTargetLinked;

        public static void AddRequest(LinkTargetRequest request)
        {
            Requests[request.TargetLinkCode] = request;
        }

        public static void CancelRequest(LinkTargetRequest request)
        {
            Requests.TryRemove(request.TargetLinkCode, out _);
        }

        public async Task<bool> LinkTarget(string targetLinkCode, string targetId, string name, string platform, string publicKey)
        {
            if (string.IsNullOrWhiteSpace(targetLinkCode) || !Requests.TryRemove(targetLinkCode, out var _))
            {
                logger.LogDebug("Received unexpected target link code - {0}", targetLinkCode);
                return false;
            }

            var isNewTarget = false;
            var dashboardActionTarget = await dashboardService.GetDashboardActionTargetAsync(targetId);
            if (dashboardActionTarget == null)
            {
                dashboardActionTarget = new DashboardActionTarget() { DashboardActionTargetId = targetId };
                isNewTarget = true;
            }

            dashboardActionTarget.DashboardActionTargetId = targetId;
            dashboardActionTarget.Name = name;
            dashboardActionTarget.Platform = platform;
            dashboardActionTarget.PublicKey = publicKey;
            dashboardActionTarget.LastSeenDateTime = DateTime.Now;
            dashboardActionTarget.LinkedDateTime = dashboardActionTarget.LastSeenDateTime;

            if (isNewTarget)
            {
                await dashboardService.AddDashboardActionTargetAsync(dashboardActionTarget);
            }
            else
            {
                await dashboardService.UpdateDashboardActionTargetAsync(dashboardActionTarget);
            }
            OnTargetLinked?.Invoke(this, new TargetLinkedEventArgs()
            {
                TargetId = targetId,
                TargetLinkCode = targetLinkCode
            });
            return true;
        }

        public async Task UnlinkTarget(string targetId)
        {
            var dashboardActionTarget = await dashboardService.GetDashboardActionTargetAsync(targetId);
            if (dashboardActionTarget == null)
            {
                return;
            }
            var parameters = new UnlinkTargetParameters { TargetId = targetId };
            var channelId = encryptedChannelService.GetChannelId(targetId);
            if (channelId != null)
            {
                var encryptedParameters = encryptedChannelService.EncryptSigned(channelId, parameters);
                await targetsHubContext.Clients.Groups(targetId).UnlinkTarget(encryptedParameters);
                encryptedChannelService.CloseChannel(channelId);
            }
            await dashboardService.DeleteDashboardActionTargetAsync(dashboardActionTarget);
        }
    }
}
