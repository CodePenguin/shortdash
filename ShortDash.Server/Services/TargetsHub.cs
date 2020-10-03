using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Models;
using ShortDash.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public TargetsHub(ILogger<TargetsHub> logger, ILoggerFactory loggerFactory, IEncryptedChannelService encryptedChannelService, DashboardService dashboardService)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.encryptedChannelService = encryptedChannelService;
            this.dashboardService = dashboardService;
        }

        public async Task Authenticate(string challengeResponse, string publicKey)
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
            // Verify target registration state
            if (!string.IsNullOrEmpty(publicKey) && !string.IsNullOrEmpty(target.PublicKey))
            {
                logger.LogError($"Target Authentication failed: Target {targetId} is already registered.");
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
                logger.LogError($"Target Authentication failed: Invalid challenge response.");
                return;
            }
            // Save target's public key if applicable
            if (!string.IsNullOrEmpty(publicKey) && string.IsNullOrEmpty(target.PublicKey))
            {
                target.PublicKey = publicKey;
                await dashboardService.UpdateDashboardActionTargetAsync(target);
            }
            // Target is authenticated so send the encrypted session key
            var channelId = encryptedChannelService.OpenChannel(target.PublicKey);
            encryptedChannelService.RegisterChannelAlias(channelId, targetId);
            await Groups.AddToGroupAsync(Context.ConnectionId, targetId);
            logger.LogDebug($"Sending session key to Target {targetId}.");
            await Clients.Caller.TargetAuthenticated(encryptedChannelService.ExportEncryptedKey(channelId));
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
            var target = (targetId == null) ? null : await dashboardService.GetDashboardActionTargetAsync(targetId);
            if (target == null)
            {
                logger.LogWarning($"Unknown target attempted connection.");
                return;
            }
            logger.LogDebug($"Sending authentication request to Target {targetId}...");
            var isNewRegistration = string.IsNullOrEmpty(target.PublicKey);
            var challenge = encryptedChannelService.GenerateChallenge(target.PublicKey, out var rawChallenge);
            Challenges[Context.ConnectionId] = rawChallenge;
            await Clients.Caller.Authenticate(challenge, isNewRegistration ? encryptedChannelService.ExportPublicKey() : null);
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
            return (targetId != null) && Regex.IsMatch(targetId, "[A-Z0-9]{6}") ? targetId : null;
        }
    }
}
