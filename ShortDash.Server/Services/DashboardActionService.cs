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
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class DashboardActionService : ActionService
    {
        private static readonly ConcurrentDictionary<Guid, ShortDashActionResultCallback> PendingExecuteActionRequests = new ConcurrentDictionary<Guid, ShortDashActionResultCallback>();
        private readonly DashboardService dashboardService;
        private readonly IEncryptedChannelService encryptedChannelService;
        private readonly ILogger<ActionService> logger;
        private readonly IHubContext<TargetsHub, ITargetsHub> targetsHubContext;

        public DashboardActionService(ILogger<ActionService> logger, PluginService pluginService, IServiceProvider serviceProvider,
            IHubContext<TargetsHub, ITargetsHub> targetsHubContext, IEncryptedChannelService encryptedChannelService,
            DashboardService dashboardService) : base(logger, pluginService, serviceProvider)
        {
            this.logger = logger;
            this.targetsHubContext = targetsHubContext;
            this.encryptedChannelService = encryptedChannelService;
            this.dashboardService = dashboardService;
        }

        public delegate void ShortDashActionResultCallback(Guid requestId, ShortDashActionResult result);

        public static void CancelExecuteActionRequest(Guid requestId)
        {
            PendingExecuteActionRequests.TryRemove(requestId, out _);
        }

        public static void HandleActionExecuted(Guid requestId, ShortDashActionResult result)
        {
            if (PendingExecuteActionRequests.TryRemove(requestId, out var callback))
            {
                callback.Invoke(requestId, result);
            }
        }

        public void Execute(Guid requestId, DashboardAction dashboardAction, bool toggleState, ShortDashActionResultCallback callback)
        {
            RegisterExecuteActionRequest(requestId, callback);
            if (!dashboardService.VerifySignature(dashboardAction))
            {
                HandleActionExecuted(requestId, new ShortDashActionResult { UserMessage = "Invalid data signature" });
                return;
            }

            try
            {
                var targetId = dashboardAction.DashboardActionTargetId;
                var actionTypeName = dashboardAction.ActionTypeName;
                var parameters = string.IsNullOrWhiteSpace(dashboardAction.Parameters) ? "{}" : dashboardService.UnprotectData<DashboardAction>(dashboardAction.Parameters);

                // Forward targeted actions to the specific target
                if (targetId != DashboardActionTarget.ServerTargetId)
                {
                    ExecuteOnTarget(requestId, targetId, actionTypeName, parameters, toggleState);
                    return;
                }
                ExecuteOnServer(requestId, actionTypeName, parameters, toggleState);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception executing action");
                HandleActionExecuted(requestId, new ShortDashActionResult { UserMessage = $"An error occurred while executing the action: {ex.Message}" });
            }
        }

        protected override void RegisterActions()
        {
            RegisterActionType(typeof(DashGroupAction));
            RegisterActionType(typeof(DashLinkAction));
            RegisterActionType(typeof(DashSeparatorAction));
            base.RegisterActions();
        }

        private static void HandleActionExecutedError(Guid requestId, string userMessage)
        {
            var result = new ShortDashActionResult
            {
                Success = false,
                UserMessage = userMessage
            };
            HandleActionExecuted(requestId, result);
        }

        private static void RegisterExecuteActionRequest(Guid requestId, ShortDashActionResultCallback callback)
        {
            PendingExecuteActionRequests[requestId] = callback;
        }

        private async void ExecuteOnServer(Guid requestId, string actionTypeName, string parameters, bool toggleState)
        {
            var result = await Task.Run(() => Execute(actionTypeName, parameters, toggleState));
            HandleActionExecuted(requestId, result);
        }

        private async void ExecuteOnTarget(Guid requestId, string targetId, string actionTypeName, string parameters, bool toggleState)
        {
            var target = await dashboardService.GetDashboardActionTargetAsync(targetId);
            if (target == null)
            {
                HandleActionExecutedError(requestId, $"The Target \"{targetId}\" was not found.");
                return;
            }
            logger.LogDebug($"Forwarding action to Target \"{target.Name}\": {actionTypeName}");
            var executeActionParameters = new ExecuteActionParameters
            {
                ActionTypeName = actionTypeName,
                Parameters = parameters,
                RequestId = requestId,
                ToggleState = toggleState
            };
            var channelId = encryptedChannelService.GetChannelId(targetId);
            if (channelId == null)
            {
                HandleActionExecutedError(requestId, $"The Target \"{target.Name}\" is not connected.");
                return;
            }
            var encryptedParameters = encryptedChannelService.EncryptSigned(channelId, executeActionParameters);
            await targetsHubContext.Clients.Groups(targetId).ExecuteAction(encryptedParameters);
        }
    }
}