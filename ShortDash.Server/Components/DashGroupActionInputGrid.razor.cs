using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class DashGroupActionInputGrid : ComponentBase
    {
        [Parameter]
        public DashboardAction DashboardAction { get; set; }

        [Parameter]
        public DashboardService DashboardService { get; set; }

        [CascadingParameter]
        private IModalService ModalService { get; set; }

        private List<DashboardSubAction> SubActions { get; } = new List<DashboardSubAction>();

        public void GenerateChanges(DashboardAction dashboardAction, out List<DashboardSubAction> removalList)
        {
            removalList = new List<DashboardSubAction>();

            // Update sequences and add new sub actions to the main list
            for (var i = 0; i < SubActions.Count; i++)
            {
                var subAction = SubActions[i];
                subAction.Sequence = i;
                if (!dashboardAction.DashboardSubActionChildren.Contains(subAction))
                {
                    dashboardAction.DashboardSubActionChildren.Add(subAction);
                }
            }
            // Create a list of sub actions that have been removed
            foreach (var subAction in dashboardAction.DashboardSubActionChildren)
            {
                if (!SubActions.Contains(subAction))
                {
                    removalList.Add(subAction);
                }
            }
        }

        protected override void OnParametersSet()
        {
            SubActions.Clear();
            SubActions.AddRange(DashboardAction.DashboardSubActionChildren.OrderBy(c => c.Sequence).ThenBy(c => c.DashboardActionChildId).ToList());
        }

        private void MoveCellLeft(DashboardSubAction cell)
        {
            var index = SubActions.IndexOf(cell);
            if (index < 1)
            {
                return;
            }
            SubActions[index] = SubActions[index - 1];
            SubActions[index - 1] = cell;
            StateHasChanged();
        }

        private void MoveCellRight(DashboardSubAction cell)
        {
            var index = SubActions.IndexOf(cell);
            if (index >= SubActions.Count - 1)
            {
                return;
            }
            SubActions[index] = SubActions[index + 1];
            SubActions[index + 1] = cell;
            StateHasChanged();
        }

        private void RemoveCell(DashboardSubAction cell)
        {
            SubActions.Remove(cell);
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

            SubActions.Add(new DashboardSubAction { DashboardActionParentId = DashboardAction.DashboardActionId, DashboardActionChildId = dashboardAction.DashboardActionId, DashboardActionChild = dashboardAction });
            StateHasChanged();
        }
    }
}
