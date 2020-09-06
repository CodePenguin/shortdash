using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShortDash.Core.Services
{
    public struct ActionResult
    {
        public bool Success;
        public bool ToggleState;
    }

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

        public Task<ActionResult> Execute(string actionTypeName, string parameters, bool toggleState)
        {
            var actionType = FindActionType(actionTypeName);
            if (actionType == null)
            {
                logger.LogError($"Unregistered action type: {actionTypeName}");
                return Task.FromResult(new ActionResult { Success = false, ToggleState = toggleState });
            }
            logger.LogDebug($"Executing action: {actionType.FullName}");
            return Task.Run(() =>
            {
                var action = GetAction(actionType);
                var actionAttribute = GetActionAttribute(action.GetType());
                var parametersObject = JsonSerializer.Deserialize(parameters, actionAttribute.ParametersType ?? typeof(object));
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