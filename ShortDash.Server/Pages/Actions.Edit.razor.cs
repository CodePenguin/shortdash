using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ShortDash.Core.Plugins;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Actions_Edit : ComponentBase
    {
        private DashGroupActionInputGrid subActionsInputGrid;

        [Parameter]
        public int DashboardActionId { get; set; }

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        protected ShortDashActionAttribute ActionAttribute { get; set; }

        protected EditContext ActionEditContext { get; set; }

        protected DashboardAction DashboardAction { get; set; }

        protected object Parameters { get; set; }

        protected EditContext ParametersEditContext { get; set; }

        [Inject]
        private DashboardActionService DashboardActionService { get; set; }

        [Inject]
        private DashboardService DashboardService { get; set; }

        [Inject]
        private NavigationManager NavigationManagerService { get; set; }

        protected void CancelChanges()
        {
            NavigationManagerService.NavigateTo($"/actions");
        }

        protected async Task ChangeActionTypeName()
        {
            var settingsDefault = DashboardActionService.GetActionDefaultSettingsAttribute(DashboardAction.ActionTypeName);
            DashboardAction.BackgroundColor = DashboardAction.BackgroundColor ?? settingsDefault.BackgroundColor;
            DashboardAction.Icon = settingsDefault.Icon;
            DashboardAction.Label = settingsDefault.Label;
            await Task.Run(() => RefreshParameters());
        }

        protected async void ConfirmDelete()
        {
            var confirmed = await ConfirmDialog.ShowAsync(ModalService,
                title: "Delete Action",
                message: "Are you sure you want to delete this action?",
                confirmLabel: "Delete",
                confirmClass: "btn-danger");
            if (!confirmed)
            {
                return;
            }
            await DashboardService.DeleteDashboardActionAsync(DashboardAction);
            NavigationManagerService.NavigateTo($"/actions");
        }

        protected override async Task OnParametersSetAsync()
        {
            if (DashboardActionId > 0)
            {
                await LoadDashboardAction();
            }
            else
            {
                await Task.Run(() => NewDashboardAction());
            }
        }

        protected async void SaveChanges()
        {
            if (!ActionEditContext.Validate())
            {
                return;
            }

            if (DashboardAction.DashboardActionId == 0 && string.IsNullOrWhiteSpace(DashboardAction.ActionTypeName))
            {
                return;
            }

            if (ParametersEditContext != null)
            {
                if (!ParametersEditContext.Validate())
                {
                    return;
                }

                DashboardAction.Parameters = JsonSerializer.Serialize(Parameters);
            }

            List<DashboardSubAction> subActionRemovalList = null;
            if (subActionsInputGrid != null)
            {
                subActionsInputGrid.GenerateChanges(DashboardAction, out subActionRemovalList);
            }

            if (DashboardAction.DashboardActionId == 0)
            {
                await DashboardService.AddDashboardActionAsync(DashboardAction);
            }
            else
            {
                await DashboardService.UpdateDashboardActionAsync(DashboardAction, subActionRemovalList);
            }

            NavigationManagerService.NavigateTo($"/actions");
        }

        private async Task LoadDashboardAction()
        {
            DashboardAction = await DashboardService.GetDashboardActionAsync(DashboardActionId);
            ActionEditContext = new EditContext(DashboardAction);

            RefreshParameters();
        }

        private void NewDashboardAction()
        {
            DashboardAction = new DashboardAction { DashboardActionTargetId = 1 };
            ActionEditContext = new EditContext(DashboardAction);

            ActionAttribute = null;
            Parameters = null;
            ParametersEditContext = null;

            RefreshParameters();
        }

        private void RefreshParameters()
        {
            ActionAttribute = DashboardActionService.GetActionAttribute(DashboardAction.ActionTypeName);
            if (ActionAttribute != null && ActionAttribute.ParametersType != null)
            {
                Parameters = JsonSerializer.Deserialize(DashboardAction.Parameters, ActionAttribute.ParametersType);
                ParametersEditContext = new EditContext(Parameters);
            }
            else
            {
                Parameters = null;
                ParametersEditContext = null;
            }
        }
    }
}