using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
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
    public partial class DashboardActionButton : ComponentBase
    {
        [Parameter]
        public DashboardAction DashboardAction { get; set; }

        [Parameter]
        public bool EditMode { get; set; }

        [Inject]
        private DashboardActionService DashboardActionService { get; set; }

        private bool IsExecuting { get; set; } = false;

        private bool IsToggle { get; set; } = false;

        [CascadingParameter]
        private IModalService ModalService { get; set; }

        [CascadingParameter]
        private ISecureContext SecureContext { get; set; }

        private bool ToggleState { get; set; } = false;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            IsExecuting = false;
            ToggleState = false;
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
                await DashboardActionService.Execute(DashboardAction, ToggleState);
            }

            // Intentional delay so the execution indicator has time to display for super fast operations
            await Task.Delay(100);
            IsExecuting = false;
            StateHasChanged();
        }

        private async Task ExecuteDashGroupAction()
        {
            var actionType = DashboardActionService.FindActionType(typeof(DashGroupAction).FullName);
            var parameters = DashboardActionService.GetActionParameters(actionType, DashboardAction.Parameters) as DashGroupParameters;
            if (parameters.DashGroupType == DashGroupType.Folder)
            {
                await DashGroupActionDialog.ShowAsync(ModalService, DashboardAction);
            }
            else if (parameters.DashGroupType == DashGroupType.List)
            {
                await DashboardActionService.Execute(DashboardAction, ToggleState);
            }
        }

        private bool ShouldShowCaption()
        {
            return DashboardAction?.ActionTypeName != typeof(DashSeparatorAction).FullName || EditMode;
        }
    }
}