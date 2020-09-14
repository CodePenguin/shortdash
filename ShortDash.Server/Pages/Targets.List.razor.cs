using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Targets_List : ComponentBase
    {
        protected List<DashboardActionTarget> DashboardActionTargets { get; } = new List<DashboardActionTarget>();

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManagerService { get; set; }

        protected void AddTarget()
        {
            NavigationManagerService.NavigateTo($"/targets/new");
        }

        protected override async Task OnParametersSetAsync()
        {
            var list = await DashboardService.GetDashboardActionTargetsAsync();
            DashboardActionTargets.Clear();
            DashboardActionTargets.AddRange(list.OrderBy(o => o.Name).ToList());
        }
    }
}