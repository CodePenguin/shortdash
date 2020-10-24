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
            RegisterActions();
        }

        private Dictionary<string, Type> ActionTypes { get; } = new Dictionary<string, Type>();

        public static ShortDashActionAttribute GetActionAttribute(Type actionType)
        {
            return actionType?.GetCustomAttribute<ShortDashActionAttribute>() ?? new ShortDashActionAttribute();
        }

        public static ShortDashActionDefaultSettingsAttribute GetActionDefaultSettingsAttribute(Type actionType)
        {
            return actionType?.GetCustomAttribute<ShortDashActionDefaultSettingsAttribute>() ?? new ShortDashActionDefaultSettingsAttribute();
        }

        public static object GetActionParameters(Type actionType, string parameters)
        {
            var actionAttribute = GetActionAttribute(actionType);
            return JsonSerializer.Deserialize(parameters, actionAttribute.ParametersType ?? typeof(object));
        }

        public ShortDashActionResult Execute(string actionTypeName, string parameters, bool toggleState)
        {
            var actionType = FindActionType(actionTypeName);
            if (actionType == null)
            {
                var errorMessage = $"Unregistered action type: {actionTypeName}";
                logger.LogError(errorMessage);
                return new ShortDashActionResult { UserMessage = errorMessage };
            }
            logger.LogDebug($"Executing action: {actionType.FullName}");
            var action = GetAction(actionType);
            var parametersObject = GetActionParameters(action.GetType(), parameters);
            try
            {
                return action.Execute(parametersObject, toggleState);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while executing action: {actionTypeName}");
                return new ShortDashActionResult { UserMessage = ex.Message };
            }
        }

        public Type FindActionType(string actionTypeName)
        {
            if (string.IsNullOrWhiteSpace(actionTypeName))
            {
                return null;
            }
            if (!ActionTypes.TryGetValue(actionTypeName, out var actionType))
            {
                return null;
            }
            return actionType;
        }

        public IShortDashAction GetAction(string actionTypeName)
        {
            var actionType = FindActionType(actionTypeName);
            return GetAction(actionType);
        }

        public IShortDashAction GetAction(Type actionType)
        {
            if (actionType == null)
            {
                return null;
            }

            return (IShortDashAction)ActivatorUtilities.CreateInstance(serviceProvider, actionType);
        }

        public ShortDashActionAttribute GetActionAttribute(string actionTypeName)
        {
            var actionType = FindActionType(actionTypeName);
            return GetActionAttribute(actionType);
        }

        public ShortDashActionDefaultSettingsAttribute GetActionDefaultSettingsAttribute(string actionTypeName)
        {
            var actionType = FindActionType(actionTypeName);
            return GetActionDefaultSettingsAttribute(actionType);
        }

        public IList<Type> GetActionTypes()
        {
            return ActionTypes.Values.ToList();
        }

        protected virtual void RegisterActions()
        {
            LoadPluginActions();
        }

        protected void RegisterActionType(Type actionType)
        {
            if (!typeof(IShortDashAction).IsAssignableFrom(actionType))
            {
                return;
            }
            if (!ActionTypes.TryAdd(actionType.FullName, actionType))
            {
                return;
            }
        }

        private void LoadPluginActions()
        {
            foreach (var actionType in pluginService.Actions)
            {
                RegisterActionType(actionType);
            }
        }
    }
}