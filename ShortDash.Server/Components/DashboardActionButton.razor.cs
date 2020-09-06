using Microsoft.AspNetCore.Components;
using ShortDash.Server.Actions;
using ShortDash.Server.Data;
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
        private Dictionary<string, object> cellAttributes;

        [Parameter]
        public DashboardCell Cell { get; set; }

        [Inject]
        protected DashboardActionService DashboardActionService { get; set; }

        private bool IsExecuting { get; set; } = false;
        private bool IsToggle { get; set; } = false;
        private bool ToggleState { get; set; } = false;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            IsExecuting = false;
            ToggleState = false;
            cellAttributes = new Dictionary<string, object>();
            if (Cell.DashboardAction.BackgroundColor != null)
            {
                cellAttributes.Add("style", "background-color: " + Cell.DashboardAction.BackgroundColor?.ToHtmlString());
            }
        }

        // TODO: Implement toggle functionality
        private async void Click()
        {
            if (IsExecuting) { return; }
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

        private string GetActiveStateClass()
        {
            return (IsToggle && ToggleState) ? "active" : "";
        }

        private bool HasIcon()
        {
            return !string.IsNullOrWhiteSpace(Cell.DashboardAction.Icon);
        }

        private bool IsExecutable(DashboardAction action)
        {
            var actionTypeName = action?.ActionTypeName;
            return !string.IsNullOrWhiteSpace(actionTypeName) && actionTypeName != typeof(DashSeparatorAction).FullName;
        }
    }
}