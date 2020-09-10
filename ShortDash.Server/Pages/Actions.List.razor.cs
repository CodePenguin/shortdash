using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Actions_List : ComponentBase
    {
        protected List<DashboardAction> DashboardActions { get; } = new List<DashboardAction>();

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManagerService { get; set; }

        protected void AddAction()
        {
            NavigationManagerService.NavigateTo($"/actions/new");
        }

        protected override async Task OnParametersSetAsync()
        {
            var list = await DashboardService.GetDashboardActionsAsync();
            DashboardActions.Clear();
            DashboardActions.AddRange(list.OrderBy(o => o.Label).ToList());
        }
    }
}