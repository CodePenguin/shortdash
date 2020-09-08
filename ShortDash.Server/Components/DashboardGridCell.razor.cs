using Microsoft.AspNetCore.Components;
using ShortDash.Server.Actions;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class DashboardGridCell : ComponentBase
    {
        [Parameter]
        public DashboardCell Cell { get; set; }

        [Parameter]
        public bool EditMode { get; set; }

        [Parameter]
        public EventCallback<DashboardCell> OnMoveCellLeft { get; set; }

        [Parameter]
        public EventCallback<DashboardCell> OnMoveCellRight { get; set; }

        [Parameter]
        public EventCallback<DashboardCell> OnRemoveCell { get; set; }

        [Inject]
        protected DashboardActionService DashboardActionService { get; set; }

        private bool IsExecuting { get; set; } = false;
        private bool IsToggle { get; set; } = false;
        private bool ToggleState { get; set; } = false;

        // TODO: Implement toggle functionality
        protected async void ExecuteAction()
        {
            if (EditMode || IsExecuting) { return; }
            IsExecuting = true;
            ToggleState = !IsToggle || !ToggleState;
            var result = await DashboardActionService.Execute(Cell.DashboardAction, ToggleState);
            if (result.Success)
            {
                ToggleState = result.ToggleState;
            }
            // Intentional delay so the execution indicator has time to display for super fast operations
            await Task.Delay(100);
            IsExecuting = false;
            StateHasChanged();
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            IsExecuting = false;
            ToggleState = false;
        }

        private Task MoveCellLeft()
        {
            return OnMoveCellLeft.InvokeAsync(Cell);
        }

        private Task MoveCellRight()
        {
            return OnMoveCellRight.InvokeAsync(Cell);
        }

        private Task RemoveCell()
        {
            return OnRemoveCell.InvokeAsync(Cell);
        }

        private bool ShouldShowCaption()
        {
            return Cell.DashboardAction?.ActionTypeName != typeof(DashSeparatorAction).FullName || EditMode;
        }
    }
}