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

        [Inject]
        public DashboardService DashboardService { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        protected bool EditMode { get; set; }

        private List<DashboardCell> DashboardCells { get; } = new List<DashboardCell>();

        [Inject]
        private NavigationManager NavigationManagerService { get; set; }

        protected void CancelChanges()
        {
            LoadDashboardCells();
            StateHasChanged();
        }

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

        protected void LoadDashboardCells()
        {
            DashboardCells.Clear();
            DashboardCells.AddRange(dashboard.DashboardCells.OrderBy(c => c.Sequence).ThenBy(c => c.DashboardCellId).ToList());
            EditMode = DashboardCells.Count == 0;
        }

        protected override async Task OnParametersSetAsync()
        {
            DashboardId ??= 1;
            dashboard = await DashboardService.GetDashboardAsync(DashboardId.Value);
            LoadDashboardCells();
        }

        protected async void RemoveCell(DashboardCell cell)
        {
            dashboard.DashboardCells.Remove(cell);
            await DashboardService.DeleteDashboardCellAsync(cell);
            StateHasChanged();
        }

        protected async void SaveChanges()
        {
            // Update sequences and add new cells to the main list
            for (var i = 0; i < DashboardCells.Count; i++)
            {
                var cell = DashboardCells[i];
                cell.Sequence = i;
                if (!dashboard.DashboardCells.Contains(cell))
                {
                    dashboard.DashboardCells.Add(cell);
                }
            }
            // Create a list of cells that have been removed
            var removalList = new List<DashboardCell>();
            foreach (var cell in dashboard.DashboardCells)
            {
                if (!DashboardCells.Contains(cell))
                {
                    removalList.Add(cell);
                }
            }
            await DashboardService.UpdateDashboardAsync(dashboard, removalList);
            EditMode = false;
            StateHasChanged();
        }

        protected void ToggleEditMode()
        {
            EditMode = !EditMode;
        }
    }
}