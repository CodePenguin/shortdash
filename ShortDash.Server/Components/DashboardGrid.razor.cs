using Blazored.Modal.Services;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class DashboardGrid : ComponentBase
    {
        [Parameter]
        public List<DashboardCell> DashboardCells { get; set; }

        [Parameter]
        public bool EditMode { get; set; } = false;

        [Parameter]
        public bool HideLabels { get; set; }

        [Inject]
        protected IToastService ToastService { get; set; }

        [CascadingParameter]
        private DashboardService DashboardService { get; set; }

        [CascadingParameter]
        private IModalService ModalService { get; set; }

        private void MoveCellLeft(DashboardCell cell)
        {
            var index = DashboardCells.IndexOf(cell);
            if (index < 1)
            {
                return;
            }
            DashboardCells[index] = DashboardCells[index - 1];
            DashboardCells[index - 1] = cell;
            StateHasChanged();
        }

        private void MoveCellRight(DashboardCell cell)
        {
            var index = DashboardCells.IndexOf(cell);
            if (index >= DashboardCells.Count - 1)
            {
                return;
            }
            DashboardCells[index] = DashboardCells[index + 1];
            DashboardCells[index + 1] = cell;
            StateHasChanged();
        }

        private void RemoveCell(DashboardCell cell)
        {
            DashboardCells.Remove(cell);
            StateHasChanged();
        }

        private async void ShowAddDialog()
        {
            var result = await AddDashboardActionDialog.ShowAsync(ModalService);
            if (result.Cancelled)
            {
                return;
            }
            var dashboardActionId = (int)result.Data;
            if (dashboardActionId <= 0)
            {
                return;
            }
            var dashboardAction = await DashboardService.GetDashboardActionAsync(dashboardActionId);
            if (dashboardAction == null)
            {
                return;
            }
            if (DashboardCells.Any(c => c.DashboardAction.DashboardActionId == dashboardActionId))
            {
                ToastService.ShowWarning("The selected action has already been added.");
                return;
            }

            DashboardCells.Add(new DashboardCell { DashboardActionId = dashboardActionId, DashboardAction = dashboardAction });
            StateHasChanged();
        }
    }
}