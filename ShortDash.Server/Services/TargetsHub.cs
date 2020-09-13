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

        public TargetsHub(ILogger<TargetsHub> logger)
        {
            this.logger = logger;
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

        public async Task SendMessage(string user, string message)
        {
            logger.LogDebug($"Received Message from {user}: {message}");
            await Clients.All.ReceiveMessage(user, message);
        }

        private string GetTargetId()
        {
            var httpContext = Context.GetHttpContext();
            return httpContext.Request.Query["targetId"];
        }
    }
}
