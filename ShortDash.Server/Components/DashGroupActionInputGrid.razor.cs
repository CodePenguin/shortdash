using Blazored.Modal.Services;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System.Collections.Generic;
using System.Linq;

namespace ShortDash.Server.Components
{
    public partial class DashGroupActionInputGrid : ComponentBase
    {
        [Parameter]
        public DashboardService DashboardService { get; set; }

        [Parameter]
        public List<DashboardSubAction> SubActions { get; set; }

        [Inject]
        protected IToastService ToastService { get; set; }

        [CascadingParameter]
        private IModalService ModalService { get; set; }

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
            var result = await AddDashboardActionsDialog.ShowAsync(ModalService);
            if (result.Cancelled)
            {
                return;
            }
            var selectedActions = result.Data as List<DashboardAction>;
            foreach (var dashboardAction in selectedActions)
            {
                if (SubActions.Any(a => a.DashboardActionChild.DashboardActionId == dashboardAction.DashboardActionId))
                {
                    continue;
                }
                SubActions.Add(new DashboardSubAction { DashboardActionChild = dashboardAction });
            }
            StateHasChanged();
        }
    }
}
