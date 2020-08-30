using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Plugins;
using ShortDash.Core.Services;
using ShortDash.Server.Actions;
using ShortDash.Server.Data;

namespace ShortDash.Server.Services
{
    public class DashboardActionService
    {
        private readonly ActionService actionService;
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        public DashboardActionService(ActionService actionService, ILogger<DashboardActionService> logger, IServiceProvider serviceProvider)
        {
            this.actionService = actionService;
            this.logger = logger;
            this.serviceProvider = serviceProvider;

            RegisterActionType(typeof(DashLinkAction));
        }

        private Dictionary<string, Type> Actions { get; } = new Dictionary<string, Type>();

        public void Execute(DashboardAction action, bool toggleState)
        {
            logger.LogDebug($"Clicked #{action.DashboardActionId} - {action.ActionTypeName} - {toggleState} - {action.Parameters}");

            if (!Actions.TryGetValue(action.ActionTypeName, out var actionType))
            {
                actionService.Execute(action.ActionTypeName, action.Parameters, ref toggleState);
                return;
            }
            logger.LogDebug($"Found {actionType.AssemblyQualifiedName}");
            var actionInstance = ActivatorUtilities.CreateInstance(serviceProvider, actionType) as IShortDashAction;
            var parametersObject = JsonSerializer.Deserialize(action.Parameters, actionInstance.ParametersType);
            actionInstance.Execute(parametersObject, ref toggleState);
        }

        private void RegisterActionType(Type actionType)
        {
            if (!typeof(IShortDashAction).IsAssignableFrom(actionType)) return;
            if (!Actions.TryAdd(actionType.FullName, actionType)) return;
        }
    }
}