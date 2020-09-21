using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Models;
using ShortDash.Core.Plugins;
using ShortDash.Core.Services;
using ShortDash.Server.Actions;
using ShortDash.Server.Data;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class DashboardActionService : ActionService
    {
        private readonly IEncryptedChannelService<TargetsHub> encryptedChannelService;
        private readonly ILogger<ActionService> logger;
        private readonly IHubContext<TargetsHub, ITargetsHub> targetsHubContext;

        public DashboardActionService(ILogger<ActionService> logger, PluginService pluginService, IServiceProvider serviceProvider,
            IHubContext<TargetsHub, ITargetsHub> targetsHubContext, IEncryptedChannelService<TargetsHub> encryptedChannelService) : base(logger, pluginService, serviceProvider)
        {
            this.logger = logger;
            this.targetsHubContext = targetsHubContext;
            this.encryptedChannelService = encryptedChannelService;
        }

        public Task Execute(DashboardAction dashboardAction, bool toggleState)
        {
            // Forward targeted actions to the specific target
            if (!dashboardAction.DashboardActionTargetId.Equals(DashboardActionTarget.ServerTargetId))
            {
                var targetId = dashboardAction.DashboardActionTargetId;
                logger.LogDebug($"Forwarding action to Target {targetId}: {dashboardAction.ActionTypeName}");
                var parameters = new ExecuteActionParameters
                {
                    ActionTypeName = dashboardAction.ActionTypeName,
                    Parameters = dashboardAction.Parameters,
                    ToggleState = toggleState
                };
                var encryptedParameters = EncryptParameters(targetId, parameters);
                return targetsHubContext.Clients.Groups(targetId).ExecuteAction(encryptedParameters);
            }
            // Handle non-targeted actions at the server
            if (dashboardAction.ActionTypeName.Equals(typeof(DashGroupAction).FullName))
            {
                return ExecuteGroupAction(dashboardAction);
            }
            return Execute(dashboardAction.ActionTypeName, dashboardAction.Parameters, toggleState);
        }

        protected override void RegisterActions()
        {
            RegisterActionType(typeof(DashGroupAction));
            RegisterActionType(typeof(DashLinkAction));
            RegisterActionType(typeof(DashSeparatorAction));
            base.RegisterActions();
        }

        private string EncryptParameters(string targetId, object parameters)
        {
            var data = JsonSerializer.Serialize(parameters);
            return encryptedChannelService.Encrypt(targetId, data);
        }

        private Task ExecuteGroupAction(DashboardAction dashboardAction)
        {
            logger.LogDebug($"Executing Group Action: {dashboardAction.Label}");
            foreach (var subAction in dashboardAction.DashboardSubActionChildren)
            {
                var toggleState = false;
                Execute(subAction.DashboardActionChild, toggleState);
            }
            return Task.CompletedTask;
        }
    }
}