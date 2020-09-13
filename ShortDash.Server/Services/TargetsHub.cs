using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class TargetsHub : Hub<ITargetsHub>
    {
        private readonly ILogger<TargetsHub> logger;
        private readonly ILoggerFactory loggerFactory;

        public TargetsHub(ILogger<TargetsHub> logger, ILoggerFactory loggerFactory)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
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

        public override async Task OnConnectedAsync()
        {
            var targetId = GetTargetId();
            if (targetId != null)
            {
                logger.LogDebug($"Target {targetId} has connected.");
                await Groups.AddToGroupAsync(Context.ConnectionId, targetId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var targetId = GetTargetId();
            if (targetId != null)
            {
                logger.LogDebug($"Target {targetId} has disconnected.");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, targetId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public Task SendMessage(string user, string message)
        {
            logger.LogDebug($"Received Message from {user}: {message}");
            return Clients.All.ReceiveMessage(user, message);
        }

        private ILogger CreateTargetLogger(string category)
        {
            var targetLogger = loggerFactory.CreateLogger("ShortDash.Target." + GetTargetId() + "." + category);
            return targetLogger;
        }

        private string GetTargetId()
        {
            var httpContext = Context.GetHttpContext();
            return httpContext.Request.Query["targetId"];
        }
    }
}
