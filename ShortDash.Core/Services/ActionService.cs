using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace ShortDash.Core.Services
{
    public class ActionService
    {
        private readonly ILogger logger;
        private readonly PluginService pluginService;
        private readonly IServiceProvider serviceProvider;

        public ActionService(ILogger<ActionService> logger, PluginService pluginService, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.pluginService = pluginService;
            LoadPluginActions();
        }

        private Dictionary<string, Type> ActionTypes { get; } = new Dictionary<string, Type>();

        public void Execute(string actionTypeName, string parameters, ref bool toggleState)
        {
            var action = GetAction(actionTypeName);
            if (action == null)
            {
                logger.LogError($"Unhandled Action Class: {actionTypeName}");
                return;
            }

            logger.LogDebug($"Executing plugin action: {action.GetType().FullName}");
            var actionAttribute = GetActionAttribute(action.GetType());
            var parametersObject = JsonSerializer.Deserialize(parameters, actionAttribute.ParametersType);
            action.Execute(parametersObject, ref toggleState);
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
            if (actionType == null) { return null; }

            return (IShortDashAction)ActivatorUtilities.CreateInstance(serviceProvider, actionType);
        }

        public ShortDashActionAttribute GetActionAttribute(string actionTypeName)
        {
            var actionType = FindActionType(actionTypeName);
            return GetActionAttribute(actionType);
        }

        public ShortDashActionAttribute GetActionAttribute(Type actionType)
        {
            return actionType?.GetCustomAttribute<ShortDashActionAttribute>() ?? new ShortDashActionAttribute();
        }

        public IList<Type> GetActionTypes()
        {
            return ActionTypes.Values.ToList();
        }

        private void LoadPluginActions()
        {
            foreach (var actionType in pluginService.Actions)
            {
                RegisterActionType(actionType);
            }
        }

        private void RegisterActionType(Type actionType)
        {
            if (!typeof(IShortDashAction).IsAssignableFrom(actionType)) { return; }
            if (!ActionTypes.TryAdd(actionType.FullName, actionType)) { return; }
        }
    }
}