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
    public partial class DashboardViewPage : ComponentBase
    {
        protected Dashboard dashboard;

        [Parameter]
        public int? DashboardId { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        [Inject]
        private DashboardService DashboardService { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            DashboardId ??= 1;
            dashboard = await DashboardService.GetDashboardAsync(DashboardId.Value);
        }

        protected async void ShowAddDialog()
        {
            var result = await AddDashboardActionDialog.ShowAsync(ModalService);
            if (result.Cancelled) { return; }
            dashboard.DashboardCells.Add(new DashboardCell { DashboardActionId = (int)result.Data });
            await DashboardService.UpdateDashboardAsync(dashboard);
            StateHasChanged();
        }
    }
}