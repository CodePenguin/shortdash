using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Actions;
using ShortDash.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            LoadBuiltInActions();
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
            logger.LogDebug($"Found {actionType.AssemblyQualifiedName}");
            var actionInstance = ActivatorUtilities.CreateInstance(serviceProvider, actionType);
            var shortDashAction = (actionInstance as IShortDashAction);
            shortDashAction?.Execute(parameters, ref toggleState);
        }

        private void LoadBuiltInActions()
        {
            RegisterActionType(typeof(ExecuteProcessAction));
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
            if (actionType.GetInterface(nameof(IShortDashAction)) == null) return;
            if (!Actions.TryAdd(actionType.FullName, actionType)) return;
        }
    }
}