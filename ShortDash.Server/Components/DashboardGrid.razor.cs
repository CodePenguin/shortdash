using Blazored.Modal.Services;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using System.Collections.Generic;
using System.Linq;

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

        [Parameter]
        public string TextClass { get; set; }

        [Inject]
        protected IToastService ToastService { get; set; }

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

        private async void ShowAddActionsDialog()
        {
            var result = await AddDashboardActionsDialog.ShowAsync(ModalService);
            if (result.Cancelled)
            {
                return;
            }
            var selectedActions = result.Data as List<DashboardAction>;
            foreach (var dashboardAction in selectedActions)
            {
                if (DashboardCells.Any(c => c.DashboardAction.DashboardActionId == dashboardAction.DashboardActionId))
                {
                    continue;
                }

                DashboardCells.Add(new DashboardCell { DashboardActionId = dashboardAction.DashboardActionId, DashboardAction = dashboardAction });
            }
            StateHasChanged();
        }
    }
}