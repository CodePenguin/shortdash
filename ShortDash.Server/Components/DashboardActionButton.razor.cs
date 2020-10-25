using Blazored.Modal.Services;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Features;
using ShortDash.Core.Models;
using ShortDash.Core.Plugins;
using ShortDash.Server.Actions;
using ShortDash.Server.Data;
using ShortDash.Server.Extensions;
using ShortDash.Server.Services;
using ShortDash.Server.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public sealed partial class DashboardActionButton : ComponentBase
    {
        private readonly Dictionary<Guid, DashboardAction> pendingRequests = new Dictionary<Guid, DashboardAction>();

        [Parameter]
        public DashboardAction DashboardAction { get; set; }

        [Parameter]
        public bool EditMode { get; set; }

        [Parameter]
        public bool HideLabel { get; set; }

        private string Label => ToggleState ? DashboardAction.ToggleLabel : DashboardAction.Label;

        private bool ToggleState => IsToggle && DashboardAction.ToggleState;

        [Inject]
        private DashboardActionService DashboardActionService { get; set; }

        [Inject]
        private DashboardService DashboardService { get; set; }

        private bool IsExecuting { get; set; } = false;

        private bool IsToggle { get; set; } = false;

        [CascadingParameter]
        private IModalService ModalService { get; set; }

        [CascadingParameter]
        private ISecureContext SecureContext { get; set; }

        [Inject]
        private IToastService ToastService { get; set; }

        public void Dispose()
        {
            CancelPendingRequests();
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            IsExecuting = false;
            CancelPendingRequests();

            var actionAttribute = DashboardActionService.GetActionAttribute(DashboardAction.ActionTypeName);
            IsToggle = actionAttribute.Toggle;
        }

        private void CancelPendingRequests()
        {
            foreach (var requestId in pendingRequests.Keys)
            {
                DashboardActionService.CancelExecuteActionRequest(requestId);
            }
            pendingRequests.Clear();
        }

        private async void ExecuteAction()
        {
            if (EditMode || IsExecuting || !await SecureContext.ValidateUserAsync())
            {
                return;
            }

            IsExecuting = true;
            if (DashboardAction.ActionTypeName.Equals(typeof(DashGroupAction).FullName))
            {
                await ExecuteDashGroupAction();
            }
            else
            {
                var requestId = Guid.NewGuid();
                pendingRequests.Add(requestId, DashboardAction);
                DashboardActionService.Execute(requestId, DashboardAction, ToggleState, HandleActionExecutedResultCallback);
            }
        }

        private async Task ExecuteDashGroupAction()
        {
            var actionType = DashboardActionService.FindActionType(typeof(DashGroupAction).FullName);
            var decryptedParameters = string.IsNullOrWhiteSpace(DashboardAction.Parameters) ? "{}" : DashboardService.UnprotectData<DashboardAction>(DashboardAction.Parameters);
            var parameters = DashboardActionService.GetActionParameters(actionType, decryptedParameters) as DashGroupParameters;
            if (parameters.DashGroupType == DashGroupType.Folder)
            {
                await DashGroupActionDialog.ShowAsync(ModalService, DashboardAction);
            }
            else if (parameters.DashGroupType == DashGroupType.List)
            {
                foreach (var subAction in DashboardAction.DashboardSubActionChildren)
                {
                    var subDashboardAction = subAction.DashboardActionChild;
                    var subIsToggle = DashboardActionService.GetActionAttribute(subDashboardAction.ActionTypeName).Toggle;
                    var subToggleState = subIsToggle && subDashboardAction.ToggleState;
                    var requestId = Guid.NewGuid();
                    pendingRequests[requestId] = subDashboardAction;
                    DashboardActionService.Execute(requestId, subDashboardAction, subToggleState, HandleActionExecutedResultCallback);
                }
            }
        }

        private async void HandleActionExecutedResult(Guid requestId, ShortDashActionResult result)
        {
            if (!string.IsNullOrEmpty(result.UserMessage))
            {
                if (result.Success)
                {
                    ToastService.ShowSuccess(result.UserMessage);
                }
                else
                {
                    ToastService.ShowError(result.UserMessage);
                }
            }
            if (pendingRequests.TryGetValue(requestId, out var dashboardAction) && result.Success)
            {
                dashboardAction.ToggleState = result.ToggleState;
                await DashboardService.UpdateDashboardActionAsync(dashboardAction);
            }
            // Intentional delay so the execution indicator has time to display for super fast operations
            await Task.Delay(100);
            IsExecuting = pendingRequests.Count == 0;
            StateHasChanged();
        }

        private void HandleActionExecutedResultCallback(Guid requestId, ShortDashActionResult result)
        {
            InvokeAsync(() => HandleActionExecutedResult(requestId, result));
        }

        private bool ShouldShowLabel()
        {
            return EditMode || (!HideLabel && DashboardAction?.ActionTypeName != typeof(DashSeparatorAction).FullName);
        }
    }
}