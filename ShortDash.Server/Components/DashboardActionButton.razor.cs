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
        public DashboardAction DashboardAction { get; set; }

        [Parameter]
        public bool IsExecuting { get; set; }

        [Parameter]
        public bool ToggleState { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            cellAttributes = new Dictionary<string, object>();
            if (DashboardAction.BackgroundColor != null)
            {
                cellAttributes.Add("style", "background-color: " + DashboardAction.BackgroundColor?.ToHtmlString());
            }
        }

        private string GetActiveStateClass()
        {
            return ToggleState ? "active" : "";
        }

        private bool HasIcon()
        {
            return !string.IsNullOrWhiteSpace(DashboardAction.Icon) && DashboardAction.Icon.StartsWith("oi-");
        }

        private bool HasImage()
        {
            return !string.IsNullOrWhiteSpace(DashboardAction.Icon);
        }

        private bool IsExecutable(DashboardAction action)
        {
            var actionTypeName = action?.ActionTypeName;
            return !string.IsNullOrWhiteSpace(actionTypeName) && actionTypeName != typeof(DashSeparatorAction).FullName;
        }
    }
}