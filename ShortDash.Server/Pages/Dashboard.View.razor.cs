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
    public partial class Dashboard_View : ComponentBase
    {
        protected Dashboard dashboard;

        [Parameter]
        public int? DashboardId { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        protected bool EditMode { get; set; }

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManagerService { get; set; }

        protected async void ConfirmDelete()
        {
            var confirmed = await ConfirmDialog.ShowAsync(ModalService,
                title: "Delete Dashboard",
                message: "Are you sure you want to delete this dashboard?",
                confirmLabel: "Delete",
                confirmClass: "btn-danger");
            if (!confirmed) { return; }
            await DashboardService.DeleteDashboardAsync(dashboard);
            NavigationManagerService.NavigateTo($"/");
        }

        protected override async Task OnParametersSetAsync()
        {
            DashboardId ??= 1;
            dashboard = await DashboardService.GetDashboardAsync(DashboardId.Value);
        }

        protected async void RemoveCell(DashboardCell cell)
        {
            dashboard.DashboardCells.Remove(cell);
            await DashboardService.DeleteDashboardCellAsync(cell);
            StateHasChanged();
        }

        protected async void SaveChanges()
        {
            await DashboardService.UpdateDashboardAsync(dashboard);
            StateHasChanged();
        }

        protected void ToggleEditMode()
        {
            EditMode = !EditMode;
        }
    }
}