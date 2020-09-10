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
        public DashboardActionService(ILogger<ActionService> logger, PluginService pluginService, IServiceProvider serviceProvider) : base(logger, pluginService, serviceProvider)
        {
        }

        public Task<ActionResult> Execute(DashboardAction dashboardAction, bool toggleState)
        {
            return Execute(dashboardAction.ActionTypeName, dashboardAction.Parameters, toggleState);
        }

        protected override void RegisterActions()
        {
            RegisterActionType(typeof(DashLinkAction));
            RegisterActionType(typeof(DashSeparatorAction));
            base.RegisterActions();
        }
    }
}