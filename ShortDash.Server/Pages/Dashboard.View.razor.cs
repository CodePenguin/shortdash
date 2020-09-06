using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;

namespace ShortDash.Server.Pages
{
    public partial class DashboardView : ComponentBase
    {
        protected Dashboard dashboard;

        [Parameter]
        public int? DashboardId { get; set; }

        [CascadingParameter]
        public IModalService Modal { get; set; }

        [Inject]
        private DashboardService DashboardService { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            DashboardId ??= 1;
            dashboard = await DashboardService.GetDashboardAsync(DashboardId.Value);
        }

        protected async void ShowAddDialog()
        {
            var modal = Modal.Show<AddDashboardActionDialog>();
            var result = await modal.Result;
            if (result.Cancelled) { return; }
            dashboard.DashboardCells.Add(new DashboardCell { DashboardActionId = int.Parse(result.Data.ToString()) });
            await DashboardService.UpdateDashboardAsync(dashboard);
            StateHasChanged();
        }
    }
}