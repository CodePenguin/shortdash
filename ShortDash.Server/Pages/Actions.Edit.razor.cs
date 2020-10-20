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
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShortDash.Server.Pages
{
    public partial class Actions_Edit : PageBase
    {
#pragma warning disable IDE0044 // Add readonly modifier
        private DashGroupActionInputGrid subActionsInputGrid;
#pragma warning restore IDE0044 // Add readonly modifier

        [Parameter]
        public int DashboardActionId { get; set; }

        [Parameter]
        public string Operation { get; set; } = "Edit";

        private ShortDashActionAttribute ActionAttribute { get; set; }

        private EditContext ActionEditContext { get; set; }

        private DashboardAction DashboardAction { get; set; }

        [Inject]
        private DashboardActionService DashboardActionService { get; set; }

        private bool IsDataSignatureValid { get; set; }
        private bool IsLoading { get; set; }

        private object Parameters { get; set; }

        private EditContext ParametersEditContext { get; set; }

        protected async void ConfirmDelete()
        {
            var confirmed = await ConfirmDialog.ShowAsync(ModalService,
                title: "Delete Action",
                message: "Are you sure you want to delete this action?",
                confirmLabel: "Delete",
                confirmClass: "btn-danger");
            if (!confirmed || !await SecureContext.ValidateUserAsync())
            {
                return;
            }
            await DashboardService.DeleteDashboardActionAsync(DashboardAction);
            ToastService.ShowInfo("The action has been deleted.", "DELETED");
            NavigationManager.NavigateTo($"/actions");
        }

        protected async override Task OnParametersSetAsync()
        {
            IsLoading = true;
            ActionEditContext = null;
            ParametersEditContext = null;
            IsDataSignatureValid = true;
            if (DashboardActionId > 0)
            {
                if (NavigationManager.Uri.EndsWith("/copy"))
                {
                    Operation = "Copy";
                    await LoadDashboardActionCopy();
                }
                else
                {
                    Operation = "Edit";
                    await LoadDashboardAction();
                }
            }
            else
            {
                Operation = "New";
                await Task.Run(() => NewDashboardAction());
            }
            if (DashboardAction != null)
            {
                ActionEditContext = new EditContext(DashboardAction);
            }
            IsLoading = false;
        }

        private void CancelChanges()
        {
            NavigationManager.NavigateTo($"/actions");
        }

        private async Task ChangeActionTypeName()
        {
            var settingsDefault = DashboardActionService.GetActionDefaultSettingsAttribute(DashboardAction.ActionTypeName);
            DashboardAction.BackgroundColor = DashboardAction.BackgroundColor ?? settingsDefault.BackgroundColor;
            DashboardAction.Icon = settingsDefault.Icon;
            DashboardAction.Label = settingsDefault.Label;
            await Task.Run(() => RefreshParameters());
        }

        private void CopyAction()
        {
            NavigationManager.NavigateTo($"/actions/{DashboardActionId}/copy");
        }

        private async Task LoadDashboardAction()
        {
            DashboardAction = await DashboardService.GetDashboardActionAsync(DashboardActionId);
            if (DashboardAction == null)
            {
                return;
            }
            IsDataSignatureValid = DashboardService.VerifySignature(DashboardAction);
            RefreshParameters();
        }

        private async Task LoadDashboardActionCopy()
        {
            DashboardAction = await DashboardService.GetDashboardActionCopyAsync(DashboardActionId);
            if (DashboardAction == null)
            {
                return;
            }
            RefreshParameters();
        }

        private void NewDashboardAction()
        {
            DashboardAction = new DashboardAction { DashboardActionTargetId = DashboardActionTarget.ServerTargetId, BackgroundColor = Color.Black };

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

        private async void SaveChanges()
        {
            if (!ActionEditContext.Validate() || !await SecureContext.ValidateUserAsync())
            {
                return;
            }

            if (DashboardAction.DashboardActionId == 0 && string.IsNullOrWhiteSpace(DashboardAction.ActionTypeName))
            {
                return;
            }

            if (!IsDataSignatureValid)
            {
                var confirmed = await DangerConfirmDialog.ShowAsync(ModalService,
                    title: "Invalid data signature",
                    message: "The data signature for this action could not be verified.  Saving the changes will store a valid signature and allow this action to be executed.  Are you sure you want to continue?",
                    confirmLabel: "Keep Changes");
                if (!confirmed)
                {
                    return;
                }
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

            NavigationManager.NavigateTo($"/actions");
        }

        private async void SelectIcon()
        {
            var result = await IconSelectDialog.ShowAsync(ModalService, DashboardAction.Icon, DashboardAction.BackgroundColor ?? Color.Black);
            if (result.Cancelled)
            {
                return;
            }
            DashboardAction.Icon = (string)result.Data;
            StateHasChanged();
        }
    }
}