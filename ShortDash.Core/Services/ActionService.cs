using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Plugins;
using System;
using System.Collections.Generic;

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

        private Dictionary<string, Type> Actions { get; } = new Dictionary<string, Type>();

        public void Execute(string actionTypeName, string parameters, ref bool toggleState)
        {
            if (!Actions.TryGetValue(actionTypeName, out var actionType))
            {
                logger.LogError($"Unhandled Action Class: {actionTypeName}");
                return;
            }
            logger.LogDebug($"Executing plugin action: {actionType.FullName}");
            var actionInstance = (IShortDashAction)ActivatorUtilities.CreateInstance(serviceProvider, actionType);
            actionInstance.Execute(parameters, ref toggleState);
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
            if (!typeof(IShortDashAction).IsAssignableFrom(actionType)) return;
            if (!Actions.TryAdd(actionType.FullName, actionType)) return;
        }
    }
}