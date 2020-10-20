using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Models;
using ShortDash.Core.Services;
using ShortDash.Server.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ShortDash.Server.Services
{
    public class TargetsHub : Hub<ITargetsHub>
    {
        public const string HubUrl = "/targetshub";

        private static readonly IDictionary<string, string> Challenges = new ConcurrentDictionary<string, string>();
        private readonly DashboardService dashboardService;
        private readonly IEncryptedChannelService encryptedChannelService;
        private readonly ILogger<TargetsHub> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly TargetLinkService targetLinkService;

        public TargetsHub(ILogger<TargetsHub> logger, ILoggerFactory loggerFactory, IEncryptedChannelService encryptedChannelService,
            DashboardService dashboardService, TargetLinkService targetLinkService)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.encryptedChannelService = encryptedChannelService;
            this.dashboardService = dashboardService;
            this.targetLinkService = targetLinkService;
        }

        public async Task Authenticate(string challengeResponse)
        {
            var targetId = GetTargetId();
            logger.LogDebug($"Received authentication response for Target {targetId}...");
            var target = await dashboardService.GetDashboardActionTargetAsync(targetId);
            // Verify target is known
            if (target == null)
            {
                logger.LogError($"Target Authentication failed: Target {targetId} does not exist.");
                return;
            }
            // Verify target signature
            if (!dashboardService.VerifySignature(target))
            {
                logger.LogError($"Target Authentication failed: Invalid data signature for Target {targetId}.");
                return;
            }
            // Verify target has public key
            if (string.IsNullOrEmpty(target.PublicKey))
            {
                logger.LogError($"Target Authentication failed: Target {targetId} does not have a public key registered.");
                return;
            }
            // Verify challenge response
            if (!Challenges.TryGetValue(Context.ConnectionId, out var challenge))
            {
                logger.LogError("Target Authentication failed: Unable to find previous challenge.");
                return;
            }
            if (!encryptedChannelService.VerifyChallengeResponse(challenge, challengeResponse))
            {
                logger.LogError("Target Authentication failed: Invalid challenge response.");
                return;
            }
            // Target is authenticated so send the encrypted session key
            var channelId = encryptedChannelService.OpenChannel(target.PublicKey);
            encryptedChannelService.RegisterChannelAlias(channelId, targetId);
            await Groups.AddToGroupAsync(Context.ConnectionId, targetId);
            logger.LogDebug($"Sending session key to Target {targetId}.");
            await Clients.Caller.TargetAuthenticated(encryptedChannelService.ExportEncryptedKey(channelId));
            // Update target information
            target.LastSeenDateTime = DateTime.Now;
            await dashboardService.UpdateDashboardActionTargetAsync(target);
        }

        public async Task LinkTarget(string encryptedParameters)
        {
            var targetId = GetTargetId();
            if (targetId == null || !encryptedChannelService.TryLocalDecrypt<LinkTargetParameters>(encryptedParameters, out var parameters))
            {
                logger.LogError("Failed to decrypt LinkTarget request.");
                return;
            }
            logger.LogDebug($"Attempting to link target {targetId}...");
            if (!await targetLinkService.LinkTarget(parameters.TargetLinkCode, parameters.TargetId, parameters.Name, parameters.PublicKey))
            {
                return;
            }
            logger.LogDebug($"Target {targetId} linked.");
            var target = await dashboardService.GetDashboardActionTargetAsync(targetId);
            if (target == null)
            {
                return;
            }
            await SendAuthenticateRequest(target);
        }

        public Task Log(string encryptedParameters)
        {
            var channelId = GetChannelId();
            if (channelId == null || !encryptedChannelService.TryDecryptVerify<LogParameters>(channelId, encryptedParameters, out var parameters))
            {
                return default;
            }
            var message = parameters.Message;
            var targetLogger = CreateTargetLogger(parameters.Category);
            switch (parameters.LogLevel)
            {
                case LogLevel.Trace: targetLogger.LogTrace(message); break;
                case LogLevel.Debug: targetLogger.LogDebug(message); break;
                case LogLevel.Information: targetLogger.LogInformation(message); break;
                case LogLevel.Warning: targetLogger.LogWarning(message); break;
                case LogLevel.Error: targetLogger.LogError(message); break;
                case LogLevel.Critical: targetLogger.LogCritical(message); break;
            }
            return Task.CompletedTask;
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var targetId = GetTargetId();
            if (string.IsNullOrWhiteSpace(targetId))
            {
                logger.LogWarning($"Unknown target connected.");
                return;
            }
            var target = (targetId == null) ? null : await dashboardService.GetDashboardActionTargetAsync(targetId);
            if (target == null)
            {
                logger.LogDebug($"Sending server identification to Target {targetId}...");
                await Clients.Caller.Identify(encryptedChannelService.ExportPublicKey());
                return;
            }
            await SendAuthenticateRequest(target);
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var targetId = GetTargetId();
            if (!string.IsNullOrEmpty(targetId))
            {
                logger.LogDebug($"Target {targetId} has disconnected.");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, targetId);
                encryptedChannelService.CloseChannel(GetChannelId());
            }
            await base.OnDisconnectedAsync(exception);
        }

        private ILogger CreateTargetLogger(string category)
        {
            var targetLogger = loggerFactory.CreateLogger("ShortDash.Target." + GetTargetId() + "." + category);
            return targetLogger;
        }

        private string GetChannelId()
        {
            var targetId = GetTargetId();
            return encryptedChannelService.GetChannelId(targetId);
        }

        private string GetTargetId()
        {
            var httpContext = Context.GetHttpContext();
            var targetId = httpContext.Request.Query["targetId"].FirstOrDefault();
            return (targetId != null) && Regex.IsMatch(targetId, @"[A-Za-z0-9/=\+]{6}") ? targetId : null;
        }

        private async Task SendAuthenticateRequest(DashboardActionTarget target)
        {
            if (!dashboardService.VerifySignature(target))
            {
                logger.LogError($"Invalid data signature for Target {target.DashboardActionTargetId}.");
                return;
            }
            logger.LogDebug($"Sending authentication request to Target {target.DashboardActionTargetId}...");
            var challenge = encryptedChannelService.GenerateChallenge(target.PublicKey, out var rawChallenge);
            Challenges[Context.ConnectionId] = rawChallenge;
            await Clients.Caller.Authenticate(challenge);
        }
    }
}
