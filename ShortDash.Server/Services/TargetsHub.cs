using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class TargetsHub : Hub<ITargetsHub>
    {
        private static ConcurrentDictionary<string, byte[]> challenges = new ConcurrentDictionary<string, byte[]>();
        private readonly DashboardService dashboardService;
        private readonly IEncryptedChannelService encryptedChannelService;
        private readonly ILogger<TargetsHub> logger;
        private readonly ILoggerFactory loggerFactory;

        public TargetsHub(ILogger<TargetsHub> logger, ILoggerFactory loggerFactory, IEncryptedChannelService<TargetsHub> encryptedChannelService, DashboardService dashboardService)
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
            var target = await dashboardService.GetDashboardActionTargetAsync(int.Parse(targetId));
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
            if (!challenges.TryGetValue(Context.ConnectionId, out var challenge))
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
            encryptedChannelService.OpenChannel(targetId, target.PublicKey);
            logger.LogDebug($"Sending session key to Target {targetId}.");
            await Clients.Caller.TargetAuthenticated(encryptedChannelService.ExportEncryptedKey(targetId));
        }

        public Task LogDebug(string category, string message, params object[] args)
        {
            var targetLogger = CreateTargetLogger(category);
            targetLogger.LogDebug(message, args);
            return Task.CompletedTask;
        }

        public Task LogError(string category, string message, params object[] args)
        {
            var targetLogger = CreateTargetLogger(category);
            targetLogger.LogError(message, args);
            return Task.CompletedTask;
        }

        public Task LogInformation(string category, string message, params object[] args)
        {
            var targetLogger = CreateTargetLogger(category);
            targetLogger.LogInformation(message, args);
            return Task.CompletedTask;
        }

        public Task LogWarning(string category, string message, params object[] args)
        {
            var targetLogger = CreateTargetLogger(category);
            targetLogger.LogWarning(message, args);
            return Task.CompletedTask;
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var targetId = GetTargetId();
            var target = (targetId == null) ? null : await dashboardService.GetDashboardActionTargetAsync(int.Parse(targetId));
            if (target == null)
            {
                logger.LogWarning($"Unknown target attempted connection.");
                return;
            }
            logger.LogDebug($"Sending authentication request to Target {targetId}...");
            var isNewRegistration = string.IsNullOrEmpty(target.PublicKey);
            var challenge = encryptedChannelService.GenerateChallenge(target.PublicKey, out var rawChallenge);
            challenges[Context.ConnectionId] = rawChallenge;
            await Clients.Caller.Authenticate(challenge, isNewRegistration ? encryptedChannelService.ExportPublicKey() : null);
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var targetId = GetTargetId();
            if (!string.IsNullOrEmpty(targetId))
            {
                logger.LogDebug($"Target {targetId} has disconnected.");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, targetId);
                encryptedChannelService.CloseChannel(targetId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        private ILogger CreateTargetLogger(string category)
        {
            var targetLogger = loggerFactory.CreateLogger("ShortDash.Target." + GetTargetId() + "." + category);
            return targetLogger;
        }

        private string GetTargetId()
        {
            var httpContext = Context.GetHttpContext();
            var targetId = httpContext.Request.Query["targetId"].FirstOrDefault();
            return (targetId != null) && int.TryParse(targetId, out _) ? targetId : string.Empty;
        }
    }
}
