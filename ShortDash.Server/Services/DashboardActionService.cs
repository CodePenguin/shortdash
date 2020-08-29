using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using ShortDash.Core.Plugins;
using ShortDash.Server.Actions;
using ShortDash.Server.Data;

namespace ShortDash.Server.Services
{
    public class DashboardActionService
    {
        private readonly ActionService actionService;
        private readonly IServiceProvider serviceProvider;

        public DashboardActionService(ActionService actionService, IServiceProvider serviceProvider)
        {
            this.actionService = actionService;
            this.serviceProvider = serviceProvider;

            RegisterActionType(typeof(DashLinkAction));
        }

        private Dictionary<string, Type> Actions { get; } = new Dictionary<string, Type>();

        public void Execute(DashboardAction action, bool toggleState)
        {
            Console.WriteLine($"Clicked {action.DashboardActionId} - {toggleState} - {action.Parameters}");

            if (!Actions.TryGetValue(action.ActionClass, out var actionType))
            {
                actionService.Execute(action, toggleState);
                return;
            }
            Console.WriteLine($"Found {actionType.AssemblyQualifiedName}");
            var actionInstance = ActivatorUtilities.CreateInstance(serviceProvider, actionType) as IShortDashAction;
            actionInstance?.Execute(action.Parameters);
        }

        private void RegisterActionType(Type actionType)
        {
            if (!typeof(IShortDashAction).IsAssignableFrom(actionType)) return;
            if (!Actions.TryAdd(actionType.FullName, actionType)) return;
        }
    }
}