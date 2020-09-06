using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
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

        public Task<ActionResult> Execute(DashboardAction dashboardAction, bool toggleState)
        {
            logger.LogDebug($"Clicked #{dashboardAction.DashboardActionId} - {dashboardAction.ActionTypeName} - {toggleState} - {dashboardAction.Parameters}");

            var actionType = FindActionType(dashboardAction.ActionTypeName);
            if (actionType == null)
            {
                return actionService.Execute(dashboardAction.ActionTypeName, dashboardAction.Parameters, toggleState);
            }
            logger.LogDebug($"Executing action: {actionType.FullName}");
            return Task.Run(() =>
            {
                var action = GetAction(actionType);
                var actionAttribute = GetActionAttribute(action.GetType());
                var parametersObject = JsonSerializer.Deserialize(dashboardAction.Parameters, actionAttribute.ParametersType ?? typeof(object));
                var success = action.Execute(parametersObject, ref toggleState);
                return new ActionResult { Success = success, ToggleState = toggleState };
            });
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
            return GetAction(actionType);
        }

        public IShortDashAction GetAction(Type actionType)
        {
            if (actionType == null) { return null; }

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