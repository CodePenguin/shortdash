using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class AddDashboardActionsDialog : ComponentBase
    {
        private List<DashboardAction> AvailableActions { get; } = new List<DashboardAction>();

        [CascadingParameter]
        private BlazoredModalInstance BlazoredModal { get; set; }

        [Inject]
        private DashboardService DashboardService { get; set; }

        private List<DashboardAction> SelectedActions { get; } = new List<DashboardAction>();

        public static Task<ModalResult> ShowAsync(IModalService modalService)
        {
            var modal = modalService.Show<AddDashboardActionsDialog>("Add actions");
            return modal.Result;
        }

        protected async override Task OnParametersSetAsync()
        {
            AvailableActions.AddRange(await DashboardService.GetDashboardActionsAsync());
        }

        private string ActionCheckboxID(DashboardAction action)
        {
            return "chkAction_" + action.DashboardActionId.ToString();
        }

        private Task CloseDialog()
        {
            return BlazoredModal.Cancel();
        }

        private bool IsSelected(DashboardAction action)
        {
            return SelectedActions.IndexOf(action) > -1;
        }

        private Task OkClick()
        {
            return BlazoredModal.Close(ModalResult.Ok(SelectedActions));
        }

        private void ToggleActionSelect(DashboardAction action)
        {
            if (!IsSelected(action))
            {
                SelectedActions.Add(action);
            }
            else
            {
                SelectedActions.Remove(action);
            }
        }

        private class SelectedItem
        {
            public string Value { get; set; }
        }
    }
}