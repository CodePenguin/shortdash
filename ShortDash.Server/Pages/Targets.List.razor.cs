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
    public partial class Targets_List : PageBase
    {
        private List<DashboardActionTarget> DashboardActionTargets { get; } = new List<DashboardActionTarget>();

        protected async override Task OnParametersSetAsync()
        {
            var list = await DashboardService.GetDashboardActionTargetsAsync();
            DashboardActionTargets.Clear();
            DashboardActionTargets.AddRange(list.Where(o => !o.DashboardActionTargetId.Equals(DashboardActionTarget.ServerTargetId)).OrderBy(o => o.Name).ToList());
        }

        private void LinkTarget()
        {
            NavigationManager.NavigateTo($"/targets/link");
        }
    }
}