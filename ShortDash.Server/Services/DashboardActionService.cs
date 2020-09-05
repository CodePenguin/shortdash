using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            RegisterActionType(typeof(DashSeparatorAction));
        }

        private Dictionary<string, Type> ActionTypes { get; } = new Dictionary<string, Type>();

        public void Execute(DashboardAction action, bool toggleState)
        {
            logger.LogDebug($"Clicked #{action.DashboardActionId} - {action.ActionTypeName} - {toggleState} - {action.Parameters}");

            var actionType = FindActionType(action.ActionTypeName);
            if (actionType == null)
            {
                actionService.Execute(action.ActionTypeName, action.Parameters, ref toggleState);
                return;
            }

            logger.LogDebug($"Found {actionType.AssemblyQualifiedName}");
            var actionInstance = ActivatorUtilities.CreateInstance(serviceProvider, actionType) as IShortDashAction;
            var actionAttribute = actionService.GetActionAttribute(actionType);
            var parametersObject = JsonSerializer.Deserialize(action.Parameters, actionAttribute.ParametersType);
            actionInstance.Execute(parametersObject, ref toggleState);
        }

        public Type FindActionType(string actionTypeName)
        {
            if (string.IsNullOrWhiteSpace(actionTypeName)) { return null; }
            if (!ActionTypes.TryGetValue(actionTypeName, out var actionType)) { return null; }
            return actionType;
        }

        public IShortDashAction GetAction(string actionTypeName)
        {
            var actionType = FindActionType(actionTypeName);
            if (actionType == null)
            {
                return actionService.GetAction(actionTypeName);
            }

            return (IShortDashAction)ActivatorUtilities.CreateInstance(serviceProvider, actionType);
        }

        public ShortDashActionAttribute GetActionAttribute(string actionTypeName)
        {
            var actionType = FindActionType(actionTypeName);
            if (actionType == null)
            {
                return actionService.GetActionAttribute(actionTypeName);
            }
            return actionService.GetActionAttribute(actionType);
        }

        public ShortDashActionAttribute GetActionAttribute(Type actionType)
        {
            return actionType?.GetCustomAttribute<ShortDashActionAttribute>() ?? new ShortDashActionAttribute();
        }

        public IList<Type> GetActionTypes()
        {
            var list = ActionTypes.Values.ToList();
            list.AddRange(actionService.GetActionTypes());
            return list;
        }

        private void RegisterActionType(Type actionType)
        {
            if (!typeof(IShortDashAction).IsAssignableFrom(actionType)) { return; }
            if (!ActionTypes.TryAdd(actionType.FullName, actionType)) { return; }
        }
    }
}