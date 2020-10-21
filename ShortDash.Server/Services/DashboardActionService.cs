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
        private readonly DashboardService dashboardService;
        private readonly IEncryptedChannelService encryptedChannelService;
        private readonly ILogger<ActionService> logger;
        private readonly IHubContext<TargetsHub, ITargetsHub> targetsHubContext;
        private readonly IToastService toastService;

        public DashboardActionService(ILogger<ActionService> logger, PluginService pluginService, IServiceProvider serviceProvider,
            IHubContext<TargetsHub, ITargetsHub> targetsHubContext, IEncryptedChannelService encryptedChannelService,
            DashboardService dashboardService, IToastService toastService) : base(logger, pluginService, serviceProvider)
        {
            this.logger = logger;
            this.targetsHubContext = targetsHubContext;
            this.encryptedChannelService = encryptedChannelService;
            this.dashboardService = dashboardService;
            this.toastService = toastService;
        }

        public async Task Execute(DashboardAction dashboardAction, bool toggleState)
        {
            if (!dashboardService.VerifySignature(dashboardAction))
            {
                toastService.ShowError("Invalid data signature.");
                return;
            }
            // Forward targeted actions to the specific target
            var targetId = dashboardAction.DashboardActionTargetId;
            var actionTypeName = dashboardAction.ActionTypeName;
            var parameters = string.IsNullOrWhiteSpace(dashboardAction.Parameters) ? "{}" : dashboardService.UnprotectData<DashboardAction>(dashboardAction.Parameters);

            try
            {
                if (targetId != DashboardActionTarget.ServerTargetId)
                {
                    await ExecuteOnTarget(targetId, actionTypeName, parameters, toggleState);
                    return;
                }
                // Handle non-targeted actions at the server
                if (actionTypeName == typeof(DashGroupAction).FullName)
                {
                    await ExecuteGroupAction(dashboardAction);
                    return;
                }
                await Execute(actionTypeName, parameters, toggleState);
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception executing action");
                toastService.ShowError($"An error occurred while executing the action: {ex.Message}");
                return;
            }
        }

        protected override void RegisterActions()
        {
            RegisterActionType(typeof(DashGroupAction));
            RegisterActionType(typeof(DashLinkAction));
            RegisterActionType(typeof(DashSeparatorAction));
            base.RegisterActions();
        }

        private async Task ExecuteGroupAction(DashboardAction dashboardAction)
        {
            logger.LogDebug($"Executing Group Action: {dashboardAction.Label}");
            foreach (var subAction in dashboardAction.DashboardSubActionChildren)
            {
                var toggleState = false;
                await Execute(subAction.DashboardActionChild, toggleState);
            }
            return;
        }

        private Task ExecuteOnTarget(string targetId, string actionTypeName, string parameters, bool toggleState)
        {
            logger.LogDebug($"Forwarding action to Target {targetId}: {actionTypeName}");
            var executeActionParameters = new ExecuteActionParameters
            {
                ActionTypeName = actionTypeName,
                Parameters = parameters,
                ToggleState = toggleState
            };
            var channelId = encryptedChannelService.GetChannelId(targetId);
            if (channelId == null)
            {
                toastService.ShowError($"The Target {targetId} is not connected.");
                return Task.CompletedTask;
            }
            var encryptedParameters = encryptedChannelService.EncryptSigned(channelId, executeActionParameters);
            return targetsHubContext.Clients.Groups(targetId).ExecuteAction(encryptedParameters);
        }
    }
}