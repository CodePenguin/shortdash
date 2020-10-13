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
        private List<DashboardAction> DashboardActions { get; } = new List<DashboardAction>();

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        protected async override Task OnParametersSetAsync()
        {
            var list = await DashboardService.GetDashboardActionsAsync();
            DashboardActions.Clear();
            DashboardActions.AddRange(list.OrderBy(o => o.Label).ToList());
        }

        private void AddAction()
        {
            NavigationManager.NavigateTo($"/actions/new");
        }
    }
}