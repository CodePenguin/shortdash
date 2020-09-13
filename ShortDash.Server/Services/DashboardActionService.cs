using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Plugins;
using ShortDash.Core.Services;
using ShortDash.Server.Actions;
using ShortDash.Server.Data;
using System;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class DashboardActionService : ActionService
    {
        private readonly IHubContext<TargetsHub, ITargetsHub> targetsHubContext;

        public DashboardActionService(ILogger<ActionService> logger, PluginService pluginService, IServiceProvider serviceProvider, IHubContext<TargetsHub, ITargetsHub> targetsHubContext) : base(logger, pluginService, serviceProvider)
        {
            this.targetsHubContext = targetsHubContext;
        }

        public Task Execute(DashboardAction dashboardAction, bool toggleState)
        {
            // Handle non-targeted actions at the server
            if (dashboardAction.DashboardActionTargetId <= 1)
            {
                return Execute(dashboardAction.ActionTypeName, dashboardAction.Parameters, toggleState);
            }
            // Forward targeted actions to the specific target
            return targetsHubContext.Clients.Groups(dashboardAction.DashboardActionTargetId.ToString()).ExecuteAction(dashboardAction.ActionTypeName, dashboardAction.Parameters, toggleState);
        }

        protected override void RegisterActions()
        {
            RegisterActionType(typeof(DashLinkAction));
            RegisterActionType(typeof(DashSeparatorAction));
            base.RegisterActions();
        }
    }
}