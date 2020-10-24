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
        private List<Guid> pendingRequests = new List<Guid>();

        [Parameter]
        public DashboardAction DashboardAction { get; set; }

        [Parameter]
        public bool EditMode { get; set; }

        [Inject]
        protected IToastService ToastService { get; set; }

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

        private bool ToggleState { get; set; } = false;

        public void Dispose()
        {
            CancelPendingRequests();
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            IsExecuting = false;
            ToggleState = false;
            CancelPendingRequests();
        }

        private void CancelPendingRequests()
        {
            foreach (var requestId in pendingRequests)
            {
                DashboardActionService.CancelExecuteActionRequest(requestId);
            }
            pendingRequests.Clear();
        }

        // TODO: Implement toggle functionality
        private async void ExecuteAction()
        {
            if (EditMode || IsExecuting || !await SecureContext.ValidateUserAsync())
            {
                return;
            }

            IsExecuting = true;
            ToggleState = !IsToggle || !ToggleState;
            if (DashboardAction.ActionTypeName.Equals(typeof(DashGroupAction).FullName))
            {
                await ExecuteDashGroupAction();
            }
            else
            {
                var requestId = Guid.NewGuid();
                pendingRequests.Add(requestId);
                var result = await DashboardActionService.Execute(requestId, DashboardAction, ToggleState);
                HandleActionExecutedResult(result);
            }

            // Intentional delay so the execution indicator has time to display for super fast operations
            await Task.Delay(100);
            IsExecuting = false;
            StateHasChanged();
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
                var tasks = new List<Task<ShortDashActionResult>>();
                foreach (var subAction in DashboardAction.DashboardSubActionChildren)
                {
                    var toggleState = false;
                    var requestId = Guid.NewGuid();
                    pendingRequests.Add(requestId);
                    var task = DashboardActionService.Execute(requestId, subAction.DashboardActionChild, toggleState);
                    tasks.Add(task);
                }
                var results = await Task.WhenAll(tasks.ToArray());
                foreach (var result in results)
                {
                    HandleActionExecutedResult(result);
                }
            }
        }

        private void HandleActionExecutedResult(ShortDashActionResult result)
        {
            if (string.IsNullOrEmpty(result.UserMessage))
            {
                return;
            }
            if (result.Success)
            {
                ToastService.ShowSuccess(result.UserMessage);
            }
            else
            {
                ToastService.ShowError(result.UserMessage);
            }
        }

        private bool ShouldShowCaption()
        {
            return DashboardAction?.ActionTypeName != typeof(DashSeparatorAction).FullName || EditMode;
        }
    }
}