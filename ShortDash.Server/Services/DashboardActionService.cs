using Blazored.Toast.Services;
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
        private readonly IEncryptedChannelService encryptedChannelService;
        private readonly ILogger<ActionService> logger;
        private readonly IHubContext<TargetsHub, ITargetsHub> targetsHubContext;
        private readonly IToastService toastService;

        public DashboardActionService(ILogger<ActionService> logger, PluginService pluginService, IServiceProvider serviceProvider,
            IHubContext<TargetsHub, ITargetsHub> targetsHubContext, IEncryptedChannelService encryptedChannelService,
            IToastService toastService) : base(logger, pluginService, serviceProvider)
        {
            this.logger = logger;
            this.targetsHubContext = targetsHubContext;
            this.encryptedChannelService = encryptedChannelService;
            this.toastService = toastService;
        }

        public Task Execute(DashboardAction dashboardAction, bool toggleState)
        {
            // Forward targeted actions to the specific target
            if (dashboardAction.DashboardActionTargetId != DashboardActionTarget.ServerTargetId)
            {
                var targetId = dashboardAction.DashboardActionTargetId;
                logger.LogDebug($"Forwarding action to Target {targetId}: {dashboardAction.ActionTypeName}");
                var parameters = new ExecuteActionParameters
                {
                    ActionTypeName = dashboardAction.ActionTypeName,
                    Parameters = dashboardAction.Parameters,
                    ToggleState = toggleState
                };
                var channelId = encryptedChannelService.GetChannelId(targetId);
                if (channelId == null)
                {
                    toastService.ShowError($"The Target {targetId} is not connected.");
                    return Task.CompletedTask;
                }
                var encryptedParameters = encryptedChannelService.EncryptSigned(channelId, parameters);
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