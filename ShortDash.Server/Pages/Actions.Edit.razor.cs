using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ShortDash.Core.Plugins;
using ShortDash.Server.Actions;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Extensions;
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
        [Parameter]
        public int DashboardActionId { get; set; }

        [Parameter]
        public string Operation { get; set; } = "Edit";

        private ShortDashActionAttribute ActionAttribute { get; set; }
        private EditContext ActionEditContext { get; set; }

        [Inject]
        private ConfigurationService ConfigurationService { get; set; }

        private DashboardAction DashboardAction { get; set; }

        [Inject]
        private DashboardActionService DashboardActionService { get; set; }

        private bool IsDataSignatureValid { get; set; }
        private bool IsLoading { get; set; }
        private bool IsToggle => DashboardActionService.GetActionAttribute(DashboardAction?.ActionTypeName).Toggle;
        private object Parameters { get; set; }
        private EditContext ParametersEditContext { get; set; }
        private List<DashboardSubAction> SubActions { get; set; }

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
            SubActions = null;
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
            DashboardAction.ToggleBackgroundColor = DashboardAction.ToggleBackgroundColor ?? settingsDefault.ToggleBackgroundColor;
            DashboardAction.ToggleIcon = settingsDefault.ToggleIcon;
            DashboardAction.ToggleLabel = settingsDefault.ToggleLabel;
            await Task.Run(() => RefreshParameters());
        }

        private void CopyAction()
        {
            NavigationManager.NavigateTo($"/actions/{DashboardActionId}/copy");
        }

        private void GenerateChanges(out List<DashboardSubAction> removalList)
        {
            if (SubActions == null)
            {
                removalList = null;
                return;
            }
            removalList = new List<DashboardSubAction>();

            // Update sequences and add new sub actions to the main list
            for (var i = 0; i < SubActions.Count; i++)
            {
                var subAction = SubActions[i];
                subAction.Sequence = i;
                if (!DashboardAction.DashboardSubActionChildren.Contains(subAction))
                {
                    DashboardAction.DashboardSubActionChildren.Add(subAction);
                }
            }
            // Create a list of sub actions that have been removed
            foreach (var subAction in DashboardAction.DashboardSubActionChildren)
            {
                if (!SubActions.Contains(subAction))
                {
                    removalList.Add(subAction);
                }
            }
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

        private void LoadSubActions()
        {
            if (DashboardAction.ActionTypeName != typeof(DashGroupAction).FullName)
            {
                return;
            }
            SubActions = new List<DashboardSubAction>();
            SubActions.AddRange(DashboardAction.DashboardSubActionChildren.OrderBy(c => c.Sequence).ThenBy(c => c.DashboardActionChildId).ToList());
        }

        private void NewDashboardAction()
        {
            var defaultSettings = ConfigurationService.DefaultSettings();
            DashboardAction = new DashboardAction
            {
                BackgroundColor = defaultSettings.ActionBackgroundColor,
                DashboardActionTargetId = DashboardActionTarget.ServerTargetId,
            };

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
                var serializedParameters = string.IsNullOrWhiteSpace(DashboardAction.Parameters) ? "{}" : DashboardService.UnprotectData<DashboardAction>(DashboardAction.Parameters);
                Parameters = JsonSerializer.Deserialize(serializedParameters, ActionAttribute.ParametersType);
                ParametersEditContext = new EditContext(Parameters);
            }
            else
            {
                Parameters = null;
                ParametersEditContext = null;
            }

            LoadSubActions();
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

                var serializedParameters = JsonSerializer.Serialize(Parameters);
                DashboardAction.Parameters = DashboardService.ProtectData<DashboardAction>(serializedParameters);
            }

            GenerateChanges(out var subActionRemovalList);

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

        private async void SelectToggleIcon()
        {
            var result = await IconSelectDialog.ShowAsync(ModalService, DashboardAction.ToggleIcon, DashboardAction.ToggleBackgroundColor ?? Color.Black);
            if (result.Cancelled)
            {
                return;
            }
            DashboardAction.ToggleIcon = (string)result.Data;
            StateHasChanged();
        }
    }
}