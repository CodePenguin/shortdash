using Microsoft.Extensions.DependencyInjection;
using ShortDash.Core.Actions;
using ShortDash.Core.Plugins;
using ShortDash.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Reflection;

namespace ShortDash.Server.Services
{
    public class ActionService
    {
        private readonly IServiceProvider serviceProvider;

        public ActionService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            LoadBuiltInActions();
            LoadPluginActions();
        }

        private Dictionary<string, Type> Actions { get; } = new Dictionary<string, Type>();

        public void Execute(DashboardAction action, bool toggleState)
        {
            if (!Actions.TryGetValue(action.ActionClass, out var actionType))
            {
                Console.WriteLine($"Unhandled Action Class: {action.ActionClass}");
                SystemSounds.Exclamation.Play();
                return;
            }
            Console.WriteLine($"Found {actionType.AssemblyQualifiedName}");
            var actionInstance = ActivatorUtilities.CreateInstance(serviceProvider, actionType) as IShortDashAction;
            actionInstance?.Execute(action.Parameters);
        }

        private IEnumerable<Type> FindActions(Assembly plugin)
        {
            foreach (Type type in plugin.GetTypes())
            {
                if (!typeof(IShortDashAction).IsAssignableFrom(type)) continue;
                yield return type;
            }
        }

        private void LoadBuiltInActions()
        {
            RegisterActionType(typeof(ExecuteProcessAction));
        }

        private Assembly LoadPlugin(string pluginPath)
        {
            // TODO: Need to implement plugin loading
            throw new NotImplementedException();
        }

        private void LoadPluginActions()
        {
            string[] pluginPaths = new string[]
            {
                // TODO: Need to figure out paths here
            };

            IEnumerable<Type> actionTypes = pluginPaths.SelectMany(pluginPath =>
            {
                Assembly plugin = LoadPlugin(pluginPath);
                return FindActions(plugin);
            });
            foreach (var actionType in actionTypes)
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